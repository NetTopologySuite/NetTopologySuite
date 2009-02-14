using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Linemerge;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// A class that manages shortest path computation.
    /// </summary>
    public class PathFinder
    {
        /// <summary>
        /// A delegate that defines how to calculate the weight 
        /// of a <see cref="ILineString">line</see>.
        /// </summary>
        /// <param name="line">A <see cref="ILineString">line</see>.</param>
        /// <returns>The weight of the line.</returns>
        public delegate double ComputeWeightDelegate(ILineString line);

        private static readonly ComputeWeightDelegate DefaultComputer = line => line.Length;

        private readonly bool bidirectional;

        private IGeometryFactory factory;
        private readonly List<ILineString> strings;
        
        private readonly AdjacencyGraph<ICoordinate, IEdge<ICoordinate>> graph;
        private IDictionary<IEdge<ICoordinate>, double> consts;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder2"/> class.
        /// </summary>
        /// <param name="bidirectional">
        /// Specify if the graph must be build using both edges directions.
        /// </param>
        public PathFinder(bool bidirectional)
        {
            this.bidirectional = bidirectional;

            factory = null;
            strings = new List<ILineString>();
            graph   = new AdjacencyGraph<ICoordinate, IEdge<ICoordinate>>(true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBuilder2"/> class,
        /// using a directed graph.
        /// </summary>
        public PathFinder() : this(false) { } 

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
            var result = true;
            foreach (var line in lines)
            {
                var newfactory = line.Factory;
                if (factory == null)
                    factory = newfactory;
                else if (!newfactory.PrecisionModel.Equals(factory.PrecisionModel))
                    throw new TopologyException("all geometries must have the same precision model");

                var lineFound = strings.Contains(line);
                result &= !lineFound;
                if (!lineFound)
                    strings.Add(line);
                else continue; // Skip vertex check because line is already present

                var coordinates = line.Coordinates;
                var start = 0;
                var end = coordinates.GetUpperBound(0);  
                AddCoordinateToGraph(coordinates[start]); // StartPoint
                AddCoordinateToGraph(coordinates[end]);   // EndPoint
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        private void AddCoordinateToGraph(ICoordinate coord)
        {
            if (!graph.ContainsVertex(coord))
                 graph.AddVertex(coord);
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

            // Counts the number of edges in the set we pass to this method.             
            var numberOfEdgesInLines = strings.Count * 2;

            // Double values because we use also reversed edges...
            if (bidirectional)
                numberOfEdgesInLines *= 2;

            consts = new Dictionary<IEdge<ICoordinate>, double>(numberOfEdgesInLines);

            foreach (var line in strings)
            {
                // Prepare a segment
                var coordinates = line.Coordinates;
                var start = 0;
                var end = coordinates.GetUpperBound(0);
                var src = coordinates[start];
                var dst = coordinates[end];

                // Here we calculate the weight of the edge                
                var weight = computer(line);

                // Add the edge
                IEdge<ICoordinate> localEdge = new Edge<ICoordinate>(src, dst);
                graph.AddEdge(localEdge);
                consts.Add(localEdge, weight);

                if (bidirectional)
                {
                    // Add the reversed edge
                    IEdge<ICoordinate> localEdgeRev = new Edge<ICoordinate>(dst, src);
                    graph.AddEdge(localEdgeRev);
                    consts.Add(localEdgeRev, weight);
                }                
            }            
        }

        /// <summary>
        /// Carries out the shortest path anlayis between the two 
        /// <see cref="IGeometry.Coordinate">nodes</see>
        /// passed as variables and returns an <see cref="ILineString" /> 
        /// giveing the shortest path.
        /// </summary>
        /// <param name="source">The source geom</param>
        /// <param name="destination">The destination geom</param>
        /// A <see cref="ILineString"/> or a <see cref="IMultiLineString"/>
        /// with all the elements of the graph that composes the shortest path,
        /// sequenced using a <see cref="LineSequencer"/>.
        /// </returns>
        public IGeometry Find(IGeometry source, IGeometry destination)
        {
            return Find(source.Coordinate, destination.Coordinate);
        }

        /// <summary>
        /// Carries out the shortest path between the two nodes
        /// ids passed as variables and returns an <see cref="ILineString" /> 
        /// giveing the shortest path.
        /// </summary>
        /// <param name="source">The source node</param>
        /// <param name="destination">The destination node</param>
        /// A <see cref="ILineString"/> or a <see cref="IMultiLineString"/>
        /// with all the elements of the graph that composes the shortest path,
        /// sequenced using a <see cref="LineSequencer"/>.
        /// </returns>
        public IGeometry Find(ICoordinate source, ICoordinate destination)
        {
            if (!graph.ContainsVertex(source))
                throw new ArgumentException("key not found in the graph", "source");
            if (!graph.ContainsVertex(destination))
                throw new ArgumentException("key not found in the graph", "destination");

            // Build algorithm
            var dijkstra =
                new DijkstraShortestPathAlgorithm<ICoordinate, IEdge<ICoordinate>>(graph, edge => consts[edge]);

            // Attach a Distance observer to give us the distances between edges
            var distanceObserver =
                new VertexDistanceRecorderObserver<ICoordinate, IEdge<ICoordinate>>(edge => consts[edge]);
            distanceObserver.Attach(dijkstra);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            var predecessorObserver =
                new VertexPredecessorRecorderObserver<ICoordinate, IEdge<ICoordinate>>();
            predecessorObserver.Attach(dijkstra);

            // Run the algorithm with A set to be the source
            dijkstra.Compute(source);

            // Get the path computed to the destination.
            IEnumerable<IEdge<ICoordinate>> path;
            var result = predecessorObserver.TryGetPath(destination, out path);

            // Then we need to turn that into a geomery.
            return result ? BuildString(new List<IEdge<ICoordinate>>(path)) : null;
        }

        /// <summary>
        /// Takes the path returned from QuickGraph library and uses the 
        /// list of coordinates to reconstruct the path into a geometric 
        /// "shape"
        /// </summary>
        /// <param name="paths">Shortest path from the QucikGraph Library</param>
        /// <returns>
        /// A <see cref="ILineString"/> or a <see cref="IMultiLineString"/>
        /// with all the elements of the graph that composes the shortest path,
        /// sequenced using a <see cref="LineSequencer"/>.
        /// </returns>
        private IGeometry BuildString(ICollection<IEdge<ICoordinate>> paths)
        {
            // if the path has no links then return a null reference
            if (paths.Count < 1)
                return null;

            var collector = new LineSequencer();
            foreach (var path in paths)
            {
                var src = path.Source;
                var dst = path.Target;
                foreach (var str in strings)
                {
                    if (IsBound(str, src) && IsBound(str, dst))
                        collector.Add(str);
                }
            }

            var sequence = collector.GetSequencedLineStrings();
            return sequence;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        private static bool IsBound(IGeometry str, ICoordinate src)
        {
            var coordinates = str.Coordinates;
            var start = 0;
            var end = str.Coordinates.GetUpperBound(0);            
            return coordinates[start].Equals(src) || 
                coordinates[end].Equals(src);
        }        
    }
}

