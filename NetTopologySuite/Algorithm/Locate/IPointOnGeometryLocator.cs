using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm.Locate
{
    ///<summary>
    /// An interface for classes which determine the <see cref="Location"/> of
    /// points in areal geometries.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IPointOnGeometryLocator
    {
        ///<summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="IGeometry"/>.
        ///</summary>
        ///<param name="p">The point to test</param>
        ///<returns>The location of the point in the geometry</returns>
        Location Locate(Coordinate p);
    }
}