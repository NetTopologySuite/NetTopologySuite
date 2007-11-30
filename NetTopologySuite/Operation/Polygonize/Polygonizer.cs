using System;
using System.Collections;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Polygonizes a set of Geometrys which contain linework that
    /// represents the edges of a planar graph.
    /// </summary>
    /// <remarks>
    /// Any dimension of Geometry is handled - the constituent linework is extracted
    /// to form the edges.
    /// The edges must be correctly noded; that is, they must only meet
    /// at their endpoints.  The Polygonizer will still run on incorrectly noded input
    /// but will not form polygons from incorrected noded edges.
    /// The Polygonizer reports the follow kinds of errors:
    /// Dangles - edges which have one or both ends which are not incident on another edge endpoint
    /// Cut Edges - edges which are connected at both ends but which do not form part of polygon
    /// Invalid Ring Lines - edges which form rings which are invalid
    /// (e.g. the component lines contain a self-intersection).
    /// </remarks>
    public class Polygonizer<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Add every linear element in a point into the polygonizer graph.
        /// </summary>
        private class LineStringAdder<TCoordinate> : IGeometryComponentFilter<TCoordinate>
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            private Polygonizer<TCoordinate> container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public LineStringAdder(Polygonizer<TCoordinate> container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="g"></param>
            public void Filter(IGeometry<TCoordinate> g)
            {
                if (g is ILineString<TCoordinate>)
                {
                    container.Add((ILineString<TCoordinate>)g);
                }
            }
        }

        /// <summary>
        /// Default factory.
        /// </summary>
        private LineStringAdder<TCoordinate> lineStringAdder = null;

        protected PolygonizeGraph<TCoordinate> graph;

        /// <summary>
        /// Initialized with empty collections, in case nothing is computed
        /// </summary>
        private IList dangles = new ArrayList();

        private IList cutEdges = new ArrayList();
        private IList invalidRingLines = new ArrayList();
        private IList holeList = null;
        private IList shellList = null;
        private IList polyList = null;

        /// <summary>
        /// Create a polygonizer with the same {GeometryFactory}
        /// as the input <see cref="Geometry{TCoordinate}"/>s.
        /// </summary>
        public Polygonizer()
        {
            lineStringAdder = new LineStringAdder<TCoordinate>(this);
        }

        /// <summary>
        /// Add a collection of geometries to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used.
        /// </summary>
        /// <param name="geomList">A list of <see cref="Geometry{TCoordinate}"/>s with linework to be polygonized.</param>
        public void Add(IList geomList)
        {
            for (IEnumerator i = geomList.GetEnumerator(); i.MoveNext();)
            {
                IGeometry<TCoordinate> geometry = (IGeometry<TCoordinate>)i.Current;
                Add(geometry);
            }
        }

        /// <summary>
        /// Add a point to the linework to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used
        /// </summary>
        /// <param name="g">A <see cref="Geometry{TCoordinate}"/> with linework to be polygonized.</param>
        public void Add(IGeometry<TCoordinate> g)
        {
            g.Apply(lineStringAdder);
        }

        /// <summary>
        /// Add a linestring to the graph of polygon edges.
        /// </summary>
        /// <param name="line">The <c>LineString</c> to add.</param>
        private void Add(ILineString<TCoordinate> line)
        {
            // create a new graph using the factory from the input Geometry
            if (graph == null)
            {
                graph = new PolygonizeGraph<TCoordinate>(line.Factory);
            }
            graph.AddEdge(line);
        }

        /// <summary>
        /// Compute and returns the list of polygons formed by the polygonization.
        /// </summary>        
        public IList Polygons
        {
            get
            {
                polygonize();
                return polyList;
            }
        }

        /// <summary> 
        /// Compute and returns the list of dangling lines found during polygonization.
        /// </summary>
        public IList Dangles
        {
            get
            {
                polygonize();
                return dangles;
            }
        }

        /// <summary>
        /// Compute and returns the list of cut edges found during polygonization.
        /// </summary>
        public IList CutEdges
        {
            get
            {
                polygonize();
                return cutEdges;
            }
        }

        /// <summary>
        /// Compute and returns the list of lines forming invalid rings found during polygonization.
        /// </summary>
        public IList InvalidRingLines
        {
            get
            {
                polygonize();
                return invalidRingLines;
            }
        }

        /// <summary>
        /// Perform the polygonization, if it has not already been carried out.
        /// </summary>
        private void polygonize()
        {
            // check if already computed
            if (polyList != null)
            {
                return;
            }
            polyList = new ArrayList();

            // if no geometries were supplied it's possible graph could be null
            if (graph == null)
            {
                return;
            }

            dangles = graph.DeleteDangles();
            cutEdges = graph.DeleteCutEdges();
            IList edgeRingList = graph.GetEdgeRings();

            IList validEdgeRingList = new ArrayList();
            invalidRingLines = new ArrayList();
            findValidRings(edgeRingList, validEdgeRingList, invalidRingLines);

            findShellsAndHoles(validEdgeRingList);
            AssignHolesToShells(holeList, shellList);

            polyList = new ArrayList();
            for (IEnumerator i = shellList.GetEnumerator(); i.MoveNext();)
            {
                EdgeRing<TCoordinate> er = i.Current;
                polyList.Add(er.Polygon);
            }
        }

        private void findValidRings(IList edgeRingList, IList validEdgeRingList, IList invalidRingList)
        {
            for (IEnumerator i = edgeRingList.GetEnumerator(); i.MoveNext();)
            {
                EdgeRing<TCoordinate> er = (EdgeRing<TCoordinate>) i.Current;
                if (er.IsValid)
                {
                    validEdgeRingList.Add(er);
                }
                else
                {
                    invalidRingList.Add(er.LineString);
                }
            }
        }

        private void findShellsAndHoles(IList edgeRingList)
        {
            holeList = new ArrayList();
            shellList = new ArrayList();
            for (IEnumerator i = edgeRingList.GetEnumerator(); i.MoveNext();)
            {
                EdgeRing<TCoordinate> er = (EdgeRing<TCoordinate>) i.Current;
                if (er.IsHole)
                {
                    holeList.Add(er);
                }
                else
                {
                    shellList.Add(er);
                }
            }
        }

        private static void AssignHolesToShells(IList holeList, IList shellList)
        {
            for (IEnumerator i = holeList.GetEnumerator(); i.MoveNext();)
            {
                EdgeRing holeER = (EdgeRing) i.Current;
                AssignHoleToShell(holeER, shellList);
            }
        }

        private static void AssignHoleToShell(EdgeRing holeER, IList shellList)
        {
            EdgeRing shell = EdgeRing.FindEdgeRingContaining(holeER, shellList);
            if (shell != null)
            {
                shell.AddHole(holeER.Ring);
            }
        }
    }
}