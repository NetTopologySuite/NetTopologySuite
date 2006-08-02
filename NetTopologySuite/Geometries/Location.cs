using System;
using System.Collections;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public enum Locations
    {
        /// <summary>
        /// DE-9IM row index of the interior of the first point and column index of
        /// the interior of the second point. Location value for the interior of a
        /// point.
        /// int value = 0;
        /// </summary>
        Interior = 0,

        /// <summary>
        /// DE-9IM row index of the boundary of the first point and column index of
        /// the boundary of the second point. Location value for the boundary of a
        /// point.
        /// int value = 1;
        /// </summary>
        Boundary = 1,

        /// <summary>
        /// DE-9IM row index of the exterior of the first point and column index of
        /// the exterior of the second point. Location value for the exterior of a
        /// point.
        /// int value = 2;
        /// </summary>
        Exterior = 2,

        /// <summary>
        /// Used for uninitialized location values.
        /// int value = -1;
        /// </summary>
        Null = -1,
    }

    /// <summary>
    /// 
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Converts the location value to a location symbol, for example, <c>EXTERIOR => 'e'</c>.
        /// </summary>
        /// <param name="locationValue"></param>
        /// <returns>Either 'e', 'b', 'i' or '-'.</returns>
        public static char ToLocationSymbol(Locations locationValue)
        {
            switch (locationValue)
            {
                case Locations.Exterior:
                    return 'e';
                case Locations.Boundary:
                    return 'b';
                case Locations.Interior:
                    return 'i';
                case Locations.Null:
                    return '-';
            }
            throw new ArgumentException("Unknown location value: " + locationValue);
        }
    }   
}
