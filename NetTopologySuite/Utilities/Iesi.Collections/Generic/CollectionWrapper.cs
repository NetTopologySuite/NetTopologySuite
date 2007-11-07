using System;
using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic
{
    public class CollectionWrapper<T> : EnumerableWrapper<T>, ICollection<T>
    {
        private ICollection innerCollection;

        public CollectionWrapper(ICollection toWrap) : base(toWrap)
        {
            innerCollection = toWrap;
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

        public Boolean Contains(T item)
        {
            foreach (object o in innerCollection)
            {
                if ((object) item == o)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            innerCollection.CopyTo(array, arrayIndex);
        }

        public Int32 Count
        {
            get { return innerCollection.Count; }
        }

        public Boolean IsReadOnly
        {
            get { return true; //always return true since the old ICollection does not support mutation 
            }
        }

        public Boolean Remove(T item)
        {
            return ThrowReadOnlyException();
        }

        #endregion

        private Boolean ThrowReadOnlyException()
        {
            throw new NotSupportedException("The ICollection is read-only.");
        }
    }
}