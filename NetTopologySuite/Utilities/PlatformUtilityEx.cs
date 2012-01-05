using System.Collections;
using System.Collections.Generic;
using System.Text;

#if SILVERLIGHT
#if !WINDOWS_PHONE

using NetTopologySuite.Encodings;

#endif

using System.Linq;

#endif

namespace NetTopologySuite.Utilities
{
    public static class PlatformUtilityEx
    {
#if SILVERLIGHT && !WINDOWS_PHONE

        private static readonly IEncodingRegistry Registry = new EncodingRegistry();

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
            return Registry.ASCII;
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

#if !WINDOWS_PHONE

        public static Encoding GetDefaultEncoding()
        {
            return Encoding.Default;
        }

        public static Encoding GetASCIIEncoding()
        {
            return new ASCIIEncoding();
        }
#else

        public static Encoding GetDefaultEncoding()
        {
            return Encoding.UTF8;
        }

#endif

#endif
    }
}