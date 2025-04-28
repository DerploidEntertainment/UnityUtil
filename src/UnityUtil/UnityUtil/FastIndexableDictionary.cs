using System;
using System.Collections.Generic;

namespace UnityUtil;

/// <summary>
/// Represents a strongly typed dictionary of objects with constant-time insertion, removal, duplicate checks, <em>and</em> index lookup,
/// at the cost of additional memory (both a dictionary and a list are used internally).
/// </summary>
/// <remarks>
/// The indices of elements are not stable, and may change as elements are added/removed;
/// Note that this collection does <em>not</em> implement <see cref="IDictionary{TKey, TValue}"/> as it does not
/// allow enumeration of <see cref="KeyValuePair{TKey, TValue}"/>s.
/// Enumeration should be done via index lookups in a <see langword="for"/> loop.
/// </remarks>
/// <typeparam name="TKey">The type of the keys in the collection.</typeparam>
/// <typeparam name="TValue">The type of the values in the collection.</typeparam>
public class FastIndexableDictionary<TKey, TValue>
{
    protected struct Element(TKey key, TValue value)
    {
        public TKey Key { get; set; } = key;
        public TValue Value { get; set; } = value;
    }

    private readonly Dictionary<TKey, int> _indexLookup = [];
    private readonly List<Element> _list = [];
    private readonly object _collectionLock = new();

    /// <summary>
    /// Gets the number of elements contained in the <see cref="FastIndexableDictionary{TKey, TValue}"/>.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>
    /// The value associated with the specified key.
    /// If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, 
    /// and a set operation creates a new element with the specified key.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
    public TValue this[TKey key] => _list[_indexLookup[key]].Value;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="Count"/>.</exception>
    public TValue this[int index] => _list[index].Value;

    /// <summary>
    /// Sets the capacity to the actual number of elements in the underlying collection, to save on memory.
    /// </summary>
    public void TrimExcess()
    {
        lock (_collectionLock) {
            _indexLookup.TrimExcess();
            _list.TrimExcess();
        }
    }

    /// <summary>
    /// Removes all elements from the <see cref="FastIndexableDictionary{TKey, TValue}"/>
    /// </summary>
    public void Clear()
    {
        lock (_collectionLock) {
            _indexLookup.Clear();
            _list.Clear();
        }
    }

    /// <summary>
    /// Determines whether the <see cref="FastIndexableDictionary{TKey, TValue}"/> contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="FastIndexableDictionary{TKey, TValue}"/></param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="FastIndexableDictionary{TKey, TValue}"/> contains an element with the specified key; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/></exception>
    public bool ContainsKey(TKey key) => _indexLookup.ContainsKey(key);

    /// <summary>
    /// Removes the value with the specified key from the <see cref="FastIndexableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.
    /// This method returns <see langword="false"/> if <paramref name="key"/> is not found in the <see cref="FastIndexableDictionary{TKey, TValue}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public bool Remove(TKey key) => Remove(key, out _);

    /// <summary>
    /// Removes the value with the specified key from the <see cref="FastIndexableDictionary{TKey, TValue}"/>,
    /// and copies the element to the <paramref name="value"/> parameter.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">The removed element.</param>
    /// <returns>
    /// <see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public bool Remove(TKey key, out TValue value)
    {
        lock (_collectionLock) {
            bool contained = _indexLookup.Remove(key, out int index);
            if (!contained) {
#pragma warning disable CS8601 // Possible null reference assignment. Not sure what else to do here; callers shouldn't use the out param if we return false anyway.
                value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
                return false;
            }

            value = _list[index].Value;
            if (index < _list.Count - 1) {
                Element last = _list[^1];
                _indexLookup[last.Key] = index;
                _list[index] = last;
            }
            _list.RemoveAt(_list.Count - 1);
        }

        return true;
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. The value can be <see langword="null"/> for reference types.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    public void Add(TKey key, TValue value)
    {
        lock (_collectionLock) {
            _indexLookup.Add(key, _list.Count);
            _list.Add(new Element(key, value));
        }
    }

    /// <summary>
    /// Attempts to add the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. It can be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the key/value pair was added to the dictionary successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public bool TryAdd(TKey key, TValue value)
    {
        lock (_collectionLock) {
            bool added = _indexLookup.TryAdd(key, _list.Count);
            if (added)
                _list.Add(new Element(key, value));
            return added;
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key, if the key is found; 
    /// otherwise, the default value for the type of the value parameter.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the dictionary contains an element 
    /// with the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_collectionLock) {
            bool exists = _indexLookup.TryGetValue(key, out int index);
#pragma warning disable CS8601 // Possible null reference assignment. Not sure what else to do here; callers shouldn't use the out param if we return false anyway.
            value = exists ? _list[index].Value : default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return exists;
        }
    }
}
