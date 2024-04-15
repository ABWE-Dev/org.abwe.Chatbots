using Rock;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace org.abwe.Chatbots.Migrations
{
    [MigrationNumber(2,"1.16.0")]
    class Configuration : Migration
    {
        public override void Up()
        {
            // This is moved to the configuration screen
            RockMigrationHelper.DeleteAttribute(SystemGuid.Attributes.ChatbotsIndex);

            // Add Page 
            //  Internal Name: Chatbots
            //  Site: Rock RMS
            RockMigrationHelper.AddPage(true, "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Chatbots", "", "7817C0B9-1CE9-47D5-931B-1C3D7D24D432", "fa fa-comments");


            // Add Block 
            //  Block Name: Chatbot Configuration
            //  Page Name: Chatbots
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock(true, "7817C0B9-1CE9-47D5-931B-1C3D7D24D432".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "17A33C9C-9161-456F-BAB3-0336765527A8".AsGuid(), "Chatbot Configuration", "Main", @"", @"", 0, "2A56FDD5-6A7D-4843-B9DD-2D736335E850");

            Sql("UPDATE [EntityType] SET IsMessageBusEventPublishEnabled = 1 WHERE [Guid] = 'BF12AE64-21FB-433B-A8A4-E40E8C426DDA'");
        }
        public override void Down()
        {

        }
    }
}
