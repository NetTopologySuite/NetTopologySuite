using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// Executes a transformation function on each element of a collection
    /// and returns the results in a new List.
    /// </summary>
    public class CollectionUtil
    {

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public delegate T FunctionDelegate<T>(T obj);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public delegate TResult FunctionDelegate< T,  TResult>(T obj);

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection{TIn}" />
        /// and returns the results in a new <see cref="IList{TOut}" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        [Obsolete]
        public static IList<TOut> Cast<TIn, TOut>(ICollection<TIn> coll)
            where TIn: class
            where TOut : class
        {
            var result = new List<TOut>(coll.Count);
            foreach (var obj in coll)
                result.Add(obj as TOut);
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="IList{T}" />
        /// and returns the results in a new <see cref="IList{T}" />.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        [Obsolete]
        public static IList<T> Transform<T>(IList<T> list, FunctionDelegate<T> function)
        {
            var result = new List<T>(list.Count);
            foreach (var item in list)
                result.Add(function(item));
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="IList{T}" />
        /// and returns the results in a new <see cref="IList{TResult}" />.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        [Obsolete]
        public static IList<TResult> Transform<T, TResult>(IList<T> list, FunctionDelegate<T, TResult> function)
        {
            var result = new List<TResult>(list.Count);
            foreach (var item in list)
                result.Add(function(item));
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="IEnumerable{T}" />
        /// but does not accumulate the result.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        [Obsolete]
        public static void Apply<T>(IEnumerable<T> coll, FunctionDelegate<T> func)
        {
            foreach (var obj in coll)
                func(obj);
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="IEnumerable{T}" />
        /// and collects all the entries for which the result
        /// of the function is equal to <c>true</c>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete]
        public static IList<T> Select<T>(IEnumerable<T> items, FunctionDelegate<T, bool> func)
        {
            var result = new List<T>();
            foreach (var obj in items)
                if (func(obj)) result.Add(obj);
            return result;
        }

        /// <summary>
        /// Copies <typeparamref name="TIn"/>s in an array to an <typeparamref name="TOut"/> array
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="array">the source array</param>
        /// <returns>An array of objects</returns>
        public static TOut[] Cast<TIn,TOut>(TIn[] array)
        {
            var res = new TOut[array.Length];
            Array.Copy(array, res, array.Length);
            return res;
        }

        internal static IEnumerable<T> StableSort<T>(IEnumerable<T> items)
        {
            return StableSort(items, Comparer<T>.Default);
        }

        internal static IEnumerable<T> StableSort<T>(IEnumerable<T> items, IComparer<T> comparer)
        {
            // LINQ's OrderBy is guaranteed to be a stable sort.
            return items.OrderBy(x => x, comparer);
        }

        #region Obsolete Utilities for Non-Generic Collections (Obsolete)

        /// <summary>
        /// Executes a function on each item in a <see cref="System.Collections.ICollection" />
        /// and returns the results in a new <see cref="System.Collections.IList" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        [Obsolete("Not used anywhere in NTS; use LINQ or a generic overload instead.", error: true)]
        public static IList<T> Cast<T>(System.Collections.ICollection coll)
        {
            var result = new List<T>(coll.Count);
            foreach (object obj in coll)
                result.Add((T)obj);
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="System.Collections.ICollection" />
        /// and returns the results in a new <see cref="System.Collections.IList" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete("Not used anywhere in NTS; use LINQ or a generic overload instead.", error: true)]
        public static System.Collections.IList Transform(System.Collections.ICollection coll, FunctionDelegate<object> func)
        {
            var result = new List<object>();
            foreach(object obj in coll)
                result.Add(func(obj));
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="System.Collections.ICollection" />
        /// but does not accumulate the result.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        [Obsolete("Not used anywhere in NTS; use LINQ or a generic overload instead.", error: true)]
        public static void Apply(System.Collections.ICollection coll, FunctionDelegate<object> func)
        {
            foreach (object obj in coll)
                func(obj);
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="System.Collections.ICollection" />
        /// and collects all the entries for which the result
        /// of the function is equal to <c>true</c>.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete("Not used anywhere in NTS; use LINQ or a generic overload instead.", error: true)]
        public static System.Collections.IList Select(System.Collections.ICollection coll, FunctionDelegate<object, bool> func)
        {
            var result = new List<object>();
            foreach (object obj in coll)
                if (func(obj))
                    result.Add(obj);
            return result;
        }

        #endregion
    }
}
