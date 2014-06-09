using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace WCompose
{
    public class TrieBuilder
    {
        public TrieBuilder()

        {
            
        }

        private static readonly Regex _regex =
            new Regex(@"^<Multi_key>(?:\s*<(?<inputs>[^>]+)>)+\s*:\s*""(?<output>[^""]+)""\s*(?<name>[^\s]+)", RegexOptions.Compiled);

        public async Task<Trie<char, string>> Build(TextReader reader)
        {
            var trie = new Trie<char, string>();

            var nameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"less", "<"},
                {"greater", ">"},
                {"equal", "="},
                {"semicolon", ";"},
                {"comma", ","},
                {"period", "."},
                {"space", " "},
                {"minus", "-"},
                {"quotedbl", "\""},
                {"underscore", "_"},
                {"colon", ":"},
                {"parenleft", "("},
                {"parenright", ")"},
                {"plus", "+"},
                {"slash", "/"},
                {"KP_divide", "/"},
                {"KP_minus", "-"},
                {"KP_add", "+"},
                {"KP_equal", "="},
                {"KP_1", "1"},
                {"KP_2", "2"},
                {"KP_3", "3"},
                {"KP_4", "4"},
                {"KP_5", "5"},
                {"KP_6", "6"},
                {"KP_7", "7"},
                {"KP_8", "8"},
                {"KP_9", "9"},
                {"KP_0", "0"},
                {"KP_Space", " "},
                {"exclam", "!"},
                {"question", "?"},
                {"asterisk", "*"},
                {"percent", "%"},
            };
            var xcomposes = new List<Tuple<IEnumerable<string>, string>>();

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.Trim().Length == 0) continue;

                var match = _regex.Match(line);
                if (!match.Success)
                {
                    //Trace.TraceWarning("Skipping line {0}", line);
                    continue;
                }
                
                var inputs = match.Groups["inputs"].Captures.Cast<Capture>().Select(x => x.Value);
                var output = match.Groups["output"].Value;
                xcomposes.Add(Tuple.Create(inputs, output));

                nameMap[match.Groups["name"].Value] = output;
            }

            //var unknownChars = new HashSet<string>();
            var buffer = new List<char>();
            foreach (var compose in xcomposes)
            {
                buffer.Clear();
                var success = true;
                foreach (var input in compose.Item1)
                {
                    if (input.Length == 1)
                    {
                        buffer.Add(input[0]);
                    }
                    else if (input[0] == 'U' && input.Substring(1).All(x => char.IsDigit(x) || x >= 'A' && x <= 'F' || x >= 'a' && x <= 'f'))
                    {
                        buffer.AddRange(char.ConvertFromUtf32(Convert.ToInt32(input.Substring(1), 16)));
                    }
                    else
                    {
                        string mapped;
                        if (!nameMap.TryGetValue(input, out mapped))
                        {
                            //unknownChars.Add(input);
                            success = false;
                            break;
                        }
                        else
                        {
                            buffer.AddRange(mapped);
                        }
                    }
                }
               
                if (success)
                {
                    //Trace.TraceInformation("Mapped '{0}' -> {1}", sb.ToString(), compose.Item2);
                    trie.Insert(buffer, compose.Item2);
                }
            }

            //foreach (var unknownChar in unknownChars.OrderBy(x => x))
            //{
            //    Trace.TraceWarning("Unknown char '{0}'", unknownChar);
            //}

            return trie;
        }
    }
}
