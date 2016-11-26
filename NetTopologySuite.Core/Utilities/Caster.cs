using System;
using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// Static utility class for casting objects
    /// </summary>
    public static class Caster
    {
        /// <summary>
        /// Cast function from arbitrary-type to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The output type</typeparam>
        /// <param name="inputs">The sequence of items to cast.</param>
        /// <returns><paramref name="inputs"/> as an enumerable of <typeparamref name="T"/></returns>
        /// <exception cref="InvalidCastException">Thrown if cast cannot be performed</exception>
        public static IEnumerable<T> Cast<T>(IEnumerable inputs)
        {
            foreach (var input in inputs)
                yield return (T) input;
        }

        /// <summary>
        /// Cast function from sub-type to super-type
        /// </summary>
        /// <typeparam name="TSuper">The output (super/base) type</typeparam>
        /// <typeparam name="TSub">The input (sub/derived) type</typeparam>
        /// <param name="inputs">The sequence of items to cast.</param>
        /// <returns><paramref name="inputs"/> as an enumerable of <typeparamref name="TSuper"/></returns>
        public static IEnumerable<TSuper> Upcast<TSub, TSuper>(IEnumerable<TSub> inputs)
            where TSub : TSuper
        {
            foreach (var input in inputs)
                yield return input;
        }

        /// <summary>
        /// Cast function from super-type to sub-type
        /// </summary>
        /// <typeparam name="TSub">The input (sub/derived) type</typeparam>
        /// <typeparam name="TSuper">The output (super/base) type</typeparam>
        /// <param name="inputs">The sequence of items to cast.</param>
        /// <returns><paramref name="inputs"/> as an enumerable of <typeparamref name="TSub"/></returns>
        public static IEnumerable<TSub> Downcast<TSuper, TSub>(IEnumerable<TSuper> inputs)
            where TSub : TSuper
        {
            foreach (var input in inputs)
                yield return (TSub)input;
        }
    }
}