using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using GisSharpBlog.NetTopologySuite.Operation.Relate;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary> 
    /// Checks that a {GeometryGraph} representing an area
    /// (a <c>Polygon</c> or <c>MultiPolygon</c> )
    /// is consistent with the SFS semantics for area geometries.
    /// Checks include:
    /// Testing for rings which self-intersect (both properly and at nodes).
    /// Testing for duplicate rings.
    /// If an inconsistency if found the location of the problem is recorded.
    /// </summary>
    public class ConsistentAreaTester 
    {
        private readonly LineIntersector li = new RobustLineIntersector();
        private GeometryGraph geomGraph;
        private RelateNodeGraph nodeGraph = new RelateNodeGraph();

        // the intersection point found (if any)
        private ICoordinate invalidPoint;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomGraph"></param>
        public ConsistentAreaTester(GeometryGraph geomGraph)
        {
            this.geomGraph = geomGraph;
        }

        /// <summary>
        /// Returns the intersection point, or <c>null</c> if none was found.
        /// </summary>        
        public ICoordinate InvalidPoint
        {
            get
            {
                return invalidPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNodeConsistentArea
        {
            get
            {
                /*
                * To fully check validity, it is necessary to
                * compute ALL intersections, including self-intersections within a single edge.
                */
                SegmentIntersector intersector = geomGraph.ComputeSelfNodes(li, true);
                if (intersector.HasProperIntersection)
                {
                    invalidPoint = intersector.ProperIntersectionPoint;
                    return false;
                }
                nodeGraph.Build(geomGraph);
                return IsNodeEdgeAreaLabelsConsistent;
            }
        }

        /// <summary>
        /// Check all nodes to see if their labels are consistent.
        /// If any are not, return false.
        /// </summary>
        private bool IsNodeEdgeAreaLabelsConsistent
        {
            get
            {
                for (IEnumerator nodeIt = nodeGraph.GetNodeEnumerator(); nodeIt.MoveNext(); )
                {
                    RelateNode node = (RelateNode) nodeIt.Current;
                    if (!node.Edges.IsAreaLabelsConsistent)
                    {
                        invalidPoint = (ICoordinate) node.Coordinate.Clone();
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Checks for two duplicate rings in an area.
        /// Duplicate rings are rings that are topologically equal
        /// (that is, which have the same sequence of points up to point order).
        /// If the area is topologically consistent (determined by calling the
        /// <c>isNodeConsistentArea</c>,
        /// duplicate rings can be found by checking for EdgeBundles which contain more than one EdgeEnd.
        /// (This is because topologically consistent areas cannot have two rings sharing
        /// the same line segment, unless the rings are equal).
        /// The start point of one of the equal rings will be placed in invalidPoint.
        /// Returns <c>true</c> if this area Geometry is topologically consistent but has two duplicate rings.
        /// </summary>
        public bool HasDuplicateRings
        {
            get
            {
                for (IEnumerator nodeIt = nodeGraph.GetNodeEnumerator(); nodeIt.MoveNext(); )
                {
                    RelateNode node = (RelateNode) nodeIt.Current;
                    for (IEnumerator i = node.Edges.GetEnumerator(); i.MoveNext(); )
                    {
                        EdgeEndBundle eeb = (EdgeEndBundle) i.Current;
                        if (eeb.EdgeEnds.Count > 1)
                        {
                            invalidPoint = eeb.Edge.GetCoordinate(0);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
