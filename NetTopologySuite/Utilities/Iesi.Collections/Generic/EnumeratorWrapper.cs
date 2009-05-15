using System;
using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic
{
        /// <summary>
        /// Simple Wrapper for wrapping an regular Enumerator as a generic Enumberator&lt;T&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidCastException">
        /// If the wrapped has any item that is not of Type T, InvalidCastException could be thrown at any time
        /// </exception>
        public struct EnumeratorWrapper<T> : IEnumerator<T>
        {
            private IEnumerator innerEnumerator;

            public EnumeratorWrapper(IEnumerator toWrap)
            {
                this.innerEnumerator = toWrap;
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return (T)innerEnumerator.Current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                this.innerEnumerator = null;
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return innerEnumerator.Current; }
            }

            public bool MoveNext()
            {
                return innerEnumerator.MoveNext();
            }

            public void Reset()
            {
                innerEnumerator.Reset();
            }

            #endregion
        }
   
}
