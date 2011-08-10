//using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Polygonizes a set of <see cref="IGeometry"/>s which contain linework that
    /// represents the edges of a planar graph.
    /// </summary>
    /// <remarks>
    /// <para>All types of Geometry are accepted as input;
    /// the constituent linework is extracted as the edges to be polygonized.
    /// The processed edges must be correctly noded; that is, they must only meet
    /// at their endpoints.  The Polygonizer will run on incorrectly noded input
    /// but will not form polygons from non-noded edges, 
    /// and will report them as errors.
    /// </para><para>
    /// The Polygonizer reports the follow kinds of errors:
    /// Dangles - edges which have one or both ends which are not incident on another edge endpoint
    /// Cut Edges - edges which are connected at both ends but which do not form part of polygon
    /// Invalid Ring Lines - edges which form rings which are invalid
    /// (e.g. the component lines contain a self-intersection).</para>
    /// </remarks>
    public class Polygonizer
    {
        /// <summary>
        /// Adds every linear element in a <see cref="IGeometry"/> into the polygonizer graph.
        /// </summary>
        private class LineStringAdder : IGeometryComponentFilter
        {
            private readonly Polygonizer container;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public LineStringAdder(Polygonizer container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="g"></param>
            public void Filter(IGeometry g) 
            {
                if (g is ILineString)
					container.Add((ILineString)g);
            }
        }

        /// <summary>
        /// Default factory.
        /// </summary>
        private readonly LineStringAdder _lineStringAdder;

        /// <summary>
        /// 
        /// </summary>
        protected PolygonizeGraph graph;

        /// <summary>
        /// Initialized with empty collections, in case nothing is computed
        /// </summary>
        protected ICollection<ILineString> dangles = new List<ILineString>();

        /// <summary>
        /// 
        /// </summary>
        protected ICollection<ILineString> cutEdges = new List<ILineString>();

        /// <summary>
        /// 
        /// </summary>
        protected IList<IGeometry> invalidRingLines = new List<IGeometry>();

        /// <summary>
        /// 
        /// </summary>
        protected IList<EdgeRing> holeList;
        
        /// <summary>
        /// 
        /// </summary>        
        protected IList<EdgeRing> shellList;

        /// <summary>
        /// 
        /// </summary>
        protected IList<IGeometry> polyList;

        /// <summary>
        /// Create a polygonizer with the same {GeometryFactory}
        /// as the input <c>Geometry</c>s.
        /// </summary>
        public Polygonizer() 
        {
            _lineStringAdder = new LineStringAdder(this);
        }

        /// <summary>
        /// Adds a collection of <see cref="IGeometry"/>s to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used.
        /// </summary>
        /// <param name="geomList">A list of <c>Geometry</c>s with linework to be polygonized.</param>
        public void Add(IList<IGeometry> geomList)
        {
            foreach (IGeometry geometry in geomList)
                Add(geometry);
        }

        /// <summary>
        /// Adds a <see cref="IGeometry"/> to the linework to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used
        /// </summary>
        /// <param name="g">A <c>Geometry</c> with linework to be polygonized.</param>
		public void Add(IGeometry g)
        {
            g.Apply(_lineStringAdder);
        }

        /// <summary>
        /// Adds a  to the graph of polygon edges.
        /// </summary>
        /// <param name="line">The <see cref="ILineString"/> to add.</param>
        private void Add(ILineString line)
        {
            // create a new graph using the factory from the input Geometry
            if (graph == null)
				graph = new PolygonizeGraph(line.Factory);
            graph.AddEdge(line);
        }

        /// <summary>
        /// Gets the list of polygons formed by the polygonization.
        /// </summary>        
        public IList<IGeometry> GetPolygons()
        {
            Polygonize();
            return polyList;
        }

        /// <summary> 
        /// Gets the list of dangling lines found during polygonization.
        /// </summary>
        public ICollection<ILineString> GetDangles()
        {
            Polygonize();
            return dangles;
        }

        /// <summary>
        /// Gets the list of cut edges found during polygonization.
        /// </summary>
        public ICollection<ILineString> GetCutEdges()
        {
            Polygonize();
            return cutEdges;
        }

        /// <summary>
        /// Gets the list of lines forming invalid rings found during polygonization.
        /// </summary>
        public IList<IGeometry> GetInvalidRingLines()
        {
            Polygonize();
            return invalidRingLines;
        }

        /// <summary>
        /// Performs the polygonization, if it has not already been carried out.
        /// </summary>
        private void Polygonize()
        {
            // check if already computed
            if (polyList != null) 
                return;

            polyList = new List<IGeometry>();

            // if no geometries were supplied it's possible that graph is null
            if (graph == null) 
                return;

            dangles = graph.DeleteDangles();
            cutEdges = graph.DeleteCutEdges();
            var edgeRingList = graph.GetEdgeRings();

            IList<EdgeRing> validEdgeRingList = new List<EdgeRing>();
            invalidRingLines = new List<IGeometry>();
            FindValidRings(edgeRingList, validEdgeRingList, invalidRingLines);

            FindShellsAndHoles(validEdgeRingList);
            AssignHolesToShells(holeList, shellList);

            polyList = new List<IGeometry>();
            foreach (EdgeRing er in shellList)
                polyList.Add(er.Polygon);
        }

        private static void FindValidRings(IEnumerable<EdgeRing> edgeRingList, IList<EdgeRing> validEdgeRingList, IList<IGeometry> invalidRingList)
        {
            for (var i = edgeRingList.GetEnumerator(); i.MoveNext(); ) 
            {
                var er = i.Current;
                if (er.IsValid)
                     validEdgeRingList.Add(er);
                else invalidRingList.Add(er.LineString);
            }
        }

        private void FindShellsAndHoles(IEnumerable<EdgeRing> edgeRingList)
        {
            holeList = new List<EdgeRing>();
            shellList = new List<EdgeRing>();
            foreach (EdgeRing er in edgeRingList)
            {
                if (er.IsHole)
                     holeList.Add(er);
                else shellList.Add(er);

            }
        }

        private static void AssignHolesToShells(IEnumerable<EdgeRing> holeList, IList<EdgeRing> shellList)
        {
            foreach (EdgeRing holeER in holeList)
                AssignHoleToShell(holeER, shellList);
        }

        private static void AssignHoleToShell(EdgeRing holeER, IList<EdgeRing> shellList)
        {
            var shell = EdgeRing.FindEdgeRingContaining(holeER, shellList);
            if (shell != null)
                shell.AddHole(holeER.Ring);
        }
    }
}
