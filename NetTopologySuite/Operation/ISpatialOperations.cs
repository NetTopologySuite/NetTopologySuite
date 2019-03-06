using GeoAPI.Geometries;
using GeoAPI.Operation.Buffer;

namespace NetTopologySuite.Operation
{
    public interface ISpatialOperations
    {
        #region unary operations

        double Area(IGeometry geom);
        double Length(IGeometry geometry);

        IPoint Centroid(IGeometry geometry);

        IPoint InteriorPoint(IGeometry geometry);

        IGeometry Buffer(IGeometry g, double distance, IBufferParameters parameters);

        IGeometry ConvexHull(IGeometry g);

        #endregion

        #region Predicates

        bool Equals(IGeometry g1, IGeometry g2);

        bool IsWithinDistance(IGeometry g1, IGeometry g2, double distance);

        bool Disjoint(IGeometry g1, IGeometry g2);

        bool Touches(IGeometry g1, IGeometry g2);

        bool Intersects(IGeometry g1, IGeometry g2);

        bool Crosses(IGeometry g1, IGeometry g2);

        //bool Within(IGeometry g1, IGeometry g2);

        bool Contains(IGeometry g1, IGeometry g2);

        bool Overlaps(IGeometry g1, IGeometry g2);

        bool Covers(IGeometry g1, IGeometry g2);

        IntersectionMatrix Relate(IGeometry g1, IGeometry g2);

        #endregion

        #region Binary operations

        double Distance(IGeometry g1, IGeometry g2);

        IGeometry Union(IGeometry g1, IGeometry g2);

        IGeometry Difference(IGeometry g1, IGeometry g2);

        IGeometry SymDifference(IGeometry g1, IGeometry g2);

        IGeometry Intersection(IGeometry g1, IGeometry g2);

        #endregion
    }
}
