using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A ring of edges which may contain nodes of degree &gt; 2.
    /// A <c>MaximalEdgeRing</c> may represent two different spatial entities:
    /// <list type="bullet">
    /// <item><description>a single polygon possibly containing inversions (if the ring is oriented CW)</description></item>
    /// <item><description>a single hole possibly containing exversions (if the ring is oriented CCW)</description></item>
    /// </list>
    /// If the <c>MaximalEdgeRing</c> represents a polygon,
    /// the interior of the polygon is strongly connected.
    /// <para/>
    /// These are the form of rings used to define polygons under some spatial data models.
    /// However, under the OGC SFS model, <see cref="MinimalEdgeRing"/>s are required.
    /// A <c>MaximalEdgeRing</c> can be converted to a list of <see cref="MinimalEdgeRing"/>s
    /// using the <see cref="BuildMinimalRings()"/> method.
    /// </summary>
    internal class MaximalEdgeRing : EdgeRing
    {
        public MaximalEdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
            : base(start, geometryFactory) { }

        public override DirectedEdge GetNext(DirectedEdge de)
        {
            return de.Next;
        }

        public override void SetEdgeRing(DirectedEdge de, EdgeRing er)
        {
            de.EdgeRing = er;
        }

        /// <summary>
        /// For all nodes in this EdgeRing,
        /// link the DirectedEdges at the node to form minimalEdgeRings
        /// </summary>
        public void LinkDirectedEdgesForMinimalEdgeRings()
        {
            var de = startDe;
            do
            {
                var node = de.Node;
                ((DirectedEdgeStar)node.Edges).LinkMinimalDirectedEdges(this);
                de = de.Next;
            }
            while (de != startDe);
        }

        public IList<EdgeRing> BuildMinimalRings()
        {
            var minEdgeRings = new List<EdgeRing>();
            var de = startDe;
            do
            {
                if (de.MinEdgeRing == null)
                {
                    var minEr = new MinimalEdgeRing(de, GeometryFactory);
                    minEdgeRings.Add(minEr);
                }
                de = de.Next;
            }
            while (de != startDe);
            return minEdgeRings;
        }
    }
}
