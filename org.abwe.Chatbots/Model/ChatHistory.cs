// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace org.abwe.Chatbots
{
    public static class ChatHistoryAgent {
        public const string User = "User";
        public const string Bot = "Bot";
        public const string System = "System";
    }

    public class ChatHistory
    {
        public string Agent { get; set; }
        public string Text { get; set; }
        public int? Id { get; set; }
    }

    public class ChatHistorySession
    {
        public ChatHistorySession() {
            this.Id = new Guid();
            this.History = new List<ChatHistory>();
        }

        public Guid? Id { get; set; }
        public List<ChatHistory> History { get; set; }

        public void AddMessage(string agent, string text) {
            this.History.Add(new ChatHistory() {
                Agent = agent,
                Text = text
            });
        }

        public static ChatHistorySession LoadFromSessionId(Guid? sessionId, string botName = "") {
            List<Interaction> interactions;
            using (RockContext rockContext = new RockContext()) {
                using (new QueryHintScope(rockContext, QueryHintType.RECOMPILE))
                {   
                    var interactionService = new InteractionService(rockContext);
                    interactions = interactionService.Queryable().AsNoTracking()
                        .Where(i => i.InteractionSession.Guid == sessionId)
                        .OrderByDescending(i => i.InteractionDateTime)
                        .ToList();
                }
            }

            var history = interactions.Select(i => new ChatHistory()
            {
                Agent = i.Operation == "Bot" ? ChatHistoryAgent.Bot : ChatHistoryAgent.User,
                Text = i.InteractionData,
                Id = i.Id
            }).ToList();

            if (!history.Any()) {
                history.Add(new ChatHistory()
                {
                    Agent = ChatHistoryAgent.System,
                    Text = $"Session started with {botName}",
                });
            }

            return new ChatHistorySession() {
                Id = sessionId,
                History = history
            };
        }

        private static Parser uaParser = Parser.GetDefault();

        public void Save(BlockCache block, RockRequestContext request) {
            // We don't need to save if there are no new messages or if there are no messages at all
            if (!this.History.Any() || !this.History.Any(m => m.Id == null)) {
                return;
            }

            // lookup the interaction channel, and create it if it doesn't exist
            using (RockContext rockContext = new RockContext()) {
                var botChannelGuid = SystemGuid.Interactions.ChatHistoryInteractionChannel.AsGuid();
                InteractionChannelService interactionChannelService = new InteractionChannelService(rockContext);
                var interactionChannel = interactionChannelService.Queryable()
                    .Where(a => a.Guid == botChannelGuid)
                    .FirstOrDefault();
                if (interactionChannel == null)
                {
                    interactionChannel = new InteractionChannel();
                    interactionChannel.Name = "Chats";
                    interactionChannel.UsesSession = true;
                    interactionChannel.SessionListTemplate = @"
                        {% if InteractionChannel != null and InteractionChannel != '' %}
                        {% for session in WebSessions %}
                            <div class='panel panel-widget pageviewsession'>
	                            <header class='panel-heading'>
	                            <div class='pull-left'>
		                            <h4 class='panel-title'>
		                                {{ session.PersonAlias.Person.FullName }}
			                            <small class='d-block d-sm-inline mt-1 mb-2 my-sm-0'>
			                                Started {{ session.StartDateTime }} /
			                                Duration: {{ session.StartDateTime | HumanizeTimeSpan:session.EndDateTime, 1 }}
			                            </small>
		                            </h4>
		                            <span class='label label-primary'></span>
		                            <span class='label label-info'>{{ InteractionChannel.Name }}</span>
		                            </div>
		                            {% assign icon = '' %}
		                            {% case session.InteractionSession.DeviceType.ClientType %}
			                            {% when 'Desktop' %}{% assign icon = 'fa-desktop' %}
			                            {% when 'Tablet' %}{% assign icon = 'fa-tablet' %}
			                            {% when 'Mobile' %}{% assign icon = 'fa-mobile-phone' %}
			                            {% else %}{% assign icon = '' %}
		                            {% endcase %}
		                            {% if icon != '' %}
    		                            <div class='pageviewsession-client d-flex align-items-center ml-2 ml-sm-auto'>
                                            <div class='pull-left'>
                                                <small>{{ session.InteractionSession.DeviceType.Application }} <br>
                                                {{ session.InteractionSession.DeviceType.OperatingSystem }} </small>
                                            </div>
                                            <i class='fa {{ icon }} fa-2x pull-left d-none d-sm-block margin-l-sm'></i>
                                        </div>
                                    {% endif %}
	                            </header>
	                            <div class='panel-body'>
		                            <ol>
		                            {% for interaction in session.Interactions %}
    			                        <li>{{ interaction.Operation }}: {{ interaction.InteractionData | FromMarkdown }}</li>
		                            {% endfor %}
		                            </ol>
	                            </div>
                            </div>
                        {% endfor %}
                    {% endif %}
                    ";
                    interactionChannel.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Block>().Id; ;
                    interactionChannel.Guid = SystemGuid.Interactions.ChatHistoryInteractionChannel.AsGuid();
                    interactionChannelService.Add(interactionChannel);
                    rockContext.SaveChanges();
                }

                // check that the block exists as a component
                var interactionComponent = new InteractionComponentService(rockContext).GetComponentByChannelIdAndEntityId(interactionChannel.Id, block.Id, block.Name);
                rockContext.SaveChanges();

                // Add the interaction
                if (interactionComponent != null)
                {
                    foreach (var message in this.History.Where(m => m.Id == null)) {
                        var personAliasId = request.CurrentPerson?.PrimaryAliasId;

                        ClientInfo client = uaParser.Parse(request.ClientInformation.UserAgent);
                        var clientOs = client.OS.ToString();
                        var clientBrowser = client.UA.ToString();
                        var clientType = InteractionDeviceType.GetClientType(request.ClientInformation.UserAgent);
                        var userAgent = request.ClientInformation.UserAgent;
                        if (userAgent.Length > 450) {
                            userAgent = userAgent.Substring(0, 450);
                        }

                        var interaction = new InteractionService(rockContext).AddInteraction(interactionComponent.Id, null, message.Agent, message.Text, personAliasId, DateTime.Now,
                            clientBrowser, clientOs, clientType, userAgent,
                            request.ClientInformation.IpAddress, this.Id);
                        message.Id = interaction.Id;
                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}
