// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.abwe.Chatbots
{
    public class Util
    {
        public static string GetOpenAIKey()
        {
            var avService = new AttributeValueService(new RockContext());
            var OpenAIComponentGuid = "8D3F25B1-4891-31AA-4FA6-365F5C808563".AsGuid();
            var attributeValue = avService.Queryable().Where(av => av.Attribute.Key == "SecretKey" && av.Attribute.EntityType.Guid == OpenAIComponentGuid).FirstOrDefault();
            return attributeValue.Value;
        }

        public static string GetClaudeKey()
        {
            return GlobalAttributesCache.Get().GetValue("ClaudeAPIKey");
        }

        public static ChatbotConfiguration GetConfiguration() {
            ChatbotConfiguration chatbotConfiguration = null;
            try
            {
                chatbotConfiguration = JsonConvert.DeserializeObject<ChatbotConfiguration>(SystemSettings.GetValue("org_abwe_Chatbot_Configuration"));
            }
            catch (Exception)
            {
            }

            if (chatbotConfiguration == null) {
                chatbotConfiguration = new ChatbotConfiguration();
                chatbotConfiguration.IndexName = "org_abwe_chatbot";
                chatbotConfiguration.ChunkSize = 1600;
                chatbotConfiguration.ChunkOverlap = 250;
                chatbotConfiguration.SecondPassChunkSize = 100;
            }

            return chatbotConfiguration;
        }
    }
}
