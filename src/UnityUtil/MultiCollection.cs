using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UnityUtil;

/// <summary>
/// Represents a strongly typed list of objects with constant-time insertion, removal, and duplicate checks,
/// as well as low-overhead iteration, at the cost of additional memory.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the collection.</typeparam>
/// <typeparam name="TValue">The type of the values in the collection.</typeparam>
public class MultiCollection<TKey, TValue> : IEnumerable<TValue>
{

    protected class Element
    {
        public TKey Key;
        public TValue Value;
        public Element(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    private readonly IDictionary<TKey, int> _indexLookup = new Dictionary<TKey, int>();
    private readonly List<Element> _list = new();

    public int Count => _list.Count;

    public TValue this[TKey key] => _list[_indexLookup[key]].Value;
    public TValue this[int index] => _list[index].Value;

    /// <summary>
    /// Sets the capacity to the actual number of elements in the underlying collection, to save on memory.
    /// </summary>
    public void TrimExcess() => _list.TrimExcess();
    public void Clear()
    {
        _indexLookup.Clear();
        _list.Clear();
    }
    public bool ContainsKey(TKey key) => _indexLookup.ContainsKey(key);
    public void CopyTo(TValue[] array, int arrayIndex)
    {
        for (int i = 0; i < _list.Count; ++i)
            array[arrayIndex + i] = _list[i].Value;
    }
    public bool Remove(TKey key)
    {
        bool contained = _indexLookup.TryGetValue(key, out int index);
        if (!contained)
            return false;

        Element last = _list[^1];
        _indexLookup[last.Key] = index;
        _list[index] = last;
        _indexLookup.Remove(key);
        _list.RemoveAt(_list.Count - 1);

        return true;
    }
    public void Add(TKey key, TValue value)
    {
        _indexLookup.Add(key, _list.Count);
        _list.Add(new Element(key, value));
    }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters required to implement interface")]
    public bool TryGetValue(TKey key, out TValue value) => throw new System.InvalidOperationException();

    public IEnumerator<TValue> GetEnumerator() => _list.Select(i => i.Value).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.Select(i => i.Value).GetEnumerator();

}
