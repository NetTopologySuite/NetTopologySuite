using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if !NET35 && !PCL

namespace GeoAPI
{
    public delegate TResult Func<T1, TResult>(T1 t1);
    public delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3);
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 t1, T2 t2, T3 t3, T4 t4);

    namespace Linq
    {
        public static class Enumerable
        {
            public static T[] ToArray<T>(IEnumerable<T> enumerable)
            {
                var asList = enumerable as List<T> ?? new List<T>(enumerable);
                return asList.ToArray();
            }

            public static TOut[] ToArray<TOut>(IEnumerable enumerable)
            {
                var res = new List<TOut>();
                foreach (var @in in enumerable)
                    res.Add((TOut)@in);
                return res.ToArray();
            }

            public static IEnumerable<T> Cast<T>(IEnumerable inputs)
            {
                return NetTopologySuite.Utilities.Caster.Cast<T>(inputs);
            }
        }
    }
}
#endif

namespace NetTopologySuite.Utilities
{
    public static class PlatformUtilityEx
    {
        public static ICollection CastPlatform(ICollection self)
        {
            return self;
        }

        public static ICollection CastPlatform(IList self)
        {
            return self;
        }

        public static ICollection<T> CastPlatform<T>(IList<T> self)
        {
            return self;
        }
#if !PCL

        [Obsolete("Not used anywhere within NTS")]
        public static Encoding GetDefaultEncoding()
        {
            return Encoding.Default;
        }

        [Obsolete("Not used anywhere within NTS")]
        public static Encoding GetASCIIEncoding()
        {
            return new ASCIIEncoding();
        }
#else

        [Obsolete("Not used anywhere within NTS")]
        public static Encoding GetDefaultEncoding()
        {
            return Encoding.UTF8;
        }

#endif
    }
}