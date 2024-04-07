using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace org.abwe.Chatbots.Migrations
{
    [MigrationNumber(1,"1.16.0")]
    class GlobalAttribute : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute(Rock.SystemGuid.FieldType.TEXT, null, null, "Chatbots Index Name", "The index name for the chatbots in Elasticsearch.", 0, "", SystemGuid.Attributes.ChatbotsIndex, "AbweChatbotIndex");
            RockMigrationHelper.AddGlobalAttribute(Rock.SystemGuid.FieldType.TEXT, null, null, "Claude API Key", "API key to use for Claude", 0, "", SystemGuid.Attributes.ClaudeAPIKey, "ClaudeAPIKey");
        }
        public override void Down()
        {

        }
    }
}
