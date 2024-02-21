# Requirements

Elasticsearch 8.10 or greater
Rock v16 or greater

# Setup

1. npm install
2. Create a symlink to Rock in the "org.abwe.Chatbots" subfolder
3. Update Rock to use System.Text.Json and System.Text.Web.Encodings 6.0.0.0 or higher
4. Add org.abwe.Chatbots.csproj to your Rock solution
5. Configure Elasticsearch and OpenAI with correct settings in Rock (they do not need to be active)
6. Add a Support Chat block to a page in Rock, and configure the content channels you would like to index
7. Setup and run a Build Chatbot Index job (this will incur charges with Open AI to generate the embeddings)
    - This will only index content channels that are checked in the settings of a Support Chat block
8. Chat!