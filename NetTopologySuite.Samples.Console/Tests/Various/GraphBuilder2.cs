using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using Iesi_NTS.Collections.Generic;
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
        private readonly ISet<ILineString> stringsSet;
        private readonly IList pointsList;

        private AdjacencyGraph<int, IEdge<int>> graph;
        private IDictionary<IEdge<int>, double> consts;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder2"/> class.
        /// </summary>
        public GraphBuilder2()
        {
            factory = null;
            stringsSet = new ListSet<ILineString>(); 
            pointsList = new ArrayList();
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

                addPointsToList(line);
                stringsSet.Add(line);                
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        private void addPointsToList(ILineString line)
        {
            foreach (ICoordinate point in line.Coordinates)
            {
                Debug.Write(point); 
                // if the point is not already on the list then
                // we need to add the point.
                if (!pointsList.Contains(point))
                {
                    pointsList.Add(point);
                    Debug.Write(" ...Added"); 
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine(" ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool PrepareAlgorithm()
        {
            return BuildEdges(DefaultComputer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="computer">
        /// A function that computes the weight 
        /// of any <see cref="ILineString">edge</see> of the graph.
        /// </param>
        /// <returns></returns>
        public bool PrepareAlgorithm(ComputeWeightDelegate computer)
        {
            return BuildEdges(computer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool BuildEdges(ComputeWeightDelegate computer)
        {
            graph = new AdjacencyGraph<int, IEdge<int>>(true);

            // If we get here then we now we have a copy of the point location
            // on the pointList object. We now need to reconstrcut the edge
            // Graph. But before that we add each vertex To The Graph

            int locationInList = 0;
            foreach (ICoordinate point in pointsList)
            {
                Debug.WriteLine(point + " added to graph at location:" + locationInList);
                graph.AddVertex(locationInList);
                locationInList++;
            }

            Debug.WriteLine(" ");
            Debug.WriteLine("Added " + locationInList + " nodes to the graph");
            Debug.WriteLine(" ");

            // Getting here means we have the vertex added to the graph. 
            // What we now need to do is to add the edges to the graph.

            int NumberOfEdgesInLines = CountNumberOfEdges(stringsSet);
            consts = new Dictionary<IEdge<int>, double>(NumberOfEdgesInLines);

            int temp = 1;
            foreach (ILineString line in stringsSet)
            {
                Debug.WriteLine("Line: " + temp + " of " + stringsSet.Count);
                // A line has to have at least two dimensions
                if (line.Coordinates.GetUpperBound(0) > 1)
                {
                    for (int counter = 0; counter < (line.Coordinates.GetUpperBound(0)); counter++)
                    {
                        Debug.Write("EDGE: " + line.Coordinates[counter] + " + " + line.Coordinates[counter + 1]);
                        
                        int src = EdgeAtLocation(line.Coordinates[counter]);
                        int dst = EdgeAtLocation(line.Coordinates[counter + 1]);
                        Debug.WriteLine("EQVILIANT EDGE: " + src + " to " + dst);
                        ICoordinate[] localLine = new ICoordinate[2];
                        localLine[0] = line.Coordinates[counter];
                        localLine[1] = line.Coordinates[counter+1];

                        // Add the edge                        
                        IEdge<int> localEdge = new Edge<int>(src, dst);
                        graph.AddEdge(localEdge);

                        // Here we calculate the weight of the edge
                        ILineString lineString = factory.CreateLineString(localLine);
                        double weight = computer(lineString);
                        consts.Add(localEdge, weight);  
                    }
                    Debug.WriteLine("");
                }
                temp++;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public int EdgeAtLocation(ICoordinate coordinate)
        {
            int index = 0;
            foreach (ICoordinate location in pointsList)
            {
                if ((location.X == coordinate.X) && (location.Y == coordinate.Y))
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
        /// <param name="pString"></param>
        /// <returns></returns>
        private int CountNumberOfEdges(ISet<ILineString> pString)
        {
            // Counts the number of edges in the set we pass to this method. 
            int edgesCount = 0;
            foreach (ILineString localString in pString)
            {
                int edges = localString.Coordinates.GetUpperBound(0);
                edgesCount = edgesCount + edges;
            }
            return edgesCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public ILineString perform(int source,int destination)
        {
            DijkstraShortestPathAlgorithm<int, IEdge<int>> dijkstra = new DijkstraShortestPathAlgorithm<int, IEdge<int>>(graph, consts);
            VertexDistanceRecorderObserver<int, IEdge<int>> distObserver = new VertexDistanceRecorderObserver<int, IEdge<int>>();
            distObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            VertexPredecessorRecorderObserver<int, IEdge<int>> predecessorObserver = new VertexPredecessorRecorderObserver<int, IEdge<int>>();
            predecessorObserver.Attach(dijkstra);

            // Run the algorithm with A set to be the source
            dijkstra.Compute(source);

            // Get the path computed to the destination.
            List<IEdge<int>> path = predecessorObserver.Path(destination);
           
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
            int iCount = path.Count;
            int i;
            int node;

            for (i = 0; i < iCount; i++)
            {
                node = path[i].Source;
                links[i] = (ICoordinate) pointsList[node];
            }

            node = path[i-1].Target;
            links[i] = (ICoordinate) pointsList[node];

            ILineString thePath = factory.CreateLineString(links);
            return thePath;
        }
    }
}

