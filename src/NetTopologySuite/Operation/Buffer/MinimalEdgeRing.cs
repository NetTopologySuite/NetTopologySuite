using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A ring of edges with the property that no node
    /// has degree greater than 2.  These are the form of rings required
    /// to represent polygons under the OGC SFS spatial data model.
    /// </summary>
    /// <seealso cref="MaximalEdgeRing"/>
    internal class MinimalEdgeRing : EdgeRing
    {
        public MinimalEdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
            : base(start, geometryFactory) { }

        public override DirectedEdge GetNext(DirectedEdge de)
        {
            return de.NextMin;
        }

        public override void SetEdgeRing(DirectedEdge de, EdgeRing er)
        {
            de.MinEdgeRing = er;
        }
    }
}
