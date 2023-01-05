using NetTopologySuite.Geometries;

namespace NetTopologySuite.Elevation
{
    /// <summary>
    /// A elevation model interface used to populate missing Z values.
    /// </summary>
    public interface IElevationModel {

        /// <summary>
        /// Gets a value indicating the id of the spatial reference system x- and y-ordinates have to be in to query z-ordinate values
        /// </summary>
        int SRID { get; }

        /// <summary>
        /// Gets a value indicating the area where the elevation model is valid
        /// </summary>
        Envelope Extent { get; }

        /// <summary>
        /// Gets the z-ordinate value for the given coordinate (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">The x-ordinate of the point for which to get the z-ordinate value</param>
        /// <param name="y">The y-ordinate of the point for which to get the z-ordinate value</param>
        /// <param name="success">A flag indicating if the returned z-ordinate value is actually useful</param>
        double GetZ(double x, double y, out bool success);

        /// <summary>
        /// Gets the z-ordinate value for the given coordinate <paramref name="p"/>
        /// </summary>
        /// <param name="p">The point for which to get the z-ordinate value</param>
        /// <param name="success">A flag indicating if the returned z-ordinate value is actually useful</param>
        double GetZ(Coordinate p, out bool success);

        /// <summary>
        /// Computes Z values for any missing Z values in a geometry.
        /// If the model has no Z value, or the geometry coordinate dimension
        /// does not include Z, the geometry is not updated.
        /// </summary>
        void PopulateZ(Geometry geom);
    }
}
