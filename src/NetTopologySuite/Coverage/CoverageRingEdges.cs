using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Models a polygonal coverage as a set of unique <see cref="CoverageEdge"/>s,
    /// linked to the parent rings in the coverage polygons.
    /// Each edge has either one or two parent rings, depending on whether
    /// it is an inner or outer edge of the coverage.
    /// The source coverage is represented as a array of polygonal geometries
    /// (either <see cref="Polygon"/>s or <see cref="MultiPolygon"/>s).
    /// </summary>
    /// <author>Martin Davis</author>
    class CoverageRingEdges
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
            this._coverage = coverage;
            _ringEdgesMap = new Dictionary<LinearRing, List<CoverageEdge>>();
            _edges = new List<CoverageEdge>();
            Build();
        }

        public IList<CoverageEdge> Edges => _edges;

        /**
         * Selects the edges with a given ring count (which can be 1 or 2).
         * 
         * @param ringCount the edge ring count to select (1 or 2)
         * @return the selected edges
         */
        public IList<CoverageEdge> SelectEdges(int ringCount)
        {
            var result = new List<CoverageEdge>();
            foreach (var edge in _edges)
            {
                if (edge.RingCount == ringCount)
                {
                    result.Add(edge);
                }
            }
            return result;
        }

        private void Build()
        {
            var nodes = FindNodes(_coverage);
            var boundarySegs = CoverageBoundarySegmentFinder.FindBoundarySegments(_coverage);
            foreach(var node in FindBoundaryNodes(boundarySegs))
                nodes.Add(node);

            var uniqueEdgeMap = new Dictionary<LineSegment, CoverageEdge>();
            foreach (var geom in _coverage)
            {
                for (int ipoly = 0; ipoly < geom.NumGeometries; ipoly++)
                {
                    var poly = (Polygon)geom.GetGeometryN(ipoly);
                    //-- extract shell
                    var shell = (LinearRing)poly.ExteriorRing;
                    AddRingEdges(shell, nodes, boundarySegs, uniqueEdgeMap);
                    //-- extract holes
                    for (int ihole = 0; ihole < poly.NumInteriorRings; ihole++)
                    {
                        var hole = (LinearRing)poly.GetInteriorRingN(ihole);
                        AddRingEdges(hole, nodes, boundarySegs, uniqueEdgeMap);
                    }
                }
            }
        }

        private void AddRingEdges(LinearRing ring, HashSet<Coordinate> nodes, ISet<LineSegment> boundarySegs,
            Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap)
        {
            AddBoundaryNodes(ring, boundarySegs, nodes);
            var ringEdges = ExtractRingEdges(ring, uniqueEdgeMap, nodes);
            _ringEdgesMap[ring] = ringEdges;
        }

        private void AddBoundaryNodes(LinearRing ring, ISet<LineSegment> boundarySegs, HashSet<Coordinate> nodes)
        {
            var seq = ring.CoordinateSequence;
            bool isBdyLast = IsBoundarySegment(seq, seq.Count - 2, boundarySegs);
            bool isBdyPrev = isBdyLast;
            for (int i = 0; i < seq.Count - 1; i++)
            {
                bool isBdy = IsBoundarySegment(seq, i, boundarySegs);
                if (isBdy != isBdyPrev)
                {
                    var nodePt = seq.GetCoordinate(i);
                    nodes.Add(nodePt);
                }
                isBdyPrev = isBdy;
            }
        }

        private bool IsBoundarySegment(CoordinateSequence seq, int i, ISet<LineSegment> boundarySegs)
        {
            var seg = CoverageBoundarySegmentFinder.CreateSegment(seq, i);
            return boundarySegs.Contains(seg);
        }

        private List<CoverageEdge> ExtractRingEdges(LinearRing ring,
            Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap,
            HashSet<Coordinate> nodes)
        {
            var ringEdges = new List<CoverageEdge>();
            int first = FindNextNodeIndex(ring, -1, nodes);
            if (first < 0)
            {
                //-- ring does not contain a node, so edge is entire ring
                var edge = CreateEdge(ring, uniqueEdgeMap);
                ringEdges.Add(edge);
            }
            else
            {
                int start = first;
                int end = start;
                do
                {
                    end = FindNextNodeIndex(ring, start, nodes);
                    var edge = CreateEdge(ring, start, end, uniqueEdgeMap);
                    ringEdges.Add(edge);
                    start = end;
                } while (end != first);
            }
            return ringEdges;
        }

        private CoverageEdge CreateEdge(LinearRing ring, Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap)
        {
            var edgeKey = CoverageEdge.Key(ring);
            if (!uniqueEdgeMap.TryGetValue(edgeKey, out var edge))
            {
                edge = CoverageEdge.CreateEdge(ring);
                uniqueEdgeMap[edgeKey] = edge;
                _edges.Add(edge);
            }
            edge.IncrementRingCount();
            return edge;
        }

        private CoverageEdge CreateEdge(LinearRing ring, int start, int end, Dictionary<LineSegment, CoverageEdge> uniqueEdgeMap)
        {
            var edgeKey = end == start ? CoverageEdge.Key(ring) : CoverageEdge.Key(ring, start, end);
            if (!uniqueEdgeMap.TryGetValue(edgeKey, out var edge))
            {
                edge = CoverageEdge.CreateEdge(ring, start, end);
                uniqueEdgeMap[edgeKey] = edge;
                _edges.Add(edge);
            }
            edge.IncrementRingCount();
            return edge;
        }

        private int FindNextNodeIndex(LinearRing ring, int start, HashSet<Coordinate> nodes)
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
                var pt = ring.GetCoordinateN(index);
                if (nodes.Contains(pt))
                {
                    return index;
                }
            } while (index != start);
            return -1;
        }

        private static int Next(int index, LinearRing ring)
        {
            index++;
            if (index >= ring.NumPoints - 1)
                index = 0;
            return index;
        }

        private HashSet<Coordinate> FindNodes(Geometry[] coverage)
        {
            var vertexCount = VertexCounter.Count(coverage);
            var nodes = new HashSet<Coordinate>();
            foreach (var v in vertexCount.Keys)
            {
                if (vertexCount[v] > 2)
                {
                    nodes.Add(v);
                }
            }
            return nodes;
        }


        private IEnumerable<Coordinate> FindBoundaryNodes(ISet<LineSegment> lineSegments)
        {
            var counter = new Dictionary<Coordinate, int>();
            foreach (var line in lineSegments)
            {
                if (!counter.TryGetValue(line.P0, out int count)) count = 0;
                counter[line.P0] = count + 1;
                if (!counter.TryGetValue(line.P1, out count)) count = 0;
                counter[line.P1] = count + 1;
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
            var polys = new Polygon[geom.NumGeometries];
            for (int i = 0; i < polys.Length; i++)
            {
                polys[i] = BuildPolygon((Polygon)geom.GetGeometryN(i));
            }
            return geom.Factory.CreateMultiPolygon(polys);
        }

        private Polygon BuildPolygon(Polygon polygon)
        {
            var shell = BuildRing((LinearRing)polygon.ExteriorRing);
            if (polygon.NumInteriorRings == 0)
            {
                return polygon.Factory.CreatePolygon(shell);
            }
            var holes = new LinearRing[polygon.NumInteriorRings];
            for (int i = 0; i < holes.Length; i++)
            {
                var hole = (LinearRing)polygon.GetInteriorRingN(i);
                holes[i] = BuildRing(hole);
            }
            return polygon.Factory.CreatePolygon(shell, holes);
        }

        private LinearRing BuildRing(LinearRing ring)
        {
            var ringEdges = _ringEdgesMap[ring];
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
