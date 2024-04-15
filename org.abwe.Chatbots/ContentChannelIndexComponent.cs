using org.abwe.Chatbots.Stores;
using org.abwe.Chatbots.TextSplitters;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.abwe.Chatbots
{
    public class ContentChannelIndexComponent
    {

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
            var chunkedItems = chunks.Select((chunk, index) => new ContentChannelItemIndexDocument
            {
                Text = chunk,
                ChunkId = Guid.NewGuid(),
                Metadata = new ContentChannelItemIndexDocumentMetadata
                {
                    ContentChannelId = item.ContentChannelId,
                    id = item.Guid,
                    title = item.Title,
                    url = item.PrimarySlug
                },
                Vector = null
            }).ToList();

            if (secondaryChunkSize > 0)
            {
                // Chunk the chunks into smaller chunks
                var secondaryChunks = new List<ContentChannelItemIndexDocument>();
                var secondaryTextSplitter = new RecursiveTextSplitter(true, secondaryChunkSize, 0);
                foreach (var chunk in chunkedItems)
                {
                    if (chunk.Text.Length > secondaryChunkSize)
                    {
                        var secondaryChunksForItem = secondaryTextSplitter.SplitText(chunk.Text, RecursiveTextSplitter.HtmlSeparators, true);
                        secondaryChunks.AddRange(secondaryChunksForItem.Select((secondaryChunk, index) => new ContentChannelItemIndexDocument
                        {
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

        public void DeleteDocument(int ContentChannelItemId) {
            var rockContext = new RockContext();
            var elasticsearch = new Elasticsearch(null);

            var item = new ContentChannelItemService(rockContext).Get(ContentChannelItemId);
            elasticsearch.RemoveContentChannelItemChunks(item.Guid).Wait();
        }

        public void IndexDocument(int ContentChannelItemId, bool removePrevious = true) {
            // Drop and rebuild index
            var rockContext = new RockContext();

            // Get the content channel item
            var item = new ContentChannelItemService(rockContext).Get(ContentChannelItemId);

            // Get all content channels that are used in chatbots
            var contentChannelGuids = GetContentChannelsUsedInChatbots(rockContext);

            // If this item is not in a content channel used by a chatbot,
            // we don't need to index it
            if (!contentChannelGuids.Any(channelGuid => channelGuid == item.ContentChannel.Guid)) {
                return;
            }

            var openAIKey = Util.GetOpenAIKey();
            var openAi = new Interfaces.OpenAI.OpenAI(openAIKey);
            var elasticsearch = new Elasticsearch(openAi);

            if (removePrevious) {
                elasticsearch.RemoveContentChannelItemChunks(item.Guid).Wait();
            }

            // Split content into chunks
            var configuration = Util.GetConfiguration();
            var chunks = CreateChunksFromItem(item, configuration.ChunkSize, configuration.ChunkOverlap, configuration.SecondPassChunkSize);

            // Use parallel requests to speed things up
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };

            Parallel.ForEach(chunks, options, chunk =>
            {
                chunk.Vector = openAi.GetEmbedding(chunk.Text).Result;
                elasticsearch.IndexDocument(chunk);
            });
        }

        // Index All Documents
        // This method will index all documents for all content channels that are used in chatbots.
        public void IndexAllDocuments()
        {
            // Drop and rebuild index
            var rockContext = new RockContext();

            // Get all content channels that are used in chatbots
            var contentChannelGuids = GetContentChannelsUsedInChatbots(rockContext);

            // Get all content channel items for the content channels
            var contentChannelItems = GetContentChannelItems(contentChannelGuids, rockContext);

            var openAIKey = Util.GetOpenAIKey();
            var openAi = new Interfaces.OpenAI.OpenAI(openAIKey);
            var elasticsearch = new Elasticsearch(openAi);

            // Split content into chunks
            var configuration = Util.GetConfiguration();
            var chunks = new List<ContentChannelItemIndexDocument>();
            foreach (var item in contentChannelItems)
            {
                chunks.AddRange(CreateChunksFromItem(item, configuration.ChunkSize, configuration.ChunkOverlap, configuration.SecondPassChunkSize));
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

        private List<ContentChannelItem> GetContentChannelItems(List<Guid?> contentChannelGuids, RockContext rockContext)
        {
            var contentChannelItems = new ContentChannelItemService(rockContext)
                        .Queryable()
                        .Where(item => contentChannelGuids.Any(channelGuid => channelGuid == item.ContentChannel.Guid))
                        .ToList();

            return contentChannelItems;
        }

        private List<Guid?> GetContentChannelsUsedInChatbots(RockContext rockContext)
        {
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
                return contentChannelGuids;
            }

            return new List<Guid?>(); // Return an empty list if no blocks are found
        }
    }
}
