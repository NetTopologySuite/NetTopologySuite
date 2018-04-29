using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// An interface for factories which create a {@link ConstraintVertex}
    /// </summary>
    /// <author>Martin Davis</author>
    public interface ConstraintVertexFactory
    {
        ConstraintVertex CreateVertex(Coordinate p, Segment constraintSeg);
    }
}