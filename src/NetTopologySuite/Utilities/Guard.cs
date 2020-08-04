using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A guard class
    /// </summary>
    [Obsolete]
    public static class Guard
    {
        /// <summary>
        /// Checks if a value is <b>not</b> <c>null</c>.
        /// </summary>
        /// <param name="candidate">The value to check for <c>null</c></param>
        /// <param name="propertyName">The name of the property that <paramref name="candidate"/> belongs to.</param>
        public static void IsNotNull(object candidate, string propertyName)
        {
            if (candidate == null)
                throw new ArgumentNullException(propertyName);
        }
    }
}
