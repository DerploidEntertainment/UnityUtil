using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine {

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

        protected readonly IDictionary<TKey, int> IndexLookup = new Dictionary<TKey, int>();
        protected readonly List<Element> List = new List<Element>();

        public int Count => List.Count;

        public TValue this[TKey key] => List[IndexLookup[key]].Value;
        public TValue this[int index] => List[index].Value;

        /// <summary>
        /// Sets the capacity to the actual number of elements in the underlying collection, to save on memory.
        /// </summary>
        public void TrimExcess() => List.TrimExcess();
        public void Clear() {
            IndexLookup.Clear();
            List.Clear();
        }
        public bool ContainsKey(TKey key) => IndexLookup.ContainsKey(key);
        public void CopyTo(TValue[] array, int arrayIndex) {
            for (int i = 0; i < List.Count; ++i)
                array[arrayIndex + i] = List[i].Value;
        }
        public bool Remove(TKey key) {
            bool contained = IndexLookup.TryGetValue(key, out int index);
            if (!contained)
                return false;

            Element last = List[List.Count - 1];
            IndexLookup[last.Key] = index;
            List[index] = last;
            IndexLookup.Remove(key);
            List.RemoveAt(List.Count - 1);

            return true;
        }
        public void Add(TKey key, TValue value) {
            IndexLookup.Add(key, List.Count);
            List.Add(new Element(key, value));
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters required to implement interface")]
        public bool TryGetValue(TKey key, out TValue value) => throw new System.NotImplementedException();

        public IEnumerator<TValue> GetEnumerator() => List.Select(i => i.Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => List.Select(i => i.Value).GetEnumerator();

    }

}
