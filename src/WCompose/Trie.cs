using System;
using System.Collections.Generic;
using System.Linq;

namespace WCompose
{
    /// <summary>
    /// A really stupid Trie implementation.
    /// </summary>
    public class Trie<TKey, TValue>
    {
        private Dictionary<TKey, Trie<TKey, TValue>> _map;
        
        public void Insert(IEnumerable<TKey> key, TValue value)
        {
            var current = Locate(key, true);
            if (current.Value != null) throw new InvalidOperationException("Duplicate item");
            current.Value = value;
        }

        public bool TryGet(IEnumerable<TKey> key, out TValue value)
        {
            var node = Locate(key, false);
            if (node != null)
            {
                value = node.Value;
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
            if (_map == null) return null;

            Trie<TKey, TValue> result = null;
            _map.TryGetValue(key, out result);
            return result;
        }

        public TValue Value { get; private set; }

        private Trie<TKey, TValue> Locate(IEnumerable<TKey> key, bool create)
        {
            var current = this;
            foreach (var item in key)
            {
                if (current._map == null) current._map = new Dictionary<TKey, Trie<TKey, TValue>>();

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
            get
            {
                if (_map == null) return Enumerable.Empty<TKey>();

                return _map.Keys;
            }
        }
        
        public bool HasNext => _map?.Count > 0;
    }
}