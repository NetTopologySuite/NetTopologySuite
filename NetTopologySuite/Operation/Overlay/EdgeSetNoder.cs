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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li = null;
        private readonly List<Edge<TCoordinate>> _inputEdges = new List<Edge<TCoordinate>>();

        public EdgeSetNoder(LineIntersector<TCoordinate> li)
        {
            _li = li;
        }

        public void AddEdges(IEnumerable<Edge<TCoordinate>> edges)
        {
            _inputEdges.AddRange(edges);
        }

        public IEnumerable<Edge<TCoordinate>> NodedEdges
        {
            get
            {
                EdgeSetIntersector<TCoordinate> esi = new SimpleMonotoneChaingSweepLineIntersector<TCoordinate>();
                SegmentIntersector<TCoordinate> si = new SegmentIntersector<TCoordinate>(_li, true, false);
                esi.ComputeIntersections(_inputEdges, si, true);

                foreach (Edge<TCoordinate> edge in _inputEdges)
                {
                    IEnumerable<Edge<TCoordinate>> splitEdges = edge.EdgeIntersectionList.GetSplitEdges();

                    foreach (Edge<TCoordinate> splitEdge in splitEdges)
                    {
                        yield return splitEdge;
                    }
                }
            }
        }
    }
}