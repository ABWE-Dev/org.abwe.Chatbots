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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatbotInterfaces
{
    public class LangChain
    {
        public async Task<bool> GetResponse(string input)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8000/rag-elasticsearch/stream");
            request.Content = new StringContent($@"
                {{
                    ""input"": {{
                        ""chat_history"": [],
                        ""question"" : ""{input}""
                    }}
                }}
            ", Encoding.UTF8, "application/json");

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
                                try {
                                    string responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(line.Substring(6));
                                    Console.Write(responseObj);
                                } catch {
                                    Console.WriteLine("Error processing JSON:", line);
                                }
                            }
                        }
                }

            }
            return true;
        }
    }
}
