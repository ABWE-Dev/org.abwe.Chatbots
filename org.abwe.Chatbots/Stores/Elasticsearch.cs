﻿// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using org.abwe.Chatbots.Interfaces;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.abwe.Chatbots.Stores
{
    public class Elasticsearch
    {
        private ElasticsearchClient _client;
        private IEmbeddingGenerator _embeddingGenerator;
        private string IndexName = "org_abwe_chatbots";
        private static string IndexAttributeKey = "AbweChatbotIndex";

        public Elasticsearch(IEmbeddingGenerator embeddingGenerator, string IndexName)
        {
            _embeddingGenerator = embeddingGenerator;
            this.IndexName = IndexName;
        }

        public void Connect()
        {
            var avService = new AttributeValueService(new RockContext());
            var ElasticSearchComponentId = "97DACCE9-F397-4E7B-9596-783A233FCFCF".AsGuid();
            var nodeUrl = avService.Queryable().Where(av => av.Attribute.Key == "NodeURL" && av.Attribute.EntityType.Guid == ElasticSearchComponentId).FirstOrDefault();
            var certificateFingerprint = avService.Queryable().Where(av => av.Attribute.Key == "CertificateFingerprint" && av.Attribute.EntityType.Guid == ElasticSearchComponentId).FirstOrDefault();
            var Username = avService.Queryable().Where(av => av.Attribute.Key == "UserName" && av.Attribute.EntityType.Guid == ElasticSearchComponentId).FirstOrDefault();
            var Password = avService.Queryable().Where(av => av.Attribute.Key == "Password" && av.Attribute.EntityType.Guid == ElasticSearchComponentId).FirstOrDefault();

            var settings = new ElasticsearchClientSettings(new Uri(nodeUrl.Value))
                .CertificateFingerprint(certificateFingerprint.Value)
                .Authentication(new BasicAuthentication(Username.Value, Password.Value))
                .EnableDebugMode();

            _client = new ElasticsearchClient(settings);
        }

        public void RecreateIndex()
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            // Check if index already exists.
            var existsResponse = _client.Indices.Exists(IndexName);

            if (existsResponse.Exists)
            {
                _client.Indices.Delete(IndexName);
            }

            var response = _client.Indices.Create(IndexName, d =>
            {
                d.Mappings(m => m.Properties<ContentChannelItemIndexDocument>(p => p
                    .DenseVector("vector", dv => dv
                        .Similarity("cosine")
                        .Dims(1024)
                        .Index(true)
                    )

                    .Text("chunkId", t => t
                        .Index(true)
                        .Analyzer("keyword")
                    )

                    .Text("metadata.id", t => t
                        .Index(true)
                        .Analyzer("keyword")
                    )
                ));
            });

            if (!response.IsValidResponse)
            {
                throw new Exception("Error creating index");
            }
        }

        public void IndexDocument<T>(T document)
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            var response = _client.Index(document, d => d.Index(IndexName));

            if (!response.IsValidResponse)
            {
                throw new Exception("Error indexing document");
            }
        }

        public void IndexDocuments<T>(List<T> documents)
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            var response = _client.Bulk(b => b.Index(IndexName).IndexMany(documents));

            if (!response.IsValidResponse)
            {
                throw new Exception("Error indexing documents");
            }
        }

        public async Task<IReadOnlyCollection<T>> SearchVector<T>(string query, List<int> channelIds = null)
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            var vector = await _embeddingGenerator.GetEmbedding(query, model: Interfaces.OpenAI.EmbeddingModels.TextEmbedding3Large);

            if (vector == null)
            {
                throw new Exception("Error generating vector");
            }

            var KnnQuery = new List<KnnQuery>() { new KnnQuery
            {
                Field = "vector",
                NumCandidates = 50,
                k = 5,
                QueryVector = vector
            } };

            if (channelIds != null)
            {
                KnnQuery[0].Filter = new List<Query>() {
                    Query.Terms(new TermsQuery
                    {
                        Field = "metadata.contentChannelId",
                        Terms = new TermsQueryField(channelIds.Select(id => FieldValue.Long(id)).ToArray())
                    })
                };
            }

            var response = await _client.SearchAsync<T>(s => s
                .Index(IndexName)
                .From(0)
                .Size(10)
                .Knn(KnnQuery)
            );

            if (response.IsValidResponse)
            {
                return response.Documents;
            }

            return null;
        }

        public async Task<bool> RemoveContentChannelItemChunks(Guid? contentChannelItemGuid)
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            var response = await _client.DeleteByQueryAsync(IndexName, d => d
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Term(t => t
                                .Field("metadata.id")
                                .Value(contentChannelItemGuid.Value.ToString())
                            )
                        )
                    )
                )
            );

            if (response.IsValidResponse)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveContentChannelChunks(int? contentChannelId)
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            var response = await _client.DeleteByQueryAsync(IndexName, d => d
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Term(t => t
                                .Field("metadata.contentChannelId")
                                .Value(FieldValue.Long(contentChannelId.Value))
                            )
                        )
                    )
                )
            );

            if (response.IsValidResponse)
            {
                return true;
            }

            return false;
        }

        public async Task<IReadOnlyCollection<T>> GetDocuments<T>(List<string> ids)
        {
            if (_client == null)
            {
                Connect();
            }

            if (_client == null)
            {
                throw new Exception("Error connecting to Elasticsearch");
            }

            var response = await _client.SearchAsync<T>(s => s
                .Index(IndexName)
                .From(0)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Terms(t => t
                                .Field("chunkId")
                                .Terms(new TermsQueryField(ids.Select(id => FieldValue.String(id)).ToArray()))
                            )
                        )
                    )
                )
            );

            if (response.IsValidResponse)
            {
                return response.Documents;
            }

            return null;
        }
    }
}
