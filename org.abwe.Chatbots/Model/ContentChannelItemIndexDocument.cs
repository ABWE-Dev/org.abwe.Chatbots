// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace org.abwe.Chatbots
{
    public class ContentChannelItemIndexDocumentMetadata {
        public string url { get; set; }
        public string title { get; set; }
        public int ContentChannelId { get; set; }
        public Guid id { get; set; }
    }

    public class ContentChannelItemIndexDocument
    {
        public string Text { get; set; }
        public ContentChannelItemIndexDocumentMetadata Metadata { get; set; }
        public List<float> Vector { get; set; }
        public Guid ChunkId { get; set; }
        public Guid? ParentChunkId { get; set; }
    }
}
