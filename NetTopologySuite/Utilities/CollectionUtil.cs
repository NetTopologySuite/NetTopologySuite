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
        public delegate TResult FunctionDelegate<T, TResult>(T obj);

        /// <summary>
        /// Copies <typeparamref name="TIn"/>s in an array to an <typeparamref name="TOut"/> array
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="array">the source array</param>
        /// <returns>An array of objects</returns>
        public static TOut[] Cast<TIn, TOut>(TIn[] array)
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
    }
}
