using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using NUnit.Framework;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class NtsGraphTest
    {                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double ComputeWeight(ILineString line) { return line.Length; }

        private const string shp = ".shp";
        private const string shx = ".shx";
        private const string dbf = ".dbf";

        private IGeometryFactory factory;
        private ILineString a, b, c, d;
        private IPoint start, end;
        private readonly GraphBuilder.ComputeWeightDelegate weightComputer = ComputeWeight;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Environment.CurrentDirectory = Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               @"../../../NetTopologySuite.Samples.Shapefiles");

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
            end   = d.EndPoint;
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
                double weight = weightComputer(str);
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
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilderExceptionUsingNoGeometries()
        {
            GraphBuilder builder = new GraphBuilder();
            builder.PrepareAlgorithm();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilderExceptionUsingOneGeometry()
        {
            GraphBuilder builder = new GraphBuilder();
            Assert.IsTrue(builder.Add(a));
            builder.PrepareAlgorithm();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilderExceptionUsingARepeatedGeometry()
        {
            GraphBuilder builder = new GraphBuilder();
            Assert.IsTrue(builder.Add(a));
            Assert.IsFalse(builder.Add(a));
            builder.PrepareAlgorithm();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilderExceptionUsingDifferentFactories()
        {
            GraphBuilder builder = new GraphBuilder();
            Assert.IsTrue(builder.Add(a));
            Assert.IsTrue(builder.Add(b, c));
            Assert.IsTrue(builder.Add(d));
            builder.Add(GeometryFactory.Default.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0 ,0),
                new Coordinate(50 , 50),
            }));
        }

        [Test]
        public void BuildGraphAndSearchShortestPathUsingGraphBuilder()
        {
            // Build algorithm
            GraphBuilder builder = new GraphBuilder();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            DijkstraShortestPathAlgorithm<IPoint, IEdge<IPoint>> algorithm = builder.PrepareAlgorithm();

            // Attach a distance observer to give us the shortest path distances
            VertexDistanceRecorderObserver<IPoint, IEdge<IPoint>> distObserver =
                new VertexDistanceRecorderObserver<IPoint, IEdge<IPoint>>();
            distObserver.Attach(algorithm);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            VertexPredecessorRecorderObserver<IPoint, IEdge<IPoint>> predecessorObserver =
                new VertexPredecessorRecorderObserver<IPoint, IEdge<IPoint>>();
            predecessorObserver.Attach(algorithm);

            // Run algorithm
            algorithm.Compute(start);

            // Check results
            int distance = distObserver.Distances[end];
            Assert.AreEqual(2, distance);
            IDictionary<IPoint, IEdge<IPoint>> predecessors = predecessorObserver.VertexPredecessors;
            for (int i = 0; i < distance; i++)
            {
                IEdge<IPoint> edge = predecessors[end];
                if (i == 0)
                {
                    Assert.AreEqual(d.GetPointN(d.NumPoints - 2), edge.Source);
                    Assert.AreEqual(d.EndPoint, edge.Target);
                }
                else if (i == 1)
                {
                    Assert.AreEqual(a.StartPoint, edge.Source);
                    Assert.AreEqual(d.GetPointN(d.NumPoints - 2), edge.Target);
                }
                end = edge.Source;
            }

            // Detach the observers
            distObserver.Detach(algorithm);
            predecessorObserver.Detach(algorithm);
        }
        
        [Ignore]
        [Test]
        public void BuildGraphBinary()
        {            
            string path = "strade" + shp;
            Assert.IsTrue(File.Exists(path));

            ShapefileReader reader = new ShapefileReader(path, factory);
            IGeometryCollection coll = reader.ReadAll();
            Assert.IsNotNull(coll);
            Assert.IsNotEmpty(coll.Geometries);

            IGeometry result = coll.Geometries[0];
            for (int i = 1; i < coll.NumGeometries; i++ )
            {
                Debug.WriteLine(String.Format("Union of {0}'th geometry", i));
                result = result.Union(coll.Geometries[i]);
            }
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(typeof(MultiLineString), result);

            WKBWriter wkbwriter = new WKBWriter();
            byte[] rawdata = wkbwriter.Write(result);
            Assert.IsNotEmpty(rawdata);

            path = "graph";
            if (File.Exists(path))
                File.Delete(path + shp);
            Assert.IsFalse(File.Exists(path));
            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.Write(rawdata, 0, rawdata.Length);
            Assert.IsTrue(File.Exists(path));
        }

        [Test]
        public void BuildGraphFromWKB()
        {
            int index = 0;
            IGeometry edges;
            WKBReader reader = new WKBReader(factory);
            using (FileStream stream = new FileStream("graph", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                edges = reader.Read(stream);
                index++;
            }
            Assert.AreEqual(1, index);
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof(MultiLineString), edges);
            Assert.AreEqual(1179, edges.NumGeometries);

            // Build graph
            GeometryCollection coll = (GeometryCollection) edges;
            Dictionary<IEdge<IGeometry>, double> consts = new Dictionary<IEdge<IGeometry>, double>(edges.NumGeometries);
            AdjacencyGraph<IGeometry, IEdge<IGeometry>> graph = new AdjacencyGraph<IGeometry, IEdge<IGeometry>>(true);            
            foreach (ILineString str in coll.Geometries)
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
                double weight = weightComputer(str);
                Assert.Greater(weight, 0.0);
                IEdge<IGeometry> edge = new Edge<IGeometry>(vertex1, vertex2);
                Assert.IsNotNull(edge);

                // Add edge
                Debug.WriteLine(String.Format("Adding edge for vertices {0} and {1} using weight {2}", vertex1, vertex2, weight));
                graph.AddEdge(edge);
                consts.Add(edge, weight);
            }

            // Perform DijkstraShortestPathAlgorithm
            DijkstraShortestPathAlgorithm<IGeometry, IEdge<IGeometry>> dijkstra =
                new DijkstraShortestPathAlgorithm<IGeometry, IEdge<IGeometry>>(graph, consts);

            // attach a distance observer to give us the shortest path distances
            VertexDistanceRecorderObserver<IGeometry, IEdge<IGeometry>> distObserver =
                new VertexDistanceRecorderObserver<IGeometry, IEdge<IGeometry>>();
            distObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            VertexPredecessorRecorderObserver<IGeometry, IEdge<IGeometry>> predecessorObserver =
                new VertexPredecessorRecorderObserver<IGeometry, IEdge<IGeometry>>();
            predecessorObserver.Attach(dijkstra);

            // Run the algorithm   
            IGeometry startPoint = ((ILineString) coll.Geometries[0]).StartPoint;
            Debug.WriteLine(String.Format("Starting algorithm from root vertex {0}", startPoint));
            dijkstra.Compute(startPoint);

            foreach (KeyValuePair<IGeometry, int> kvp in distObserver.Distances)
                Debug.WriteLine(String.Format("Distance from root to node {0} is {1}",
                    kvp.Key, kvp.Value));
            foreach (KeyValuePair<IGeometry, IEdge<IGeometry>> kvp in predecessorObserver.VertexPredecessors)
                Debug.WriteLine(String.Format(
                    "If you want to get to {0} you have to enter through the IN edge {1}", kvp.Key, kvp.Value));

            // Detach the observers
            distObserver.Detach(dijkstra);
            predecessorObserver.Detach(dijkstra);
        }
    }
}
