using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.EdgeGraph
{
    /// <summary>
    /// Builds an edge graph from geometries containing edges.
    /// </summary>
    public class EdgeGraphBuilder
    {
        public static EdgeGraph Build(IEnumerable<Geometry> geoms)
        {
            var builder = new EdgeGraphBuilder();
            builder.Add(geoms);
            return builder.GetGraph();
        }

        private readonly EdgeGraph graph = new EdgeGraph();

        public EdgeGraphBuilder() { }

        public EdgeGraph GetGraph()
        {
            return graph;
        }

        /// <summary>
        /// Adds the edges of a Geometry to the graph.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent edges are extracted.
        /// </summary>
        /// <param name="geometry">geometry to be added</param>
        public void Add(Geometry geometry)
        {
            geometry.Apply(new GeometryComponentFilter(c =>
            {
                if (c is LineString)
                    Add(c as LineString);
            }));
        }

        /// <summary>
        ///  Adds the edges in a collection of <see cref="Geometry"/>s to the graph.
        /// May be called multiple times.
        /// Any dimension of <see cref="Geometry"/> may be added.
        /// </summary>
        /// <param name="geometries">the geometries to be added</param>
        public void Add(IEnumerable<Geometry> geometries)
        {
            foreach (var geometry in geometries)
                Add(geometry);
        }

        private void Add(LineString lineString)
        {
            var seq = lineString.CoordinateSequence;
            for (int i = 1; i < seq.Count; i++)
            {
                var prev = seq.GetCoordinate(i - 1);
                var curr = seq.GetCoordinate(i);
                graph.AddEdge(prev, curr);
            }
        }
    }
}
