# Requirements

- Elasticsearch 8.10 or greater
- Rock v16 or greater

# Dev Environment Setup

1. npm install in the /org.abwe.Chatbots/Javascript directory
2. npm run build in the /org.abwe.Chatbots/Javascript directory
3. Create a symlink or copy /org.abwe.Chatbots/RockWeb/Plugins/org_abwe in your /RockWeb/Plugins directory (deploying the block javascripts)
4. Create a symlink to Rock in the "org.abwe.Chatbots" subfolder
5. Update Rock to use System.Text.Json and System.Text.Web.Encodings 6.0.0.0 or higher
6. Add org.abwe.Chatbots.csproj to your Rock solution
7. Configure Elasticsearch and OpenAI with correct settings in Rock (they do not need to be active)
8. Add a Support Chat block to a page in Rock, and configure the content channels you would like to index
9. Setup and run a Build Chatbot Index job (this will incur charges with Open AI to generate the embeddings)
    - This will only index content channels that are checked in the settings of a Support Chat block
10. Chat!

![Chat](https://github.com/ABWE-Dev/org.abwe.Chatbots/assets/5644394/46f793a5-66cf-42bc-bebc-c2c7fa43ea7d)
