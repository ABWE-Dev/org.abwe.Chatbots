// <copyright>
// Copyright by ABWE International
//
// Licensed under the Rock Community License
// </copyright>
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Web.Cache;
using Rock.Transactions;
using Rock;
using Rock.Web.UI.Controls;

namespace org.abwe.Chatbots.Transactions
{
    /// <summary>
    /// Executes the "IndexDocument()" method for the Entity. Use this instead of ProcessEntityTypeIndex since it filters out duplicate index operations.
    /// </summary>
    public class IndexBotContentTransaction : ITransaction
    {

        /// <summary>
        /// A thread-safe list of index requests
        /// </summary>
        private static readonly ConcurrentQueue<BotContentIndexInfo> BoxContentQueue = new ConcurrentQueue<BotContentIndexInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexBotContentTransaction" /> class.
        /// </summary>
        /// <param name="entityIndexInfo">The entity index information.</param>
        public IndexBotContentTransaction(BotContentIndexInfo entityIndexInfo )
        {
            BoxContentQueue.Enqueue( entityIndexInfo );
        }

        /// <summary>
        /// Index the content channel or the content channel item.
        /// </summary>
        public void Execute()
        {
            var contentIndexInfos = new List<BotContentIndexInfo>();

            // Each time this runs (~60 seconds) it will process all the index requests that have been queued up to this point.
            while ( BoxContentQueue.TryDequeue( out BotContentIndexInfo entityIndexInfo ) )
            {
                contentIndexInfos.Add( entityIndexInfo );
            }

            if ( !contentIndexInfos.Any() )
            {
                // Nothing to do.
                return;
            }

            // De-duplicate the index requests. It's still possible that a ContentChannelItem could be listed, and then the parent
            // ContentChannel could be listed as well. Upstream processes will override that, even though the entity will be indexed twice.
            contentIndexInfos = contentIndexInfos.GroupBy( i => new { i.EntityTypeGuid, i.EntityGuid } ).Select( i => i.FirstOrDefault() ).ToList();

            foreach ( var contentIndexInfo in contentIndexInfos )
            {
                IndexDocumentExecute( contentIndexInfo );
            }
        }

        private void IndexDocumentExecute(BotContentIndexInfo entityIndexInfo )
        {            
            if ( entityIndexInfo.EntityTypeGuid != null )
            {
                var component = new ContentChannelIndexComponent();
                if (entityIndexInfo.EntityTypeGuid == Rock.SystemGuid.EntityType.CONTENT_CHANNEL.AsGuid())
                {
                    component.IndexAllDocuments(new List<Guid?>() { entityIndexInfo.EntityGuid });
                }
                else if (entityIndexInfo.EntityTypeGuid == Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid())
                {
                    component.IndexDocument(entityIndexInfo.EntityGuid, true);
                }
            }
        }
    }

    public class BotContentIndexInfo
    {
        /// <summary>
        /// Gets or sets the entity type guid of the entity we're going to index.
        /// ContentChannels and ContentChannelItems are supported.
        /// </summary>
        /// <value>The group type identifier.</value>
        public Guid EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier of the entity we're going to index
        /// </summary>
        /// <value>The group identifier.</value>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Entity: {EntityGuid} {EntityTypeCache.Get(EntityTypeGuid).FriendlyName}, EntityId: {EntityGuid}";
        }
    }
}