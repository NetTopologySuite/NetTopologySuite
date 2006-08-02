using System;
using System.Collections;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Utilities
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
        public interface Function
        {
            Object Execute(Object obj);
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and returns the results in a new <see cref="IList" />.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IList Transform(ICollection coll, Function func)
        {
            IList result = new ArrayList();
            IEnumerator i = coll.GetEnumerator(); 
            while(i.MoveNext())            
                result.Add(func.Execute(i.Current));            
            return result;
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" /> 
        /// but does not accumulate the result.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="func"></param>
        public static void Apply(ICollection coll, Function func)
        {
            IEnumerator i = coll.GetEnumerator();
            while(i.MoveNext())
                func.Execute(i.Current);
        }

        /// <summary>
        /// Executes a function on each item in a <see cref="ICollection" />
        /// and collects all the entries for which the result
        /// of the function is equal to <c>true</c>.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IList Select(ICollection collection, Function func)
        {
            IList result = new ArrayList();
            IEnumerator i = collection.GetEnumerator();
            while(i.MoveNext())
            {
                Object item = i.Current;
                if (true.Equals(func.Execute(item)))
                    result.Add(item);                
            }
            return result;
        }
    }
}
