using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
#if !HAS_SYSTEM_APPLICATIONEXCEPTION
using ApplicationException = System.Exception;
#endif

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Computes the distance and
    /// closest points between two <c>Geometry</c>s.
    /// The distance computation finds a pair of points in the input geometries
    /// which have minimum distance between them.  These points may
    /// not be vertices of the geometries, but may lie in the interior of
    /// a line segment. In this case the coordinate computed is a close
    /// approximation to the exact point.
    /// The algorithms used are straightforward O(n^2)
    /// comparisons.  This worst-case performance could be improved on
    /// by using Voronoi techniques.
    /// </summary>
    public class DistanceOp
    {
        /// <summary>
        /// Compute the distance between the closest points of two geometries.
        /// </summary>
        /// <param name="g0">A <c>Geometry</c>.</param>
        /// <param name="g1">Another <c>Geometry</c>.</param>
        /// <returns>The distance between the geometries.</returns>
        public static double Distance(IGeometry g0, IGeometry g1)
        {
            var distOp = new DistanceOp(g0, g1);
            return distOp.Distance();
        }

        /// <summary>
        /// Test whether two geometries lie within a given distance of each other.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsWithinDistance(IGeometry g0, IGeometry g1, double distance)
        {
            var distOp = new DistanceOp(g0, g1, distance);
            return distOp.Distance() <= distance;
        }

        /// <summary>
        /// Compute the the closest points of two geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <c>Geometry</c>.</param>
        /// <param name="g1">Another <c>Geometry</c>.</param>
        /// <returns>The closest points in the geometries.</returns>
        public static Coordinate[] NearestPoints(IGeometry g0, IGeometry g1)
        {
            var distOp = new DistanceOp(g0, g1);
            return distOp.NearestPoints();
        }

        /// <summary>
        /// Compute the the closest points of two geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <c>Geometry</c>.</param>
        /// <param name="g1">Another <c>Geometry</c>.</param>
        /// <returns>The closest points in the geometries.</returns>
        [Obsolete("Renamed to NearestPoints")]
        public static Coordinate[] ClosestPoints(IGeometry g0, IGeometry g1)
        {
            return NearestPoints(g0, g1);
        }

        private readonly PointLocator _ptLocator = new PointLocator();
        private readonly IGeometry[] _geom;
        private GeometryLocation[] _minDistanceLocation;
        private double _minDistance = double.MaxValue;
        private readonly double _terminateDistance;

        /// <summary>
        /// Constructs a <see cref="DistanceOp" />  that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        public DistanceOp(IGeometry g0, IGeometry g1)
        : this(g0, g1, 0) { }

        /// <summary>
        /// Constructs a <see cref="DistanceOp" /> that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <param name="terminateDistance">The distance on which to terminate the search.</param>
        public DistanceOp(IGeometry g0, IGeometry g1, double terminateDistance)
        {
            _geom = new[] { g0, g1 };
            _terminateDistance = terminateDistance;
        }

        /// <summary>
        /// Report the distance between the closest points on the input geometries.
        /// </summary>
        /// <returns>The distance between the geometries<br/>
        /// or <value>0</value> if either input geometry is empty.</returns>
        /// <exception cref="ApplicationException"> if either input geometry is null</exception>
        public double Distance()
        {
            if (_geom[0] == null || _geom[1] == null)
                throw new ApplicationException("null geometries are not supported");
            if (_geom[0].IsEmpty || _geom[1].IsEmpty)
                return 0.0;
            ComputeMinDistance();
            return _minDistance;
        }

        /// <summary>
        /// Report the coordinates of the closest points in the input geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <c>Coordinate</c>s of the closest points.</returns>
        [Obsolete("Renamed to NearestPoints")]
        public Coordinate[] ClosestPoints()
        {
            return NearestPoints();
        }

        /// <summary>
        /// Report the locations of the closest points in the input geometries.
        /// The locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <see cref="GeometryLocation"/>s for the closest points.</returns>
        [Obsolete("Renamed to NearestLocations")]
        public GeometryLocation[] ClosestLocations()
        {
            return NearestLocations();
        }

        /// <summary>
        /// Report the coordinates of the nearest points in the input geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <c>Coordinate</c>s of the nearest points.</returns>
        public Coordinate[] NearestPoints()
        {
            ComputeMinDistance();
            var nearestPts = new[] { _minDistanceLocation[0].Coordinate,
                                     _minDistanceLocation[1].Coordinate };
            return nearestPts;
        }

        /// <summary>
        /// Report the locations of the nearest points in the input geometries.
        /// The locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <see cref="GeometryLocation"/>s for the nearest points.</returns>
        public GeometryLocation[] NearestLocations()
        {
            ComputeMinDistance();
            return _minDistanceLocation;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dist"></param>
        private void UpdateMinDistance(double dist)
        {
            if (dist < _minDistance)
                _minDistance = dist;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="locGeom"></param>
        /// <param name="flip"></param>
        private void UpdateMinDistance(GeometryLocation[] locGeom, bool flip)
        {
            // if not set then don't update
            if (locGeom[0] == null)
                return;
            if (flip)
            {
                _minDistanceLocation[0] = locGeom[1];
                _minDistanceLocation[1] = locGeom[0];
            }
            else
            {
                _minDistanceLocation[0] = locGeom[0];
                _minDistanceLocation[1] = locGeom[1];
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void ComputeMinDistance()
        {
            if (_minDistanceLocation != null)
                return;
            _minDistanceLocation = new GeometryLocation[2];
            ComputeContainmentDistance();
            if (_minDistance <= _terminateDistance)
                return;
            ComputeFacetDistance();
        }

        /// <summary>
        ///
        /// </summary>
        private void ComputeContainmentDistance()
        {
            var locPtPoly = new GeometryLocation[2];
            ComputeContainmentDistance(0, locPtPoly);
            if (_minDistance <= _terminateDistance) return;
            ComputeContainmentDistance(1, locPtPoly);

            // test if either geometry has a vertex inside the other

            /*
            IList<IPolygon> polys1 = PolygonExtracter.GetPolygons(_geom[1]);
            if (polys1.Count > 0)
            {
                IList<GeometryLocation> insideLocs0 = ConnectedElementLocationFilter.GetLocations(_geom[0]);
                ComputeInside(insideLocs0, polys1, locPtPoly);
                if (_minDistance <= _terminateDistance)
                {
                    _minDistanceLocation[0] = locPtPoly[0];
                    _minDistanceLocation[1] = locPtPoly[1];
                    return;
                }
            }

            IList<IPolygon> polys0 = PolygonExtracter.GetPolygons(_geom[0]);
            if (polys0.Count > 0)
            {
                IList<GeometryLocation> insideLocs1 = ConnectedElementLocationFilter.GetLocations(_geom[1]);
                ComputeInside(insideLocs1, polys0, locPtPoly);
                if (_minDistance <= _terminateDistance)
                {
                    // flip locations, since we are testing geom 1 VS geom 0
                    _minDistanceLocation[0] = locPtPoly[1];
                    _minDistanceLocation[1] = locPtPoly[0];
                    return;
                }
            }
             */
        }

        private void ComputeContainmentDistance(int polyGeomIndex, GeometryLocation[] locPtPoly)
        {
            var polyGeom = _geom[polyGeomIndex];
            // if no polygon then nothing to do
            if (polyGeom.Dimension < Dimension.Surface) return;

            int locationsIndex = 1 - polyGeomIndex;
            var polys = PolygonExtracter.GetPolygons(polyGeom);
            if (polys.Count > 0)
            {
                var insideLocs = ConnectedElementLocationFilter.GetLocations(_geom[locationsIndex]);
                ComputeContainmentDistance(insideLocs, polys, locPtPoly);
                if (_minDistance <= _terminateDistance)
                {
                    // this assigment is determined by the order of the args in the computeInside call above
                    _minDistanceLocation[locationsIndex] = locPtPoly[0];
                    _minDistanceLocation[polyGeomIndex] = locPtPoly[1];
                }
            }
        }

        private void ComputeContainmentDistance(IList<GeometryLocation> locs, ICollection<IGeometry> polys, GeometryLocation[] locPtPoly)
        {
            for (int i = 0; i < locs.Count; i++)
            {
                var loc = locs[i];
                foreach (IPolygon t in polys)
                {
                    ComputeContainmentDistance(loc, t, locPtPoly);
                    if (_minDistance <= _terminateDistance) return;
                }
            }
        }

        private void ComputeContainmentDistance(GeometryLocation ptLoc,
            IPolygon poly,
            GeometryLocation[] locPtPoly)
        {
            var pt = ptLoc.Coordinate;
            // if pt is not in exterior, distance to geom is 0
            if (Location.Exterior != _ptLocator.Locate(pt, poly))
            {
                _minDistance = 0.0;
                locPtPoly[0] = ptLoc;
                locPtPoly[1] = new GeometryLocation(poly, pt);
            }
        }

        /*
        /// <summary>
        ///
        /// </summary>
        /// <param name="locs"></param>
        /// <param name="polys"></param>
        /// <param name="locPtPoly"></param>
        private void ComputeInside(IEnumerable<GeometryLocation> locs, IEnumerable<IPolygon> polys, GeometryLocation[] locPtPoly)
        {
            foreach (GeometryLocation loc in locs)
            {
                foreach (IPolygon poly in polys)
                {
                    ComputeInside(loc, poly, locPtPoly);
                    if (_minDistance <= _terminateDistance)
                        return;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ptLoc"></param>
        /// <param name="poly"></param>
        /// <param name="locPtPoly"></param>
        private void ComputeInside(GeometryLocation ptLoc, IPolygon poly, GeometryLocation[] locPtPoly)
        {
            Coordinate pt = ptLoc.Coordinate;
            // if pt is not in exterior, distance to geom is 0
            if (Location.Exterior != _ptLocator.Locate(pt, poly))
            {
                _minDistance = 0.0;
                locPtPoly[0] = ptLoc;
                GeometryLocation locPoly = new GeometryLocation(poly, pt);
                locPtPoly[1] = locPoly;
                return;
            }
        }

        */

        /// <summary>
        /// Computes distance between facets (lines and points) of input geometries.
        /// </summary>
        private void ComputeFacetDistance()
        {
            var locGeom = new GeometryLocation[2];

            /*
             * Geometries are not wholely inside, so compute distance from lines and points
             * of one to lines and points of the other
             */
            var lines0 = LinearComponentExtracter.GetLines(_geom[0]);
            var lines1 = LinearComponentExtracter.GetLines(_geom[1]);

            var pts0 = PointExtracter.GetPoints(_geom[0]);
            var pts1 = PointExtracter.GetPoints(_geom[1]);

            // exit whenever minDistance goes LE than terminateDistance
            ComputeMinDistanceLines(lines0, lines1, locGeom);
            UpdateMinDistance(locGeom, false);
            if (_minDistance <= _terminateDistance) return;

            locGeom[0] = null;
            locGeom[1] = null;
            ComputeMinDistanceLinesPoints(lines0, pts1, locGeom);
            UpdateMinDistance(locGeom, false);
            if (_minDistance <= _terminateDistance) return;

            locGeom[0] = null;
            locGeom[1] = null;
            ComputeMinDistanceLinesPoints(lines1, pts0, locGeom);
            UpdateMinDistance(locGeom, true);
            if (_minDistance <= _terminateDistance) return;

            locGeom[0] = null;
            locGeom[1] = null;
            ComputeMinDistancePoints(pts0, pts1, locGeom);
            UpdateMinDistance(locGeom, false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lines0"></param>
        /// <param name="lines1"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistanceLines(IEnumerable<IGeometry> lines0, ICollection<IGeometry> lines1, GeometryLocation[] locGeom)
        {
            foreach (ILineString line0 in lines0)
            {
                foreach (ILineString line1 in lines1)
                {
                    ComputeMinDistance(line0, line1, locGeom);
                    if (_minDistance <= _terminateDistance) return;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="points0"></param>
        /// <param name="points1"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistancePoints(IEnumerable<IGeometry> points0, ICollection<IGeometry> points1, GeometryLocation[] locGeom)
        {
            foreach (IPoint pt0 in points0)
            {
                foreach (IPoint pt1 in points1)
                {
                    double dist = pt0.Coordinate.Distance(pt1.Coordinate);
                    if (dist < _minDistance)
                    {
                        _minDistance = dist;
                        locGeom[0] = new GeometryLocation(pt0, 0, pt0.Coordinate);
                        locGeom[1] = new GeometryLocation(pt1, 0, pt1.Coordinate);
                    }
                    if (_minDistance <= _terminateDistance) return;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="points"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistanceLinesPoints(IEnumerable<IGeometry> lines, ICollection<IGeometry> points, GeometryLocation[] locGeom)
        {
            foreach (ILineString line in lines)
            {
                foreach (IPoint pt in points)
                {
                    ComputeMinDistance(line, pt, locGeom);
                    if (_minDistance <= _terminateDistance) return;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="line0"></param>
        /// <param name="line1"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistance(ILineString line0, ILineString line1, GeometryLocation[] locGeom)
        {
            if (line0.EnvelopeInternal.Distance(line1.EnvelopeInternal) > _minDistance)
                return;
            var coord0 = line0.Coordinates;
            var coord1 = line1.Coordinates;
            // brute force approach!
            for (int i = 0; i < coord0.Length - 1; i++)
            {
                for (int j = 0; j < coord1.Length - 1; j++)
                {
                    double dist = DistanceComputer.SegmentToSegment(
                                                    coord0[i], coord0[i + 1],
                                                    coord1[j], coord1[j + 1]);
                    if (dist < _minDistance)
                    {
                        _minDistance = dist;
                        var seg0 = new LineSegment(coord0[i], coord0[i + 1]);
                        var seg1 = new LineSegment(coord1[j], coord1[j + 1]);
                        var closestPt = seg0.ClosestPoints(seg1);
                        locGeom[0] = new GeometryLocation(line0, i, closestPt[0]);
                        locGeom[1] = new GeometryLocation(line1, j, closestPt[1]);
                    }
                    if (_minDistance <= _terminateDistance) return;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="line"></param>
        /// <param name="pt"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistance(ILineString line, IPoint pt, GeometryLocation[] locGeom)
        {
            if (line.EnvelopeInternal.Distance(pt.EnvelopeInternal) > _minDistance) return;
            var coord0 = line.Coordinates;
            var coord = pt.Coordinate;
            // brute force approach!
            for (int i = 0; i < coord0.Length - 1; i++)
            {
                double dist = DistanceComputer.PointToSegment(coord, coord0[i], coord0[i + 1]);
                if (dist < _minDistance)
                {
                    _minDistance = dist;
                    var seg = new LineSegment(coord0[i], coord0[i + 1]);
                    var segClosestPoint = seg.ClosestPoint(coord);
                    locGeom[0] = new GeometryLocation(line, i, segClosestPoint);
                    locGeom[1] = new GeometryLocation(pt, 0, coord);
                }
                if (_minDistance <= _terminateDistance)
                    return;
            }
        }
    }
}
