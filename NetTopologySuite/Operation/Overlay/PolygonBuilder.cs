using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

#if DOTNET35
using sl = System.Linq;
#endif

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Forms <see cref="IPolygon{TCoordinate}" />s out of a graph of 
    /// <see cref="DirectedEdge{TCoordinate}"/>s.
    /// The edges to use are marked as being in the result Area.
    /// </summary>
    public class PolygonBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;
        private readonly List<EdgeRing<TCoordinate>> _shellList = new List<EdgeRing<TCoordinate>>();

        public PolygonBuilder(IGeometryFactory<TCoordinate> geometryFactory)
        {
            _geometryFactory = geometryFactory;
        }

        public IEnumerable<IPolygon<TCoordinate>> Polygons
        {
            get { return computePolygons(_shellList); }
        }

        /// <summary>
        /// Add a complete graph.
        /// The graph is assumed to contain one or more polygons,
        /// possibly with holes.
        /// </summary>
        public void Add(PlanarGraph<TCoordinate> graph)
        {
            Add(graph.EdgeEnds, graph.Nodes);
        }

        /// <summary> 
        /// Add a set of edges and nodes, which form a graph.
        /// The graph is assumed to contain one or more polygons,
        /// possibly with holes.
        /// </summary>
        public void Add(IEnumerable<EdgeEnd<TCoordinate>> dirEdges, IEnumerable<Node<TCoordinate>> nodes)
        {
            PlanarGraph<TCoordinate>.LinkResultDirectedEdges(nodes);
            IEnumerable<MaximalEdgeRing<TCoordinate>> maxEdgeRings = buildMaximalEdgeRings(dirEdges);
            //= sl.Enumerable.ToArray(buildMaximalEdgeRings(dirEdges));
            List<EdgeRing<TCoordinate>> freeHoleList = new List<EdgeRing<TCoordinate>>();
            IEnumerable<EdgeRing<TCoordinate>> edgeRings = buildMinimalEdgeRings(maxEdgeRings,
                                                                                 _shellList,
                                                                                 freeHoleList);
            //= sl.Enumerable.ToArray(buildMinimalEdgeRings(maxEdgeRings, _shellList, freeHoleList));
            sortShellsAndHoles(edgeRings, _shellList, freeHoleList);
            placeFreeHoles(_shellList, freeHoleList);
            //Assert: every hole on freeHoleList has a shell assigned to it
        }

        /// <summary> 
        /// Checks the current set of shells (with their associated holes) to
        /// see if any of them contain the point.
        /// </summary>
        public Boolean ContainsPoint(TCoordinate p)
        {
            foreach (EdgeRing<TCoordinate> ring in _shellList)
            {
                if (ring.ContainsPoint(p))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 
        /// For all <see cref="DirectedEdge{TCoordinate}"/>s in result, 
        /// form them into MaximalEdgeRings.
        /// </summary>
        private IEnumerable<MaximalEdgeRing<TCoordinate>> buildMaximalEdgeRings(
            IEnumerable<EdgeEnd<TCoordinate>> dirEdges)
        {
            List<MaximalEdgeRing<TCoordinate>> ret = new List<MaximalEdgeRing<TCoordinate>>();
            foreach (DirectedEdge<TCoordinate> edge in dirEdges)
            {
                if (edge.IsInResult && edge.Label.Value.IsArea())
                {
                    // if this edge has not yet been processed
                    if (edge.EdgeRing == null)
                    {
                        MaximalEdgeRing<TCoordinate> er = new MaximalEdgeRing<TCoordinate>(edge,
                                                                                           _geometryFactory);
                        er.SetInResult();
                        ret.Add(er);
                        //yield return er;
                    }
                }
            }
            return ret;
        }

        private static IEnumerable<EdgeRing<TCoordinate>> buildMinimalEdgeRings(
            IEnumerable<MaximalEdgeRing<TCoordinate>> maxEdgeRings,
            ICollection<EdgeRing<TCoordinate>> shellList,
            ICollection<EdgeRing<TCoordinate>> freeHoleList)
        {
            List<EdgeRing<TCoordinate>> ret = new List<EdgeRing<TCoordinate>>();
            foreach (MaximalEdgeRing<TCoordinate> er in maxEdgeRings)
            {
                if (er.MaxNodeDegree > 2)
                {
                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    IList<MinimalEdgeRing<TCoordinate>> minEdgeRings = er.BuildMinimalRings();

                    // at this point we can go ahead and attempt to place holes, if this EdgeRing is a polygon
                    EdgeRing<TCoordinate> shell = findShell(minEdgeRings);

                    if (shell != null)
                    {
                        placePolygonHoles(shell, minEdgeRings);
                        shellList.Add(shell);
                    }
                    else
                    {
                        IEnumerable<EdgeRing<TCoordinate>> holes =
                            Caster.Upcast<EdgeRing<TCoordinate>, MinimalEdgeRing<TCoordinate>>(minEdgeRings);

                        foreach (EdgeRing<TCoordinate> hole in holes)
                        {
                            freeHoleList.Add(hole);
                        }
                    }
                }
                else
                {
                    ret.Add(er);
                    //yield return er;
                }
            }
            return ret;
        }

        /// <summary>
        /// This method takes a list of MinimalEdgeRings derived from a MaximalEdgeRing,
        /// and tests whether they form a Polygon.  This is the case if there is a single shell
        /// in the list.  In this case the shell is returned.
        /// The other possibility is that they are a series of connected holes, in which case
        /// no shell is returned.
        /// </summary>
        /// <returns>The shell EdgeRing, if there is one.</returns>
        /// <returns><see langword="null" />, if all the rings are holes.</returns>
        private static EdgeRing<TCoordinate> findShell(IEnumerable<MinimalEdgeRing<TCoordinate>> minEdgeRings)
        {
            Int32 shellCount = 0;
            EdgeRing<TCoordinate> shell = null;

            foreach (MinimalEdgeRing<TCoordinate> ring in minEdgeRings)
            {
                if (!ring.IsHole)
                {
                    shell = ring;
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
        private static void placePolygonHoles(EdgeRing<TCoordinate> shell,
                                              IEnumerable<MinimalEdgeRing<TCoordinate>> minEdgeRings)
        {
            foreach (MinimalEdgeRing<TCoordinate> ring in minEdgeRings)
            {
                if (ring.IsHole)
                {
                    ring.Shell = shell;
                }
            }
        }

        /// <summary> 
        /// For all rings in the input list,
        /// determine whether the ring is a shell or a hole
        /// and add it to the appropriate list.
        /// Due to the way the DirectedEdges were linked,
        /// a ring is a shell if it is oriented CW, a hole otherwise.
        /// </summary>
        private static void sortShellsAndHoles(IEnumerable<EdgeRing<TCoordinate>> edgeRings,
                                               ICollection<EdgeRing<TCoordinate>> shellList,
                                               ICollection<EdgeRing<TCoordinate>> freeHoleList)
        {
            foreach (EdgeRing<TCoordinate> edgeRing in edgeRings)
            {
                edgeRing.SetInResult();

                if (edgeRing.IsHole)
                {
                    freeHoleList.Add(edgeRing);
                }
                else
                {
                    shellList.Add(edgeRing);
                }
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
        private static void placeFreeHoles(IEnumerable<EdgeRing<TCoordinate>> shellList,
                                           IEnumerable<EdgeRing<TCoordinate>> freeHoleList)
        {
            foreach (EdgeRing<TCoordinate> hole in freeHoleList)
            {
                // only place this hole if it doesn't yet have a shell
                if (hole.Shell == null)
                {
                    EdgeRing<TCoordinate> shell = findEdgeRingContaining(hole, shellList);
                    Assert.IsTrue(shell != null, "unable to assign hole to a shell");
                    hole.Shell = shell;
                }
            }
        }

        // Find the innermost enclosing shell EdgeRing containing the argument 
        // EdgeRing, if any.
        // The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        // The algorithm used depends on the fact that:
        // ring A contains ring B iff envelope(ring A) contains envelope(ring B).
        // This routine is only safe to use if the chosen point of the hole
        // is known to be properly contained in a shell
        // (which is guaranteed to be the case if the hole does not touch its shell).
        // Returns containing EdgeRing, if there is one, OR null if no containing EdgeRing is found.
        private static EdgeRing<TCoordinate> findEdgeRingContaining(EdgeRing<TCoordinate> testEdgeRing,
                                                                    IEnumerable<EdgeRing<TCoordinate>> shellList)
        {
            ILinearRing<TCoordinate> teString = testEdgeRing.LinearRing;
            IExtents<TCoordinate> testEnv = teString.Extents;
            TCoordinate testPt = teString.Coordinates[0];

            EdgeRing<TCoordinate> minShell = null;
            IExtents minEnv = null;

            foreach (EdgeRing<TCoordinate> tryShell in shellList)
            {
                ILinearRing<TCoordinate> tryRing = tryShell.LinearRing;
                IExtents<TCoordinate> tryEnv = tryRing.Extents;

                if (minShell != null)
                {
                    minEnv = minShell.LinearRing.Extents;
                }

                Boolean isContained = false;

                if (tryEnv.Contains(testEnv) &&
                    CGAlgorithms<TCoordinate>.IsPointInRing(testPt, tryRing.Coordinates))
                {
                    isContained = true;
                }

                // check if this new containing ring is smaller than the current minimum ring
                if (isContained &&
                    (minShell == null || minEnv.Contains(tryEnv)))
                {
                    minShell = tryShell;
                }
            }

            return minShell;
        }

        private IEnumerable<IPolygon<TCoordinate>> computePolygons(IEnumerable<EdgeRing<TCoordinate>> shellList)
        {
            foreach (EdgeRing<TCoordinate> ring in shellList)
            {
                yield return ring.ToPolygon(_geometryFactory);
            }
        }
    }
}