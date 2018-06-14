using System;
using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// Static utility class for casting objects
    /// </summary>
    [Obsolete("No longer used anywhere in NTS; see error messages on individual methods for what to use instead.", error: true)]
    public static class Caster
    {
        /// <summary>
        /// Cast function from arbitrary-type to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The output type</typeparam>
        /// <param name="inputs">The sequence of items to cast.</param>
        /// <returns><paramref name="inputs"/> as an enumerable of <typeparamref name="T"/></returns>
        /// <exception cref="InvalidCastException">Thrown if cast cannot be performed</exception>
        [Obsolete("No longer used anywhere in NTS; use System.Linq.Enumerable.Cast<TResult>(this IEnumerable source) instead.", error: true)]
        public static IEnumerable<T> Cast<T>(IEnumerable inputs)
        {
            foreach (object input in inputs)
                yield return (T) input;
        }

        /// <summary>
        /// Cast function from sub-type to super-type
        /// </summary>
        /// <typeparam name="TSuper">The output (super/base) type</typeparam>
        /// <typeparam name="TSub">The input (sub/derived) type</typeparam>
        /// <param name="inputs">The sequence of items to cast.</param>
        /// <returns><paramref name="inputs"/> as an enumerable of <typeparamref name="TSuper"/></returns>
        [Obsolete("No longer used anywhere in NTS; use generic type parameter covariance instead.  Upcast<TSub, TSuper>(something) should be equivalent to System.Linq.Enumerable.AsEnumerable<TSuper>(something), but you might be able to do better in your specific case.", error: true)]
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
        [Obsolete("No longer used anywhere in NTS; use System.Linq.Enumerable.Cast<TResult>(this IEnumerable source) instead.", error: true)]
        public static IEnumerable<TSub> Downcast<TSuper, TSub>(IEnumerable<TSuper> inputs)
            where TSub : TSuper
        {
            foreach (var input in inputs)
                yield return (TSub)input;
        }
    }
}