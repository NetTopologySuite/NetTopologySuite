using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.EdgeGraph
{
    /// <summary>
    /// Builds an edge graph from geometries containing edges.
    /// </summary>
    public class EdgeGraphBuilder
    {
        public static EdgeGraph Build(IEnumerable<IGeometry> geoms)
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
        public void Add(IGeometry geometry)
        {
            geometry.Apply(new GeometryComponentFilter(c =>
            {
                if (c is ILineString)
                    Add(c as ILineString);
            }));
        }

        /// <summary>
        ///  Adds the edges in a collection of <see cref="IGeometry"/>s to the graph.
        /// May be called multiple times.
        /// Any dimension of <see cref="IGeometry"/> may be added.
        /// </summary>
        /// <param name="geometries">the geometries to be added</param>
        public void Add(IEnumerable<IGeometry> geometries)
        {
            foreach (var geometry in geometries)
                Add(geometry);
        }

        private void Add(ILineString lineString)
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
