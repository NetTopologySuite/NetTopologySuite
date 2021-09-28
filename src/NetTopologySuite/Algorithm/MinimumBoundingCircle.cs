using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the <b>Minimum Bounding Circle</b> (MBC) for the points in a <see cref="Geometry"/>.
    /// The MBC is the smallest circle which <tt>cover</tt>s all the input points
    /// (this is also sometimes known as the <b>Smallest Enclosing Circle</b>).
    /// This is equivalent to computing the Maximum Diameter of the input point set.
    /// </summary>
    /// <remarks>
    /// <para/>
    /// The computed circle can be specified in two equivalent ways,
    /// both of which are provide as output by this class:
    /// <list type="bullet">
    /// <item><description>As a centre point and a radius</description></item>
    /// <item><description>By the set of points defining the circle.
    /// Depending on the number of points in the input
    /// and their relative positions, this set
    /// contains from 0 to 3 points.
    /// <list type="bullet">
    /// <item><description>0 or 1 points indicate an empty or trivial input point arrangement.</description></item>
    /// <item><description>2 points define the diameter of the minimum bounding circle.</description></item>
    /// <item><description>3 points define an inscribed triangle of the minimum bounding circle.</description></item>
    /// </list>
    /// </description></item></list>
    /// <para/>
    /// The class can also output a <see cref="Geometry"/> which approximates the
    /// shape of the Minimum Bounding Circle (although as an approximation 
    /// it is <b>not</b> guaranteed to <tt>cover</tt> all the input points.)
    /// <para/>
    /// The Maximum Diameter of the input point set can
    /// be computed as well. The Maximum Diameter is
    /// defined by the pair of input points with maximum distance between them.
    /// The points of the maximum diameter are two of the extremal points of the Minimum Bounding Circle.
    /// They lie on the convex hull of the input.
    /// However, that the maximum diameter is not a diameter
    /// of the Minimum Bounding Circle in the case where the MBC is
    /// defined by an inscribed triangle.
    /// </remarks>
    /// <author>Martin Davis</author>
    public class MinimumBoundingCircle
    {
        /*
         * The algorithm used is based on the one by Jon Rokne in
         * the article "An Easy Bounding Circle" in <i>Graphic Gems II</i>.
         */

        private readonly Geometry _input;
        private Coordinate[] _extremalPts;
        private Coordinate _centre;
        private double _radius;

        /// <summary>
        /// Creates a new object for computing the minimum bounding circle for the
        /// point set defined by the vertices of the given geometry.
        /// </summary>
        /// <param name="geom">The geometry to use to obtain the point set </param>
        public MinimumBoundingCircle(Geometry geom)
        {
            _input = geom;
        }

        /// <summary>
        /// Gets a geometry which represents the Minimum Bounding Circle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the input is degenerate (empty or a single unique point),
        /// this method will return an empty geometry or a single Point geometry.
        /// Otherwise, a Polygon will be returned which approximates the
        /// Minimum Bounding Circle. (Note that because the computed polygon is only an approximation, it may not precisely contain all the input points.)
        /// </para>
        /// </remarks>
        /// <returns>A Geometry representing the Minimum Bounding Circle.</returns>
        public Geometry GetCircle()
        {
            //TODO: ensure the output circle contains the external points.
            //TODO: or maybe even ensure that the returned geometry contains ALL the input points?

            Compute();
            if (_centre == null)
                return _input.Factory.CreatePolygon();
            var centrePoint = _input.Factory.CreatePoint(_centre);
            if (_radius == 0.0)
                return centrePoint;
            return centrePoint.Buffer(_radius);
        }

        /// <summary>Gets a geometry representing the maximum diameter of the
        /// input. The maximum diameter is the longest line segment
        /// between any two points of the input.
        /// <para/>
        /// The points are two of the extremal points of the Minimum Bounding Circle.
        /// They lie on the convex hull of the input.
        /// </summary>
        /// <returns>
        /// The result is 
        /// <list type="Bullet">
        /// <item><description>a LineString between the two farthest points of the input</description></item>
        /// <item><description>a empty LineString if the input is empty</description></item>
        /// <item><description>a Point if the input is a point</description></item>
        /// </list>
        /// </returns>
        public Geometry GetMaximumDiameter()
        {
            Compute();
            switch (_extremalPts.Length)
            {
                case 0:
                    return _input.Factory.CreateLineString();
                case 1:
                    return _input.Factory.CreatePoint(_centre);
                case 2:
                    return _input.Factory.CreateLineString(
                        new [] { _extremalPts[0], _extremalPts[1] });
                default: // case 3
                     var maxDiameter = FarthestPoints(_extremalPts);
                    return _input.Factory.CreateLineString(maxDiameter);
            }
        }


        /// <summary>
        /// Gets a geometry representing a line between the two farthest points
        /// in the input.
        /// <para/>
        /// These points are two of the extremal points of the Minimum Bounding Circle
        /// They lie on the convex hull of the input.
        /// </summary>
        /// <returns>A LineString between the two farthest points of the input</returns>
        /// <returns>An empty LineString if the input is empty</returns>
        /// <returns>A Point if the input is a point</returns>
        [Obsolete("Use GetMaximumDiameter()")]
        public Geometry GetFarthestPoints()
        {
            return GetMaximumDiameter();

        }

        /// <summary>
        /// Finds the farthest pair out of 3 extremal points
        /// </summary>
        /// <param name="pts">The array of extremal points</param>
        /// <returns>The pair of farthest points</returns>
        private static Coordinate[] FarthestPoints(Coordinate[] pts)
        {
            double dist01 = pts[0].Distance(pts[1]);
            double dist12 = pts[1].Distance(pts[2]);
            double dist20 = pts[2].Distance(pts[0]);
            if (dist01 >= dist12 && dist01 >= dist20) {
                return new [] { pts[0], pts[1] };
            }
            if (dist12 >= dist01 && dist12 >= dist20) {
                return new [] { pts[1], pts[2] };
            }
            return new [] { pts[2], pts[0] };
        }

        /// <summary>
        /// Gets a geometry representing the diameter of the computed Minimum Bounding Circle.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>the diameter line of the Minimum Bounding Circle</description></item>
        /// <item><description>an empty line if the input is empty</description></item>
        /// <item><description>a Point if the input is a point</description></item>
        /// </list>
        /// </returns>
        public Geometry GetDiameter()
        {
            Compute();
            switch (_extremalPts.Length)
            {
                case 0:
                    return _input.Factory.CreateLineString();
                case 1:
                    return _input.Factory.CreatePoint(_centre);
            }
            //TODO: handle case of 3 extremal points, by computing a line from one of them through the centre point with len = 2*radius
            var p0 = _extremalPts[0];
            var p1 = _extremalPts[1];
            return _input.Factory.CreateLineString(new [] { p0, p1 });
        }

        /// <summary>
        /// Gets the extremal points which define the computed Minimum Bounding Circle.
        /// There may be zero, one, two or three of these points, depending on the number
        /// of points in the input and the geometry of those points.
        /// <list type="Bullet">
        /// <item><description>0 or 1 points indicate an empty or trivial input point arrangement.</description></item>
        /// <item><description>2 points define the diameter of the Minimum Bounding Circle.</description></item>
        /// <item><description>3 points define an inscribed triangle of which the Minimum Bounding Circle is the circumcircle.
        /// The longest chords of the circle are the line segments [0-1] and[1 - 2]</description></item>
        /// </list>
        /// </summary>
        /// <returns>The points defining the Minimum Bounding Circle</returns>
        public Coordinate[] GetExtremalPoints()
        {
            Compute();
            return _extremalPts;
        }

        /// <summary>
        /// Gets the centre point of the computed Minimum Bounding Circle.
        /// </summary>
        /// <returns>
        /// <list type="Bullet">
        /// <item><description>The centre point of the Minimum Bounding Circle or</description></item>
        /// <item><description><c>null</c> if the input is empty</description></item>
        /// </list>
        /// </returns>
        public Coordinate GetCentre()
        {
            Compute();
            return _centre;
        }

        /// <summary>
        /// Gets the radius of the computed Minimum Bounding Circle.
        /// </summary>
        /// <returns>The radius of the Minimum Bounding Circle</returns>
        public double GetRadius()
        {
            Compute();
            return _radius;
        }

        private void ComputeCentre()
        {
            switch (_extremalPts.Length)
            {
                case 0:
                    _centre = null;
                    break;
                case 1:
                    _centre = _extremalPts[0];
                    break;
                case 2:
                    _centre = new Coordinate(
                            (_extremalPts[0].X + _extremalPts[1].X) / 2.0,
                            (_extremalPts[0].Y + _extremalPts[1].Y) / 2.0
                            );
                    break;
                case 3:
                    _centre = Triangle.Circumcentre(_extremalPts[0], _extremalPts[1], _extremalPts[2]);
                    break;
            }
        }

        private void Compute()
        {
            if (_extremalPts != null)
                return;

            ComputeCirclePoints();
            ComputeCentre();
            if (_centre != null)
                _radius = _centre.Distance(_extremalPts[0]);
        }

        private void ComputeCirclePoints()
        {
            Coordinate[] pts;
            // handle degenerate or trivial cases
            if (_input.IsEmpty)
            {
                _extremalPts = new Coordinate[0];
                return;
            }
            if (_input.NumPoints == 1)
            {
                pts = _input.Coordinates;
                _extremalPts = new[] { pts[0].Copy() };
                return;
            }

            /*
             * The problem is simplified by reducing to the convex hull.
             * Computing the convex hull also has the useful effect of eliminating duplicate points
             */
            var convexHull = _input.ConvexHull();

            // check for degenerate or trivial cases
            var hullPts = convexHull.Coordinates;

            // strip duplicate final point, if any
            pts = hullPts;
            if (hullPts[0].Equals2D(hullPts[hullPts.Length - 1]))
            {
                pts = new Coordinate[hullPts.Length - 1];
                CoordinateArrays.CopyDeep(hullPts, 0, pts, 0, hullPts.Length - 1);
            }

            /*
             * Optimization for the trivial case where the CH has fewer than 3 points
             */
            if (pts.Length <= 2)
            {
                _extremalPts = CoordinateArrays.CopyDeep(pts);
                return;
            }

            // find a point P with minimum Y ordinate
            var P = LowestPoint(pts);

            // find a point Q such that the angle that PQ makes with the x-axis is minimal
            var Q = PointWitMinAngleWithX(pts, P);

            /*
             * Iterate over the remaining points to find
             * a pair or triplet of points which determine the minimal circle.
             * By the design of the algorithm,
             * at most <tt>pts.length</tt> iterations are required to terminate
             * with a correct result.
             */
            for (int i = 0; i < pts.Length; i++)
            {
                var R = PointWithMinAngleWithSegment(pts, P, Q);

                if (AngleUtility.IsObtuse(P, R, Q))
                {
                    // if PRQ is obtuse, then MBC is determined by P and Q
                    _extremalPts = new Coordinate[] { P.Copy(), Q.Copy() };
                    return;
                }
                else if (AngleUtility.IsObtuse(R, P, Q))
                {
                    // if RPQ is obtuse, update baseline and iterate
                    P = R;
                    continue;
                }
                else if (AngleUtility.IsObtuse(R, Q, P))
                {
                    // if RQP is obtuse, update baseline and iterate
                    Q = R;
                    continue;
                }
                else
                {
                    // otherwise all angles are acute, and the MBC is determined by the triangle PQR
                    _extremalPts = new Coordinate[] { P.Copy(), Q.Copy(), R.Copy() };
                    return;
                }
            }
            Assert.ShouldNeverReachHere("Logic failure in Minimum Bounding Circle algorithm!");
        }

        private static Coordinate LowestPoint(Coordinate[] pts)
        {
            var min = pts[0];
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].Y < min.Y)
                    min = pts[i];
            }
            return min;
        }

        private static Coordinate PointWitMinAngleWithX(Coordinate[] pts, Coordinate P)
        {
            double minSin = double.MaxValue;
            Coordinate minAngPt = null;
            for (int i = 0; i < pts.Length; i++)
            {

                var p = pts[i];
                if (p == P) continue;

                /*
                 * The sin of the angle is a simpler proxy for the angle itself
                 */
                double dx = p.X - P.X;
                double dy = p.Y - P.Y;
                if (dy < 0) dy = -dy;
                double len = Math.Sqrt(dx * dx + dy * dy);
                double sin = dy / len;

                if (sin < minSin)
                {
                    minSin = sin;
                    minAngPt = p;
                }
            }
            return minAngPt;
        }

        private static Coordinate PointWithMinAngleWithSegment(Coordinate[] pts, Coordinate P, Coordinate Q)
        {
            double minAng = double.MaxValue;
            Coordinate minAngPt = null;
            for (int i = 0; i < pts.Length; i++)
            {
                var p = pts[i];
                if (p == P) continue;
                if (p == Q) continue;

                double ang = AngleUtility.AngleBetween(P, p, Q);
                if (ang < minAng)
                {
                    minAng = ang;
                    minAngPt = p;
                }
            }
            return minAngPt;

        }
    }
}
