using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
    [Obsolete("Not used anywhere within NTS")]
    public static class PlatformUtilityEx
    {
        [Obsolete("Not used anywhere within NTS")]
        public static ICollection CastPlatform(ICollection self)
        {
            return self;
        }

        [Obsolete("Not used anywhere within NTS")]
        public static ICollection CastPlatform(IList self)
        {
            return self;
        }

        [Obsolete("Not used anywhere within NTS")]
        public static ICollection<T> CastPlatform<T>(IList<T> self)
        {
            return self;
        }
    }
}
