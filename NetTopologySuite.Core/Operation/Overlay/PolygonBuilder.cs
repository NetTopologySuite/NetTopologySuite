using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Forms <c>Polygon</c>s out of a graph of {DirectedEdge}s.
    /// The edges to use are marked as being in the result Area.
    /// </summary>
    public class PolygonBuilder
    {
        private readonly IGeometryFactory _geometryFactory;        
        private readonly List<EdgeRing> _shellList = new List<EdgeRing>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryFactory"></param>
        public PolygonBuilder(IGeometryFactory geometryFactory)
        {
            _geometryFactory = geometryFactory;
        }

        /// <summary>
        /// Add a complete graph.
        /// The graph is assumed to contain one or more polygons,
        /// possibly with holes.
        /// </summary>
        /// <param name="graph"></param>
        public void Add(PlanarGraph graph)
        {
            Add(graph.EdgeEnds, graph.Nodes);
        }

        /// <summary> 
        /// Add a set of edges and nodes, which form a graph.
        /// The graph is assumed to contain one or more polygons,
        /// possibly with holes.
        /// </summary>
        /// <param name="dirEdges"></param>
        /// <param name="nodes"></param>
        public void Add(IList<EdgeEnd> dirEdges, IList<Node> nodes)
        {
            PlanarGraph.LinkResultDirectedEdges(nodes);
            var maxEdgeRings = BuildMaximalEdgeRings(dirEdges);
            var freeHoleList = new List<EdgeRing>();
            var edgeRings = BuildMinimalEdgeRings(maxEdgeRings, _shellList, freeHoleList);
            SortShellsAndHoles(edgeRings, _shellList, freeHoleList);
            PlaceFreeHoles(_shellList, freeHoleList);
            //Assert: every hole on freeHoleList has a shell assigned to it
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<IGeometry> Polygons
        {
            get
            {
                var resultPolyList = ComputePolygons(_shellList);
                return resultPolyList;
            }
        }

        /// <summary> 
        /// For all DirectedEdges in result, form them into MaximalEdgeRings.
        /// </summary>
        /// <param name="dirEdges"></param>
        /// <returns></returns>
        private List<EdgeRing> BuildMaximalEdgeRings(IEnumerable<EdgeEnd> dirEdges)
        {
            var maxEdgeRings = new List<EdgeRing>();
            foreach (DirectedEdge de in dirEdges)
            {
                if (de.IsInResult && de.Label.IsArea())
                {
                    // if this edge has not yet been processed
                    if (de.EdgeRing == null)
                    {
                        var er = new MaximalEdgeRing(de, _geometryFactory);
                        maxEdgeRings.Add(er);
                        er.SetInResult();
                    }
                }
            }
            return maxEdgeRings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxEdgeRings"></param>
        /// <param name="shellList"></param>
        /// <param name="freeHoleList"></param>
        /// <returns></returns>
        private List<EdgeRing> BuildMinimalEdgeRings(List<EdgeRing> maxEdgeRings, IList<EdgeRing> shellList, IList<EdgeRing> freeHoleList)
        {
            var edgeRings = new List<EdgeRing>();
            foreach (MaximalEdgeRing er in maxEdgeRings)
            {
                if (er.MaxNodeDegree > 2)
                {
                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    var minEdgeRings = er.BuildMinimalRings();
                    // at this point we can go ahead and attempt to place holes, if this EdgeRing is a polygon
                    var shell = FindShell(minEdgeRings);
                    if (shell != null)
                    {
                        PlacePolygonHoles(shell, minEdgeRings);
                        shellList.Add(shell);
                    }
                    else
                    {
                        // freeHoleList.addAll(minEdgeRings);
                        foreach (EdgeRing obj in minEdgeRings)
                            freeHoleList.Add(obj);
                    }
                }
                else edgeRings.Add(er);
            }
            return edgeRings;
        }

        /// <summary>
        /// This method takes a list of MinimalEdgeRings derived from a MaximalEdgeRing,
        /// and tests whether they form a Polygon.  This is the case if there is a single shell
        /// in the list.  In this case the shell is returned.
        /// The other possibility is that they are a series of connected holes, in which case
        /// no shell is returned.
        /// </summary>
        /// <returns>The shell EdgeRing, if there is one<br/> or
        /// <value>null</value>, if all the rings are holes.</returns>
        private static EdgeRing FindShell(IEnumerable<EdgeRing> minEdgeRings)
        {
            int shellCount = 0;
            EdgeRing shell = null;
            foreach (/*Minimal*/EdgeRing er in minEdgeRings)
            {
                if (!er.IsHole)
                {
                    shell = er;
                    shellCount++;
                }
            }
            Assert.IsTrue(shellCount <= 1, "found two shells in MinimalEdgeRing list");
            return shell;
        }

        /// <summary>
        /// This method assigns the holes for a Polygon (formed from a list of
        /// MinimalEdgeRings) to its shell.
        /// Determining the holes for a MinimalEdgeRing polygon serves two purposes:
        /// it is faster than using a point-in-polygon check later on.
        /// it ensures correctness, since if the PIP test was used the point
        /// chosen might lie on the shell, which might return an incorrect result from the
        /// PIP test.
        /// </summary>
        /// <param name="shell"></param>
        /// <param name="minEdgeRings"></param>
        private static void PlacePolygonHoles(EdgeRing shell, IEnumerable<EdgeRing> minEdgeRings)
        {
            foreach (MinimalEdgeRing er in minEdgeRings)
            {
                if (er.IsHole) 
                    er.Shell = shell;
            }
        }

        /// <summary> 
        /// For all rings in the input list,
        /// determine whether the ring is a shell or a hole
        /// and add it to the appropriate list.
        /// Due to the way the DirectedEdges were linked,
        /// a ring is a shell if it is oriented CW, a hole otherwise.
        /// </summary>
        /// <param name="edgeRings"></param>
        /// <param name="shellList"></param>
        /// <param name="freeHoleList"></param>
        private static void SortShellsAndHoles(IEnumerable<EdgeRing> edgeRings, IList<EdgeRing> shellList, IList<EdgeRing> freeHoleList)
        {
            foreach (EdgeRing er in edgeRings)
            {
                er.SetInResult();
                if (er.IsHole)
                     freeHoleList.Add(er);
                else shellList.Add(er);
            }
        }

        /// <summary>
        /// This method determines finds a containing shell for all holes
        /// which have not yet been assigned to a shell.
        /// These "free" holes should
        /// all be properly contained in their parent shells, so it is safe to use the
        /// <c>findEdgeRingContaining</c> method.
        /// (This is the case because any holes which are NOT
        /// properly contained (i.e. are connected to their
        /// parent shell) would have formed part of a MaximalEdgeRing
        /// and been handled in a previous step).
        /// </summary>
        /// <param name="shellList"></param>
        /// <param name="freeHoleList"></param>
        private static void PlaceFreeHoles(IList<EdgeRing> shellList, IEnumerable<EdgeRing> freeHoleList)
        {
            foreach (EdgeRing hole in freeHoleList)
            {
                // only place this hole if it doesn't yet have a shell
                if (hole.Shell == null)
                {
                    EdgeRing shell = FindEdgeRingContaining(hole, shellList);
                    //Assert.IsTrue(shell != null, "unable to assign hole to a shell");
                    if (shell == null)
                        throw new TopologyException("unable to assign hole to a shell", hole.GetCoordinate(0));
                    hole.Shell = shell;
                }
             }
        }

        /// <summary> 
        /// Find the innermost enclosing shell EdgeRing containing the argument EdgeRing, if any.
        /// The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        /// The algorithm used depends on the fact that:
        /// ring A contains ring B iff envelope(ring A) contains envelope(ring B).
        /// This routine is only safe to use if the chosen point of the hole
        /// is known to be properly contained in a shell
        /// (which is guaranteed to be the case if the hole does not touch its shell).
        /// </summary>
        /// <param name="testEr"></param>
        /// <param name="shellList"></param>
        /// <returns>Containing EdgeRing, if there is one <br/> or
        /// <value>null</value> if no containing EdgeRing is found.</returns>
        private static EdgeRing FindEdgeRingContaining(EdgeRing testEr, IEnumerable<EdgeRing> shellList)
        {
            ILinearRing teString = testEr.LinearRing;
            Envelope testEnv = teString.EnvelopeInternal;
            Coordinate testPt = teString.GetCoordinateN(0);

            EdgeRing minShell = null;
            Envelope minEnv = null;
            foreach (EdgeRing tryShell in shellList)
            {
                ILinearRing tryRing = tryShell.LinearRing;
                Envelope tryEnv = tryRing.EnvelopeInternal;
                if (minShell != null)
                    minEnv = minShell.LinearRing.EnvelopeInternal;
                bool isContained = false;
                if (tryEnv.Contains(testEnv) && CGAlgorithms.IsPointInRing(testPt, tryRing.Coordinates))
                        isContained = true;
                // check if this new containing ring is smaller than the current minimum ring
                if (isContained)
                {
                    if (minShell == null || minEnv.Contains(tryEnv))
                        minShell = tryShell;                    
                }
            }
            return minShell;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shellList"></param>
        /// <returns></returns>
        private IList<IGeometry> ComputePolygons(IEnumerable<EdgeRing> shellList)
        {
            IList<IGeometry> resultPolyList = new List<IGeometry>();
            // add Polygons for all shells
            foreach (EdgeRing er in shellList)
            {
                IPolygon poly = er.ToPolygon(_geometryFactory);
                resultPolyList.Add(poly);
            }
            return resultPolyList;
        }

        /// <summary> 
        /// Checks the current set of shells (with their associated holes) to
        /// see if any of them contain the point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ContainsPoint(Coordinate p)
        {
            foreach (EdgeRing er in _shellList)
            {
                if (er.ContainsPoint(p))
                    return true;
            }
            return false;
        }
    }
}