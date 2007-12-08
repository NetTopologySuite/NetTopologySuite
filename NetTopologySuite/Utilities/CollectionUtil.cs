using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// Executes a transformation function on each element of a collection
    /// and returns the results in a new List.
    /// </summary>
    public class CollectionUtil
    {
        public delegate T FunctionDelegate<T>(T obj);

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and returns the results in a new <see cref="IList" />.
        /// </summary>
        public static IEnumerable<TItem> Transform<TItem>(IEnumerable<TItem> items, FunctionDelegate<TItem> func)
        {
            foreach (TItem item in items)
            {
                yield return func(item);
            }
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" /> 
        /// but does not accumulate the result.
        /// </summary>
        public static void Apply<TItem>(IEnumerable<TItem> items, FunctionDelegate<TItem> func)
        {
            foreach (TItem item in items)
            {
                func(item);
            }
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and collects all the entries for which the result
        /// of the function is equal to <see langword="true"/>.
        /// </summary>
        public static IEnumerable<TItem> Select<TItem>(IEnumerable<TItem> items, FunctionDelegate<TItem> func)
        {
            foreach (TItem item in items)
            {
                if (true.Equals(func(item)))
                {
                    yield return item;
                }
            }
        }
    }
}