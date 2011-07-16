using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Computes the distance and
    /// closest points between two <see cref="Geometry{TCoordinate}"/>s.
    /// </summary>
    /// <remarks>
    /// The distance computation finds a pair of points in the input geometries
    /// which have minimum distance between them.  These points may
    /// not be vertices of the geometries, but may lie in the interior of
    /// a line segment. In this case the coordinate computed is a close
    /// approximation to the exact point.
    /// The algorithms used are straightforward O(n^2)
    /// comparisons.  This worst-case performance could be improved on
    /// by using Voronoi techniques.
    /// </remarks>
    public class DistanceOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IGeometry<TCoordinate> _g0;
        private readonly IGeometry<TCoordinate> _g1;
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly PointLocator<TCoordinate> _ptLocator = new PointLocator<TCoordinate>();
        private readonly Double _terminateDistance;
        private Double _minDistance = Double.MaxValue;
        private GeometryLocation<TCoordinate>? _minDistanceLocation0;
        private GeometryLocation<TCoordinate>? _minDistanceLocation1;

        /// <summary>
        /// Constructs a <see cref="DistanceOp{TCoordinate}" />  that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        public DistanceOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
            : this(g0, g1, 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="DistanceOp{TCoordinate}" /> that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        /// <param name="terminateDistance">The distance on which to terminate the search.</param>
        public DistanceOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, Double terminateDistance)
        {
            if (g0 == null) throw new ArgumentNullException("g0");
            if (g1 == null) throw new ArgumentNullException("g1");

            _g0 = g0;
            _g1 = g1;
            _geoFactory = _g0.Factory ?? _g1.Factory;
            _terminateDistance = terminateDistance;
        }

        /// <summary>
        /// Gets the distance between the closest points on the input geometries.
        /// </summary>
        /// <returns>The distance between the geometries.</returns>
        public Double Distance
        {
            get
            {
                if (_g0 == null || _g1 == null)
                    throw new ArgumentNullException("null geometries are not supported");
                if (_g0.IsEmpty || _g1.IsEmpty)
                    return 0.0;

                ComputeMinDistance();
                return _minDistance;
            }
        }

        /// <summary>
        /// Compute the distance between the closest points of two geometries.
        /// </summary>
        /// <param name="g0">A <see cref="IGeometry{TCoordinate}"/>.</param>
        /// <param name="g1">Another <see cref="IGeometry{TCoordinate}"/>.</param>
        /// <returns>The distance between the geometries.</returns>
        public static Double FindDistance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1);
            return distOp.Distance;
        }

        /// <summary>
        /// Test whether two geometries lie within a given distance of each other.
        /// </summary>
        /// <param name="g0">The first <see cref="IGeometry{TCoordinate}"/> to comapre.</param>
        /// <param name="g1">The second <see cref="IGeometry{TCoordinate}"/> to comapre.</param>
        /// <param name="distance">The distance value to test.</param>
        public static Boolean IsWithinDistance(IGeometry<TCoordinate> g0,
                                               IGeometry<TCoordinate> g1,
                                               Double distance)
        {
            return IsWithinDistance(g0, g1, distance, Tolerance.Global);
        }

        /// <summary>
        /// Test whether two geometries lie within a given distance of each other, within
        /// the specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="g0">The first <see cref="IGeometry{TCoordinate}"/> to comapre.</param>
        /// <param name="g1">The second <see cref="IGeometry{TCoordinate}"/> to comapre.</param>
        /// <param name="distance">The distance value to test.</param>
        /// <param name="tolerance">The tolerance which the comparison should be in.</param>
        public static Boolean IsWithinDistance(IGeometry<TCoordinate> g0,
                                               IGeometry<TCoordinate> g1,
                                               Double distance,
                                               Tolerance tolerance)
        {
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1, distance);
            return tolerance.LessOrEqual(distOp.Distance, distance);
        }

        /// <summary>
        /// Compute the the closest points of two geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <see cref="Geometry{TCoordinate}"/>.</param>
        /// <param name="g1">Another <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>The closest points in the geometries.</returns>
        [Obsolete("Use NearestPoints(g1,g2) instead")]
        public static IEnumerable<TCoordinate> ClosestPoints(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return NearestPoints(g0, g1);
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1);
            return distOp.ClosestPoints();
        }

        /// <summary>
        /// Report the coordinates of the closest points in the input geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <c>Coordinate</c>s of the closest points.</returns>
        [Obsolete("Use NearestPoints instead")]
        public Pair<TCoordinate>? ClosestPoints()
        {
            return NearestPoints();
        }

        /// <summary>
        /// Report the locations of the closest points in the input geometries.
        /// The locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>
        /// A <see cref="Pair{TItem}"/> of <see cref="GeometryLocation{TCoordinate}"/>s 
        /// for the closest points.
        /// </returns>
        [Obsolete("Use NearestLocations() instead")]
        public Pair<GeometryLocation<TCoordinate>>? ClosestLocations()
        {
            return NearestLocations();
        }

        // [codekaizen 2008-01-14] Not used in JTS 
        // /JTS/src/com/vividsolutions/jts/operation/distance/DistanceOp.java:1.17

        //private void updateMinDistance(Double dist)
        //{
        //    if (dist < _minDistance)
        //    {
        //        _minDistance = dist;
        //    }
        //}

        private void UpdateMinDistance(GeometryLocation<TCoordinate>? locGeom0,
                                       GeometryLocation<TCoordinate>? locGeom1,
                                       Boolean flip)
        {
            // if not set then don't update
            if (locGeom0 == null || locGeom1 == null)
            {
                return;
            }

            if (flip)
            {
                _minDistanceLocation0 = locGeom1.Value;
                _minDistanceLocation1 = locGeom0.Value;
            }
            else
            {
                _minDistanceLocation0 = locGeom0.Value;
                _minDistanceLocation1 = locGeom1.Value;
            }
        }

        private void ComputeMinDistance()
        {
            if (_minDistanceLocation0.HasValue && _minDistanceLocation1.HasValue)
            {
                return;
            }

            ComputeContainmentDistance();

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            ComputeLineDistance();
        }

        private void ComputeContainmentDistance()
        {
            IEnumerable<IPolygon<TCoordinate>> polys0 = PolygonExtracter<TCoordinate>.GetPolygons(_g0);
            IEnumerable<IPolygon<TCoordinate>> polys1 = PolygonExtracter<TCoordinate>.GetPolygons(_g1);

            GeometryLocation<TCoordinate>? locPtPoly0;
            GeometryLocation<TCoordinate>? locPtPoly1;

            // test if either point is wholly inside the other
            if (Slice.CountGreaterThan(polys1, 0))
            {
                IEnumerable<GeometryLocation<TCoordinate>> insideLocs0
                    = GetLocations(_g0);

                locPtPoly0 = ComputeInside(insideLocs0, polys1, out locPtPoly1);

                if (_minDistance <= _terminateDistance)
                {
                    _minDistanceLocation0 = locPtPoly0;
                    _minDistanceLocation1 = locPtPoly1;
                    return;
                }
            }

            if (Slice.CountGreaterThan(polys0, 0))
            {
                IEnumerable<GeometryLocation<TCoordinate>> insideLocs1
                    = GetLocations(_g1);

                locPtPoly0 = ComputeInside(insideLocs1, polys0, out locPtPoly1);

                if (_minDistance <= _terminateDistance)
                {
                    // flip locations, since we are testing geom 1 to geom 0
                    _minDistanceLocation0 = locPtPoly1;
                    _minDistanceLocation1 = locPtPoly0;
                    return;
                }
            }
        }

        private static IEnumerable<GeometryLocation<TCoordinate>> GetLocations(IGeometry<TCoordinate> g)
        {
            if (g is IGeometryCollection<TCoordinate>)
            {
                foreach (IGeometry<TCoordinate> geometry in (g as IGeometryCollection<TCoordinate>))
                {
                    foreach (GeometryLocation<TCoordinate> location in GetLocations(geometry))
                    {
                        yield return location;
                    }
                }
            }
            else
            {
                if (g is IPoint || g is ILineString || g is IPolygon)
                {
                    yield return new GeometryLocation<TCoordinate>(g, 0, g.Coordinates[0]);
                }
            }
        }

        private GeometryLocation<TCoordinate>? ComputeInside(IEnumerable<GeometryLocation<TCoordinate>> locs,
                                                             IEnumerable<IPolygon<TCoordinate>> polys,
                                                             out GeometryLocation<TCoordinate>? locPtPoly1)
        {
            GeometryLocation<TCoordinate>? locPtPoly0 = null;
            locPtPoly1 = null;

            foreach (GeometryLocation<TCoordinate> loc in locs)
            {
                foreach (IPolygon<TCoordinate> poly in polys)
                {
                    GeometryLocation<TCoordinate>? l0, l1;
                    l0 = ComputeInside(loc, poly, out l1);;
                    if (l0.HasValue)
                    {
                        locPtPoly0 = l0;
                        locPtPoly1 = l1;
                    }
                    if (_minDistance <= _terminateDistance)
                    {
                        return locPtPoly0;
                    }
                }
            }

            return locPtPoly0;
        }

        private GeometryLocation<TCoordinate>? ComputeInside(GeometryLocation<TCoordinate> ptLoc,
                                                             IPolygon<TCoordinate> poly,
                                                             out GeometryLocation<TCoordinate>? locPtPoly1)
        {
            TCoordinate pt = ptLoc.Coordinate;
            locPtPoly1 = null;

            if (Locations.Exterior != _ptLocator.Locate(pt, poly))
            {
                _minDistance = 0.0;
                GeometryLocation<TCoordinate> locPoly = new GeometryLocation<TCoordinate>(poly, pt);
                locPtPoly1 = locPoly;
                return ptLoc;
            }

            return null;
        }

        private void ComputeLineDistance()
        {
            GeometryLocation<TCoordinate>? locGeom0;
            GeometryLocation<TCoordinate>? locGeom1;

            /*
             * Geometries are not wholely inside, so compute distance from lines and points
             * of one to lines and points of the other
             */
            IEnumerable<ILineString<TCoordinate>> lines0 = LinearComponentExtracter<TCoordinate>.GetLines(_g0);
            IEnumerable<ILineString<TCoordinate>> lines1 = LinearComponentExtracter<TCoordinate>.GetLines(_g1);

            IEnumerable<IPoint<TCoordinate>> pts0 = PointExtracter<TCoordinate>.GetPoints(_g0);
            IEnumerable<IPoint<TCoordinate>> pts1 = PointExtracter<TCoordinate>.GetPoints(_g1);

            // bail whenever minDistance goes to zero, since it can't get any less
            locGeom0 = computeMinDistanceLines(lines0, lines1, out locGeom1);

            UpdateMinDistance(locGeom0, locGeom1, false);

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            locGeom0 = ComputeMinDistanceLinesPoints(lines0, pts1, out locGeom1);

            UpdateMinDistance(locGeom0, locGeom1, false);

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            locGeom0 = ComputeMinDistanceLinesPoints(lines1, pts0, out locGeom1);

            UpdateMinDistance(locGeom0, locGeom1, true);

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            locGeom0 = computeMinDistancePoints(pts0, pts1, out locGeom1);

            UpdateMinDistance(locGeom0, locGeom1, false);
        }

        private GeometryLocation<TCoordinate>? computeMinDistanceLines(IEnumerable<ILineString<TCoordinate>> lines0,
                                                                       IEnumerable<ILineString<TCoordinate>> lines1,
                                                                       out GeometryLocation<TCoordinate>? locGeom1)
        {
            GeometryLocation<TCoordinate>? locGeom0 = null;
            locGeom1 = null;

            foreach (ILineString<TCoordinate> line0 in lines0)
            {
                foreach (ILineString<TCoordinate> line1 in lines1)
                {

                    GeometryLocation<TCoordinate>? l0, l1;
                    l0 = ComputeMinDistance(line0, line1, out l1);
                    if( l0.HasValue )
                    {
                        locGeom0 = l0;
                        locGeom1 = l1;
                    }

                    if (_minDistance <= _terminateDistance)
                    {
                        return locGeom0;
                    }
                }
            }

            return locGeom0;
        }

        private GeometryLocation<TCoordinate>? computeMinDistancePoints(IEnumerable<IPoint<TCoordinate>> points0,
                                                                        IEnumerable<IPoint<TCoordinate>> points1,
                                                                        out GeometryLocation<TCoordinate>? locGeom1)
        {
            locGeom1 = null;
                    GeometryLocation<TCoordinate>? locGeom0 = null;

            foreach (IPoint<TCoordinate> pt0 in points0)
            {
                foreach (IPoint<TCoordinate> pt1 in points1)
                {

                    Double dist = pt0.Coordinate.Distance(pt1.Coordinate);

                    if (dist < _minDistance)
                    {
                        _minDistance = dist;

#warning this is wrong - need to determine closest points on both segments!!!
                        locGeom0 = new GeometryLocation<TCoordinate>(pt0, 0, pt0.Coordinate);
                        locGeom1 = new GeometryLocation<TCoordinate>(pt1, 0, pt1.Coordinate);
                    }

                    if (_minDistance <= _terminateDistance)
                    {
                        return locGeom0;
                    }
                }
            }

            return locGeom0;
        }

        private GeometryLocation<TCoordinate>? ComputeMinDistanceLinesPoints(
            IEnumerable<ILineString<TCoordinate>> lines,
            IEnumerable<IPoint<TCoordinate>> points,
            out GeometryLocation<TCoordinate>? locGeom1)
        {
            GeometryLocation<TCoordinate>? locGeom0 = null;
            locGeom1 = null;

            foreach (ILineString<TCoordinate> line in lines)
            {
                foreach (IPoint<TCoordinate> point in points)
                {
                    GeometryLocation<TCoordinate>? l0, l1;
                    l0 = ComputeMinDistance(line, point, out l1);
                    if (l0.HasValue)
                    {
                        locGeom0 = l0;
                        locGeom1 = l1;
                    }

                    if (_minDistance <= _terminateDistance)
                    {
                        return locGeom0;
                    }
                }
            }
            return locGeom0;
        }

        private GeometryLocation<TCoordinate>? ComputeMinDistance(ILineString<TCoordinate> line0,
                                                                  ILineString<TCoordinate> line1,
                                                                  out GeometryLocation<TCoordinate>? locGeom1)
        {
            GeometryLocation<TCoordinate>? locGeom0 = null;
            locGeom1 = null;

            if (line0.Extents.Distance(line1.Extents) > _minDistance)
            {
                return null;
            }

            IEnumerable<TCoordinate> coord0 = line0.Coordinates;
            IEnumerable<TCoordinate> coord1 = line1.Coordinates;

            Int32 i = 0, j = 0;

            // brute force approach!
            foreach (Pair<TCoordinate> pair0 in Slice.GetOverlappingPairs(coord0))
            {
                foreach (Pair<TCoordinate> pair1 in Slice.GetOverlappingPairs(coord1))
                {
                    Double dist = CGAlgorithms<TCoordinate>.DistanceLineLine(pair0, pair1);

                    if (dist < _minDistance)
                    {
                        _minDistance = dist;
                        LineSegment<TCoordinate> seg0 = new LineSegment<TCoordinate>(pair0);
                        LineSegment<TCoordinate> seg1 = new LineSegment<TCoordinate>(pair1);
                        IEnumerable<TCoordinate> points = seg0.ClosestPoints(seg1, _geoFactory);
                        Pair<TCoordinate> closestPt = Slice.GetPair(points).Value;
                        locGeom0 = new GeometryLocation<TCoordinate>(line0, i, closestPt.First);
                        locGeom1 = new GeometryLocation<TCoordinate>(line1, j, closestPt.Second);
                    }

                    if (_minDistance <= _terminateDistance)
                    {
                        return locGeom0;
                    }

                    j += 1;
                }

                i += 1;
            }

            return locGeom0;
        }

        private GeometryLocation<TCoordinate>? ComputeMinDistance(ILineString<TCoordinate> line,
                                                                  IPoint<TCoordinate> pt,
                                                                  out GeometryLocation<TCoordinate>? locGeom1)
        {
            GeometryLocation<TCoordinate>? locGeom0 = _minDistanceLocation0;
            locGeom1 = _minDistanceLocation1;

            if (line.Extents.Distance(pt.Extents) > _minDistance)
            {
                return null;
            }

            IEnumerable<TCoordinate> lineCoordinates = line.Coordinates;
            TCoordinate coord = pt.Coordinate;

            Int32 i = 0;

            // brute force approach!
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(lineCoordinates))
            {

                TCoordinate coord0 = pair.First;
                TCoordinate coord1 = pair.Second;

                Double dist = CGAlgorithms<TCoordinate>.DistancePointLine(coord, coord0, coord1);

                if (dist < _minDistance)
                {
                    _minDistance = dist;
                    LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(coord0, coord1);
                    TCoordinate segClosestPoint = seg.ClosestPoint(coord, _geoFactory.CoordinateFactory);
                    locGeom0 = new GeometryLocation<TCoordinate>(line, i, segClosestPoint);
                    locGeom1 = new GeometryLocation<TCoordinate>(pt, 0, coord);
                }

                if (_minDistance <= _terminateDistance)
                {
                    return locGeom0;
                }

                i += 1;
            }

            return locGeom0;
        }
        /// <summary>
        /// Compute the the nearest points of two geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <see cref="Geometry{TCoordinate}"/>.</param>
        /// <param name="g1">Another <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>The nearest points in the geometries.</returns>
        public static IEnumerable<TCoordinate> NearestPoints(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1);
            return distOp.NearestPoints();
        }


        /// <summary>
        /// Report the coordinates of the nearest points in the input geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <c>Coordinate</c>s of the nearest points.</returns>
        public Pair<TCoordinate>? NearestPoints()
        {
            ComputeMinDistance();

            if (!_minDistanceLocation0.HasValue || !_minDistanceLocation1.HasValue)
            {
                return null;
            }

            return new Pair<TCoordinate>(_minDistanceLocation0.Value.Coordinate,
                                         _minDistanceLocation1.Value.Coordinate);
        }

        /// <summary>
        /// Report the locations of the nearest points in the input geometries.
        /// The locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>
        /// A <see cref="Pair{TItem}"/> of <see cref="GeometryLocation{TCoordinate}"/>s 
        /// for the nearest points.
        /// </returns>
        public Pair<GeometryLocation<TCoordinate>>? NearestLocations()
        {
            ComputeMinDistance();

            if (!_minDistanceLocation0.HasValue || !_minDistanceLocation1.HasValue)
            {
                return null;
            }

            return new Pair<GeometryLocation<TCoordinate>>(_minDistanceLocation0.Value,
                                                           _minDistanceLocation1.Value);
        }
    }
}