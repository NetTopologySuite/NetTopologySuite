using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// The location of a <see cref="Coordinate"/> relative to a <see cref="Geometry"/>
    /// </summary>
    public enum Location
    {
        /// <summary>
        /// DE-9IM row index of the interior of the first point and column index of
        /// the interior of the second point. Location value for the interior of a
        /// point.
        /// </summary>
        /// <remarks>int value = 0;</remarks>
        Interior = 0,

        /// <summary>
        /// DE-9IM row index of the boundary of the first point and column index of
        /// the boundary of the second point. Location value for the boundary of a
        /// point.
        /// </summary>
        /// <remarks>int value = 1;</remarks>
        Boundary = 1,

        /// <summary>
        /// DE-9IM row index of the exterior of the first point and column index of
        /// the exterior of the second point. Location value for the exterior of a
        /// point.
        /// </summary>
        /// <remarks>int value = 2;</remarks>
        Exterior = 2,

        /// <summary>
        /// Used for uninitialized location values.
        /// </summary>
        /// <remarks>int value = 1;</remarks>
        Null = -1,
    }

    /// <summary>
    /// Utility class for <see cref="Location"/> enumeration
    /// </summary>
    public class LocationUtility
    {
        /// <summary>
        /// Converts the location value to a location symbol, for example, <c>EXTERIOR => 'e'</c>.
        /// </summary>
        /// <param name="locationValue"></param>
        /// <returns>Either 'e', 'b', 'i' or '-'.</returns>
        public static char ToLocationSymbol(Location locationValue)
        {
            switch (locationValue)
            {
                case Location.Exterior:
                    return 'e';
                case Location.Boundary:
                    return 'b';
                case Location.Interior:
                    return 'i';
                case Location.Null:
                    return '-';
            }
            throw new ArgumentException("Unknown location value: " + locationValue);
        }
    }   
}
