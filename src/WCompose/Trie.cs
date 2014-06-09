using System.Collections.Generic;

namespace WCompose
{
    /// <summary>
    /// A really stupid Trie implementation.
    /// </summary>
    public class Trie<TKey, TValue>
    {
        private readonly Dictionary<TKey, Trie<TKey, TValue>> _map = new Dictionary<TKey, Trie<TKey, TValue>>();
        private TValue _value;
        
        public void Insert(IEnumerable<TKey> key, TValue value)
        {
            var current = Locate(key, true);

            current._value = value;
        }

        public bool TryGet(IEnumerable<TKey> key, out TValue value)
        {
            var node = Locate(key, false);
            if (node != null)
            {
                value = node._value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public Trie<TKey, TValue> Step(TKey key)
        {
            Trie<TKey, TValue> result = null;
            _map.TryGetValue(key, out result);
            return result;
        }

        public TValue Value { get { return _value; } }

        private Trie<TKey, TValue> Locate(IEnumerable<TKey> key, bool create)
        {
            var current = this;
            foreach (var item in key)
            {
                Trie<TKey, TValue> next;
                if (!current._map.TryGetValue(item, out next))
                {
                    if (!create) return null;
                    next = current._map[item] = new Trie<TKey, TValue>();
                }
                current = next;
            }
            return current;
        }

        public IEnumerable<TKey> Keys
        {
            get { return _map.Keys; }
        }
    }
}