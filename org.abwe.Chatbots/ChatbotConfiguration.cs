using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.abwe.Chatbots
{
    public class ChatbotConfiguration
    {
        public string IndexName { get; set; }
        public int ChunkSize { get; set; }
        public int ChunkOverlap { get; set; }
        public int SecondPassChunkSize { get; set; }
    }
}
