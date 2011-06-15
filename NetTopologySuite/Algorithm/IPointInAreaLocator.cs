using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    ///<summary>
    /// An interface for classes which determine the <see cref="Locations"/> of points in a <see cref="IGeometry"/>
    ///</summary>
    ///<author>Martin Davis</author>
    public interface IPointInAreaLocator
    {
        ///<summary>
        /// Determines the  <see cref="Locations"/> of a point in the <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <returns>the location of the point in the geometry</returns>
        Locations Locate(ICoordinate p);
    }
}