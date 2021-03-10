using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNG
{
    internal sealed class PolygonBuilder
    {

        private readonly GeometryFactory _geometryFactory;
        private readonly List<OverlayEdgeRing> _shellList = new List<OverlayEdgeRing>();
        private readonly List<OverlayEdgeRing> _freeHoleList = new List<OverlayEdgeRing>();
        private readonly bool _isEnforcePolygonal;

        public PolygonBuilder(IReadOnlyCollection<OverlayEdge> resultAreaEdges, GeometryFactory geomFact)
        : this(resultAreaEdges, geomFact, true)
        {
        }

        public PolygonBuilder(IReadOnlyCollection<OverlayEdge> resultAreaEdges, GeometryFactory geomFact, bool isEnforcePolygonal)
        {
            _geometryFactory = geomFact;
            _isEnforcePolygonal = isEnforcePolygonal;
            BuildRings(resultAreaEdges);
        }

        public IReadOnlyList<Polygon> GetPolygons()
        {
            return ComputePolygons(_shellList);
        }

        public IReadOnlyList<OverlayEdgeRing> GetShellRings()
        {
            return _shellList;
        }

        private List<Polygon> ComputePolygons(IEnumerable<OverlayEdgeRing> shellList)
        {
            var resultPolyList = new List<Polygon>();
            // add Polygons for all shells
            foreach (var er in shellList)
            {
                var poly = er.ToPolygon(_geometryFactory);
                resultPolyList.Add(poly);
            }
            return resultPolyList;
        }

        private void BuildRings(IReadOnlyCollection<OverlayEdge> resultAreaEdges)
        {
            LinkResultAreaEdgesMax(resultAreaEdges);
            var maxRings = BuildMaximalRings(resultAreaEdges);
            BuildMinimalRings(maxRings);
            PlaceFreeHoles(_shellList, _freeHoleList);
            //Assert: every hole on freeHoleList has a shell assigned to it
        }

        private void LinkResultAreaEdgesMax(IEnumerable<OverlayEdge> resultEdges)
        {
            foreach (var edge in resultEdges)
            {
                //Assert.isTrue(edge.isInResult());
                // TODO: find some way to skip nodes which are already linked
                MaximalEdgeRing.LinkResultAreaMaxRingAtNode(edge);
            }
        }

        /// <summary>
        /// For all OverlayEdges in result, form them into MaximalEdgeRings
        /// </summary>
        private static List<MaximalEdgeRing> BuildMaximalRings(IEnumerable<OverlayEdge> edges)
        {
            var edgeRings = new List<MaximalEdgeRing>();
            foreach (var e in edges)
            {
                if (e.IsInResultArea && e.Label.IsBoundaryEither)
                {
                    // if this edge has not yet been processed
                    if (e.MaxEdgeRing == null)
                    {
                        var er = new MaximalEdgeRing(e);
                        edgeRings.Add(er);
                    }
                }
            }
            return edgeRings;
        }

        private void BuildMinimalRings(IEnumerable<MaximalEdgeRing> maxRings)
        {
            foreach (var erMax in maxRings)
            {
                var minRings = erMax.BuildMinimalRings(_geometryFactory);
                AssignShellsAndHoles(minRings);
            }
        }

        private void AssignShellsAndHoles(List<OverlayEdgeRing> minRings)
        {
            /*
             * Two situations may occur:
             * - the rings are a shell and some holes
             * - rings are a set of holes
             * This code identifies the situation
             * and places the rings appropriately 
             */
            var shell = FindSingleShell(minRings);
            if (shell != null)
            {
                AssignHoles(shell, minRings);
                _shellList.Add(shell);
            }
            else
            {
                // all rings are holes; their shell will be found later
                _freeHoleList.AddRange(minRings);
            }
        }

        /// <summary>
        /// Finds the single shell, if any, out of 
        /// a list of minimal rings derived from a maximal ring.
        /// The other possibility is that they are a set of (connected) holes, 
        /// in which case no shell will be found.
        /// </summary>
        /// <returns>The shell ring, if there is one
        /// or <c>null</c>, if all rings are holes
        /// </returns>
        private OverlayEdgeRing FindSingleShell(IEnumerable<OverlayEdgeRing> edgeRings)
        {
            int shellCount = 0;
            OverlayEdgeRing shell = null;
            foreach (var er in edgeRings)
            {
                if (!er.IsHole)
                {
                    shell = er;
                    shellCount++;
                }
            }
            Assert.IsTrue(shellCount <= 1, "found two shells in EdgeRing list");
            return shell;
        }

        /// <summary>
        /// For the set of minimal rings comprising a maximal ring, 
        /// assigns the holes to the shell known to contain them.
        /// Assigning the holes directly to the shell serves two purposes:
        /// <list type="bullet">
        /// <item><description>it is faster than using a point-in-polygon check later on.</description></item>
        /// <item><description>it ensures correctness, since if the PIP test was used the point
        /// chosen might lie on the shell, which might return an incorrect result from the
        /// PIP test</description></item>
        /// </list>
        /// </summary>
        private static void AssignHoles(OverlayEdgeRing shell, IEnumerable<OverlayEdgeRing> edgeRings)
        {
            foreach (var er in edgeRings)
            {
                if (er.IsHole)
                {
                    er.Shell = shell;
                }
            }
        }

        /// <summary>
        /// Place holes have not yet been assigned to a shell.
        /// These "free" holes should
        /// all be <b>properly</b> contained in their parent shells, so it is safe to use the
        /// <c>findEdgeRingContaining</c> method.
        /// (This is the case because any holes which are NOT
        /// properly contained (i.e. are connected to their
        /// parent shell) would have formed part of a MaximalEdgeRing
        /// and been handled in a previous step).
        /// </summary>
        /// <exception cref="TopologyException">If a hole cannot be assigned to a shell</exception>
        private void PlaceFreeHoles(IReadOnlyCollection<OverlayEdgeRing> shellList, IEnumerable<OverlayEdgeRing> freeHoleList)
        {
            // TODO: use a spatial index to improve performance
            foreach (var hole in freeHoleList)
            {
                // only place this hole if it doesn't yet have a shell
                if (hole.Shell == null)
                {
                    var shell = hole.FindEdgeRingContaining(shellList);
                    // only when building a polygon-valid result
                    if (_isEnforcePolygonal && shell == null)
                    {
                        throw new TopologyException("unable to assign free hole to a shell", hole.Coordinate);
                    }
                    hole.Shell = shell;
                }
            }
        }

    }
}
