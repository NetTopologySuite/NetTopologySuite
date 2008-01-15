using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;
using GeoAPI.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Operation.Distance
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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Compute the distance between the closest points of two geometries.
        /// </summary>
        /// <param name="g0">A <see cref="Geometry{TCoordinate}"/>.</param>
        /// <param name="g1">Another <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>The distance between the geometries.</returns>
        public static Double Distance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1);
            return distOp.Distance();
        }

        /// <summary>
        /// Test whether two geometries lie within a given distance of each other.
        /// </summary>
        public static Boolean IsWithinDistance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, Double distance)
        {
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1, distance);
            return distOp.Distance() <= distance;
        }

        /// <summary>
        /// Compute the the closest points of two geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <see cref="Geometry{TCoordinate}"/>.</param>
        /// <param name="g1">Another <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>The closest points in the geometries.</returns>
        public static IEnumerable<TCoordinate> ClosestPoints(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            DistanceOp<TCoordinate> distOp = new DistanceOp<TCoordinate>(g0, g1);
            return distOp.ClosestPoints();
        }

        private PointLocator<TCoordinate> _ptLocator = new PointLocator<TCoordinate>();
        private IGeometry<TCoordinate> _g0;
        private IGeometry<TCoordinate> _g1;
        private GeometryLocation<TCoordinate>? _minDistanceLocation0;
        private GeometryLocation<TCoordinate>? _minDistanceLocation1;
        private Double _minDistance = Double.MaxValue;
        private readonly Double _terminateDistance = 0.0;

        /// <summary>
        /// Constructs a <see cref="DistanceOp{TCoordinate}" />  that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        public DistanceOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
            : this(g0, g1, 0) {}

        /// <summary>
        /// Constructs a <see cref="DistanceOp{TCoordinate}" /> that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        /// <param name="terminateDistance">The distance on which to terminate the search.</param>
        public DistanceOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, Double terminateDistance)
        {
            _g0 = g0;
            _g1 = g1;
            _terminateDistance = terminateDistance;
        }

        /// <summary>
        /// Report the distance between the closest points on the input geometries.
        /// </summary>
        /// <returns>The distance between the geometries.</returns>
        public Double Distance()
        {
            computeMinDistance();
            return _minDistance;
        }

        /// <summary>
        /// Report the coordinates of the closest points in the input geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <c>Coordinate</c>s of the closest points.</returns>
        public Pair<TCoordinate>? ClosestPoints()
        {
            computeMinDistance();

            if(!_minDistanceLocation0.HasValue || !_minDistanceLocation1.HasValue)
            {
                return null;
            }

            return new Pair<TCoordinate>(_minDistanceLocation0.Value.Coordinate, 
                _minDistanceLocation1.Value.Coordinate);
        }

        /// <summary>
        /// Report the locations of the closest points in the input geometries.
        /// The locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>
        /// A <see cref="Pair{TItem}"/> of <see cref="GeometryLocation{TCoordinate}"/>s 
        /// for the closest points.
        /// </returns>
        public Pair<GeometryLocation<TCoordinate>>? ClosestLocations()
        {
            computeMinDistance();

            if (!_minDistanceLocation0.HasValue || !_minDistanceLocation1.HasValue)
            {
                return null;
            }

            return new Pair<GeometryLocation<TCoordinate>>(_minDistanceLocation0.Value, 
                _minDistanceLocation1.Value);
        }

        private void updateMinDistance(Double dist)
        {
            if (dist < _minDistance)
            {
                _minDistance = dist;
            }
        }

        private void updateMinDistance(GeometryLocation<TCoordinate>? locGeom0, GeometryLocation<TCoordinate>? locGeom1, Boolean flip)
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

        private void computeMinDistance()
        {
            if (_minDistanceLocation0.HasValue && _minDistanceLocation1.HasValue)
            {
                return;
            }

            computeContainmentDistance();

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            computeLineDistance();
        }

        private void computeContainmentDistance()
        {
            IEnumerable<IPolygon<TCoordinate>> polys0 = GeometryFilter.Filter<IPolygon<TCoordinate>>(_g0);
            IEnumerable<IPolygon<TCoordinate>> polys1 = GeometryFilter.Filter<IPolygon<TCoordinate>>(_g1);
            //IList polys0 = PolygonExtracter<TCoordinate>.GetPolygons(geom[0]);
            //IList polys1 = PolygonExtracter<TCoordinate>.GetPolygons(geom[1]);

            GeometryLocation<TCoordinate>? locPtPoly0;
            GeometryLocation<TCoordinate>? locPtPoly1;

            // test if either point is wholly inside the other
            if (Slice.CountGreaterThan(polys1, 0))
            {
                IEnumerable<GeometryLocation<TCoordinate>> insideLocs0 
                    = ConnectedElementLocationFilter<TCoordinate>.GetLocations(_g0);
                
                locPtPoly0 = computeInside(insideLocs0, polys1, out locPtPoly1);

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
                    = ConnectedElementLocationFilter<TCoordinate>.GetLocations(_g1);

                locPtPoly0 = computeInside(insideLocs1, polys0, out locPtPoly1);
                
                if (_minDistance <= _terminateDistance)
                {
                    // flip locations, since we are testing geom 1 VS geom 0
                    _minDistanceLocation0 = locPtPoly1;
                    _minDistanceLocation1 = locPtPoly0;
                    return;
                }
            }
        }

        private GeometryLocation<TCoordinate>? computeInside(IEnumerable<GeometryLocation<TCoordinate>> locs, 
            IEnumerable<IPolygon<TCoordinate>> polys, out GeometryLocation<TCoordinate>? locPtPoly1)
        {
            locPtPoly1 = null;

            foreach (GeometryLocation<TCoordinate> loc in locs)
            {
                foreach (IPolygon<TCoordinate> poly in polys)
                {
                    GeometryLocation<TCoordinate>? locPtPoly0;

                    locPtPoly0 = computeInside(loc, poly, out locPtPoly1);

                    if (_minDistance <= _terminateDistance)
                    {
                        return locPtPoly0;
                    }
                }
            }

            return null;
        }

        private GeometryLocation<TCoordinate>? computeInside(GeometryLocation<TCoordinate> ptLoc, IPolygon<TCoordinate> poly, 
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

        private void computeLineDistance()
        {
            GeometryLocation<TCoordinate>? locGeom0;
            GeometryLocation<TCoordinate>? locGeom1;

            /*
             * Geometries are not wholely inside, so compute distance from lines and points
             * of one to lines and points of the other
             */
            IEnumerable<ILineString<TCoordinate>> lines0 = GeometryFilter.Filter<ILineString<TCoordinate>>(_g0); // LinearComponentExtracter<TCoordinate>.GetLines(_g0);
            IEnumerable<ILineString<TCoordinate>> lines1 = GeometryFilter.Filter<ILineString<TCoordinate>>(_g1); // LinearComponentExtracter<TCoordinate>.GetLines(_g1);

            IEnumerable<IPoint<TCoordinate>> pts0 = GeometryFilter.Filter<IPoint<TCoordinate>>(_g0); // PointExtracter<TCoordinate>.GetPoints(_g0);
            IEnumerable<IPoint<TCoordinate>> pts1 = GeometryFilter.Filter<IPoint<TCoordinate>>(_g1); // PointExtracter<TCoordinate>.GetPoints(_g1);

            // bail whenever minDistance goes to zero, since it can't get any less
            locGeom0 = computeMinDistanceLines(lines0, lines1, out locGeom1);

            updateMinDistance(locGeom0, locGeom1, false);

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            locGeom0 = computeMinDistanceLinesPoints(lines0, pts1, out locGeom1);
            
            updateMinDistance(locGeom0, locGeom1, false);

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            locGeom0 = computeMinDistanceLinesPoints(lines1, pts0, out locGeom1);

            updateMinDistance(locGeom0, locGeom1, true);

            if (_minDistance <= _terminateDistance)
            {
                return;
            }

            locGeom0 = computeMinDistancePoints(pts0, pts1, out locGeom1);

            updateMinDistance(locGeom0, locGeom1, false);
        }

        private GeometryLocation<TCoordinate>? computeMinDistanceLines(IEnumerable<ILineString<TCoordinate>> lines0, 
            IEnumerable<ILineString<TCoordinate>> lines1, out GeometryLocation<TCoordinate>? locGeom1)
        {
            locGeom1 = null;

            foreach (ILineString<TCoordinate> line0 in lines0)
            {
                foreach (ILineString<TCoordinate> line1 in lines1)
                {
                    GeometryLocation<TCoordinate>? locGeom0 = computeMinDistance(line0, line1, out locGeom1);

                    if (_minDistance <= _terminateDistance)
                    {
                        return locGeom0;
                    }
                }
            }

            return null;
        }

        private GeometryLocation<TCoordinate>? computeMinDistancePoints(IEnumerable<IPoint<TCoordinate>> points0, 
            IEnumerable<IPoint<TCoordinate>> points1, out GeometryLocation<TCoordinate>? locGeom1)
        {
            locGeom1 = null;

            foreach (IPoint<TCoordinate> pt0 in points0)
            {
                foreach (IPoint<TCoordinate> pt1 in points1)
                {
                    GeometryLocation<TCoordinate>? locGeom0 = null;

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

            return null;
        }

        private GeometryLocation<TCoordinate>? computeMinDistanceLinesPoints(
            IEnumerable<ILineString<TCoordinate>> lines, IEnumerable<IPoint<TCoordinate>> points, 
            out GeometryLocation<TCoordinate>? locGeom1)
        {
            locGeom1 = null; 

            foreach (ILineString<TCoordinate> line in lines)
            {
                foreach (IPoint<TCoordinate> point in points)
                {
                    GeometryLocation<TCoordinate>? locGeom0 = computeMinDistance(line, point, out locGeom1);

                    if (_minDistance <= _terminateDistance)
                    {
                        return locGeom0;
                    }   
                }
            }
            return null;
        }

        private GeometryLocation<TCoordinate>? computeMinDistance(ILineString<TCoordinate> line0, 
            ILineString<TCoordinate> line1, out GeometryLocation<TCoordinate>? locGeom1)
        {
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
                    GeometryLocation<TCoordinate>? locGeom0 = null;

                    Double dist = CGAlgorithms<TCoordinate>.DistanceLineLine(pair0, pair1);

                    if (dist < _minDistance)
                    {
                        _minDistance = dist;
                        LineSegment<TCoordinate> seg0 = new LineSegment<TCoordinate>(pair0);
                        LineSegment<TCoordinate> seg1 = new LineSegment<TCoordinate>(pair1);
                        Pair<TCoordinate> closestPt = Slice.GetPair(seg0.ClosestPoints(seg1)).Value;
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
            
            return null;
        }

        private GeometryLocation<TCoordinate>? computeMinDistance(ILineString<TCoordinate> line, IPoint<TCoordinate> pt, out GeometryLocation<TCoordinate>? locGeom1)
        {
            locGeom1 = null;

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
                GeometryLocation<TCoordinate>? locGeom0 = null;

                TCoordinate coord0 = pair.First;
                TCoordinate coord1 = pair.Second;

                Double dist = CGAlgorithms<TCoordinate>.DistancePointLine(coord, coord0, coord1);

                if (dist < _minDistance)
                {
                    _minDistance = dist;
                    LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(coord0, coord1);
                    TCoordinate segClosestPoint = seg.ClosestPoint(coord);
                    locGeom0 = new GeometryLocation<TCoordinate>(line, i, segClosestPoint);
                    locGeom1 = new GeometryLocation<TCoordinate>(pt, 0, coord);
                }

                if (_minDistance <= _terminateDistance)
                {
                    return locGeom0;
                }

                i += 1;
            }

            return null;
        }
    }
}