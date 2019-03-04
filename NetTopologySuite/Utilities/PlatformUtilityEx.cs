using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

        [Obsolete("Not used anywhere within NTS")]
        public static Encoding GetDefaultEncoding()
        {
            return Encoding.Default;
        }

        [Obsolete("Not used anywhere within NTS")]
        public static Encoding GetASCIIEncoding()
        {
            return Encoding.ASCII;
        }
    }
}
