using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the Minimum Bounding Circle (MBC) for the points in a <see cref="IGeometry"/>.
    /// The MBC is the smallest circle which contains all the input points (this is sometimes known as the Smallest Enclosing Circle).
    /// This is equivalent to computing the Maximum Diameter of the input point set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The geometric circle can be specified in two equivalent ways, 
    /// both of which are provide as output by this class:
    /// <list type="Bullet">
    /// <item>As a centre point and a radius</item>
    /// <item>By the set of points defining the circle.</item>
    /// </list>
    /// Depending on the number of points in the input
    /// and their relative positions, this
    /// will be specified by anywhere from 0 to 3 points. 
    /// 0 or 1 points indicate an empty or trivial input point arrangment.
    /// 2 or 3 points define a circle which contains 
    /// all the input points.
    /// </para>
    /// <para>The class also provides a Geometry which approximates the
    /// shape of the MBC (although as an approximation it is <b>not</b>
    /// guaranteed to <tt>cover</tt> all the input points.)</para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class MinimumBoundingCircle
    {
        /**
         * The algorithm used is based on the one by Jon Rokne in 
         * the article "An Easy Bounding Circle" in <i>Graphic Gems II</i>.
         */

        private readonly IGeometry _input;
        private Coordinate[] _extremalPts;
        private Coordinate _centre;
        private double _radius;

        /// <summary>
        /// Creates a new object for computing the minimum bounding circle for the
        /// point set defined by the vertices of the given geometry.
        /// </summary>
        /// <param name="geom">The geometry to use to obtain the point set </param>
        public MinimumBoundingCircle(IGeometry geom)
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
        public IGeometry GetCircle()
        {
            //TODO: ensure the output circle contains the extermal points.
            //TODO: or maybe even ensure that the returned geometry contains ALL the input points?

            Compute();
            if (_centre == null)
                return _input.Factory.CreatePolygon(null, null);
            IPoint centrePoint = _input.Factory.CreatePoint(_centre);
            if (_radius == 0.0)
                return centrePoint;
            return centrePoint.Buffer(_radius);
        }

        /// <summary>
        /// Gets the extremal points which define the computed Minimum Bounding Circle.
        /// </summary>
        /// <remarks>
        /// There may be zero, one, two or three of these points,
        /// depending on the number of points in the input
        /// and the geometry of those points.
        /// </remarks>
        /// <returns>The points defining the Minimum Bounding Circle</returns>
        public Coordinate[] GetExtremalPoints()
        {
            Compute();
            return _extremalPts;
        }

        /// <summary>
        /// Gets the centre point of the computed Minimum Bounding Circle.
        /// </summary>
        /// <returns>the centre point of the Minimum Bounding Circle, null if the input is empty</returns>
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
            // handle degenerate cases
            if (_input.IsEmpty)
            {
                _extremalPts = new Coordinate[0];
                return;
            }
            if (_input.NumPoints == 1)
            {
                pts = _input.Coordinates;
                _extremalPts = new Coordinate[] { new Coordinate(pts[0]) };
                return;
            }

            IGeometry convexHull = _input.ConvexHull();

            /**
             * Computing the convex hull also have the effect of eliminating duplicate points
             */

            // check for degenerate or trivial cases
            Coordinate[] hullPts = convexHull.Coordinates;

            // strip duplicate final point, if any
            pts = hullPts;
            if (hullPts[0].Equals2D(hullPts[hullPts.Length - 1]))
            {
                pts = new Coordinate[hullPts.Length - 1];
                CoordinateArrays.CopyDeep(hullPts, 0, pts, 0, hullPts.Length - 1);
            }

            if (pts.Length <= 3)
            {
                _extremalPts = CoordinateArrays.CopyDeep(pts);
                return;
            }

            // find a point P with minimum Y ordinate
            Coordinate P = LowestPoint(pts);

            // find a point Q such that the angle that PQ makes with the x-axis is minimal
            Coordinate Q = PointWitMinAngleWithX(pts, P);

            /**
             * Iterate over the remaining points to find 
             * a pair or triplet of points which determine the minimal circle.
             * By the design of the algorithm, 
             * at most <tt>pts.length</tt> iterations are required to terminate 
             * with a correct result.
             */
            for (int i = 0; i < pts.Length; i++)
            {
                Coordinate R = PointWithMinAngleWithSegment(pts, P, Q);

                // if PRQ is obtuse, then MBC is determined by P and Q
                if (AngleUtility.IsObtuse(P, R, Q))
                {
                    _extremalPts = new Coordinate[] { new Coordinate(P), new Coordinate(Q) };
                    return;
                }
                // if RPQ is obtuse, update baseline and iterate
                if (AngleUtility.IsObtuse(R, P, Q))
                {
                    P = R;
                    continue;
                }
                // if RQP is obtuse, update baseline and iterate
                if (AngleUtility.IsObtuse(R, Q, P))
                {
                    Q = R;
                    continue;
                }
                // otherwise all angles are acute, and the MBC is determined by the triangle PQR
                _extremalPts = new Coordinate[] { new Coordinate(P), new Coordinate(Q), new Coordinate(R) };
                return;
            }
            Assert.ShouldNeverReachHere("Logic failure in Minimum Bounding Circle algorithm");
        }

        private static Coordinate LowestPoint(Coordinate[] pts)
        {
            Coordinate min = pts[0];
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].Y < min.Y)
                    min = pts[i];
            }
            return min;
        }

        private static Coordinate PointWitMinAngleWithX(Coordinate[] pts, Coordinate P)
        {
            double minSin = Double.MaxValue;
            Coordinate minAngPt = null;
            for (int i = 0; i < pts.Length; i++)
            {

                Coordinate p = pts[i];
                if (p == P) continue;

                /**
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
            double minAng = Double.MaxValue;
            Coordinate minAngPt = null;
            for (int i = 0; i < pts.Length; i++)
            {
                Coordinate p = pts[i];
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