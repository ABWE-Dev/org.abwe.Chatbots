// Copyright (c) 2024 Association of Baptists for World Evangelism

// Licensed under the MIT license, see LICENSE file

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace org.abwe.Chatbots.TextSplitters
{
    public class RecursiveTextSplitter
    {
        private bool _keepSeparator;
        private int _chunkSize;
        private int _chunkOverlap = 250;

        public static List<string> HtmlSeparators = new List<string>() {
            "<body",
            "<div",
            "<p",
            "<br",
            "<li",
            "<h1",
            "<h2",
            "<h3",
            "<h4",
            "<h5",
            "<h6",
            "<span",
            "<table",
            "<tr",
            "<td",
            "<th",
            "<ul",
            "<ol",
            "<header",
            "<footer",
            "<nav",

            "<head",
            "<style",
            "<script",
            "<meta",
            "<title",
            ""
        };

        public RecursiveTextSplitter(bool keepSeparator, int chunkSize, int chunkOverlap)
        {
            _keepSeparator = keepSeparator;
            _chunkSize = chunkSize;
            _chunkOverlap = chunkOverlap;
        }

        public List<string> SplitText(string text, List<string> separators, bool isSeparatorRegex = true)
        {
            List<string> finalChunks = new List<string>();
            string separator = separators[separators.Count - 1];
            List<string> newSeparators = new List<string>();

            for (int i = 0; i < separators.Count; i++)
            {
                string currentSeparator = separators[i];
                string regexSeparator = isSeparatorRegex ? currentSeparator : Regex.Escape(currentSeparator);

                if (currentSeparator == "")
                {
                    separator = currentSeparator;
                    break;
                }

                if (Regex.IsMatch(text, regexSeparator))
                {
                    separator = currentSeparator;
                    newSeparators = separators.GetRange(i + 1, separators.Count - (i + 1));
                    break;
                }
            }

            string finalSeparator = isSeparatorRegex ? separator : Regex.Escape(separator);
            List<string> splits = SplitTextWithRegex(text, finalSeparator, _keepSeparator);

            List<string> goodSplits = new List<string>();
            string mergeSeparator = _keepSeparator ? "" : separator;

            foreach (var s in splits)
            {
                if (s.Length < _chunkSize)
                {
                    goodSplits.Add(s);
                }
                else
                {
                    if (goodSplits.Count > 0)
                    {
                        List<string> mergedText = MergeSplits(goodSplits, mergeSeparator);
                        finalChunks.AddRange(mergedText);
                        goodSplits.Clear();
                    }

                    if (newSeparators.Count == 0)
                    {
                        finalChunks.Add(s);
                    }
                    else
                    {
                        List<string> otherInfo = SplitText(s, newSeparators);
                        finalChunks.AddRange(otherInfo);
                    }
                }
            }

            if (goodSplits.Count > 0)
            {
                List<string> mergedText = MergeSplits(goodSplits, mergeSeparator);
                finalChunks.AddRange(mergedText);
            }

            return finalChunks;
        }

        public List<string> SplitTextWithRegex(string text, string separator, bool keepSeparator)
        {
            List<string> splits = new List<string>();
            if (!string.IsNullOrEmpty(separator))
            {
                if (keepSeparator)
                {
                    // The parentheses in the pattern keep the delimiters in the result.
                    string pattern = $"({Regex.Escape(separator)})";
                    string[] _splits = Regex.Split(text, pattern);
                    for (int i = 1; i < _splits.Length; i += 2)
                    {
                        splits.Add(_splits[i] + _splits[i + 1]);
                    }
                    if (_splits.Length % 2 == 0)
                    {
                        splits.Add(_splits[_splits.Length - 1]);
                    }
                    splits.Insert(0, _splits[0]);
                }
                else
                {
                    splits.AddRange(Regex.Split(text, Regex.Escape(separator)));
                }
            }
            else
            {
                splits.Add(text);
            }
            return splits.FindAll(s => !string.IsNullOrEmpty(s));
        }

        private string JoinDocs(List<string> docs, string separator)
        {
            var text = string.Join(separator, docs);
            text = text.Trim();
            return text == "" ? null : text;
        }

        public List<string> MergeSplits(IEnumerable<string> splits, string separator)
        {
            int separatorLen = separator.Length;

            List<string> docs = new List<string>();
            List<string> currentDoc = new List<string>();
            int total = 0;

            foreach (var d in splits)
            {
                int len = d.Length;
                if (total + len + (currentDoc.Count > 0 ? separatorLen : 0) > _chunkSize)
                {
                    if (total > _chunkSize)
                    {
                        Debug.WriteLine($"Created a chunk of size {total}, which is longer than the specified {_chunkSize}");
                    }

                    var doc = JoinDocs(currentDoc, separator);
                    if (doc != null)
                    {
                        docs.Add(doc);
                    }
                    while (total > _chunkOverlap || (total + len + (currentDoc.Count > 0 ? separatorLen : 0) > _chunkSize && total > 0))
                    {
                        total -= currentDoc[0].Length + (currentDoc.Count > 1 ? separatorLen : 0);
                        currentDoc.RemoveAt(0);
                    }
                }
                currentDoc.Add(d);
                total += len + (currentDoc.Count > 1 ? separatorLen : 0);
            }
            var finalDoc = JoinDocs(currentDoc, separator);
            if (finalDoc != null)
            {
                docs.Add(finalDoc);
            }
            return docs;
        }
    }
}
