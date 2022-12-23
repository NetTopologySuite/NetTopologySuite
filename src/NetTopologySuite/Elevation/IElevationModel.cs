using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Elevation
{
    /// <summary>
    /// A elevation model interface used to populate missing Z values.
    /// </summary>
    public interface IElevationModel {
        /// <summary>
        /// Creates an elevation model from two geometries (which may be null).
        /// </summary>
        IElevationModel Create(Geometry geom1, Geometry geom2);
        /// <summary>
        /// Gets the Z value of the first argument if present,
        /// otherwise the value of the second argument.
        /// </summary>
        double GetZFrom(Coordinate p, Coordinate q);
        /// <summary>
        /// Gets the Z value of a coordinate if present, or
        /// interpolates it.
        /// </summary>
        double GetZFromOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        /// <summary>
        /// Interpolates a Z value for a point along
        /// a line segment between two points.
        /// The Z value of the interpolation point (if any) is ignored.
        /// </summary>
        double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2);
        /// <summary>
        /// Interpolates a Z value for a point along
        /// two line segments and computes their average.
        /// The Z value of the interpolation point (if any) is ignored.
        /// </summary>
        double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2);
        /// <summary>
        /// Computes Z values for any missing Z values in a geometry.
        /// If the model has no Z value, or the geometry coordinate dimension
        /// does not include Z, the geometry is not updated.
        /// </summary>
        void PopulateZ(Geometry geom);
        /// <summary>
        /// Gets the model Z value at a given location.
        /// </summary>
        double GetZ(double x, double y);
        /// <summary>
        /// Makes a copy of a coordinate with z iterpolated from p1 and p2.
        /// </summary>
        Coordinate CopyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        /// <summary>
        /// Makes a copy of a coordinate with z set to the specified value.
        /// </summary>
        Coordinate CopyWithZ(Coordinate p, double z);
    }
}