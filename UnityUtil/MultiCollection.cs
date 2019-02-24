using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine {

    /// <summary>
    /// Represents a strongly typed list of objects with constant-time insertion, removal, and duplicate checks,
    /// as well as low-overhead iteration, at the cost of additional memory.
    /// </summary>
    /// <typeparam name="TValue">The type of the items in the collection.</typeparam>
    public class MultiCollection<T> : MultiCollection<T, T>, ICollection<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable {

        public bool IsReadOnly => false;

        public bool Contains(T item) => _dict.ContainsKey(item);
        public void Add(T item) => Add(item, item);

    }

    /// <summary>
    /// Represents a strongly typed list of objects with constant-time insertion, removal, and duplicate checks,
    /// as well as low-overhead iteration, at the cost of additional memory.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of the values in the collection.</typeparam>
    public class MultiCollection<TKey, TValue> : IEnumerable<TValue> {

        protected class Element {
            public TKey Key;
            public TValue Value;
            public Element(TKey key, TValue value) {
                Key = key;
                Value = value;
            }
        }

        protected readonly IDictionary<TKey, int> _dict = new Dictionary<TKey, int>();
        protected readonly List<Element> _list = new List<Element>();

        public int Count => _list.Count;

        public TValue this[TKey key] => _list[_dict[key]].Value;
        public TValue this[int index] => _list[index].Value;

        public void TrimExcess() => _list.TrimExcess();
        public void Clear() {
            _dict.Clear();
            _list.Clear();
        }
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public void CopyTo(TValue[] array, int arrayIndex) {
            for (int i = 0; i < _list.Count; ++i)
                array[arrayIndex + i] = _list[i].Value;
        }
        public bool Remove(TKey key) {
            bool contained = _dict.TryGetValue(key, out int index);
            if (!contained)
                return false;

            Element last = _list[_list.Count - 1];
            _dict[last.Key] = index;
            _list[index] = last;
            _dict.Remove(key);
            _list.RemoveAt(_list.Count - 1);

            return true;
        }
        public void Add(TKey key, TValue value) {
            _dict.Add(key, _list.Count);
            _list.Add(new Element(key, value));
        }
        public bool TryGetValue(TKey key, out TValue value) => throw new System.NotImplementedException();

        public IEnumerator<TValue> GetEnumerator() => _list.Select(i => i.Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.Select(i => i.Value).GetEnumerator();

    }

}
