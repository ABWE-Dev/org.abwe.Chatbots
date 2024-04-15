// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;
using Rock.Web.Cache;

namespace org.abwe.Chatbots
{
    /// <summary>
    /// Clear Referenced Entity Dependencies Consumer class.
    /// </summary>
    public sealed class EntityUpdatedConsumer : RockConsumer<EntityUpdateQueue, EntityWasUpdatedMessage>
    {
        /// <summary>
        /// The cache types
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> _cacheTypeMethodInfoLookup = new ConcurrentDictionary<string, MethodInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdatedConsumer"/> class.
        /// </summary>
        public EntityUpdatedConsumer()
        {
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( EntityWasUpdatedMessage message )
        {
            var contentChannelItemTypeId = EntityTypeCache.GetId<Rock.Model.ContentChannelItem>();
            if (message.EntityTypeId == contentChannelItemTypeId) {
                var component = new ContentChannelIndexComponent();

                if (message.EntityState == EntityState.Deleted.ToString()) {
                    component.DeleteDocument(message.EntityId);
                } else {
                    component.IndexDocument(message.EntityId, true);
                }
            }
        }
    }
}
