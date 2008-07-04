using System;
using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic
{
    /// <summary>
    /// A Simple Wrapper for wrapping an regular Enumerable as a generic Enumberable&lt;T&gt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidCastException">
    /// If the wrapped has any item that is not of Type T, InvalidCastException could be thrown at any time
    /// </exception>
    public  class EnumerableWrapper <T> : IEnumerable<T>
    {
        private IEnumerable innerEnumerable;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerEnumerable"></param>
        public EnumerableWrapper(IEnumerable innerEnumerable)
        {
            this.innerEnumerable = innerEnumerable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {            
            if (!obj.GetType().Equals(GetType())) 
                return false;
            if (obj == this) 
                return true;
            return innerEnumerable.Equals(
                ((EnumerableWrapper<T>) obj).innerEnumerable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return innerEnumerable.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new EnumeratorWrapper<T>(innerEnumerable.GetEnumerator());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerEnumerable.GetEnumerator();    
        }              
    }
}
