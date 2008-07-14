using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap;
using NUnit.Framework;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class NtsGraphTest
    {        
        private IGeometryFactory factory;
        private ILineString a, b, c, d;
        private IGeometry start;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            factory = GeometryFactory.Fixed;

            // Build sample geometries
            a = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(200, 100),
                new Coordinate(200, 200),
            });
            b = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 100),
                new Coordinate(200, 200),
            });
            c = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 100),
                new Coordinate(100, 200),
                new Coordinate(200, 200),
            });
            d = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(300, 0),
                new Coordinate(300, 200),
                new Coordinate(150, 200),
                new Coordinate(150, 300),
            });
            start = a.StartPoint;
        }

        [Test]
        public void BuildGraphAndSearchShortestPathUsingGeometryUnion()
        {            
            IGeometry edges = a.Union(b).Union(c).Union(d);
            Assert.IsNotNull(edges);            
            Assert.IsTrue(edges.GetType() == typeof(MultiLineString));
            Assert.Greater(edges.NumGeometries, 0);
            foreach (IGeometry edge in ((GeometryCollection) edges).Geometries)
            {
                Assert.IsNotNull(edge);
                Assert.IsTrue(edge.GetType() == typeof(LineString));
                Debug.WriteLine(edge);
            }

            // Build graph
            Dictionary<Edge<IGeometry>, double> consts = new Dictionary<Edge<IGeometry>, double>(edges.NumGeometries);
            AdjacencyGraph<IGeometry, Edge<IGeometry>> graph = new AdjacencyGraph<IGeometry, Edge<IGeometry>>(true);
            foreach (ILineString str in ((GeometryCollection) edges).Geometries)
            {               
                // Add vertex 1
                IGeometry vertex1 = str.StartPoint;
                Assert.IsNotNull(vertex1);
                if (!graph.ContainsVertex(vertex1))
                {
                    Debug.WriteLine(String.Format("Adding vertex {0} to the list", vertex1));
                    graph.AddVertex(vertex1);
                }
                else Debug.WriteLine(String.Format("Vertex {0} already present", vertex1));

                // Add vertex 2
                IGeometry vertex2 = str.EndPoint;
                Assert.IsNotNull(vertex2);
                if (!graph.ContainsVertex(vertex2))
                {
                    Debug.WriteLine(String.Format("Adding vertex {0} to the list", vertex2));
                    graph.AddVertex(vertex2);
                }
                else Debug.WriteLine(String.Format("Vertex {0} already present", vertex2));

                // Compute weight
                double weight = str.Length;
                Assert.Greater(weight, 0.0);
                Edge<IGeometry> edge = new Edge<IGeometry>(vertex1, vertex2);
                Assert.IsNotNull(edge);

                // Add edge
                Debug.WriteLine(String.Format("Adding edge for vertices {0} and {1} using weight {2}", 
                    vertex1, vertex2, weight));
                graph.AddEdge(edge);
                consts.Add(edge, weight);
            }

            // Perform DijkstraShortestPathAlgorithm
            DijkstraShortestPathAlgorithm<IGeometry, Edge<IGeometry>> dijkstra =
                new DijkstraShortestPathAlgorithm<IGeometry, Edge<IGeometry>>(graph, consts);

            // attach a distance observer to give us the shortest path distances
            VertexDistanceRecorderObserver<IGeometry, Edge<IGeometry>> distObserver =
                new VertexDistanceRecorderObserver<IGeometry, Edge<IGeometry>>();
            distObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            VertexPredecessorRecorderObserver<IGeometry, Edge<IGeometry>> predecessorObserver =
                new VertexPredecessorRecorderObserver<IGeometry, Edge<IGeometry>>();
            predecessorObserver.Attach(dijkstra);

            // Run the algorithm             
            Debug.WriteLine(String.Format("Starting algorithm from root vertex {0}", this.start));
            dijkstra.Compute(this.start);

            foreach (KeyValuePair<IGeometry, int> kvp in distObserver.Distances)
                Debug.WriteLine(String.Format("Distance from root to node {0} is {1}", 
                    kvp.Key, kvp.Value));
            foreach (KeyValuePair<IGeometry, Edge<IGeometry>> kvp in predecessorObserver.VertexPredecessors)
                Debug.WriteLine(String.Format(
                    "If you want to get to {0} you have to enter through the IN edge {1}", kvp.Key, kvp.Value));
            
            // Detach the observers
            distObserver.Detach(dijkstra);
            predecessorObserver.Detach(dijkstra);
        }        
    }
}
