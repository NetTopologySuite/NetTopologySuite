using System.Collections;
using System.Collections.Generic;
#if NET35
using System.Linq;
#endif
#if PCL
using ArrayList = System.Collections.Generic.List<object>;
#endif

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
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and returns the results in a new <see cref="IList" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IList Transform(ICollection coll, FunctionDelegate<object> func)
        {
            IList result = new ArrayList();
            foreach(object obj in coll)
                result.Add(func(obj));
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and returns the results in a new <see cref="IList" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static IList<T> Cast<T>(ICollection coll)
        {
            IList<T> result = new List<T>(coll.Count);
            foreach (var obj in coll)
                result.Add((T)obj);
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection{TIn}" />
        /// and returns the results in a new <see cref="IList{TOut}" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static IList<TOut> Cast<TIn, TOut>(ICollection<TIn> coll)
            where TIn: class
            where TOut : class
        {
            IList<TOut> result = new List<TOut>(coll.Count);
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
        public static IList<T> Transform<T>(IList<T> list, FunctionDelegate<T> function)
        {
            IList<T> result = new List<T>(list.Count);
            foreach (T item in list)
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
        public static IList<TResult> Transform<T, TResult>(IList<T> list, FunctionDelegate<T, TResult> function)
        {
            IList<TResult> result = new List<TResult>(list.Count);
            foreach (T item in list)
                result.Add(function(item));
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" /> 
        /// but does not accumulate the result.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        public static void Apply(ICollection coll, FunctionDelegate<object> func)
        {
            foreach (object obj in coll)
                func(obj);
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="IEnumerable{T}" /> 
        /// but does not accumulate the result.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        public static void Apply<T>(IEnumerable<T> coll, FunctionDelegate<T> func)
        {
            foreach (var obj in coll)
                func(obj);
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and collects all the entries for which the result
        /// of the function is equal to <c>true</c>.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IList Select(ICollection coll, FunctionDelegate<object, bool> func)
        {
            IList result = new ArrayList();
            foreach (object obj in coll)
                if (func(obj))
                    result.Add(obj);
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and collects all the entries for which the result
        /// of the function is equal to <c>true</c>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IList<T> Select<T>(IEnumerable<T> items, FunctionDelegate<T, bool> func)
        {
            IList<T> result = new List<T>();
            foreach (var obj in items)
                if (func(obj)) result.Add(obj);
            return result;
        }

        /// <summary>
        /// Copies <typeparamref name="T"/>s in an array to an object array
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="array">the source array</param>
        /// <returns>An array of objects</returns>
        public static TOut[] Cast<TIn,TOut>(TIn[] array)
        {
            var res = new TOut[array.Length];
            System.Array.Copy(array, res, array.Length);
            return res;
        }

        internal static IEnumerable<T> StableSort<T>(IEnumerable<T> items)
        {
            return StableSort(items, Comparer<T>.Default);
        }

        internal static IEnumerable<T> StableSort<T>(IEnumerable<T> items, IComparer<T> comparer)
        {
#if NET35
            // LINQ's OrderBy is guaranteed to be a stable sort.
            return items.OrderBy(x => x, comparer);
#else

            // otherwise, tag each item with the index and sort the wrappers.
            // if we're given a collection (and we always are), use its count
            // to prevent unnecessary array copies.
            var itemCollection = items as ICollection<T>;
            var taggedItems = itemCollection == null
                ? new List<IndexTaggedItem<T>>()
                : new List<IndexTaggedItem<T>>(itemCollection.Count);

            int index = 0;
            foreach (var item in items)
            {
                taggedItems.Add(new IndexTaggedItem<T>(item, index++));
            }

            taggedItems.Sort(new IndexAwareComparer<T>(comparer));

            var sorted = new List<T>(taggedItems.Count);
            foreach (var taggedItem in taggedItems)
            {
                sorted.Add(taggedItem.Item);
            }

            return sorted;
#endif
        }

#if !NET35
        private sealed class IndexTaggedItem<T>
        {
            internal readonly T Item;
            internal readonly int Index;

            internal IndexTaggedItem(T item, int index)
            {
                this.Item = item;
                this.Index = index;
            }
        }

        private sealed class IndexAwareComparer<T> : Comparer<IndexTaggedItem<T>>
        {
            private readonly IComparer<T> primaryComparer;

            internal IndexAwareComparer(IComparer<T> primaryComparer)
            {
                this.primaryComparer = primaryComparer;
            }

            public override int Compare(IndexTaggedItem<T> x, IndexTaggedItem<T> y)
            {
                int cmp = this.primaryComparer.Compare(x.Item, y.Item);

                // compare equal elements by their index.
                return cmp == 0 ? x.Index.CompareTo(y.Index) : cmp;
            }
        }
#endif
    }
}
