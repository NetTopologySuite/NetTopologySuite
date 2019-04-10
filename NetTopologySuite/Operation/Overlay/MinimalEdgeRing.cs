using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// A ring of edges with the property that no node
    /// has degree greater than 2.  These are the form of rings required
    /// to represent polygons under the OGC SFS spatial data model.
    /// </summary>
    public class MinimalEdgeRing : EdgeRing
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        /// <param name="geometryFactory"></param>
        public MinimalEdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
            : base(start, geometryFactory) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <returns></returns>
        public override DirectedEdge GetNext(DirectedEdge de)
        {
            return de.NextMin;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <param name="er"></param>
        public override void SetEdgeRing(DirectedEdge de, EdgeRing er)
        {
            de.MinEdgeRing = er;
        }
    }
}
