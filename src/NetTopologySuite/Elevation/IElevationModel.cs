using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Elevation
{
    public interface IElevationModel {
        IElevationModel Create(Geometry geom1, Geometry geom2);
        double GetZFrom(Coordinate p, Coordinate q);
        double GetZFromOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2);
        double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2);
        void PopulateZ(Geometry geom);
        double GetZ(double x, double y);
        Coordinate CopyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        Coordinate CopyWithZ(Coordinate p, double z);
    }
}