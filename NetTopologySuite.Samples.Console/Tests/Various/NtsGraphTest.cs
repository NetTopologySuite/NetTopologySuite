using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Linemerge;
using NUnit.Framework;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class NtsGraphTest
    {
        private ILineString a, b, c;
        private IGeometryFactory factory;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            factory = GeometryFactory.Default;
            a = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 100),
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
                new Coordinate(100, 0),
                new Coordinate(100, 200),
                new Coordinate(200, 200),
            });
        }

        [Test]
        public void BuildGraphAndSearchShortestPathUsingGeometryUnion()
        {            
            // Build segments
            IGeometry segments = a.Union(b).Union(c);
            Assert.IsNotNull(segments);            
            Assert.IsTrue(segments.GetType() == typeof(MultiLineString));
            Assert.Greater(segments.NumGeometries, 0);
            foreach (IGeometry segment in ((GeometryCollection) segments).Geometries)
            {
                Assert.IsNotNull(segment);
                Assert.IsTrue(segment.GetType() == typeof(LineString));
                Debug.WriteLine(segment);
            }

            // Build graph
            Dictionary<Edge<IGeometry>, double> edgeCost = new Dictionary<Edge<IGeometry>, double>(segments.NumGeometries);
            AdjacencyGraph<IGeometry, Edge<IGeometry>> graph = new AdjacencyGraph<IGeometry, Edge<IGeometry>>(true);
            foreach (ILineString str in ((GeometryCollection) segments).Geometries)
            {               
                // Add vertex 1
                IGeometry vertex1 = str.StartPoint;
                Assert.IsNotNull(vertex1);
                if (!graph.ContainsVertex(vertex1))
                {
                    Debug.WriteLine(String.Format("Adding vertex {0} to the list", vertex1));
                    graph.AddVertex(vertex1);
                }

                // Add vertex 2
                IGeometry vertex2 = str.EndPoint;
                Assert.IsNotNull(vertex2);
                if (!graph.ContainsVertex(vertex2))
                {
                    Debug.WriteLine(String.Format("Adding vertex {0} to the list", vertex2));
                    graph.AddVertex(vertex2);
                }

                // Compute weight
                double weight = str.Length;
                Assert.Greater(weight, 0.0);
                Edge<IGeometry> edge = new Edge<IGeometry>(vertex1, vertex2);
                Assert.IsNotNull(edge);

                // Add edge
                Debug.WriteLine(String.Format("Adding edge for vertices {0} and {1} using weight {2}", 
                    vertex1, vertex2, weight));
                graph.AddEdge(edge);
                edgeCost.Add(edge, weight);
            }

            // Perform DijkstraShortestPathAlgorithm
            DijkstraShortestPathAlgorithm<IGeometry, Edge<IGeometry>> dijkstra =
                new DijkstraShortestPathAlgorithm<IGeometry, Edge<IGeometry>>(graph, edgeCost);

            // attach a distance observer to give us the shortest path distances
            VertexDistanceRecorderObserver<IGeometry, Edge<IGeometry>> distObserver =
                new VertexDistanceRecorderObserver<IGeometry, Edge<IGeometry>>();
            distObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            VertexPredecessorRecorderObserver<IGeometry, Edge<IGeometry>> predecessorObserver =
                new VertexPredecessorRecorderObserver<IGeometry, Edge<IGeometry>>();
            predecessorObserver.Attach(dijkstra);

            // Run the algorithm with a's start point set to be the source
            IGeometry start = ((LineString) a).StartPoint;
            Debug.WriteLine(String.Format("Starting algorithm from root vertex {0}", start));
            dijkstra.Compute(start);

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

        [Test]
        public void BuildGraphAndSearchShortestPathUsingGeometryGraph()
        {
            LineSequencer sequencer = new LineSequencer();
            sequencer.Add(new ILineString[] { a, b, c, });
            Assert.IsTrue(sequencer.IsSequenceable()); // Generate graph
            Assert.IsNotNull(sequencer.Graph);
            
            IGeometry sequence = sequencer.GetSequencedLineStrings();
            Assert.IsNotNull(sequence);
            
        }
    }
}
