using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Elevation
{
    public interface IElevationModel {
        IElevationModel Create(Geometry geom1, Geometry geom2);
        double zGet(Coordinate p, Coordinate q);
        double zGetOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2);
        void PopulateZ(Geometry geom);
        double GetZ(double x, double y);
        Coordinate CopyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2);
        Coordinate CopyWithZ(Coordinate p, double z);
    }
}