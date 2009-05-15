using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic
{
    public class ListWrapper<T> : EnumerableWrapper<T>,  IList<T>
    {

        private IList innerList;
       
        public ListWrapper(IList toWrapp):base(toWrapp)
        {
            this.innerList = toWrapp;
        }
       
        #region IList<T> Members
        
        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            innerList.Remove(index);
        }

        public T this[int index]
        {
            get
            {
                return (T)innerList[index];
            }
            set
            {
                innerList[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            innerList.Add(item);
        }

        public void Clear()
        {
            innerList.Clear();
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return innerList.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            if (!innerList.Contains(item))
                return false;
            innerList.Remove(item);
            return true;
        }

        #endregion

       
       
    }
}
