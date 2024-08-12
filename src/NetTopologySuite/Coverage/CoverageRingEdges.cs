using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Models a polygonal coverage as a set of unique <see cref="CoverageEdge"/>s,
    /// linked to the parent rings in the coverage polygons.
    /// Each edge has either one or two parent rings, depending on whether
    /// it is an inner or outer edge of the coverage.
    /// The source coverage is represented as a array of polygonal geometries
    /// (either <see cref="Polygon"/>s or <see cref="MultiPolygon"/>s).
    ///  <para/>
    ///  Coverage edges are found by identifying vertices which are nodes in the coverage,
    ///  splitting edges at nodes, and then identifying unique edges.
    ///  The unique edges are associated to their parent ring(in order),
    ///  to allow reforming the coverage polygons.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class CoverageRingEdges
    {
        /// <summary>
        /// Create a new instance for a given coverage.
        /// </summary>
        /// <param name="coverage">The set of polygonal geometries in the coverage</param>
        /// <returns>The edges of the coverage</returns>
        public static CoverageRingEdges Create(Geometry[] coverage)
        {
            var edges = new CoverageRingEdges(coverage);
            return edges;
        }

        private readonly Geometry[] _coverage;
        private readonly Dictionary<LinearRing, List<CoverageEdge>> _ringEdgesMap;
        private readonly List<CoverageEdge> _edges;

        public CoverageRingEdges(Geometry[] coverage)
        {
            _coverage = coverage;
            _ringEdgesMap = new Dictionary<LinearRing, List<CoverageEdge>>();
            _edges = new List<CoverageEdge>();
            Build();
        }

        public IList<CoverageEdge> Edges => _edges;

        private void Build()
        {
            var nodes = FindMultiRingNodes(_coverage);
            var boundarySegs = CoverageBoundarySegmentFinder.FindBoundarySegments(_coverage);
            foreach(var node in FindBoundaryNodes(boundarySegs))
                nodes.Add(node);

            var uniqueEdgeMap = new Dictionary<LineSegment, CoverageEdge>();
            for (int i = 0; i < _coverage.Length; i++)
            {
                //-- geom is a Polygon or MultiPolygon
                var geom = _coverage[i];
                int indexLargest = FindLargestPolygonIndex(geom);
                for (int ipoly = 0; ipoly < geom.NumGeometries; ipoly++)
                {
                    var poly = (Polygon)geom.GetGeometryN(ipoly);

                    //-- skip empty elements. Missing elements are copied in result
                    if (poly.IsEmpty)
                        continue;

                    //-- largest polygon is the primary one, which is never removed
                    bool isPrimary = ipoly == indexLargest;

                    //-- extract shell
                    var shell = (LinearRing)poly.ExteriorRing;
                    AddRingEdges(i, shell, isPrimary, nodes, boundarySegs, uniqueEdgeMap);
                    //-- extract holes
                    for (int ihole = 0; ihole < poly.NumInteriorRings; ihole++)
                    {
                        var hole = (LinearRing)poly.GetInteriorRingN(ihole);
                        //-- skip empty holes. Missing rings are copied in result
                        if (hole.IsEmpty)
                            continue;
                        //-- holes are never primary
                        AddRingEdges(i, hole, false, nodes, boundarySegs, uniqueEdgeMap);
                    }
                }
            }
        }

        private int FindLargestPolygonIndex(Geometry geom)
        {
            if (geom is Polygon)
                return 0;

            int indexLargest = -1;
            double areaLargest = -1;
            for (int ipoly = 0; ipoly < geom.NumGeometries; ipoly++)
            {
                var poly = (Polygon)geom.GetGeometryN(ipoly);
                double area = poly.Area;
                if (area > areaLargest)
                {
                    areaLargest = area;
                    indexLargest = ipoly;
                }
            }
            return indexLargest;
        }

        private void AddRingEdges(int index, LinearRing ring, bool isPrimary, ISet<Coordinate> nodes, ISet<LineSegment> boundarySegs,
            Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap)
        {
            AddBoundaryInnerNodes(ring, boundarySegs, nodes);
            var ringEdges = ExtractRingEdges(index, ring, isPrimary, uniqueEdgeMap, nodes);
            if (ringEdges != null)
                _ringEdgesMap[ring] = ringEdges;
        }

        /// <summary>
        /// Detects nodes occurring at vertices which are between a boundary segment
        /// and an inner (shared) segment.
        /// These occur where two polygons are adjacent at the coverage boundary
        /// (this is not detected by <see cref="FindMultiRingNodes(Geometry[])"/>.
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="boundarySegs"></param>
        /// <param name="nodes"></param>
        private void AddBoundaryInnerNodes(LinearRing ring, ISet<LineSegment> boundarySegs, ISet<Coordinate> nodes)
        {
            var seq = ring.CoordinateSequence;
            bool isBdyLast = CoverageBoundarySegmentFinder.IsBoundarySegment(boundarySegs, seq, seq.Count - 2);
            bool isBdyPrev = isBdyLast;
            for (int i = 0; i < seq.Count - 1; i++)
            {
                bool isBdy = CoverageBoundarySegmentFinder.IsBoundarySegment(boundarySegs, seq, i);
                if (isBdy != isBdyPrev)
                {
                    var nodePt = seq.GetCoordinate(i);
                    nodes.Add(nodePt);
                }
                isBdyPrev = isBdy;
            }
        }

        /// <summary>
        /// Extracts the <see cref="CoverageEdge"/>s for a ring.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ring"></param>
        /// <param name="isPrimary"></param>
        /// <param name="uniqueEdgeMap"></param>
        /// <param name="nodes"></param>
        /// <returns><c>null</c> if the ring has too few distinct vertices</returns>
        private List<CoverageEdge> ExtractRingEdges(int index, LinearRing ring,
            bool isPrimary, Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap,
            ISet<Coordinate> nodes)
        {
            var ringEdges = new List<CoverageEdge>();
            var pts = ring.Coordinates;
            pts = CoordinateArrays.RemoveRepeatedPoints(pts);
            //-- if compacted ring is too short, don't process it
            if (pts.Length < 3)
                return null;

            int first = FindNextNodeIndex(pts, -1, nodes);
            if (first < 0)
            {
                //-- ring does not contain a node, so edge is entire ring
                var edge = CreateEdge(pts, -1, -1, index, isPrimary, uniqueEdgeMap);
                ringEdges.Add(edge);
            }
            else
            {
                int start = first;
                int end = start;
                //-- two-node edges are always primary
                bool isEdgePrimary = true;
                do
                {
                    end = FindNextNodeIndex(pts, start, nodes);
                    //-- a single-node ring is only retained if specified
                    if (end == start)
                        isEdgePrimary = isPrimary;

                    var edge = CreateEdge(pts, start, end, index, isEdgePrimary, uniqueEdgeMap);
                    ringEdges.Add(edge);
                    start = end;
                } while (end != first);
            }
            return ringEdges;
        }

        private CoverageEdge CreateEdge(Coordinate[] ring, int start, int end, int index, bool isPrimary, Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap)
        {
            var edgeKey = end == start ? CoverageEdge.Key(ring) : CoverageEdge.Key(ring, start, end);
            if (uniqueEdgeMap.TryGetValue(edgeKey, out var edge))
            {
                // update shared attributes
                edge.IsPrimary = isPrimary;
            }
            else
            {
                if (start < 0)
                    edge = CoverageEdge.CreateEdge(ring, isPrimary);
                else
                    edge = CoverageEdge.CreateEdge(ring, start, end, isPrimary);

                uniqueEdgeMap[edgeKey] = edge;
                _edges.Add(edge);
            }
            edge.AddIndex(index);
            edge.IncrementRingCount();
            return edge;
        }

        private int FindNextNodeIndex(Coordinate[] ring, int start, ISet<Coordinate> nodes)
        {
            int index = start;
            bool isScanned0 = false;
            do
            {
                index = Next(index, ring);
                if (index == 0)
                {
                    if (start < 0 && isScanned0)
                        return -1;
                    isScanned0 = true;
                }
                var pt = ring[index];
                if (nodes.Contains(pt))
                {
                    return index;
                }
            } while (index != start);
            return -1;
        }

        private static int Next(int index, Coordinate[] ring)
        {
            index++;
            if (index >= ring.Length - 1)
                index = 0;
            return index;
        }

        /// <summary>
        /// Finds nodes in a coverage at vertices which are shared by 3 or more rings.
        /// </summary>
        /// <param name="coverage">A list of polygonal geometries</param>
        /// <returns>The set of nodes contained in 3 or more rings</returns>
        private ISet<Coordinate> FindMultiRingNodes(Geometry[] coverage)
        {
            var vertexRingCount = VertexRingCounter.Count(coverage);
            var nodes = new HashSet<Coordinate>();
            foreach (var v in vertexRingCount)
            {
                if (v.Value >= 3)
                {
                    nodes.Add(v.Key);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Finds nodes occurring between boundary segments.
        /// Nodes on boundaries occur at vertices which have
        /// 3 or more incident boundary segments.
        /// This detects situations where two rings touch only at a vertex
        /// (i.e. two polygons touch, or a polygon shell touches a hole)
        /// These nodes lie in only 2 adjacent rings,
        /// so are not detected by <see cref="FindMultiRingNodes"/>{@link #findMultiRingNodes(Geometry[])}. 
        /// </summary>
        /// <param name="boundarySegments"></param>
        /// <returns>A set of vertices which are nodes where two rings touch</returns>
        private IEnumerable<Coordinate> FindBoundaryNodes(ISet<LineSegment> boundarySegments)
        {
            var counter = new Dictionary<Coordinate, int>();
            foreach (var seg in boundarySegments)
            {
                if (!counter.TryGetValue(seg.P0, out int count)) count = 0;
                counter[seg.P0] = count + 1;
                if (!counter.TryGetValue(seg.P1, out count)) count = 0;
                counter[seg.P1] = count + 1;
            }

            foreach (var kvp in counter)
                if (kvp.Value > 2) yield return kvp.Key;
        }

        /// <summary>
        /// Recreates the polygon coverage from the current edge values.
        /// </summary>
        /// <returns>An array of polygonal geometries representing the coverage</returns>
        public Geometry[] BuildCoverage()
        {
            var result = new Geometry[_coverage.Length];
            for (int i = 0; i < _coverage.Length; i++)
            {
                result[i] = BuildPolygonal(_coverage[i]);
            }
            return result;
        }

        private Geometry BuildPolygonal(Geometry geom)
        {
            if (geom is MultiPolygon mp) {
                return BuildMultiPolygon(mp);
            }
            else
            {
                return BuildPolygon((Polygon)geom);
            }
        }

        private Geometry BuildMultiPolygon(MultiPolygon geom)
        {
            var polyList = new List<Polygon>(geom.NumGeometries);
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var poly = BuildPolygon((Polygon)geom.GetGeometryN(i));
                if (poly != null)
                    polyList.Add(poly);
            }

            if (polyList.Count == 1)
                return polyList[0];

            var polys = polyList.ToArray();
            return geom.Factory.CreateMultiPolygon(polys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns><c>null</c> if the polygon has been removed</returns>
        private Polygon BuildPolygon(Polygon polygon)
        {
            var shell = BuildRing((LinearRing)polygon.ExteriorRing);
            if (shell == null)
                return null;

            if (polygon.NumInteriorRings == 0)
            {
                return polygon.Factory.CreatePolygon(shell);
            }
            var holeList = new List<LinearRing>(polygon.NumInteriorRings);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                var hole = (LinearRing)polygon.GetInteriorRingN(i);
                var newHole = BuildRing(hole);
                if (newHole != null)
                    holeList.Add(newHole);
            }
            var holes = holeList.ToArray();
            return polygon.Factory.CreatePolygon(shell, holes);
        }

        private LinearRing BuildRing(LinearRing ring)
        {
            //-- if ring is not in map, must have been invalid. Just copy original
            if (!_ringEdgesMap.TryGetValue(ring, out var ringEdges))
                return (LinearRing)ring.Copy();

            bool isRemoved = ringEdges.Count == 1
                && ringEdges[0].Coordinates.Length == 0;
            if (isRemoved)
                return null;

            var ptsList = new CoordinateList();
            for (int i = 0; i < ringEdges.Count; i++)
            {
                var lastPt = ptsList.Count > 0
                                      ? ptsList.GetCoordinate(ptsList.Count - 1)
                                      : null;
                bool dir = IsEdgeDirForward(ringEdges, i, lastPt);
                ptsList.Add(ringEdges[i].Coordinates, false, dir);
            }
            var pts = ptsList.ToCoordinateArray();
            return ring.Factory.CreateLinearRing(pts);
        }

        private bool IsEdgeDirForward(List<CoverageEdge> ringEdges, int index, Coordinate prevPt)
        {
            int size = ringEdges.Count;
            if (size <= 1) return true;
            if (index == 0)
            {
                //-- if only 2 edges, first one can keep orientation
                if (size == 2)
                    return true;
                var endPt0 = ringEdges[0].EndCoordinate;
                return endPt0.Equals2D(ringEdges[1].StartCoordinate)
                    || endPt0.Equals2D(ringEdges[1].EndCoordinate);
            }
            //-- previous point determines required orientation
            return prevPt.Equals2D(ringEdges[index].StartCoordinate);
        }

    }
}
