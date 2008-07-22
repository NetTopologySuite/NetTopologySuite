using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using Iesi_NTS.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [Obsolete("use GraphBuilder2", false)]
    public class GraphBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public delegate double ComputeWeightDelegate(ILineString line);

        private static readonly ComputeWeightDelegate DefaultComputer =
            delegate(ILineString line) { return line.Length; };


        private IGeometryFactory factory;
        private readonly ISet<ILineString> strings;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder"/> class.
        /// </summary>
        public GraphBuilder()
        {
            factory = null;
            strings = new ListSet<ILineString>();            
        }

        /// <summary>
        /// Adds one or more lines to the graph.
        /// </summary>
        /// <param name="lines">A generic linestring.</param>
        /// <returns><c>true</c> if all elements are inserted, <c>false</c> otherwise</returns>
        public bool Add(params ILineString[] lines)
        {
            bool result = true;
            foreach (ILineString line in lines)
            {
                IGeometryFactory newfactory = line.Factory;
                if (factory == null)
                    factory = newfactory;
                else if (!newfactory.PrecisionModel.Equals(factory.PrecisionModel))
                    throw new TopologyException("all geometries must have the same precision model");

                if (result)
                    result = strings.Add(line);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Initializes the builder using a function 
        /// that computes the weights using <see cref="ILineString">edge</see>'s length.
        /// </remarks>        
        /// <returns></returns>
        public DijkstraShortestPathAlgorithm<IPoint, IEdge<IPoint>> PrepareAlgorithm()
        {
            return PrepareAlgorithm(DefaultComputer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="computer">
        /// A function that computes the weight 
        /// of any <see cref="ILineString">edge</see> of the graph
        /// </param>
        /// <returns></returns>
        public DijkstraShortestPathAlgorithm<IPoint, IEdge<IPoint>> PrepareAlgorithm(ComputeWeightDelegate computer)
        {
            if (strings.Count < 2)
                throw new TopologyException("you must specify two or more geometries to build a graph");

            IMultiLineString edges = BuildEdges();

            Dictionary<IEdge<IPoint>, double> consts = new Dictionary<IEdge<IPoint>, double>(edges.NumGeometries);
            AdjacencyGraph<IPoint, IEdge<IPoint>> graph = new AdjacencyGraph<IPoint, IEdge<IPoint>>(true);
            foreach (ILineString str in edges.Geometries)
            {
                IPoint vertex1 = str.StartPoint;
                Assert.IsTrue(vertex1 != null);
                if (!graph.ContainsVertex(vertex1))
                    graph.AddVertex(vertex1);

                IPoint vertex2 = str.EndPoint;
                Assert.IsTrue(vertex2 != null);
                if (!graph.ContainsVertex(vertex2))
                    graph.AddVertex(vertex2);

                double weight = computer(str);                
                Edge<IPoint> edge = new Edge<IPoint>(vertex1, vertex2);
                Assert.IsTrue(edge != null);

                graph.AddEdge(edge);
                consts.Add(edge, weight);
            }

            // Use Dijkstra
            return new DijkstraShortestPathAlgorithm<IPoint, IEdge<IPoint>>(graph, consts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IMultiLineString BuildEdges()
        {
            IGeometry temp = null;
            foreach (ILineString line in strings)
            {
                if (temp == null)
                    temp = line;
                else temp = temp.Union(line);
            }

            IMultiLineString edges;
            if (temp == null || temp.NumGeometries == 0)
                edges = MultiLineString.Empty;
            else if (temp.NumGeometries == 1)
                edges =  factory.CreateMultiLineString(new ILineString[] { (ILineString) temp, });
            else edges = (IMultiLineString) temp;
            return edges;
        }
    }
}
