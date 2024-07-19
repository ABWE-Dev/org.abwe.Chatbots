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

namespace org.abwe.Chatbots.Interfaces.Groq
{
    public static class Models
    {
        public const string Llama3 = "llama3-70b-8192";
    }

    public class Groq : IResponseGenerator
    {
        private string _apiKey;

        public Groq(string apiKey) {
            _apiKey = apiKey;
        }

        public async Task<string> GetResponse(string input, string model = Models.Llama3, Action<string> handle = null)
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
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
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
                                OpenAI.Response responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenAI.Response>(line.Substring(6));

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
