using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.GeometriesGraph.Index;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    ///     Nodes a set of edges.
    ///     Takes one or more sets of edges and constructs a
    ///     new set of edges consisting of all the split edges created by
    ///     noding the input edges together.
    /// </summary>
    public class EdgeSetNoder
    {
        private readonly List<Edge> _inputEdges = new List<Edge>();
        private readonly LineIntersector _li;

        /// <summary>
        /// </summary>
        /// <param name="li"></param>
        public EdgeSetNoder(LineIntersector li)
        {
            _li = li;
        }

        /// <summary>
        /// </summary>
        public IList<Edge> NodedEdges
        {
            get
            {
                EdgeSetIntersector esi = new SimpleMCSweepLineIntersector();
                var si = new SegmentIntersector(_li, true, false);
                esi.ComputeIntersections(_inputEdges, si, true);

                IList<Edge> splitEdges = new List<Edge>();
                foreach (var e in _inputEdges)
                    e.EdgeIntersectionList.AddSplitEdges(splitEdges);
                return splitEdges;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="edges"></param>
        public void AddEdges(IEnumerable<Edge> edges)
        {
            foreach (var obj in edges)
                _inputEdges.Add(obj);
        }
    }
}