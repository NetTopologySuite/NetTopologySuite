using System;
using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic 
{
    public class CollectionWrapper<T> : EnumerableWrapper<T>, ICollection<T>
    {
        private ICollection innerCollection;
        public CollectionWrapper(ICollection toWrap) :base(toWrap)
        {
            this.innerCollection = toWrap;
        }
        

        #region ICollection<T> Members

        public void Add(T item)
        {
            ThrowReadOnlyException();
        }

        public void Clear()
        {
            ThrowReadOnlyException();
        }

        public bool Contains(T item)
        {
            foreach (object o in innerCollection)
                if ( (object)item == o) return true;
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerCollection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; //always return true since the old ICollection does not support mutation 
            }
        }

        public bool Remove(T item)
        {
            return ThrowReadOnlyException();
        }

        #endregion
        
        private bool ThrowReadOnlyException()
        {
            throw new NotSupportedException("The ICollection is read-only.");
        
        }
    }
}
