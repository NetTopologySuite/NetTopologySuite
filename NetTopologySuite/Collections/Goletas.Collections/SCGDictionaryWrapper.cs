using System;
using GC = Goletas.Collections;
using SC = System.Collections;
using SCG = System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Collections.Goletas.Collections
{
    public sealed class SCGDictionaryWrapper<K, V> : SCG.IDictionary<K, V>
    {
        private readonly GC.IDictionary<K, V> _innerDictionary;
        private readonly SCGCollectionWrapper<K> _keysWrapper;
        private readonly SCGCollectionWrapper<V> _valuesWrapper;
        public SCGDictionaryWrapper(GC.IDictionary<K, V> innerDictionary)
        {
            _innerDictionary = innerDictionary;
            _keysWrapper = new SCGCollectionWrapper<K>(_innerDictionary.Keys);
            _valuesWrapper = new SCGCollectionWrapper<V>(_innerDictionary.Values);
        }

        #region IDictionary<K,V> Members

        public void Add(K key, V value)
        {
            _innerDictionary.Add(key, value);
        }

        public bool ContainsKey(K key)
        {
            return _innerDictionary.Keys.Contains(key);
        }

        public SCG.ICollection<K> Keys
        {
            get { return _keysWrapper; }
        }

        public bool Remove(K key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        public System.Collections.Generic.ICollection<V> Values
        {
            get { return _valuesWrapper; }
        }

        public V this[K key]
        {
            get
            {
                return _innerDictionary[key];
            }
            set
            {
                _innerDictionary[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public void Add(SCG.KeyValuePair<K, V> item)
        {
            _innerDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(SCG.KeyValuePair<K, V> item)
        {
            return Equals(_innerDictionary[item.Key], item.Value);
        }

        public void CopyTo(SCG.KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _innerDictionary.IsReadOnly; }
        }

        public bool Remove(SCG.KeyValuePair<K, V> item)
        {
            if (Contains(item))
                return Remove(item.Key);
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public SCG.IEnumerator<SCG.KeyValuePair<K, V>> GetEnumerator()
        {
            foreach (GC.KeyValuePair<K, V> pair in _innerDictionary)
            {
                yield return new SCG.KeyValuePair<K, V>(pair.Key, pair.Value);
            }
        }

        #endregion

        #region IEnumerable Members

        SC.IEnumerator SC.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
