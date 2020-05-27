using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Performs merging on the noded edges of the input geometries.
     * Merging takes place on edges which are coincident 
     * (i.e. have the same coordinate list, modulo direction).
     * The following situations can occur:
     * <ul>
     * <li>Coincident edges from different input geometries have their labels combined
     * <li>Coincident edges from the same area geometry indicate a topology collapse.
     * In this case the topology locations are "summed" to provide a final
     * assignment of side location
     * <li>Coincident edges from the same linear geometry can simply be merged 
     * using the same ON location
     * </ul>
     * 
     * One constraint that is maintained is that the direction of linear
     * edges should be preserved if possible (which is the case if there is 
     * no other coincident edge, or if all coincident edges have the same direction).
     * This ensures that the overlay output line direction will be as consistent
     * as possible with input lines.
     * 
     * @author mdavis
     *
     */
    class EdgeMerger
    {

        public static IList<Edge> merge(List<Edge> edges)
        {
            var merger = new EdgeMerger(edges);
            return merger.merge();
        }

        private readonly ICollection<Edge> _edges;
        private readonly IDictionary<EdgeKey, Edge> _edgeMap = new Dictionary<EdgeKey, Edge>();

        public EdgeMerger(ICollection<Edge> edges)
        {
            _edges = edges;
        }

        public IList<Edge> merge()
        {
            foreach (var edge in _edges)
            {
                var edgeKey = EdgeKey.Create(edge);
                if (!_edgeMap.TryGetValue(edgeKey, out var baseEdge))
                {
                    // this is the first (and maybe only) edge for this line
                    _edgeMap.Add(edgeKey, edge);
                    //Debug.println("edge added: " + edge);
                    //Debug.println(edge.toLineString());
                }
                else
                {
                    // found an existing edge

                    // Assert: edges are identical (up to direction)
                    // this is a fast (but incomplete) sanity check
                    Assert.IsTrue(baseEdge.Count == edge.Count,
                        "Merge of edges of different sizes - probable noding error.");

                    baseEdge.merge(edge);
                    //Debug.println("edge merged: " + existing);
                    //Debug.println(edge.toLineString());
                }
            }
            return new List<Edge>(_edgeMap.Values);
        }

    }
}
