using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.abwe.Chatbots.TextSplitters;
using System;
using System.Collections.Generic;

namespace org.abwe.Chatbots.Tests
{
    [TestClass]
    public class RecursiveTextSplitterTests
    {
        [TestMethod]
        public void SplitStrings_SplitsOnHtmlTags()
        {
            var input = @"<p>The&nbsp;<a href=""https://rock.abwe.org/GetFile.ashx?Id=435640"">Care Today 2023 Plan Summary</a>&nbsp;provides an overview of Medicare Parts A &amp; B based on 2022 coverage amounts. For both Part A &amp; B medical services, it summaries what Medicare pays, what the plan pays, and what you pay.</p>";
            var expected = new List<string>() { "<p>The&nbsp;<a href=\"https://rock.abwe.org/GetFile.ashx?Id=435640\">Care Today 2023 Plan Summary</a>&nbsp;provides an overview of Medicare Parts A &amp; B based on 2022 coverage amounts. For both Part A &amp; B medical services, it summaries what Medicare pays, what the plan pays, and what you pay.</p>" };
            var recursiveTextSpliter = new TextSplitters.RecursiveTextSplitter(true, 1600, 250);

            var actual = recursiveTextSpliter.SplitText(input, RecursiveTextSplitter.HtmlSeparators);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
