using System;

namespace NetTopologySuite.Utilities
{
    public static class Guard
    {
        public static void IsNotNull(object candidate, string propertyName)
        {
            if (candidate == null)
                throw new ArgumentNullException(propertyName);
        }
    }
}
