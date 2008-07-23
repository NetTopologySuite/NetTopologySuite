using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// A class that manages shortest path computation.
    /// </summary>
    public class GraphBuilder2
    {
        /// <summary>
        /// A delegate that defines how to calculate the weight 
        /// of a <see cref="ILineString">line</see>.
        /// </summary>
        /// <param name="line">A <see cref="ILineString">line</see>.</param>
        /// <returns>The weight of the line.</returns>
        public delegate double ComputeWeightDelegate(ILineString line);

        private static readonly ComputeWeightDelegate DefaultComputer =
            delegate(ILineString line) { return line.Length; };

        private readonly bool bidirectional;

        private IGeometryFactory factory;
        private readonly IList<ILineString> strings;
        private readonly IList<ICoordinate> coords;

        private AdjacencyGraph<int, IEdge<int>> graph;
        private IDictionary<IEdge<int>, double> consts;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder2"/> class.
        /// </summary>
        /// <param name="bidirectional">
        /// Specify if the graph must be build using both edges directions.
        /// </param>
        public GraphBuilder2(bool bidirectional)
        {
            this.bidirectional = bidirectional;

            factory = null;
            strings = new List<ILineString>();
            coords  = new List<ICoordinate>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder2"/> class,
        /// using a directed graph.
        /// </summary>
        public GraphBuilder2() : this (false) { } // TODO: maybe the default value must be true...

        /// <summary>
        /// Adds each line to the graph structure.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>
        /// <c>true</c> if all <paramref name="lines">lines</paramref> 
        /// are added, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="TopologyException">
        /// If geometries don't have the same <see cref="IGeometryFactory">factory</see>.
        /// </exception>
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

                bool lineFound = strings.Contains(line);
                result &= !lineFound;
                if (!lineFound)
                    strings.Add(line);
                else continue; // Skip vertex check because line is already present

                foreach (ICoordinate coord in line.Coordinates)
                {
                    if (!coords.Contains(coord))
                        coords.Add(coord);
                }                
            }
            return result;
        }        

        /// <summary>
        /// Initialize the algorithm using the default 
        /// <see cref="ComputeWeightDelegate">weight computer</see>,
        /// that uses <see cref="IGeometry.Length">string length</see>
        /// as weight value.
        /// </summary>
        /// <exception cref="TopologyException">
        /// If you've don't added two or more geometries to the builder.
        /// </exception>
        /// <exception cref="ApplicationException">
        /// If builder is already initialized.
        /// </exception>
        public void Initialize()
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
        /// <exception cref="TopologyException">
        /// If you've don't added two or more geometries to the builder.
        /// </exception>
        /// <exception cref="ApplicationException">
        /// If builder is already initialized.
        /// </exception>
        public void Initialize(ComputeWeightDelegate computer)
        {
            BuildEdges(computer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="computer"></param>
        private void BuildEdges(ComputeWeightDelegate computer)
        {
            if (strings.Count < 2)
                throw new TopologyException("you must specify two or more geometries to build a graph");

            if (graph != null)
                throw new ApplicationException("builder already initialized");

            graph = new AdjacencyGraph<int, IEdge<int>>(true);

            // If we get here then we now we have a copy of the point location
            // on the pointList object. We now need to reconstrcut the edge
            // Graph. But before that we add each vertex To The Graph

            int locationInList = 0;
            foreach (ICoordinate coord in coords)
            {
                graph.AddVertex(locationInList);
                locationInList++;
            }

            // Getting here means we have the vertex added to the graph. 
            // What we now need to do is to add the edges to the graph.

            // Counts the number of edges in the set we pass to this method.             
            int numberOfEdgesInLines = 0;
            foreach (ILineString str in strings)
            {
                int edges = str.Coordinates.GetUpperBound(0);
                numberOfEdgesInLines += edges;
            }

            // Double values because we use also reversed edges...
            if (bidirectional)
                numberOfEdgesInLines *= 2;
            consts = new Dictionary<IEdge<int>, double>(numberOfEdgesInLines);

            foreach (ILineString line in strings)
            {
                // A line has to have at least two dimensions
                int bound = line.Coordinates.GetUpperBound(0);
                if (bound > 0)
                {
                    for (int counter = 0; counter < bound; counter++)
                    {
                        int src = VertexAtLocation(line.Coordinates[counter]);
                        int dst = VertexAtLocation(line.Coordinates[counter + 1]);

                        ICoordinate[] localLine = new ICoordinate[2];
                        localLine[0] = line.Coordinates[counter];
                        localLine[1] = line.Coordinates[counter + 1];

                        // Here we calculate the weight of the edge
                        ILineString lineString = factory.CreateLineString(localLine);
                        double weight = computer(lineString);

                        // Add the edge
                        IEdge<int> localEdge = new Edge<int>(src, dst);
                        graph.AddEdge(localEdge);
                        consts.Add(localEdge, weight);

                        if (bidirectional)
                        {
                            // Add the reversed edge
                            IEdge<int> localEdgeRev = new Edge<int>(dst, src);
                            graph.AddEdge(localEdgeRev);
                            consts.Add(localEdgeRev, weight);
                        }
                    }
                 }
            }            
        }

        /// <summary>
        /// Find the edge index at the <paramref name="coordinate">specified</paramref> location.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns>The edge index, or <c>-1</c> if not found</returns>
        public int VertexAtLocation(ICoordinate coordinate)
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
        /// Find the edge index at the <paramref name="geom">geometry</paramref>'s start point.
        /// </summary>
        /// <param name="geom"></param>
        /// <returns>The edge index, or <c>-1</c> if not found</returns>
        public int VertexAtLocation(IGeometry geom)
        {
            return VertexAtLocation(geom.Coordinate);
        }

        /// <summary>
        /// Carries out the shortest path anlayis between the two nodes
        /// ids passed as variables and returns a ILineString giveing the
        /// shortest path.
        /// </summary>
        /// <param name="source">The source node (index of)</param>
        /// <param name="destination">The desitnation node (index of)</param>
        /// <returns>A line string geometric shape of the path</returns>
        public ILineString perform(int source, int destination)
        {
            // Build algorithm
            DijkstraShortestPathAlgorithm<int, IEdge<int>> dijkstra = 
                new DijkstraShortestPathAlgorithm<int, IEdge<int>>(graph, consts);

            // Attach a Distance observer to give us the distances between edges
            VertexDistanceRecorderObserver<int, IEdge<int>> distanceObserver = 
                new VertexDistanceRecorderObserver<int, IEdge<int>>();
            distanceObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            VertexPredecessorRecorderObserver<int, IEdge<int>> predecessorObserver =
                new VertexPredecessorRecorderObserver<int, IEdge<int>>();
            predecessorObserver.Attach(dijkstra);

            // Run the algorithm with A set to be the source
            dijkstra.Compute(source);

            // Get the path computed to the destination.
            List<IEdge<int>> path = predecessorObserver.Path(destination);

            // Then we need to turn that into a geomery.
            if (path.Count > 1)
                return buildString(path);

            // if the count is greater than one then a 
            // path could not be found, so we return null 
            return null;
        }

        /// <summary>
        /// Takes the path returned from QuickGraph library and uses the 
        /// list of coordinates to reconstruct the path into a geometric 
        /// "shape"
        /// </summary>
        /// <param name="path">Shortest path from the QucikGraph Library</param>
        /// <returns></returns>
        private ILineString buildString(IList<IEdge<int>> path)
        {
            // if the path has no links then return a null reference
            if (path.Count < 1)
                return null;

            // if we get here then we now that there is at least one
            // edge in the path.
            ICoordinate[] links = new ICoordinate[path.Count + 1];
            int i;
            int node;

            // Add each node to the list of coordinates in to the 
            // array.
            for (i = 0; i < path.Count; i++)
            {
                node = path[i].Source;
                links[i] = coords[node];
            }

            // Add the target node to the last loction in the list 
            node = path[i - 1].Target;
            links[i] = coords[node];

            // Turn the list of coordinates into a geometry.
            ILineString thePath = factory.CreateLineString(links);
            return thePath;
        }

        /// <summary>
        /// Outputs the graph as a DIMACS Graph
        /// </summary>
        /// <param name="fileName">The name of the output graph</param>
        /// <returns>Indicates if the method was worked.</returns>
        public bool WriteAsDIMACS(string fileName)
        {
            // The DIMACS format is a reasonabley standard method
            // of preparing graphs for analys in SP algortihm.
            // This method *could* be used to prepare the graph beforehand
            // so the turning from a GIS layer into a graph is not so 
            // intensive.
            //
            // NOTE: Follows the 9th DIMACS format: http://www.dis.uniroma1.it/~challenge9/format.shtml
            return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool ReadFromDIMACS(string fileName)
        {
            return false;
        }
    }
}

