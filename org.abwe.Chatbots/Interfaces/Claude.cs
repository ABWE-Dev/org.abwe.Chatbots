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

namespace org.abwe.Chatbots.Interfaces.Claude
{
    public static class Models
    {
        public const string Sonnet = "claude-3-sonnet-20240229";
        public const string Haiku = "claude-3-haiku-20240307";
        public const string Opus = "claude-3-opus-20240229";
    }

    public class Claude : IResponseGenerator
    {
        private string _apiKey;

        public Claude(string apiKey) {
            _apiKey = apiKey;
        }

        public async Task<string> GetResponse(string input, string model = Models.Haiku, Action<string> handle = null)
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
                stream = true,
                max_tokens = 2000,
            });
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Headers.Add("x-api-key", $"{_apiKey}");
            request.Headers.Add("anthropic-version", "2023-06-01");
            //request.Headers.Add("content-type", "application/json");
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

                                if (responseObj.type == "content_block_delta") {
                                    textResponse += ((Newtonsoft.Json.Linq.JObject)responseObj.delta).GetValue("text");
                                    if (handle != null) {
                                        handle(((Newtonsoft.Json.Linq.JObject)responseObj.delta).GetValue("text").ToString());
                                    }
                                }
                            }
                        }
                }

            }

            return textResponse;
        }
    }
}
