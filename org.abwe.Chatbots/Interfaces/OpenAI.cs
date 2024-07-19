// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace org.abwe.Chatbots.Interfaces.OpenAI
{
    public static class Models
    {
        public const string GPT35Turbo = "gpt-3.5-turbo";
        public const string GPT4 = "gpt-4";
        public const string GPT4Turbo = "gpt-4-0125-preview";
        public const string GPT4o = "gpt-4o";
    }

    public static class EmbeddingModels {
        public const string TextEmbedding3Large = "text-embedding-3-large";
    }

    public class Embedding {
        public List<float> embedding;
        public int index;
    }

    public class EmbeddingResponse {
        public List<Embedding> data;
        public int index;
    }

    public class OpenAI : IEmbeddingGenerator, IResponseGenerator
    {
        private string _apiKey;

        public OpenAI(string apiKey) {
            _apiKey = apiKey;
        }

        public async Task<List<float>> GetEmbedding(string input, string model = EmbeddingModels.TextEmbedding3Large)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                var requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(new {
                    input,
                    model = model,
                    encoding_format = "float",
                    dimensions = 1024
                });
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    using (var response = await client.SendAsync(request))
                    {
                        try {
                            response.EnsureSuccessStatusCode();
                        } catch {
                            return null;
                        }
                        string responseBody = await response.Content.ReadAsStringAsync();
                        EmbeddingResponse responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbeddingResponse>(responseBody);
                        return responseObj.data[0].embedding;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<string> GetResponse(string input, string model = Models.GPT4Turbo, Action<string> handle = null)
        {
            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                model = model,
                messages = new[] {
                    new {
                        role = "user",
                        content = input
                    }
                },
                temperature = 0,
                stream = true
            });
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var textResponse = "";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                using (var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead))
                {
                    using (var body = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(body))
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line.StartsWith("data: ") && line != "data: [DONE]")
                            {
                                Response responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(line.Substring(6));

                                textResponse += responseObj.Choices[0].Delta.Content;
                                if (handle != null) {
                                    handle(responseObj.Choices[0].Delta.Content);
                                }
                            }
                            //else
                            //{
                            //    if (line != "data: [DONE]")
                            //    {
                            //        Console.Write(line);
                            //    }
                            //}
                        }
                }

            }

            return textResponse;
        }
    }
}
