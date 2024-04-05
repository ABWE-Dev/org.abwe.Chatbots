// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Newtonsoft.Json;
using org.abwe.Chatbots.Interfaces;
using org.abwe.Chatbots.Stores;
using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Lava.Blocks;
using Rock.Model;
using Rock.Net;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.UniversalSearch.IndexModels;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using static Rock.WebFarm.RockWebFarm;

namespace org.abwe.Chatbots.Blocks
{
    /// <summary>
    /// Displays a chatbot on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName("Support Chat")]
    [Category("org_abwe > Chatbots")]
    [Description("Display a support chat")]
    [SupportedSiteTypes(Rock.Model.SiteType.Web)]

    #region Block Attributes

    [LavaField("Pre Prompt", "The Lava template to use to generate the pre-prompt. This should condense the chat history to a single question. Merge fields are History and Input", false, @"
Given the following conversation and a follow up question, rephrase the follow up question to be a standalone question, in its original language. If there is no chat history, just rephrase the question to be a standalone question.

Chat History:
{{ History }}
Follow Up Input: {{ Input }}
    ", order: 1)]
    [LavaField("Prompt", "The Lava template to use to generate the prompt. Merge fields are Articles and Input", false, @"
You are a support assistant for ABWE international. We sent missionaries around the world to various locations in service of Jesus Christ.
Answer the user's questions thorougly and politely.

Use the following passages to answer the user's question.
Each passage has a SOURCE which is the title of the document. When answering, cite source name of the passages you are answering inline where the answers appear.
Use markdown to make each source a link to the source document. Format your entire response in markdown.

----
{{ Articles }}
----

Question: {{ Input }}
    ", order: 2)]
    [BooleanField("Respect User Security", "If enabled, the block will only include content channels in the results which the current user has access to.", false, "", 3)]
    [ContentChannelsField("Content Channels", "The content channels to search for responses.", false, "", "", 4)]
    [CustomRadioListField("Style", "The style of the chat", "Inline,Popup", true, "Inline", "", 5)]
    [LinkedPage("Content Channel Item Detail Page", "This page will be used as the landing page for source links that are clicked", true, "", "", 6)]
    [CustomRadioListField("Model", "The OpenAI model to use for the chat. GPT-3.5 is the cheapest, and GPT-4 most expensive", "GPT3.5,GPT4,GPT4Turbo", true, "GPT3.5", "", 5)]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid("ffb0aed5-f2b0-46da-8943-5b7e7bc4171e")]
    [Rock.SystemGuid.BlockTypeGuid("b5505578-bc48-4fde-94fd-e23c00cb64da")]
    public class SupportChat : RockBlockType
    {
        public class ContentChannelItemMetadata
        {
            public string title { get; set; }
            public string url { get; set; }
            public int contentChannelId { get; set; }
            public Guid id { get; set; }
        }

        public class ContentChannelItemIndex
        {
            public string text { get; set; }
            public List<float> vector { get; set; }
            public ContentChannelItemMetadata metadata { get; set; }
        }

        public override string ObsidianFileUrl => $"/Plugins/org_abwe/Chatbots/Blocks/SupportChat.obs";

        #region Keys

        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using (var rockContext = new RockContext())
            {
                var box = new SupportChatBox();

                box.Style = GetAttributeValue("Style");

                return box;
            }
        }

        public string GetPrePrompt(string input, List<ChatHistory> history)
        {
            var historyFormatted = "";

            if (history.Any())
            {
                historyFormatted = history.Where(h => h.Agent != "System").Select(h => $"{h.Agent}: {h.Text}").Aggregate((a, b) => $"{a}\n{b}\n\n");
            }

            var mergeFields = new Dictionary<string, object>
            {
                { "History", historyFormatted },
                { "Input", input }
            };

            var prompt = GetAttributeValue("PrePrompt");

            return prompt.ResolveMergeFields(mergeFields);
        }

        public string GetMainPrompt(string input, IReadOnlyCollection<ContentChannelItemIndex> items)
        {
            var context = new StringBuilder();
            var docMergeFields = new Dictionary<string, object>();
            var linkedPage = GetAttributeValue("ContentChannelItemDetailPage");
            var currentPerson = GetCurrentPerson();
            if (items != null && items.Any()) {
                foreach (var item in items)
                {
                    context.Append($@"
                        ---                
                        NAME: {item.metadata.title}
                        URL: { new PageReference(linkedPage, parameters: new Dictionary<string, string>() { { "Item", item.metadata.id.ToString() } }).BuildUrl() }
                        PASSAGE: {item.text.ResolveMergeFields(docMergeFields, currentPerson)}
                        ---
                    ");
                }
            }

            if (context.Length == 0) {
                context.Append("--- No articles found ---");
            }

            var mergeFields = new Dictionary<string, object>
            {
                { "Articles", context.ToString() },
                { "Input", input }
            };

            var prompt = GetAttributeValue("Prompt");

            return prompt.ResolveMergeFields(mergeFields);
        }

        #endregion

        #region Block Actions

        ///// <summary>
        ///// Searches for content channel items related to the user message and processes the results through
        ///// the prompts to generate a response.
        ///// </summary>
        ///// <param name="message">The user message</param>
        ///// <param name="sessionId">A session id (used to save/retrieve conversation history)</param>
        ///// <returns>A string with the processed lava</returns>
        [BlockAction]
        public BlockActionResult GetMessage(string message, Guid sessionId)
        {
            try {
                var contentChannelsToSearch = GetAttributeValue("ContentChannels").Split(',').AsGuidList();
                var onlyAuthorized = GetAttributeValue("RespectUserSecurity").AsBoolean();
                List<int> contentChannelIds = null;
                if (contentChannelsToSearch.Any()) {
                    var contentChannels = new ContentChannelService(new RockContext()).Queryable().Where(c => contentChannelsToSearch.Any(cts => cts == c.Guid)).ToList();
                    var currentPerson = GetCurrentPerson();
                    if (onlyAuthorized) {
                        contentChannelIds = contentChannels.Where(c => c.IsAuthorized(Authorization.VIEW, currentPerson)).Select(c => c.Id).ToList();
                    } else {
                        contentChannelIds = contentChannels.Select(c => c.Id).ToList();
                    }
                }

                // var response = Request.CreateResponse();
                var history = ChatHistorySession.LoadFromSessionId(sessionId);
                history.AddMessage(ChatHistoryAgent.User, message);
                history.Save(this.BlockCache, this.RequestContext);

                var responseToSend = new PushStreamContent(async (stream, content, context) =>
                {
                    // Use StreamWriter or similar to write to the stream
                    using (var writer = new StreamWriter(stream))
                    {
                        var model = GetAttributeValue("Model");
                        var modelToUse = Models.GPT35Turbo;
                        switch (model) {
                            case "GPT3.5":
                                modelToUse = Models.GPT35Turbo;
                                break;
                            case "GPT4":
                                modelToUse = Models.GPT4;
                                break;
                            case "GPT4Turbo":
                                modelToUse = Models.GPT4Turbo;
                                break;
                            default:
                                modelToUse = Models.GPT35Turbo;
                                break;
                        }
                        var openAIKey = Util.GetOpenAIKey();
                        var openAi = new OpenAI(openAIKey);
                        var elasticsearch = new Elasticsearch(openAi);
                        var linkedPage = GetAttributeValue("ContentChannelItemDetailPage");
                        var docs = await elasticsearch.SearchVector<ContentChannelItemIndex>(message, contentChannelIds);
                        if (docs != null && docs.Any())
                        {
                            var docsAsJson = docs.Select(doc => new {
                                name = doc.metadata.title,
                                url = new PageReference(linkedPage, parameters: new Dictionary<string, string>() { { "Item", doc.metadata.id.ToString() } }).BuildUrl(),
                                id = doc.metadata.id 
                            }).ToJson();

                            await writer.WriteLineAsync($"data: DESCRIPTOR:{docsAsJson}");
                            await writer.FlushAsync();
                        }

                        var prePrompt = GetPrePrompt(message, history.History.Take(history.History.Count - 1).ToList());

                        var revisedQuestion = await openAi.GetResponse(prePrompt, model: modelToUse);
                        history.AddMessage(ChatHistoryAgent.System, revisedQuestion);
                        history.Save(this.BlockCache, this.RequestContext);

                        var prompt = GetMainPrompt(revisedQuestion, docs);

                        var text = await openAi.GetResponse(prompt, model: modelToUse, (string output) => {
                            writer.WriteLine("data: " + JsonConvert.SerializeObject(output));
                            writer.Flush();
                        });

                        history.AddMessage(ChatHistoryAgent.Bot, text);
                        history.Save(this.BlockCache, this.RequestContext);

                        writer.Close();
                    }
                }, "text/event-stream"); // Set the appropriate media type

                return ActionOk(responseToSend);
            } catch (Exception e) {
                ExceptionLogService.LogException(e);
                return ActionBadRequest();
            }
        }

        #endregion
    }
}
