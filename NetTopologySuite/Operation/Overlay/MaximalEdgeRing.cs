using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// A ring of edges which may contain nodes of degree > 2.
    /// A MaximalEdgeRing may represent two different spatial entities:
    /// a single polygon possibly containing inversions (if the ring is oriented CW)
    /// a single hole possibly containing exversions (if the ring is oriented CCW)
    /// If the MaximalEdgeRing represents a polygon,
    /// the interior of the polygon is strongly connected.
    /// These are the form of rings used to define polygons under some spatial data models.
    /// However, under the OGC SFS model, MinimalEdgeRings are required.
    /// A MaximalEdgeRing can be converted to a list of MinimalEdgeRings using the
    /// <c>BuildMinimalRings()</c> method.
    /// </summary>
    public class MaximalEdgeRing : EdgeRing
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        /// <param name="geometryFactory"></param>
        public MaximalEdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
            : base(start, geometryFactory) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <returns></returns>
        public override DirectedEdge GetNext(DirectedEdge de)
        {
            return de.Next;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <param name="er"></param>
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
                ((DirectedEdgeStar) node.Edges).LinkMinimalDirectedEdges(this);
                de = de.Next;
            }
            while (de != startDe);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
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
