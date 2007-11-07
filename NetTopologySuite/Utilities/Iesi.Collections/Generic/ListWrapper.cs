using System;
using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic
{
    public class ListWrapper<T> : EnumerableWrapper<T>, IList<T>
    {
        private IList innerList;

        public ListWrapper(IList toWrapp) : base(toWrapp)
        {
            innerList = toWrapp;
        }

        #region IList<T> Members

        public Int32 IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(Int32 index, T item)
        {
            innerList.Insert(index, item);
        }

        public void RemoveAt(Int32 index)
        {
            innerList.Remove(index);
        }

        public T this[Int32 index]
        {
            get { return (T) innerList[index]; }
            set { innerList[index] = value; }
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

        public Boolean Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public Int32 Count
        {
            get { return innerList.Count; }
        }

        public Boolean IsReadOnly
        {
            get { return innerList.IsReadOnly; }
        }

        public Boolean Remove(T item)
        {
            if (!innerList.Contains(item))
            {
                return false;
            }
            innerList.Remove(item);
            return true;
        }

        #endregion
    }
}