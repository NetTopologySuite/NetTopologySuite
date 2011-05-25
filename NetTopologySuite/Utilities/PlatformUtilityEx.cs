using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if SILVERLIGHT
using NetTopologySuite.Encodings;
using NetTopologySuite.Encodings;

#endif
namespace NetTopologySuite.Utilities
{
    public static class PlatformUtilityEx
    {
#if SILVERLIGHT

        private static IEncodingRegistry _registry = new EncodingRegistry();

        public static IEnumerable<object> CastPlatform(this ICollection self)
        {
            return self.Cast<object>();
        }

        public static IEnumerable<object> CastPlatform(this IList self)
        {
            return self.Cast<object>();
        }

        public static IEnumerable<T> CastPlatform<T>(this IList<T> self)
        {
            return self;
        }

        public static Encoding GetDefaultEncoding()
        {
            return Encoding.Unicode;
        }

        public static Encoding GetASCIIEncoding()
        {
            return _registry.ASCII;
        }

#else
        public static ICollection CastPlatform(this ICollection self)
        {
            return self;
        }

        public static ICollection CastPlatform(this IList self)
        {
            return self;
        }

        public static ICollection<T> CastPlatform<T>(this IList<T> self)
        {
            return self;
        }

        public static Encoding GetDefaultEncoding()
        {
            return Encoding.Default;
        }

        public static Encoding GetASCIIEncoding()
        {
            return new ASCIIEncoding();
        }
#endif

    }
}
