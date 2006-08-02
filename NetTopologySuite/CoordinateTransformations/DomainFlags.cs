using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
    // Diego Guidi say's:
    // Modified class to enumeration with flags attribute.

    /// <summary>
    /// Flags indicating parts of domain covered by a convex hull.
    /// </summary>
    /// <remarks>
    /// These flags can be combined with Binary OR operator.
    /// </remarks>
    [Flags]
    public enum DomainFlags
    {        
        /// <summary>
		/// At least one point in a convex hull is inside the transform's domain.
		/// </summary>
		Inside = 0x0001,

		/// <summary>
		/// At least one point in a convex hull is outside the transform's domain.
		/// </summary>
        Outside = 0x0002,

		/// <summary>
		/// At least one point in a convex hull is not transformed continuously.
		/// </summary>
		/// <remarks>
		/// As an example, consider a "Longitude_Rotation" transform which adjusts
		/// longitude coordinates to take account of a change in Prime Meridian.
		/// If the rotation is 5 degrees east, then the point (Lat=175,Lon=0)
		/// is not transformed continuously, since it is on the meridian line
		/// which will be split at +180/-180 degrees.
		/// </remarks>
        Discontinuous = 0x0004,
    }
}
