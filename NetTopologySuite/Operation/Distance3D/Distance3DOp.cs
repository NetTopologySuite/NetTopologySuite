using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Operation.Distance3D
{
    /// <summary>
    /// Find two points on two  3D <see cref="IGeometry"/>s which lie within a given distance,
    /// or else are the nearest points on the geometries (in which case this also
    /// provides the distance between the geometries).
    /// <para/>
    /// 3D geometries have vertex Z ordinates defined.
    /// 3D <see cref="IPolygon"/>s are assumed to lie in a single plane (which is enforced if not actually the case).
    /// 3D <see cref="ILineString"/>s and <see cref="IPoint"/>s may have any configuration.
    /// <para/>
    /// The distance computation also finds a pair of points in the input geometries
    /// which have the minimum distance between them. If a point lies in the interior
    /// of a line segment, the coordinate computed is a close approximation to the
    /// exact point.
    /// <para/>
    /// The algorithms used are straightforward O(n^2) comparisons. This worst-case
    /// performance could be improved on by using Voronoi techniques or spatial
    /// indexes.
    /// </summary>
    /// <version>1.13</version>
    public class Distance3DOp
    {
        /// <summary>
        /// Compute the distance between the nearest points of two geometries.
        /// </summary>
        /// <param name="g0">A <see cref="IGeometry">geometry</see></param>
        /// <param name="g1">A <see cref="IGeometry">geometry</see></param>
        /// <returns>The distance between the geometries</returns>
        public static double Distance(IGeometry g0, IGeometry g1)
        {
            var distOp = new Distance3DOp(g0, g1);
            return distOp.Distance();
        }

        /// <summary>
        /// Test whether two geometries lie within a given distance of each other.
        /// </summary>
        /// <param name="g0">A <see cref="IGeometry">geometry</see></param>
        /// <param name="g1">A <see cref="IGeometry">geometry</see></param>
        /// <param name="distance">The distance to test</param>
        /// <returns><value>true</value> if <c>g0.distance(g1) &lt;= <paramref name="distance"/></c></returns>
        public static bool IsWithinDistance(IGeometry g0, IGeometry g1,
                                            double distance)
        {
            var distOp = new Distance3DOp(g0, g1, distance);
            return distOp.Distance() <= distance;
        }

        /// <summary>
        /// Compute the the nearest points of two geometries. The points are
        /// presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <see cref="IGeometry">geometry</see></param>
        /// <param name="g1">A <see cref="IGeometry">geometry</see></param>
        /// <returns>The nearest points in the geometries</returns>
        public static Coordinate[] NearestPoints(IGeometry g0, IGeometry g1)
        {
            var distOp = new Distance3DOp(g0, g1);
            return distOp.NearestPoints();
        }

        // input
        private readonly IGeometry[] _geom;
        private readonly double _terminateDistance;
        // working
        private GeometryLocation[] _minDistanceLocation;
        private double _minDistance = double.MaxValue;
        private bool _isDone;

        /// <summary>
        /// Constructs a DistanceOp that computes the distance and nearest points
        /// between the two specified geometries.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        public Distance3DOp(IGeometry g0, IGeometry g1)
            : this(g0, g1, 0.0)
        {
        }

        /// <summary>
        /// Constructs a DistanceOp that computes the distance and nearest points
        /// between the two specified geometries.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <param name="terminateDistance">The distance on which to terminate the search</param>
        public Distance3DOp(IGeometry g0, IGeometry g1, double terminateDistance)
        {
            _geom = new IGeometry[2];
            _geom[0] = g0;
            _geom[1] = g1;
            _terminateDistance = terminateDistance;
        }

        /// <summary>
        /// Report the distance between the nearest points on the input geometries.
        /// </summary>
        /// <returns>The distance between the geometries<br/>
        /// or <value>0</value> if either input geometry is empty</returns>
        /// <exception cref="ArgumentException">Thrown if either input geometry is null.</exception>
        public double Distance()
        {
            if (_geom[0] == null || _geom[1] == null)
                throw new ArgumentException(
                    "null geometries are not supported");
            if (_geom[0].IsEmpty || _geom[1].IsEmpty)
                return 0.0;

            ComputeMinDistance();
            return _minDistance;
        }

        /// <summary>
        /// Report the coordinates of the nearest points in the input geometries. The
        /// points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <see cref="Coordinate"/>s of the nearest points</returns>
        public Coordinate[] NearestPoints()
        {
            ComputeMinDistance();
            var nearestPts = new[]
                {
                    _minDistanceLocation[0].Coordinate,
                    _minDistanceLocation[1].Coordinate
                };
            return nearestPts;
        }

        /// <summary>
        /// Gets the locations of the nearest points in the input geometries. The
        /// locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <see cref="GeometryLocation"/>s for the nearest points</returns>
        public GeometryLocation[] NearestLocations()
        {
            ComputeMinDistance();
            return _minDistanceLocation;
        }

        private void UpdateDistance(double dist,
                                    GeometryLocation loc0, GeometryLocation loc1,
                                    bool flip)
        {
            _minDistance = dist;
            int index = flip ? 1 : 0;
            _minDistanceLocation[index] = loc0;
            _minDistanceLocation[1 - index] = loc1;
            if (_minDistance < _terminateDistance)
                _isDone = true;
        }

        private void ComputeMinDistance()
        {
            // only compute once
            if (_minDistanceLocation != null)
                return;
            _minDistanceLocation = new GeometryLocation[2];

            int geomIndex = MostPolygonalIndex();
            bool flip = geomIndex == 0;
            ComputeMinDistanceMultiMulti(_geom[geomIndex], _geom[1 - geomIndex], flip);
        }

        /// <summary>
        /// Finds the index of the "most polygonal" input geometry.
        /// This optimizes the computation of the best-fit plane,
        /// since it is cached only for the left-hand geometry.
        /// </summary>
        /// <returns>The index of the most polygonal geometry</returns>
        private int MostPolygonalIndex()
        {
            var dim0 = _geom[0].Dimension;
            var dim1 = _geom[1].Dimension;
            if (dim0 >= Dimension.Surface && dim1 >= Dimension.Surface)
            {
                if (_geom[0].NumPoints > _geom[1].NumPoints)
                    return 0;
                return 1;
            }
            // no more than one is dim 2
            if (dim0 >= Dimension.Surface) return 0;
            if (dim1 >= Dimension.Surface) return 1;
            // both dim <= 1 - don't flip
            return 0;
        }

        private void ComputeMinDistanceMultiMulti(IGeometry g0, IGeometry g1, bool flip)
        {
            if (g0 is IGeometryCollection)
            {
                int n = g0.NumGeometries;
                for (int i = 0; i < n; i++)
                {
                    var g = g0.GetGeometryN(i);
                    ComputeMinDistanceMultiMulti(g, g1, flip);
                    if (_isDone) return;
                }
            }
            else
            {
                // handle case of multigeom component being empty
                if (g0.IsEmpty)
                    return;

                // compute planar polygon only once for efficiency
                if (g0 is IPolygon)
                {
                    ComputeMinDistanceOneMulti(PolyPlane(g0), g1, flip);
                }
                else
                    ComputeMinDistanceOneMulti(g0, g1, flip);
            }
        }

        private void ComputeMinDistanceOneMulti(IGeometry g0, IGeometry g1, bool flip)
        {
            if (g1 is IGeometryCollection)
            {
                int n = g1.NumGeometries;
                for (int i = 0; i < n; i++)
                {
                    var g = g1.GetGeometryN(i);
                    ComputeMinDistanceOneMulti(g0, g, flip);
                    if (_isDone) return;
                }
            }
            else
            {
                ComputeMinDistance(g0, g1, flip);
            }
        }

        private void ComputeMinDistanceOneMulti(PlanarPolygon3D poly, IGeometry geom, bool flip)
        {
            if (geom is IGeometryCollection)
            {
                int n = geom.NumGeometries;
                for (int i = 0; i < n; i++)
                {
                    var g = geom.GetGeometryN(i);
                    ComputeMinDistanceOneMulti(poly, g, flip);
                    if (_isDone) return;
                }
            }
            else
            {
                if (geom is IPoint)
                {
                    ComputeMinDistancePolygonPoint(poly, (IPoint)geom, flip);
                    return;
                }
                if (geom is ILineString)
                {
                    ComputeMinDistancePolygonLine(poly, (ILineString)geom, flip);
                    return;
                }
                if (geom is IPolygon)
                {
                    ComputeMinDistancePolygonPolygon(poly, (IPolygon)geom, flip);
                    //return;
                }
            }
        }

        /// <summary>
        /// Convenience method to create a Plane3DPolygon
        /// </summary>
        private static PlanarPolygon3D PolyPlane(IGeometry poly)
        {
            return new PlanarPolygon3D((IPolygon)poly);
        }

        private void ComputeMinDistance(IGeometry g0, IGeometry g1, bool flip)
        {
            if (g0 is IPoint)
            {
                if (g1 is IPoint)
                {
                    ComputeMinDistancePointPoint((IPoint)g0, (IPoint)g1, flip);
                    return;
                }
                if (g1 is ILineString)
                {
                    ComputeMinDistanceLinePoint((ILineString)g1, (IPoint)g0, !flip);
                    return;
                }
                if (g1 is IPolygon)
                {
                    ComputeMinDistancePolygonPoint(PolyPlane(g1), (IPoint)g0, !flip);
                    return;
                }
            }
            if (g0 is ILineString)
            {
                if (g1 is IPoint)
                {
                    ComputeMinDistanceLinePoint((ILineString)g0, (IPoint)g1, flip);
                    return;
                }
                if (g1 is ILineString)
                {
                    ComputeMinDistanceLineLine((ILineString)g0, (ILineString)g1, flip);
                    return;
                }
                if (g1 is IPolygon)
                {
                    ComputeMinDistancePolygonLine(PolyPlane(g1), (ILineString)g0, !flip);
                    return;
                }
            }
            if (g0 is IPolygon)
            {
                if (g1 is IPoint)
                {
                    ComputeMinDistancePolygonPoint(PolyPlane(g0), (IPoint)g1, flip);
                    return;
                }
                if (g1 is ILineString)
                {
                    ComputeMinDistancePolygonLine(PolyPlane(g0), (ILineString)g1, flip);
                    return;
                }
                if (g1 is IPolygon)
                {
                    ComputeMinDistancePolygonPolygon(PolyPlane(g0), (IPolygon)g1, flip);
                    //return;
                }
            }
        }

        /// <summary>
        /// Computes distance between two polygons.
        /// </summary>
        /// <remarks>
        /// To compute the distance, compute the distance
        /// between the rings of one polygon and the other polygon,
        /// and vice-versa.
        /// If the polygons intersect, then at least one ring must
        /// intersect the other polygon.
        /// Note that it is NOT sufficient to test only the shell rings.
        /// A counter-example is a "figure-8" polygon A
        /// and a simple polygon B at right angles to A, with the ring of B
        /// passing through the holes of A.
        /// The polygons intersect,
        /// but A's shell does not intersect B, and B's shell does not intersect A.</remarks>
        private void ComputeMinDistancePolygonPolygon(PlanarPolygon3D poly0, IPolygon poly1,
                                                      bool flip)
        {
            ComputeMinDistancePolygonRings(poly0, poly1, flip);
            if (_isDone) return;
            var polyPlane1 = new PlanarPolygon3D(poly1);
            ComputeMinDistancePolygonRings(polyPlane1, poly0.Polygon, flip);
        }

        /// <summary>Compute distance between a polygon and the rings of another.</summary>
        private void ComputeMinDistancePolygonRings(PlanarPolygon3D poly, IPolygon ringPoly,
                                                    bool flip)
        {
            // compute shell ring
            ComputeMinDistancePolygonLine(poly, ringPoly.ExteriorRing, flip);
            if (_isDone) return;
            // compute hole rings
            int nHole = ringPoly.NumInteriorRings;
            for (int i = 0; i < nHole; i++)
            {
                ComputeMinDistancePolygonLine(poly, ringPoly.GetInteriorRingN(i), flip);
                if (_isDone) return;
            }
        }

        private void ComputeMinDistancePolygonLine(PlanarPolygon3D poly, ILineString line,
                                                   bool flip)
        {

            // first test if line intersects polygon
            var intPt = Intersection(poly, line);
            if (intPt != null)
            {
                UpdateDistance(0,
                               new GeometryLocation(poly.Polygon, 0, intPt),
                               new GeometryLocation(line, 0, intPt),
                               flip
                    );
                return;
            }

            // if no intersection, then compute line distance to polygon rings
            ComputeMinDistanceLineLine(poly.Polygon.ExteriorRing, line, flip);
            if (_isDone) return;
            int nHole = poly.Polygon.NumInteriorRings;
            for (int i = 0; i < nHole; i++)
            {
                ComputeMinDistanceLineLine(poly.Polygon.GetInteriorRingN(i), line, flip);
                if (_isDone) return;
            }
        }

        private static Coordinate Intersection(PlanarPolygon3D poly, ILineString line)
        {
            var seq = line.CoordinateSequence;
            if (seq.Count == 0)
                return null;

            // start point of line
            var p0 = new Coordinate();
            seq.GetCoordinate(0, p0);
            double d0 = poly.Plane.OrientedDistance(p0);

            // for each segment in the line
            var p1 = new Coordinate();
            for (int i = 0; i < seq.Count - 1; i++)
            {
                seq.GetCoordinate(i, p0);
                seq.GetCoordinate(i + 1, p1);
                double d1 = poly.Plane.OrientedDistance(p1);

                /**
                 * If the oriented distances of the segment endpoints have the same sign,
                 * the segment does not cross the plane, and is skipped.
                 */
                if (d0 * d1 > 0)
                    continue;

                /**
                 * Compute segment-plane intersection point
                 * which is then used for a point-in-polygon test.
                 * The endpoint distances to the plane d0 and d1
                 * give the proportional distance of the intersection point
                 * along the segment.
                 */
                var intPt = SegmentPoint(p0, p1, d0, d1);
                // Coordinate intPt = polyPlane.intersection(p0, p1, s0, s1);
                if (poly.Intersects(intPt))
                {
                    return intPt;
                }

                // shift to next segment
                d0 = d1;
            }
            return null;
        }

        private void ComputeMinDistancePolygonPoint(PlanarPolygon3D polyPlane, IPoint point,
                                                    bool flip)
        {
            var pt = point.Coordinate;

            var shell = polyPlane.Polygon.ExteriorRing;
            if (polyPlane.Intersects(pt, shell))
            {
                // point is either inside or in a hole

                int nHole = polyPlane.Polygon.NumInteriorRings;
                for (int i = 0; i < nHole; i++)
                {
                    var hole = polyPlane.Polygon.GetInteriorRingN(i);
                    if (polyPlane.Intersects(pt, hole))
                    {
                        ComputeMinDistanceLinePoint(hole, point, flip);
                        return;
                    }
                }
                // point is in interior of polygon
                // distance is distance to polygon plane
                double dist = Math.Abs(polyPlane.Plane.OrientedDistance(pt));
                UpdateDistance(dist,
                               new GeometryLocation(polyPlane.Polygon, 0, pt),
                               new GeometryLocation(point, 0, pt),
                               flip
                    );
            }
            // point is outside polygon, so compute distance to shell linework
            ComputeMinDistanceLinePoint(shell, point, flip);
        }

        private void ComputeMinDistanceLineLine(ILineString line0, ILineString line1,
                                                bool flip)
        {
            var coord0 = line0.Coordinates;
            var coord1 = line1.Coordinates;
            // brute force approach!
            for (int i = 0; i < coord0.Length - 1; i++)
            {
                for (int j = 0; j < coord1.Length - 1; j++)
                {
                    double dist = CGAlgorithms3D.DistanceSegmentSegment(coord0[i],
                                                                     coord0[i + 1], coord1[j], coord1[j + 1]);
                    if (dist < _minDistance)
                    {
                        _minDistance = dist;
                        // TODO: compute closest pts in 3D
                        var seg0 = new LineSegment(coord0[i], coord0[i + 1]);
                        var seg1 = new LineSegment(coord1[j], coord1[j + 1]);
                        var closestPt = seg0.ClosestPoints(seg1);
                        UpdateDistance(dist,
                                       new GeometryLocation(line0, i, closestPt[0]),
                                       new GeometryLocation(line1, j, closestPt[1]),
                                       flip
                            );
                    }
                    if (_isDone) return;
                }
            }
        }

        private void ComputeMinDistanceLinePoint(ILineString line, IPoint point,
                                                 bool flip)
        {
            var lineCoord = line.Coordinates;
            var coord = point.Coordinate;
            // brute force approach!
            for (int i = 0; i < lineCoord.Length - 1; i++)
            {
                double dist = CGAlgorithms3D.DistancePointSegment(coord, lineCoord[i],
                                                               lineCoord[i + 1]);
                if (dist < _minDistance)
                {
                    var seg = new LineSegment(lineCoord[i], lineCoord[i + 1]);
                    var segClosestPoint = seg.ClosestPoint(coord);
                    UpdateDistance(dist,
                                   new GeometryLocation(line, i, segClosestPoint),
                                   new GeometryLocation(point, 0, coord),
                                   flip);
                }
                if (_isDone) return;
            }
        }

        private void ComputeMinDistancePointPoint(IPoint point0, IPoint point1, bool flip)
        {
            double dist = CGAlgorithms3D.Distance(
                point0.Coordinate,
                point1.Coordinate);
            if (dist < _minDistance)
            {
                UpdateDistance(dist,
                               new GeometryLocation(point0, 0, point0.Coordinate),
                               new GeometryLocation(point1, 0, point1.Coordinate),
                               flip);
            }
        }

        /**
         * Computes a point at a distance along a segment
         * specified by two relatively proportional values.
         * The fractional distance along the segment is d0/(d0+d1).
         *
         * @param p0
         *            start point of the segment
         * @param p1
         *            end point of the segment
         * @param d0
         *            proportional distance from start point to computed point
         * @param d1
         *            proportional distance from computed point to end point
         * @return the computed point
         */

        private static Coordinate SegmentPoint(Coordinate p0, Coordinate p1, double d0,
                                               double d1)
        {
            if (d0 <= 0) return new Coordinate(p0);
            if (d1 <= 0) return new Coordinate(p1);

            double f = Math.Abs(d0) / (Math.Abs(d0) + Math.Abs(d1));
            double intx = p0.X + f * (p1.X - p0.X);
            double inty = p0.Y + f * (p1.Y - p0.Y);
            double intz = p0.Z + f * (p1.Z - p0.Z);
            return new Coordinate(intx, inty, intz);
        }
    }
}