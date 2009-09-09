using System.Collections;
using System.Collections.Generic;

namespace SimpleCoordinateTests
{
    public class EnumerableIsolater<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _backingObject;

        public EnumerableIsolater(IEnumerable<T> backingObject)
        {
            _backingObject = backingObject;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _backingObject.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_backingObject as IEnumerable).GetEnumerator();
        }

        #endregion
    }
}