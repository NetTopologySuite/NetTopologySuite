using System.Collections;
using System.Collections.Generic;
#if SILVERLIGHT
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
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and returns the results in a new <see cref="IList" />.
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

    }
}
