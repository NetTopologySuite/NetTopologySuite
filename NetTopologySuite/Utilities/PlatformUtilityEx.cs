using System.Collections;
using System.Collections.Generic;

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
    }
}
