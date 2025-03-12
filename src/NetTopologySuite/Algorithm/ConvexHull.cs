using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the convex hull of a <see cref="Geometry" />.
    /// The convex hull is the smallest convex Geometry that contains all the
    /// points in the input Geometry.
    /// <para/>
    /// Uses the Graham Scan algorithm.
    /// <para/>
    /// Incorporates heuristics to optimize checking for degenerate results,
    /// and to reduce the number of points processed for large inputs.
    /// </summary>
    public class ConvexHull
    {
        /// <summary>
        /// Computes the convex hull for the given sequence of <see cref="Geometry"/> instances.
        /// </summary>
        /// <param name="geoms">
        /// The <see cref="Geometry"/> instances whose convex hull to compute.
        /// </param>
        /// <returns>
        /// The convex hull of <paramref name="geoms"/>.
        /// </returns>
        public static Geometry Create(IEnumerable<Geometry> geoms)
        {
            GeometryFactory factory = null;
            var filter = new CustomUniqueCoordinateFilter();
            foreach (var geom in geoms ?? Enumerable.Empty<Geometry>())
            {
                if (geom is null)
                {
                    continue;
                }

                if (factory is null)
                {
                    factory = geom.Factory;
                }

                geom.Apply(filter);
            }

            return new ConvexHull(filter.Coordinates.ToArray(), factory).GetConvexHull();
        }

        private const int TUNING_REDUCE_SIZE = 50;

        private readonly GeometryFactory _geomFactory;
        private readonly Coordinate[] _inputPts;

        /// <summary>
        /// Create a new convex hull construction for the input <c>Geometry</c>.
        /// </summary>
        /// <param name="geometry"></param>
        public ConvexHull(Geometry geometry)
            : this(geometry?.Coordinates, geometry?.Factory) { }

        /// <summary>
        /// Create a new convex hull construction for the input <see cref="Coordinate" /> array.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="geomFactory">The factory to create the convex hull geometry</param>
        public ConvexHull(IEnumerable<Coordinate> pts, GeometryFactory geomFactory)
            : this((pts ?? Array.Empty<Coordinate>()).ToArray(), geomFactory)
        {
        }

        /// <summary>
        /// Create a new convex hull construction for the input <see cref="Coordinate"/> array.
        /// </summary>
        /// <param name="pts">The coordinate array</param>
        /// <param name="geomFactory">The factory to create the convex hull geometry</param>
        public ConvexHull(Coordinate[] pts, GeometryFactory geomFactory)
        {
            //-- suboptimal early uniquing - for performance testing only
            //inputPts = ExtractCoordinates(pts);

            _inputPts = pts ?? Array.Empty<Coordinate>();
            _geomFactory = geomFactory ?? NtsGeometryServices.Instance.CreateGeometryFactory();
        }

        //private static HashSet<Coordinate> ExtractCoordinates(Geometry geom)
        //{
        //    // DEVIATION: UniqueCoordinateArrayFilter is expensive (until we port 5e01aea, anyway).
        //    // Nobody actually needs the original input to be Coordinate[], and the original array
        //    // could never be used as-is because we could never assume that it's unique.
        //    var filter = new CustomUniqueCoordinateFilter();
        //    geom?.Apply(filter);
        //    return filter.Coordinates;
        //}

        /// <summary>
        /// Returns a <c>Geometry</c> that represents the convex hull of the input point.
        /// The point will contain the minimal number of points needed to
        /// represent the convex hull.  In particular, no more than two consecutive
        /// points will be collinear.
        /// </summary>
        /// <returns>
        /// If the convex hull contains 3 or more points, a <c>Polygon</c>;
        /// 2 points, a <c>LineString</c>;
        /// 1 point, a <c>Point</c>;
        /// 0 points, an empty <c>GeometryCollection</c>.
        /// </returns>
        public Geometry GetConvexHull()
        {
            var fewPointsGeom = CreateFewPointsResult();
            if (fewPointsGeom != null)
                return fewPointsGeom;

            var reducedPts = _inputPts;
            //-- use heuristic to reduce points, if large
            if (_inputPts.Length > TUNING_REDUCE_SIZE)
                reducedPts = Reduce(_inputPts);
            else
                //-- the points must be made unique
                reducedPts = ExtractUnique(_inputPts);

            // sort points for Graham scan.
            var sortedPts = PreSort(reducedPts);

            // Use Graham scan to find convex hull.
            var convexHull = GrahamScan(sortedPts);

            // Convert array to appropriate output geometry.
            // (an empty or point result will be detected earlier)
            return LineOrPolygon(convexHull);
        }

        /// <summary>
        /// Checks if there are &#8804;2 unique points,
        /// which produce an obviously degenerate result.
        /// If there are more points, returns null to indicate this.
        /// <para/>
        /// This is a fast check for an obviously degenerate result.
        /// If the result is not obviously degenerate (at least 3 unique points found)
        /// the full uniquing of the entire point set is
        /// done only once during the reduce phase.
        /// </summary>
        /// <returns>A degenerate hull geometry, or null if the number of input points is large</returns>
        private Geometry CreateFewPointsResult()
        {
            var uniquePts = ExtractUnique(_inputPts, 2);
            if (uniquePts == null)
            {
                return null;
            }
            else if (uniquePts.Length == 0)
            {
                return _geomFactory.CreateGeometryCollection();
            }
            else if (uniquePts.Length == 1)
            {
                return _geomFactory.CreatePoint(uniquePts[0]);
            }
            else
            {
                return _geomFactory.CreateLineString(uniquePts);
            }
        }

        private static Coordinate[] ExtractUnique(Coordinate[] pts)
        {
            return ExtractUnique(pts, -1);
        }

        /// <summary>
        /// Extracts unique coordinates from an array of coordinates,
        /// up to a maximum count of values.
        /// If more than the given maximum of unique values are found,
        /// this is reported by returning <c>null</c>.
        /// (the expectation is that the original array can then be used).
        /// </summary>
        /// <param name="pts">An array of coordinates</param>
        /// <param name="maxPts">The maximum number of unique points</param>
        /// <returns>An array of unique values, or null</returns>
        private static Coordinate[] ExtractUnique(Coordinate[] pts, int maxPts)
        {
            var uniquePts = new HashSet<Coordinate>();
            foreach (var pt in pts)
            {
                uniquePts.Add(pt);
                if (maxPts >= 0 && uniquePts.Count > maxPts) return null;
            }
            return CoordinateArrays.ToCoordinateArray(uniquePts);
        }

        /// <summary>
        /// Uses a heuristic to reduce the number of points scanned to compute the hull.
        /// The heuristic is to find a polygon guaranteed to
        /// be in (or on) the hull, and eliminate all points inside it.
        /// A quadrilateral defined by the extremal points
        /// in the four orthogonal directions
        /// can be used, but even more inclusive is
        /// to use an octilateral defined by the points in the 8 cardinal directions.
        /// Note that even if the method used to determine the polygon vertices
        /// is not 100% robust, this does not affect the robustness of the convex hull.
        /// <para/>
        /// To satisfy the requirements of the Graham Scan algorithm,
        /// the returned array has at least 3 entries.
        /// <para/>
        /// This has the side effect of making the reduced points unique,
        /// as required by the convex hull algorithm used.
        /// </summary>
        /// <param name="pts">The coordinates to reduce</param>
        /// <returns>The reduced array of coordinates</returns>
        private static Coordinate[] Reduce(Coordinate[] pts)
        {
            var innerPolyPts = ComputeInnerOctolateralRing(pts/*_inputPts*/);

            // unable to compute interior polygon for some reason
            if(innerPolyPts == null)
                return pts;

            // add points defining polygon
            var reducedSet = new HashSet<Coordinate>();
            for (int i = 0; i < innerPolyPts.Length; i++)
                reducedSet.Add(innerPolyPts[i]);

            /*
             * Add all unique points not in the interior poly.
             * PointLocation.IsInRing is not defined for points exactly on the ring,
             * but this doesn't matter since the points of the interior polygon
             * are forced to be in the reduced set.
             */
            for (int i = 0; i < pts.Length; i++)
                if (!PointLocation.IsInRing(pts[i], innerPolyPts))
                    reducedSet.Add(pts[i]);

            var reducedPts = CoordinateArrays.ToCoordinateArray(reducedSet);// new Coordinate[reducedSet.Count];
            Array.Sort(reducedPts);

            // ensure that computed array has at least 3 points (not necessarily unique)
            if (reducedPts.Length < 3)
                return PadArray3(reducedPts);

            return reducedPts;
        }

        private static Coordinate[] PadArray3(Coordinate[] pts)
        {
            var pad = new Coordinate[3];
            for (int i = 0; i < pad.Length; i++)
            {
                if (i < pts.Length)
                {
                    pad[i] = pts[i];
                }
                else
                    pad[i] = pts[0];
            }
            return pad;
        }

        /// <summary>
        /// Sorts the points radially CW around the point with minimum Y and then X.
        /// </summary>
        /// <param name="pts">The points to sort</param>
        /// <returns>The sorted points</returns>
        private static Coordinate[] PreSort(Coordinate[] pts)
        {
            /*
             * find the lowest point in the set. If two or more points have
             * the same minimum Y coordinate choose the one with the minimum X.
             * This focal point is put in array location pts[0].
             */
            for (int i = 1; i < pts.Length; i++)
            {
                if ((pts[i].Y < pts[0].Y) || ((pts[i].Y == pts[0].Y)
                     && (pts[i].X < pts[0].X)))
                {
                    var t = pts[0];
                    pts[0] = pts[i];
                    pts[i] = t;
                }
            }

            // sort the points radially around the focal point.
            Array.Sort(pts, 1, pts.Length - 1, new RadialComparator(pts[0]));
            return pts;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static Coordinate[] GrahamScan(Coordinate[] c)
        {
            // NOTE: Original implementation uses a Stack<Coordinate>
            // Unlike java's Stack implementation .NET's implementation
            // of ToArray() has the LIFO order.
            var ps = new List<Coordinate>(new []{ c[0], c[1], c[2] });
            for (int i = 3; i < c.Length; i++)
            {
                var cp = c[i];
                var p = RemoveLast(ps);

                // check for empty stack to guard against robustness problems
                while (
                    ps.Count > 0 /*(IsEmpty Hack)*/ &&
                    Orientation.Index(Enumerable.Last(ps), p, cp) > 0)
                    p = RemoveLast(ps);
                ps.Add(p);
                ps.Add(cp);
            }
            ps.Add(c[0]);
            return ps.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Coordinate RemoveLast(List<Coordinate> list)
        {
            var res = list[list.Count-1];
            list.RemoveAt(list.Count-1);
            return res;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <returns>
        /// Whether the three coordinates are collinear
        /// and c2 lies between c1 and c3 inclusive.
        /// </returns>
        private static bool IsBetween(Coordinate c1, Coordinate c2, Coordinate c3)
        {
            if (Orientation.Index(c1, c2, c3) != 0)
                return false;
            if (c1.X != c3.X)
            {
                if (c1.X <= c2.X && c2.X <= c3.X)
                    return true;
                if (c3.X <= c2.X && c2.X <= c1.X)
                    return true;
            }
            if (c1.Y != c3.Y)
            {
                if (c1.Y <= c2.Y && c2.Y <= c3.Y)
                    return true;
                if (c3.Y <= c2.Y && c2.Y <= c1.Y)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputPts"></param>
        /// <returns></returns>
        private static Coordinate[] ComputeInnerOctolateralRing(Coordinate[] inputPts)
        {
            var octPts = ComputeInnerOctolateralPts(inputPts);
            var coordList = new CoordinateList(octPts.Length + 1);
            coordList.Add(octPts, false);

            // points must all lie in a line
            if (coordList.Count < 3)
                return null;

            coordList.CloseRing();
            return coordList.ToCoordinateArray();
        }

        /// <summary>
        /// Computes the extremal points of an inner octolateral.
        /// Some points may be duplicates - these are collapsed later.
        /// </summary>
        /// <param name="inputPts">The points to compute the octolateral for</param>
        /// <returns>The extremal points of the octolateral</returns>
        private static Coordinate[] ComputeInnerOctolateralPts(Coordinate[] inputPts)
        {
            var pts = new Coordinate[8];
            for (int j = 0; j < pts.Length; j++)
                pts[j] = inputPts[0];

            for (int i = 1; i < inputPts.Length; i++)
            {
                if (inputPts[i].X < pts[0].X)
                    pts[0] = inputPts[i];

                if (inputPts[i].X - inputPts[i].Y < pts[1].X - pts[1].Y)
                    pts[1] = inputPts[i];

                if (inputPts[i].Y > pts[2].Y)
                    pts[2] = inputPts[i];

                if (inputPts[i].X + inputPts[i].Y > pts[3].X + pts[3].Y)
                    pts[3] = inputPts[i];

                if (inputPts[i].X > pts[4].X)
                    pts[4] = inputPts[i];

                if (inputPts[i].X - inputPts[i].Y > pts[5].X - pts[5].Y)
                    pts[5] = inputPts[i];

                if (inputPts[i].Y < pts[6].Y)
                    pts[6] = inputPts[i];

                if (inputPts[i].X + inputPts[i].Y < pts[7].X + pts[7].Y)
                    pts[7] = inputPts[i];
            }
            return pts;

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinates"> The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>A 2-vertex <c>LineString</c> if the vertices are collinear;
        /// otherwise, a <c>Polygon</c> with unnecessary (collinear) vertices removed. </returns>
        private Geometry LineOrPolygon(Coordinate[] coordinates)
        {
            coordinates = CleanRing(coordinates);
            if (coordinates.Length == 3)
                return _geomFactory.CreateLineString(new[] { coordinates[0], coordinates[1] });
            var linearRing = _geomFactory.CreateLinearRing(coordinates);
            return _geomFactory.CreatePolygon(linearRing);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="original">The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>The coordinates with unnecessary (collinear) vertices removed.</returns>
        private static Coordinate[] CleanRing(Coordinate[] original)
        {
            Assert.IsEquals(original[0], original[original.Length - 1]);
            var cleanedRing = new List<Coordinate>();
            Coordinate previousDistinctCoordinate = null;
            for (int i = 0; i <= original.Length - 2; i++)
            {
                var currentCoordinate = original[i];
                var nextCoordinate = original[i + 1];
                if (currentCoordinate.Equals(nextCoordinate))
                    continue;
                if (previousDistinctCoordinate != null &&
                    IsBetween(previousDistinctCoordinate, currentCoordinate, nextCoordinate))
                    continue;
                cleanedRing.Add(currentCoordinate);
                previousDistinctCoordinate = currentCoordinate;
            }
            cleanedRing.Add(original[original.Length - 1]);
            return cleanedRing.ToArray();
        }

        /// <summary>
        /// Compares <see cref="Coordinate" />s for their angle and distance
        /// relative to an origin.
        /// <para/>
        /// The origin is assumed to be lower in Y and then X than
        /// all other point inputs.
        /// The points are ordered CCW around the origin
        /// </summary>
        private class RadialComparator : IComparer<Coordinate>
        {
            private readonly Coordinate _origin;

            /// <summary>
            /// Creates a new comparator using a given origin.
            /// The origin must be lower in Y and then X to all
            /// compared points,
            /// using <see cref="Coordinate.CompareTo(Coordinate)"/>.
            /// </summary>
            /// <param name="origin">The origin of the radial comparison</param>
            public RadialComparator(Coordinate origin)
            {
                _origin = origin;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <returns></returns>
            public int Compare(Coordinate p1, Coordinate p2)
            {
                int comp = PolarCompare(_origin, p1, p2);
                return comp;
            }

            /// <summary>
            /// Given two points p and q compare them with respect to their radial
            /// ordering about point o.<br/>
            /// First checks radial ordering using a CCW orientation.
            /// If the points are collinear, the comparison is based
            /// on their distance to the origin.
            /// <para/>
            /// p &lt; q iff
            /// <list type="bullet">
            /// <item><description><c>ang(o-p) &lt; ang(o-q)</c> (e.g.o-p-q is CCW)</description></item>
            /// <item><description><c>or ang(o-p) == ang(o-q) &amp;&amp; dist(o, p) &lt; dist(o, q)</c></description></item>
            /// </list>
            /// </summary>
            /// <param name="o">The origin</param>
            /// <param name="p">A point</param>
            /// <param name="q">Another point</param>
            /// <returns>-1, 0 or 1 depending on whether p is less than,
            /// equal to or greater than q</returns>
            private static int PolarCompare(Coordinate o, Coordinate p, Coordinate q)
            {
                var orient = Orientation.Index(o, p, q);
                if (orient == OrientationIndex.CounterClockwise) return 1;
                if (orient == OrientationIndex.Clockwise) return -1;

                /*
                 * The points are collinear,
                 * so compare based on distance from the origin.  
                 * The points p and q are >= to the origin,
                 * so they lie in the closed half-plane above the origin.
                 * If they are not in a horizontal line, 
                 * the Y ordinate can be tested to determine distance.
                 * This is more robust than computing the distance explicitly.
                 */
                if (p.Y > q.Y) return 1;
                if (p.Y < q.Y) return -1;

                /*
                 * The points lie in a horizontal line, which should also contain the origin
                 * (since they are collinear).
                 * Also, they must be above the origin.
                 * Use the X ordinate to determine distance. 
                 */
                if (p.X > q.X) return 1;
                if (p.X < q.X) return -1;

                // Assert: p = q
                return 0;

            }
        }

        private sealed class CustomUniqueCoordinateFilter : ICoordinateFilter
        {
            public HashSet<Coordinate> Coordinates { get; } = new HashSet<Coordinate>();

            public void Filter(Coordinate coord)
            {
                if (!(coord is null))
                {
                    Coordinates.Add(coord);
                }
            }
        }
    }
}
