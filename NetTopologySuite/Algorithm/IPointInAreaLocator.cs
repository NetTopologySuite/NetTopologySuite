using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// An interface for classes which determine the <see cref="Location"/> of points in a <see cref="Geometry"/>
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IPointInAreaLocator
    {
        /// <summary>
        /// Determines the  <see cref="Location"/> of a point in the <see cref="Geometry"/>.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <returns>the location of the point in the geometry</returns>
        Location Locate(Coordinate p);
    }
}