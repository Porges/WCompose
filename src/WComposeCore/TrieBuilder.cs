using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WCompose
{
    public class XComposeTrieWriter
    {
        private StringBuilder keyBuffer = new StringBuilder();
        private StringBuilder lineBuffer = new StringBuilder();

        public Task Write(StreamWriter writer, Trie<char, string> trie)
        {
            return RecurseWrite(writer, trie);
        }

        private async Task RecurseWrite(StreamWriter writer, Trie<char, string> trie)
        {
            if (trie.Value != null)
            {
                lineBuffer
                    .Append("<Multi_key> ")
                    .Append(keyBuffer)
                    .Append(":\"")
                    .Append(trie.Value)
                    .AppendLine("\"");

                await writer.WriteAsync(lineBuffer.ToString());
                lineBuffer.Clear();
            }
            else
            {
                var len = keyBuffer.Length;
                foreach (var c in trie.Keys)
                {
                    keyBuffer.Append('<').Append(c).Append("> ");
                    await RecurseWrite(writer, trie.Step(c));
                    keyBuffer.Length = len;
                }
            }
        }
    }
}
