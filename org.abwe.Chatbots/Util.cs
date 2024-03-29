﻿// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Rock;
using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.abwe.Chatbots
{
    public class Util
    {
        public static string GetOpenAIKey()
        {
            var avService = new AttributeValueService(new RockContext());
            var OpenAIComponentGuid = "8D3F25B1-4891-31AA-4FA6-365F5C808563".AsGuid();
            var attributeValue = avService.Queryable().Where(av => av.Attribute.Key == "SecretKey" && av.Attribute.EntityType.Guid == OpenAIComponentGuid).FirstOrDefault();
            return attributeValue.Value;
        }
    }
}
