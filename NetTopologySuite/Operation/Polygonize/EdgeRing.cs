using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Represents a ring of <c>PolygonizeDirectedEdge</c>s which form
    /// a ring of a polygon.  The ring may be either an outer shell or a hole.
    /// </summary>
    public class EdgeRing<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        // cache the following data for efficiency
        private ILinearRing<TCoordinate> _ring = null;
        private readonly IGeometryFactory<TCoordinate> _factory = null;
        private readonly List<DirectedEdge<TCoordinate>> _deList = new List<DirectedEdge<TCoordinate>>();
        private readonly List<TCoordinate> _ringPoints = new List<TCoordinate>();
        private List<ILinearRing<TCoordinate>> _holes;

        /// <summary>
        /// Find the innermost enclosing shell EdgeRing containing the argument EdgeRing, if any.
        /// The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        /// The algorithm used depends on the fact that:
        /// ring A contains ring B iff envelope(ring A) contains envelope(ring B).
        /// This routine is only safe to use if the chosen point of the hole
        /// is known to be properly contained in a shell
        /// (which is guaranteed to be the case if the hole does not touch its shell).
        /// </summary>
        /// <returns>
        /// Containing EdgeRing, if there is one, OR
        /// null if no containing EdgeRing is found.
        /// </returns>
        public static EdgeRing<TCoordinate> FindEdgeRingContaining(EdgeRing<TCoordinate> testEr,
                                                                   IEnumerable<EdgeRing<TCoordinate>> shellList)
        {
            ILinearRing<TCoordinate> teString = testEr.Ring;
            IExtents<TCoordinate> testEnv = teString.Extents;
            TCoordinate testPt = teString.Coordinates[0];

            EdgeRing<TCoordinate> minShell = null;
            IExtents<TCoordinate> minEnv = null;

            foreach (EdgeRing<TCoordinate> shell in shellList)
            {
                ILinearRing<TCoordinate> tryRing = shell.Ring;
                IExtents<TCoordinate> tryEnv = tryRing.Extents;

                if (minShell != null)
                {
                    minEnv = minShell.Ring.Extents;
                }

                Boolean isContained = false;

                // the hole envelope cannot equal the shell envelope
                if (tryEnv.Equals(testEnv))
                {
                    continue;
                }

                testPt = CoordinateArrays.PointNotInList(teString.Coordinates, tryRing.Coordinates);

                if (tryEnv.Contains(testEnv) && CGAlgorithms<TCoordinate>.IsPointInRing(testPt, tryRing.Coordinates))
                {
                    isContained = true;
                }

                // check if this new containing ring is smaller than the current minimum ring
                if (isContained)
                {
                    if (minShell == null || minEnv.Contains(tryEnv))
                    {
                        minShell = shell;
                    }
                }
            }

            return minShell;
        }

        /// <summary>
        /// Finds a point in a list of points which is not contained in another list of points.
        /// </summary>
        /// <param name="testPoints">The <typeparamref name="TCoordinate"/>s to test.</param>
        /// <param name="points">
        /// An array of <typeparamref name="TCoordinate"/>s 
        /// to test the input points against.
        /// </param>
        /// <returns>
        /// A <c>Coordinate</c> from <c>testPts</c> which is not in <c>pts</c>, 
        /// or <see langword="null" />.</returns>
        public static TCoordinate PointNotInList(IEnumerable<TCoordinate> testPoints, IEnumerable<TCoordinate> points)
        {
            foreach (TCoordinate testPoint in testPoints)
            {
                if (!IsInList(testPoint, points))
                {
                    return testPoint;
                }
            }

            return default(TCoordinate);
        }

        /// <summary>
        /// Tests whether a given point is in an array of points.
        /// Uses a value-based test.
        /// </summary>
        /// <param name="testPoint">A <typeparamref name="TCoordinate"/> for the test point.</param>
        /// <param name="points">An array of <typeparamref name="TCoordinate"/>s to test,</param>
        /// <returns><see langword="true"/> if the point is in the array.</returns>
        public static Boolean IsInList(TCoordinate testPoint, IEnumerable<TCoordinate> points)
        {
            foreach (TCoordinate point in points)
            {
                if (testPoint.Equals(point))
                {
                    return true;
                }
            }

            return false;
        }

        public EdgeRing(IGeometryFactory<TCoordinate> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Adds a DirectedEdge which is known to form part of this ring.
        /// </summary>
        /// <param name="de">The DirectedEdge to add.</param>
        public void Add(DirectedEdge<TCoordinate> de)
        {
            _deList.Add(de);
        }

        /// <summary>
        /// Tests whether this ring is a hole.
        /// Due to the way the edges in the polyongization graph are linked,
        /// a ring is a hole if it is oriented counter-clockwise.
        /// </summary>
        /// <returns><see langword="true"/> if this ring is a hole.</returns>
        public Boolean IsHole
        {
            get { return CGAlgorithms<TCoordinate>.IsCCW(Ring.Coordinates); }
        }

        /// <summary>
        /// Adds a hole to the polygon formed by this ring.
        /// </summary>
        /// <param name="hole">The LinearRing forming the hole.</param>
        public void AddHole(ILinearRing<TCoordinate> hole)
        {
            if (_holes == null)
            {
                _holes = new List<ILinearRing<TCoordinate>>();
            }

            _holes.Add(hole);
        }

        /// <summary>
        /// Computes and returns the Polygon formed by this ring and any 
        /// contained holes.
        /// </summary>
        public IPolygon<TCoordinate> Polygon
        {
            get
            {
                IPolygon<TCoordinate> poly = _factory.CreatePolygon(_ring, _holes);
                return poly;
            }
        }

        /// <summary>
        /// Tests if the LinearRing ring formed by this edge ring is topologically valid.
        /// </summary>
        public Boolean IsValid
        {
            get
            {
                if (_ringPoints.Count <= 3)
                {
                    return false;
                }

                return _ring.IsValid;
            }
        }

        ///// <summary>
        ///// Computes and returns the list of coordinates which are contained in this ring.
        ///// The coordinatea are computed once only and cached.
        ///// </summary>
        //private IEnumerable<TCoordinate> Coordinates
        //{
        //    get
        //    {
        //        if (_ringPoints == null)
        //        {
        //            for (IEnumerator i = _deList.GetEnumerator(); i.MoveNext();)
        //            {
        //                DirectedEdge de = (DirectedEdge) i.Current;
        //                PolygonizeEdge<TCoordinate> edge = de.Edge as PolygonizeEdge<TCoordinate>;
        //                Debug.Assert(edge != null);
        //                addEdge(edge.Line.Coordinates, de.EdgeDirection, _ringPoints);
        //            }
        //        }

        //        return _ringPoints;
        //    }
        //}

        /// <summary>
        /// Gets the coordinates for this ring as a <c>LineString</c>.
        /// Used to return the coordinates in this ring
        /// as a valid point, when it has been detected that the ring is topologically
        /// invalid.
        /// </summary>        
        public ILineString<TCoordinate> LineString
        {
            get { return _factory.CreateLineString(_ringPoints); }
        }

        /// <summary>
        /// Returns this ring as a LinearRing, or null if an Exception occurs while
        /// creating it (such as a topology problem). Details of problems are written to
        /// standard output.
        /// </summary>
        public ILinearRing<TCoordinate> Ring
        {
            get
            {
                if (_ring != null)
                {
                    return _ring;
                }

                try
                {
                    _ring = _factory.CreateLinearRing(_ringPoints);
                }
                catch (NtsException) {}

                return _ring;
            }
        }

        private static void addEdge(IEnumerable<TCoordinate> coords, Boolean isForward, IList<TCoordinate> coordinates)
        {
            foreach (TCoordinate coord in coords)
            {
                if (!coordinates.Contains(coord))
                {
                    if (isForward)
                    {
                        coordinates.Add(coord);
                    }
                    else
                    {
                        coordinates.Insert(0, coord);
                    }
                }
            }
        }
    }
}