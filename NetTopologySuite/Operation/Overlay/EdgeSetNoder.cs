using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Nodes a set of edges.
    /// Takes one or more sets of edges and constructs a
    /// new set of edges consisting of all the split edges created by
    /// noding the input edges together.
    /// </summary>
    public class EdgeSetNoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly List<Edge<TCoordinate>> _inputEdges = new List<Edge<TCoordinate>>();
        private readonly LineIntersector<TCoordinate> _li;

        public EdgeSetNoder(LineIntersector<TCoordinate> li)
        {
            _li = li;
        }

        public IEnumerable<Edge<TCoordinate>> NodedEdges
        {
            get
            {
                EdgeSetIntersector<TCoordinate> esi
                    = new SimpleMonotoneChainSweepLineIntersector<TCoordinate>();
                SegmentIntersector<TCoordinate> si
                    = new SegmentIntersector<TCoordinate>(_li, true, false);

                esi.ComputeIntersections(_inputEdges, si, true);

                foreach (Edge<TCoordinate> edge in _inputEdges)
                {
                    IEnumerable<Edge<TCoordinate>> splitEdges = edge.EdgeIntersections.GetSplitEdges();

                    foreach (Edge<TCoordinate> splitEdge in splitEdges)
                    {
                        yield return splitEdge;
                    }
                }
            }
        }

        public void AddEdges(IEnumerable<Edge<TCoordinate>> edges)
        {
            _inputEdges.AddRange(edges);
        }
    }
}