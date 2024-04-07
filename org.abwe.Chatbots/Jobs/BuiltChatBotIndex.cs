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

    [IntegerField("Chunk Size", "The chunk size to use when splitting content into smaller pieces for processing.", false, 1600, "", 0, AttributeKey.ChunkSize)]
    [IntegerField("Chunk Overlap", "The number of characters to overlap between chunks.", false, 250, "", 1, AttributeKey.ChunkOverlap)]
    [IntegerField("Second Pass Chunk Size", "Break content into even smaller chunks for search. The larger chunk above will be passed to the LLM.", false, 100, "", 0, AttributeKey.SecondPassChunkSize)]

    [DisallowConcurrentExecution]
    public class BuildChatbotIndex : RockJob
    {
        private class AttributeKey
        {
            public const string ChunkSize = "ChunkSize";
            public const string ChunkOverlap = "ChunkOverlap";
            public const string SecondPassChunkSize = "SecondPassChunkSize";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildChatbotIndex"/> class.
        /// </summary>
        public BuildChatbotIndex()
        {
        }


        /// <summary>
        /// Splits ContentChannelItems into chunks while trying to keep relevant texts together.
        /// Prefers splits on paragraph breaks.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="chunkSize">The number of characters to use as the chunk size for the content channel item</param>
        /// <param name="chunkOverlap">The number of characters to overlap each chunk</param>
        /// <returns>List of index documents representing the chunks of the content channel item.</returns>
        public List<ContentChannelItemIndexDocument> CreateChunksFromItem(ContentChannelItem item, int chunkSize, int chunkOverlap, int secondaryChunkSize = 0)
        {
            var textSplitter = new RecursiveTextSplitter(true, chunkSize, chunkOverlap);
            var chunks = textSplitter.SplitText(String.Concat(item.Title, item.Content), RecursiveTextSplitter.HtmlSeparators, true);
            var chunkedItems = chunks.Select((chunk, index) => new ContentChannelItemIndexDocument {
                Text = chunk,
                ChunkId = Guid.NewGuid(),
                Metadata = new ContentChannelItemIndexDocumentMetadata {
                    ContentChannelId = item.ContentChannelId,
                    id = item.Guid,
                    title = item.Title,
                    url = item.PrimarySlug
                },
                Vector = null
            }).ToList();

            if (secondaryChunkSize > 0) {
                // Chunk the chunks into smaller chunks
                var secondaryChunks = new List<ContentChannelItemIndexDocument>();
                var secondaryTextSplitter = new RecursiveTextSplitter(true, secondaryChunkSize, 0);
                foreach (var chunk in chunkedItems)
                {
                    if (chunk.Text.Length > secondaryChunkSize) {
                        var secondaryChunksForItem = secondaryTextSplitter.SplitText(chunk.Text, RecursiveTextSplitter.HtmlSeparators, true);
                        secondaryChunks.AddRange(secondaryChunksForItem.Select((secondaryChunk, index) => new ContentChannelItemIndexDocument {
                            Text = secondaryChunk,
                            Metadata = chunk.Metadata,
                            Vector = null,
                            ChunkId = Guid.NewGuid(),
                            ParentChunkId = chunk.ChunkId
                        }));
                    }
                }

                chunkedItems.AddRange(secondaryChunks);
            }

            return chunkedItems;
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

            var rockContext = new RockContext();
            // Only index content that is used by a chatbot
            var blockTypeId = BlockTypeCache.Get("b5505578-bc48-4fde-94fd-e23c00cb64da".AsGuid()).Id;
            var blockIds = new BlockService(rockContext)
                .GetByBlockTypeId(blockTypeId)
                .Select(b => b.Id)
                .ToList();

            if (blockIds.Any())
            {
                var contentChannelGuids = new AttributeValueService(rockContext)
                    .Queryable()
                    .Where(av => av.Attribute.Key == "ContentChannels")
                    .Where(av => av.Attribute.EntityTypeQualifierColumn == "BlockTypeId" && av.Attribute.EntityTypeQualifierValue == blockTypeId.ToString())
                    .Where(av => blockIds.Any(blockId => blockId == av.EntityId))
                    .Select(av => av.Value)
                    .ToList()
                    .SelectMany(guidStringList =>
                    {
                        var guids = guidStringList.Split(',').AsGuidOrNullList();
                        return guids;
                    })
                    .ToList();

                if (contentChannelGuids.Any())
                {
                    var contentChannelItems = new ContentChannelItemService(rockContext)
                        .Queryable()
                        .Where(item => contentChannelGuids.Any(channelGuid => channelGuid == item.ContentChannel.Guid))
                        .ToList();


                    var openAIKey = Util.GetOpenAIKey();
                    var openAi = new Interfaces.OpenAI.OpenAI(openAIKey);
                    var elasticsearch = new Elasticsearch(openAi);

                    // Split content into chunks
                    var chunkSize = GetAttributeValue(AttributeKey.ChunkSize).AsInteger();
                    var secondaryChunkSize = GetAttributeValue(AttributeKey.SecondPassChunkSize).AsInteger();
                    var chunkOverlap = GetAttributeValue(AttributeKey.ChunkOverlap).AsInteger();
                    var chunks = new List<ContentChannelItemIndexDocument>();
                    foreach (var item in contentChannelItems)
                    {
                        chunks.AddRange(CreateChunksFromItem(item, chunkSize, chunkOverlap, secondaryChunkSize));
                    }

                    // Drop and rebuild index
                    elasticsearch.RecreateIndex();

                    // Use parallel requests to speed things up
                    ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };

                    Parallel.ForEach(chunks, options, chunk =>
                    {
                        chunk.Vector = openAi.GetEmbedding(chunk.Text).Result;
                        elasticsearch.IndexDocument(chunk);
                    });
                }
            }
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