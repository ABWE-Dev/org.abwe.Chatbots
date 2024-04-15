// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Newtonsoft.Json;
using org.abwe.Chatbots.Interfaces;
using org.abwe.Chatbots.Stores;
using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Lava.Blocks;
using Rock.Model;
using Rock.Net;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.UniversalSearch.IndexModels;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using static Rock.WebFarm.RockWebFarm;

namespace org.abwe.Chatbots.Blocks
{
    /// <summary>
    /// Displays configuration options for the chatbot plugin.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName("Chatbot Configuration")]
    [Category("org_abwe > Chatbots")]
    [Description("Display configuration options for the chatbot plugin")]
    [SupportedSiteTypes(Rock.Model.SiteType.Web)]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid("9c2b3d5c-6d66-4fc5-9b61-5f64375343f6")]
    [Rock.SystemGuid.BlockTypeGuid("17a33c9c-9161-456f-bab3-0336765527a8")]
    public class ChatbotConfigurationBlock : RockBlockType
    {
        public override string ObsidianFileUrl => $"/Plugins/org_abwe/Chatbots/Blocks/chatbotConfiguration.obs";

        #region Keys

        #endregion Keys

        #region Methods

        #endregion

        #region Block Actions

        ///// <summary>
        ///// Gets the current configuration
        ///// </summary>
        ///// <param name="message">The user message</param>
        ///// <param name="sessionId">A session id (used to save/retrieve conversation history)</param>
        ///// <returns>The configuration object</returns>
        [BlockAction]
        public BlockActionResult GetConfiguration()
        {
            try {
                return ActionOk(Util.GetConfiguration());
            } catch (Exception e) {
                ExceptionLogService.LogException(e);
                return ActionBadRequest();
            }
        }

        ///// <summary>
        ///// Saves the configuration
        ///// </summary>
        ///// <param name="configuration">The configuration to save</param>
        ///// <returns>Nothing</returns>
        [BlockAction]
        public BlockActionResult SaveConfiguration(ChatbotConfiguration configuration)
        {
            try
            {
                SystemSettings.SetValue("org_abwe_Chatbot_Configuration", JsonConvert.SerializeObject(configuration));
                return ActionOk(configuration);
            }
            catch (Exception e)
            {
                ExceptionLogService.LogException(e);
                return ActionBadRequest();
            }
        }

        #endregion
    }
}
