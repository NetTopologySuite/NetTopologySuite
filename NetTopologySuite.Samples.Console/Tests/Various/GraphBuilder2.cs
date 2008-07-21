using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    public class GraphBuilder2
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
        private readonly IList<ILineString> strings;
        private readonly IList<ICoordinate> coords;

        private AdjacencyGraph<int, IEdge<int>> graph;
        private IDictionary<IEdge<int>, double> consts;
        private VertexPredecessorRecorderObserver<int, IEdge<int>> observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder2"/> class.
        /// </summary>
        public GraphBuilder2()
        {
            factory = null;
            strings = new List<ILineString>();
            coords  = new List<ICoordinate>();
            observer = new VertexPredecessorRecorderObserver<int, IEdge<int>>();
        }

        /// <summary>
        /// Adds each line to the graph strucutre.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public bool Add(params ILineString[] lines)
        {
            foreach (ILineString line in lines)
            {
                IGeometryFactory newfactory = line.Factory;
                if (factory == null)
                    factory = newfactory;
                else if (!newfactory.PrecisionModel.Equals(factory.PrecisionModel))
                    throw new TopologyException("all geometries must have the same precision model");

                foreach (ICoordinate coord in line.Coordinates)
                {
                    if (!coords.Contains(coord))
                    {
                        coords.Add(coord);
                        Debug.Write(String.Format("coord {0} added", coord));
                    }
                }

                if (!strings.Contains(line))
                {
                    strings.Add(line);
                    Debug.Write(String.Format("string {0} added", line));
                }
            }
            return true;
        }        

        /// <summary>
        /// Initialize the algorithm using the default 
        /// <see cref="ComputeWeightDelegate">weight computer</see>,
        /// that uses <see cref="IGeometry.Length">string length</see>
        /// as weight value.
        /// </summary>
        public void PrepareAlgorithm()
        {
            BuildEdges(DefaultComputer);
        }

        /// <summary>
        /// Initialize the algorithm using the specified 
        /// <paramref name="computer">weight computer</paramref>
        /// </summary>
        /// <param name="computer">
        /// A function that computes the weight 
        /// of any <see cref="ILineString">edge</see> of the graph.
        /// </param>
        public void PrepareAlgorithm(ComputeWeightDelegate computer)
        {
            BuildEdges(computer);
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildEdges(ComputeWeightDelegate computer)
        {
            if (strings.Count < 2)
                throw new TopologyException("you must specify two or more geometries to build a graph");

            graph = new AdjacencyGraph<int, IEdge<int>>(true);

            // If we get here then we now we have a copy of the point location
            // on the pointList object. We now need to reconstrcut the edge
            // Graph. But before that we add each vertex To The Graph

            int locationInList = 0;
            foreach (ICoordinate coord in coords)
            {
                Debug.WriteLine(String.Format("{0} added to graph at location: {1}", coord, locationInList));
                graph.AddVertex(locationInList);
                locationInList++;
            }

            Debug.WriteLine(String.Empty);
            Debug.WriteLine(String.Format("Added {0} nodes to the graph", locationInList));
            Debug.WriteLine(String.Empty);

            // Getting here means we have the vertex added to the graph. 
            // What we now need to do is to add the edges to the graph.

            // Counts the number of edges in the set we pass to this method.             
            int numberOfEdgesInLines = 0;
            foreach (ILineString str in strings)
            {
                int edges = str.Coordinates.GetUpperBound(0);
                numberOfEdgesInLines += edges;
            }

            consts = new Dictionary<IEdge<int>, double>(numberOfEdgesInLines);

            int temp = 1;
            foreach (ILineString line in strings)
            {
                Debug.WriteLine(String.Format("line: {0} of {1}", temp, strings.Count));
                // A line has to have at least two dimensions
                int bound = line.Coordinates.GetUpperBound(0);
                if (bound > 1)
                {
                    for (int counter = 0; counter < bound; counter++)
                    {
                        Debug.Write(String.Format("edge: {0} + {1}", 
                            line.Coordinates[counter], line.Coordinates[counter + 1]));
                        
                        int src = EdgeAtLocation(line.Coordinates[counter]);
                        int dst = EdgeAtLocation(line.Coordinates[counter + 1]);
                        Debug.WriteLine(String.Format("eqviliant edge: {0} to {1}", src, dst));
                        ICoordinate[] localLine = new ICoordinate[2];
                        localLine[0] = line.Coordinates[counter];
                        localLine[1] = line.Coordinates[counter + 1];

                        // Add the edge                        
                        IEdge<int> localEdge = new Edge<int>(src, dst);
                        graph.AddEdge(localEdge);

                        // Here we calculate the weight of the edge
                        ILineString lineString = factory.CreateLineString(localLine);
                        double weight = computer(lineString);
                        consts.Add(localEdge, weight);  
                    }
                    Debug.WriteLine(String.Empty);
                }
                temp++;
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public int EdgeAtLocation(ICoordinate coordinate)
        {
            int index = 0;
            foreach (ICoordinate coord in coords)
            {
                if (coordinate.Equals(coord))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int EdgeAtLocation(IGeometry point)
        {
            return EdgeAtLocation(point.Coordinate);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public ILineString perform(int source, int destination)
        {
            DijkstraShortestPathAlgorithm<int, IEdge<int>> dijkstra = 
                new DijkstraShortestPathAlgorithm<int, IEdge<int>>(graph, consts);
            VertexDistanceRecorderObserver<int, IEdge<int>> distObserver = 
                new VertexDistanceRecorderObserver<int, IEdge<int>>();
            distObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            observer.Attach(dijkstra);

            // Run the algorithm with A set to be the source
            dijkstra.Compute(source);

            // Get the path computed to the destination.
            List<IEdge<int>> path = observer.Path(destination);
           
            // Then we need to turn that into a geomery.
            if (path.Count > 1)
                return buildString(path);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private ILineString buildString(IList<IEdge<int>> path)
        {
            ICoordinate[] links = new ICoordinate[path.Count + 1];
            int i;
            int node;

            for (i = 0; i < path.Count; i++)
            {
                node = path[i].Source;
                links[i] = coords[node];
            }

            node = path[i - 1].Target;
            links[i] = coords[node];

            ILineString thePath = factory.CreateLineString(links);
            return thePath;
        }
    }
}

