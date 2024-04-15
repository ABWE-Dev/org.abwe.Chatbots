// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using org.abwe.Chatbots.Interfaces;
using org.abwe.Chatbots.Stores;
using org.abwe.Chatbots.TextSplitters;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace org.abwe.Chatbots
{
    /// <summary>
    /// This job will build the indexes inside Elasticsearch needed for chat bots to work. This uses OpenAI to get embeddings. The embeddings must match what the retrieval function for the bots use.
    /// It rebuilds the indexes every time the job runs, and it only stores content of the channels that are used in bot blocks.
    /// </summary>
    [DisplayName("Build ChatBot Index")]
    [Description("This job generates embeddings for content channels used in chat bots and recreates the bot index in Elasticsearch.")]

    [DisallowConcurrentExecution]
    public class BuildChatbotIndex : RockJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildChatbotIndex"/> class.
        /// </summary>
        public BuildChatbotIndex()
        {
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        public override void Execute()
        {
            // Steps
            // 1. Get all content channels used in blocks
            // 2. If any, get Elasticsearch connection info and OpenAI key
            // 3. If successful, split all content channels into chunks according to chunks attribute (use recursive text splitter)
            //      TODO: Make it intelligent to HTML and Lava
            // 4. Generate embeddings for each section of text and attach to metadata/content
            // 5. Drop Elasticsearch index
            // 6. Rebuild elasticsearch index
            try {
                var component = new ContentChannelIndexComponent();
                component.IndexAllDocuments();
            }
            catch (Exception e)
            {
                ExceptionLogService.LogException(e);
                this.Result = e.Message;
                throw new Exception(e.Message);
            }
        }
    }
}