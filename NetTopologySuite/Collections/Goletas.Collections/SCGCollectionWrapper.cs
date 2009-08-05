
using GC = Goletas.Collections;
using SCG = System.Collections.Generic;
using SC = System.Collections;

namespace GisSharpBlog.NetTopologySuite.Collections.Goletas.Collections
{
    public sealed class SCGCollectionWrapper<V> : SCG.ICollection<V>
    {
        private readonly GC.ICollection<V> _innerCollection;

        public SCGCollectionWrapper(GC.ICollection<V> innerCollection)
        {
            _innerCollection = innerCollection;
        }

        #region ICollection<V> Members

        public void Add(V item)
        {
            _innerCollection.Add(item);
        }

        public void Clear()
        {
            _innerCollection.Clear();
        }

        public bool Contains(V item)
        {
            return _innerCollection.Contains(item);
        }

        public void CopyTo(V[] array, int arrayIndex)
        {
            _innerCollection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _innerCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return _innerCollection.IsReadOnly; }
        }

        public bool Remove(V item)
        {
            return _innerCollection.Remove(item);
        }

        public SCG.IEnumerator<V> GetEnumerator()
        {
            return _innerCollection.GetEnumerator();
        }

        SC.IEnumerator SC.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}