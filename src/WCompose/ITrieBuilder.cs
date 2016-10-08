using System.Threading.Tasks;

namespace WCompose
{

    internal interface ITrieBuilder
    {
        Task<Trie<char, string>> Build(string path);
    }
}
