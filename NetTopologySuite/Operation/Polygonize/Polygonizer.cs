using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Polygonizes a set of Geometrys which contain linework that
    /// represents the edges of a planar graph.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any dimension of Geometry is handled - the constituent linework is extracted
    /// to form the edges.
    /// </para>
    /// <para>
    /// The edges must be correctly noded; that is, they must only meet
    /// at their endpoints.  The Polygonizer will still run on incorrectly noded input
    /// but will not form polygons from incorrected noded edges.
    /// </para>
    /// The Polygonizer reports the follow kinds of errors:
    /// <list type="table">
    /// <item>
    /// <term>Dangles</term>
    /// <description>Edges which have one or both ends which are not incident on another edge endpoint.</description>
    /// </item>
    /// <item>
    /// <term>Cut Edges</term>
    /// <description>Edges which are connected at both ends but which do not form part of polygon.</description>
    /// </item>
    /// <item>
    /// <term>Invalid Ring Lines</term>
    /// <description>Edges which form rings which are invalid (e.g. the component lines contain a self-intersection).</description>
    /// </item>
    /// </list>
    /// </remarks>
    public class Polygonizer<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /*
         * [codekaizen 2008-01-14]  removed during translation of visitor patterns
         *                          to enumeration / query patterns.
         */

        ///// <summary>
        ///// Add every linear element in a point into the polygonizer graph.
        ///// </summary>
        //private class LineStringAdder : IGeometryComponentFilter<TCoordinate>
        //{
        //    private readonly Polygonizer<TCoordinate> _container = null;

        //    public LineStringAdder(Polygonizer<TCoordinate> container)
        //    {
        //        _container = container;
        //    }

        //    public void Filter(IGeometry<TCoordinate> g)
        //    {
        //        if (g is ILineString<TCoordinate>)
        //        {
        //            _container.Add((ILineString<TCoordinate>)g);
        //        }
        //    }
        //}

        //private readonly LineStringAdder _lineStringAdder = null;

        private readonly List<EdgeRing<TCoordinate>> _holeList = new List<EdgeRing<TCoordinate>>();
        private readonly List<ILineString<TCoordinate>> _invalidRingLines = new List<ILineString<TCoordinate>>();
        private readonly List<IPolygon<TCoordinate>> _polyList = new List<IPolygon<TCoordinate>>();
        private readonly List<EdgeRing<TCoordinate>> _shellList = new List<EdgeRing<TCoordinate>>();
        private IEnumerable<ILineString<TCoordinate>> _cutEdges;

        /// <summary>
        /// Initialized with empty collections, in case nothing is computed
        /// </summary>
        private IEnumerable<ILineString<TCoordinate>> _dangles;

        private Boolean _doneComputing;
        private PolygonizeGraph<TCoordinate> _graph;

        /*
         * [codekaizen 2008-01-14]  removed during translation of visitor patterns
         *                          to enumeration / query patterns.
         */
        ///// <summary>
        ///// Create a polygonizer with the same {GeometryFactory}
        ///// as the input <see cref="Geometry{TCoordinate}"/>s.
        ///// </summary>
        //public Polygonizer()
        //{
        //    _lineStringAdder = new LineStringAdder(this);
        //}

        /// <summary>
        /// Compute and returns the list of polygons formed by the polygonization.
        /// </summary>        
        public IList<IPolygon<TCoordinate>> Polygons
        {
            get
            {
                polygonize();
                return _polyList;
            }
        }

        /// <summary> 
        /// Compute and returns the list of dangling lines found during polygonization.
        /// </summary>
        public IEnumerable<ILineString<TCoordinate>> Dangles
        {
            get
            {
                polygonize();
                return _dangles;
            }
        }

        /// <summary>
        /// Compute and returns the list of cut edges found during polygonization.
        /// </summary>
        public IEnumerable<ILineString<TCoordinate>> CutEdges
        {
            get
            {
                polygonize();
                return _cutEdges;
            }
        }

        /// <summary>
        /// Compute and returns the list of lines forming invalid rings found during polygonization.
        /// </summary>
        public IEnumerable<ILineString<TCoordinate>> InvalidRingLines
        {
            get
            {
                polygonize();
                return _invalidRingLines;
            }
        }

        /// <summary>
        /// Add a collection of geometries to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used.
        /// </summary>
        /// <param name="geomList">A list of <see cref="Geometry{TCoordinate}"/>s with linework to be polygonized.</param>
        public void Add(IEnumerable<IGeometry<TCoordinate>> geomList)
        {
            foreach (IGeometry<TCoordinate> geometry in geomList)
            {
                Add(geometry);
            }
        }

        /// <summary>
        /// Add a point to the linework to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used
        /// </summary>
        /// <param name="g">
        /// A <see cref="Geometry{TCoordinate}"/> with linework to be polygonized.
        /// </param>
        public void Add(IGeometry<TCoordinate> g)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            if (g is IHasGeometryComponents<TCoordinate>)
            {
                IHasGeometryComponents<TCoordinate> container
                    = g as IHasGeometryComponents<TCoordinate>;

                foreach (ILineString<TCoordinate> s in container.Components)
                {
                    if (s != null)
                    {
                        addLine(s);
                    }
                }
            }
            else if (g is ILineString<TCoordinate>)
            {
                addLine(g as ILineString<TCoordinate>);
            }
        }

        /// <summary>
        /// Add a linestring to the graph of polygon edges.
        /// </summary>
        /// <param name="line">The <c>LineString</c> to add.</param>
        private void addLine(ILineString<TCoordinate> line)
        {
            // create a new graph using the factory from the input Geometry
            if (_graph == null)
            {
                _graph = new PolygonizeGraph<TCoordinate>(line.Factory);
            }
            _graph.AddEdge(line);
        }

        /// <summary>
        /// Perform the polygonization, if it has not already been carried out.
        /// </summary>
        private void polygonize()
        {
            // check if already computed
            if (_doneComputing)
            {
                return;
            }

            // if no geometries were supplied it's possible graph could be null
            if (_graph == null)
            {
                return;
            }

            _dangles = _graph.DeleteDangles();
            _cutEdges = _graph.DeleteCutEdges();
            IEnumerable<EdgeRing<TCoordinate>> edgeRingList = _graph.GetEdgeRings();

            List<EdgeRing<TCoordinate>> validEdgeRingList = new List<EdgeRing<TCoordinate>>();
            findValidRings(edgeRingList, validEdgeRingList, _invalidRingLines);

            findShellsAndHoles(validEdgeRingList);
            assignHolesToShells(_holeList, _shellList);

            foreach (EdgeRing<TCoordinate> ring in _shellList)
            {
                _polyList.Add(ring.Polygon);
            }

            _doneComputing = true;
        }

        // in Ruby: valid, invalid = edgeRingList.partition{|ring| ring.IsValid?}
        private static void findValidRings(IEnumerable<EdgeRing<TCoordinate>> edgeRingList,
                                           ICollection<EdgeRing<TCoordinate>> validEdgeRingList,
                                           ICollection<ILineString<TCoordinate>> invalidRingList)
        {
            foreach (EdgeRing<TCoordinate> ring in edgeRingList)
            {
                if (ring.IsValid)
                {
                    validEdgeRingList.Add(ring);
                }
                else
                {
                    invalidRingList.Add(ring.LineString);
                }
            }
        }

        private void findShellsAndHoles(IEnumerable<EdgeRing<TCoordinate>> edgeRingList)
        {
            foreach (EdgeRing<TCoordinate> ring in edgeRingList)
            {
                if (ring.IsHole)
                {
                    _holeList.Add(ring);
                }
                else
                {
                    _shellList.Add(ring);
                }
            }
        }

        private static void assignHolesToShells(IEnumerable<EdgeRing<TCoordinate>> holeList,
                                                IEnumerable<EdgeRing<TCoordinate>> shellList)
        {
            foreach (EdgeRing<TCoordinate> hole in holeList)
            {
                AssignHoleToShell(hole, shellList);
            }
        }

        private static void AssignHoleToShell(EdgeRing<TCoordinate> holeER, IEnumerable<EdgeRing<TCoordinate>> shellList)
        {
            EdgeRing<TCoordinate> shell = EdgeRing<TCoordinate>.FindEdgeRingContaining(holeER, shellList);

            if (shell != null)
            {
                shell.AddHole(holeER.Ring);
            }
        }
    }
}