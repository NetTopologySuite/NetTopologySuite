using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Forms <see cref="Polygon"/>s out of a graph of <see cref="DirectedEdge"/>s.
    /// The edges to use are marked as being in the result Area.
    /// <para/>
    /// This is a buffer-specific version of <see cref="NetTopologySuite.Operation.Overlay.PolygonBuilder"/>
    /// that differs in <c>PlaceFreeHoles</c>: holes that do not lie within any
    /// shell are silently discarded (they are eroded elements) rather than
    /// causing a <see cref="TopologyException"/>.
    /// </summary>
    internal class PolygonBuilder
    {
        private readonly GeometryFactory _geometryFactory;
        private readonly List<EdgeRing> _shellList = new List<EdgeRing>();

        public PolygonBuilder(GeometryFactory geometryFactory)
        {
            _geometryFactory = geometryFactory;
        }

        public void Add(PlanarGraph graph)
        {
            Add(graph.EdgeEnds, graph.Nodes);
        }

        public void Add(IList<EdgeEnd> dirEdges, IList<Node> nodes)
        {
            PlanarGraph.LinkResultDirectedEdges(nodes);
            var maxEdgeRings = BuildMaximalEdgeRings(dirEdges);
            var freeHoleList = new List<EdgeRing>();
            var edgeRings = BuildMinimalEdgeRings(maxEdgeRings, _shellList, freeHoleList);
            SortShellsAndHoles(edgeRings, _shellList, freeHoleList);
            PlaceFreeHoles(_shellList, freeHoleList);
        }

        public IList<Geometry> Polygons
        {
            get { return ComputePolygons(_shellList); }
        }

        private List<EdgeRing> BuildMaximalEdgeRings(IEnumerable<EdgeEnd> dirEdges)
        {
            var maxEdgeRings = new List<EdgeRing>();
            foreach (DirectedEdge de in dirEdges)
            {
                if (de.IsInResult && de.Label.IsArea())
                {
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

        private List<EdgeRing> BuildMinimalEdgeRings(List<EdgeRing> maxEdgeRings, IList<EdgeRing> shellList, IList<EdgeRing> freeHoleList)
        {
            var edgeRings = new List<EdgeRing>();
            foreach (MaximalEdgeRing er in maxEdgeRings)
            {
                if (er.MaxNodeDegree > 2)
                {
                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    var minEdgeRings = er.BuildMinimalRings();
                    var shell = FindShell(minEdgeRings);
                    if (shell != null)
                    {
                        PlacePolygonHoles(shell, minEdgeRings);
                        shellList.Add(shell);
                    }
                    else
                    {
                        foreach (var obj in minEdgeRings)
                            freeHoleList.Add(obj);
                    }
                }
                else edgeRings.Add(er);
            }
            return edgeRings;
        }

        private static EdgeRing FindShell(IEnumerable<EdgeRing> minEdgeRings)
        {
            int shellCount = 0;
            EdgeRing shell = null;
            foreach (var er in minEdgeRings)
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

        private static void PlacePolygonHoles(EdgeRing shell, IEnumerable<EdgeRing> minEdgeRings)
        {
            foreach (MinimalEdgeRing er in minEdgeRings)
            {
                if (er.IsHole)
                    er.Shell = shell;
            }
        }

        private static void SortShellsAndHoles(IEnumerable<EdgeRing> edgeRings, IList<EdgeRing> shellList, IList<EdgeRing> freeHoleList)
        {
            foreach (var er in edgeRings)
            {
                //er.SetInResult();
                if (er.IsHole)
                    freeHoleList.Add(er);
                else shellList.Add(er);
            }
        }

        /// <summary>
        /// Determines a containing shell for all holes which have not yet been
        /// assigned to a shell.
        /// <para/>
        /// Holes which do not lie within any shell are eroded elements and are
        /// silently discarded (unlike <see cref="NetTopologySuite.Operation.Overlay.PolygonBuilder"/> which
        /// throws a <see cref="TopologyException"/>).
        /// </summary>
        private static void PlaceFreeHoles(IList<EdgeRing> shellList, IEnumerable<EdgeRing> freeHoleList)
        {
            foreach (var hole in freeHoleList)
            {
                if (hole.Shell == null)
                {
                    var shell = FindEdgeRingContaining(hole, shellList);
                    // If hole lies outside all shells it is an eroded element — discard it.
                    if (shell != null)
                        hole.Shell = shell;
                }
            }
        }

        private static EdgeRing FindEdgeRingContaining(EdgeRing testEr, IEnumerable<EdgeRing> shellList)
        {
            var testRing = testEr.LinearRing;
            var testEnv = testRing.EnvelopeInternal;

            EdgeRing minShell = null;
            Envelope minShellEnv = null;
            foreach (var tryShell in shellList)
            {
                var tryShellRing = tryShell.LinearRing;
                var tryShellEnv = tryShellRing.EnvelopeInternal;
                if (tryShellEnv.Equals(testEnv)) continue;
                if (!tryShellEnv.Contains(testEnv)) continue;

                var testPt = CoordinateArrays.PointNotInList(testRing.Coordinates, tryShellRing.Coordinates);
                bool isContained = PointLocation.IsInRing(testPt, tryShellRing.Coordinates);
                if (isContained)
                {
                    if (minShell == null || minShellEnv.Contains(tryShellEnv))
                    {
                        minShell = tryShell;
                        minShellEnv = minShell.LinearRing.EnvelopeInternal;
                    }
                }
            }
            return minShell;
        }

        private IList<Geometry> ComputePolygons(IEnumerable<EdgeRing> shellList)
        {
            var resultPolyList = new List<Geometry>();
            foreach (var er in shellList)
            {
                var poly = er.ToPolygon(_geometryFactory);
                resultPolyList.Add(poly);
            }
            return resultPolyList;
        }
    }
}
