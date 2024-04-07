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
using System.Linq.Expressions;
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
    [CustomRadioListField("Pre-Prompt Model", "Using a more expensive model for the pre-prompt can improve results, especially for Anthropic.", "GPT3.5,GPT4,GPT4Turbo,Claude Haiku,Claude Sonnet,Claude Opus", true, "GPT3.5", "", 7, AttributeKey.PrePromptModel)]
    [CustomRadioListField("Model", "The model to use for the chat. GPT-3.5 is the cheapest, and GPT-4 most expensive. For Anthropic, Opus is the most expensive, and Haiku least.", "GPT3.5,GPT4,GPT4Turbo,Claude Haiku,Claude Sonnet,Claude Opus", true, "GPT3.5", "", 8, AttributeKey.Model)]

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
            public string _id { get; set; }
            public string text { get; set; }
            public List<float> vector { get; set; }
            public ContentChannelItemMetadata metadata { get; set; }
            public Guid chunkId { get; set; }
            public Guid? parentChunkId { get; set; }
        }

        public override string ObsidianFileUrl => $"/Plugins/org_abwe/Chatbots/Blocks/SupportChat.obs";

        #region Keys

        private static class AttributeKey
        {
            public const string PrePrompt = "PrePrompt";
            public const string Model = "Model";
            public const string PrePromptModel = "PrePromptModel";
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
                    var cancellationToken = context; // CancellationToken for client disconnection
                    try {
                        // Use StreamWriter or similar to write to the stream
                        using (var writer = new StreamWriter(stream))
                        {
                            var model = GetAttributeValue("Model");
                            var modelToUse = Interfaces.OpenAI.Models.GPT35Turbo;
                            var llmVendor = "openai";
                            switch (model) {
                                case "GPT3.5":
                                    modelToUse = Interfaces.OpenAI.Models.GPT35Turbo;
                                    break;
                                case "GPT4":
                                    modelToUse = Interfaces.OpenAI.Models.GPT4;
                                    break;
                                case "GPT4Turbo":
                                    modelToUse = Interfaces.OpenAI.Models.GPT4Turbo;
                                    break;
                                case "Claude Haiku":
                                    llmVendor = "anthropic";
                                    modelToUse = Interfaces.Claude.Models.Haiku;
                                    break;
                                case "Claude Sonnet":
                                    llmVendor = "anthropic";
                                    modelToUse = Interfaces.Claude.Models.Sonnet;
                                    break;
                                case "Claude Opus":
                                    llmVendor = "anthropic";
                                    modelToUse = Interfaces.Claude.Models.Opus;
                                    break;
                                default:
                                    modelToUse = Interfaces.OpenAI.Models.GPT35Turbo;
                                    break;
                            }

                            var prePromptModel = GetAttributeValue(AttributeKey.PrePromptModel);
                            var prePromptModelToUse = Interfaces.OpenAI.Models.GPT35Turbo;
                            var prePromptLLMVendor = "openai";
                            switch (prePromptModel)
                            {
                                case "GPT3.5":
                                    prePromptModelToUse = Interfaces.OpenAI.Models.GPT35Turbo;
                                    break;
                                case "GPT4":
                                    prePromptModelToUse = Interfaces.OpenAI.Models.GPT4;
                                    break;
                                case "GPT4Turbo":
                                    prePromptModelToUse = Interfaces.OpenAI.Models.GPT4Turbo;
                                    break;
                                case "Claude Haiku":
                                    prePromptLLMVendor = "anthropic";
                                    prePromptModelToUse = Interfaces.Claude.Models.Haiku;
                                    break;
                                case "Claude Sonnet":
                                    prePromptLLMVendor = "anthropic";
                                    prePromptModelToUse = Interfaces.Claude.Models.Sonnet;
                                    break;
                                case "Claude Opus":
                                    prePromptLLMVendor = "anthropic";
                                    prePromptModelToUse = Interfaces.Claude.Models.Opus;
                                    break;
                                default:
                                    prePromptModelToUse = Interfaces.OpenAI.Models.GPT35Turbo;
                                    break;
                            }
                            var openAIKey = Util.GetOpenAIKey();
                            var claudeAPIKey = Util.GetClaudeKey();

                            // OpenAI is currently always used for embeddings
                            var openAi = new Interfaces.OpenAI.OpenAI(openAIKey);

                            IResponseGenerator LLM;
                            IResponseGenerator prepromptLLM;
                            
                            if (llmVendor == "anthropic") {
                                
                                LLM = new Interfaces.Claude.Claude(claudeAPIKey);
                            } else {
                                LLM = openAi;
                            }

                            if (prePromptLLMVendor == "anthropic") {
                                prepromptLLM = new Interfaces.Claude.Claude(claudeAPIKey);
                            } else {
                                prepromptLLM = openAi;
                            }

                            var elasticsearch = new Elasticsearch(openAi);
                            var linkedPage = GetAttributeValue("ContentChannelItemDetailPage");
                            var docs = await elasticsearch.SearchVector<ContentChannelItemIndex>(message, contentChannelIds);

                            // If we're using two-pass chunks, we need to get the larger chunks
                            // to pass to the LLM as it will have more context.
                            var fullDocs = docs.Where(d => d.parentChunkId == null).ToList();
                            var partialDocs = docs.Where(d => d.parentChunkId != null).ToList();
                            var partialParentIds = partialDocs.Select(d => d.parentChunkId.ToString()).Distinct().ToList();
                            var partialParents = await elasticsearch.GetDocuments<ContentChannelItemIndex>(partialParentIds);
                            fullDocs.AddRange(partialParents);

                            if (fullDocs != null && fullDocs.Any())
                            {
                                var docsAsJson = fullDocs.Select(doc => new {
                                    name = doc.metadata.title,
                                    url = new PageReference(linkedPage, parameters: new Dictionary<string, string>() { { "Item", doc.metadata.id.ToString() } }).BuildUrl(),
                                    id = doc.metadata.id 
                                }).ToJson();

                                await writer.WriteLineAsync($"data: DESCRIPTOR:{docsAsJson}");
                                await writer.FlushAsync();
                            }

                            var prePrompt = GetPrePrompt(message, history.History.Take(history.History.Count - 1).ToList());

                            var revisedQuestion = await prepromptLLM.GetResponse(prePrompt, model: prePromptModelToUse);
                            history.AddMessage(ChatHistoryAgent.System, revisedQuestion);
                            history.Save(this.BlockCache, this.RequestContext);

                            var prompt = GetMainPrompt(revisedQuestion, fullDocs);

                            var text = "";
                            try
                            {
                                await LLM.GetResponse(prompt, model: modelToUse, (string output) =>
                                {
                                    text += output;
                                    writer.WriteLine("data: " + JsonConvert.SerializeObject(output));
                                    writer.Flush();
                                });
                            } catch (Exception ex) {
                                ExceptionLogService.LogException(ex);
                            } finally {
                                history.AddMessage(ChatHistoryAgent.Bot, text);
                                history.Save(this.BlockCache, this.RequestContext);

                                writer.Close();
                            }
                        }
                    } catch (Exception ex) {
                        ExceptionLogService.LogException(ex);
                    } finally {
                        stream.Close();
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
