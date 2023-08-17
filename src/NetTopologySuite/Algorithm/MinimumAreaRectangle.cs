using NetTopologySuite.Geometries;
using System;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the minimum-area rectangle enclosing a <see cref="Geometry"/>.
    /// Unlike the <see cref="Envelope"/>, the rectangle may not be axis-parallel.
    /// <para/>
    /// The first step in the algorithm is computing the convex hull of the Geometry.
    /// If the input Geometry is known to be convex, a hint can be supplied to
    /// avoid this computation.
    /// <para/>
    /// In degenerate cases the minimum enclosing geometry
    /// may be a <see cref="LineString"/> or a <see cref="Point"/>.
    /// <para/>
    /// The minimum - area enclosing rectangle does not necessarily
    /// have the minimum possible width.
    /// Use {@link MinimumDiameter} to compute this.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="MinimumDiameter"/>
    /// <seealso cref="ConvexHull"/>
    public sealed class MinimumAreaRectangle
    {
        /// <summary>
        /// Gets the minimum-area rectangular {@link Polygon} which encloses the input geometry.
        /// If the convex hull of the input is degenerate (a line or point)
        /// a {@link LineString} or {@link Point} is returned.
        /// </summary>
        /// <param name="geom">A Geometry</param>
        /// <returns>The minimum rectangle enclosing the geometry</returns>
        public static Geometry GetMinimumRectangle(Geometry geom)
        {
            return (new MinimumAreaRectangle(geom)).GetMinimumRectangle();
        }

        private readonly Geometry _inputGeom;
        private readonly bool _isConvex;

        /// <summary>
        /// Compute a minimum-area rectangle for a given {@link Geometry}.
        /// </summary>
        /// <param name="inputGeom">A Geometry</param>
        public MinimumAreaRectangle(Geometry inputGeom) :
            this(inputGeom, false)
        { }

        /// <summary>
        /// Compute a minimum rectangle for a <see cref="Geometry"/>,
        /// with a hint if the geometry is convex
        /// (e.g. a convex Polygon or LinearRing,
        /// or a two-point LineString, or a Point).
        /// </summary>
        /// <param name="inputGeom">A Geometry</param>
        /// <param name="isConvex">A flag indicating if <paramref name="inputGeom"/> is convex</param>
        public MinimumAreaRectangle(Geometry inputGeom, bool isConvex)
        {
            _inputGeom = inputGeom;
            _isConvex = isConvex;
        }

        private Geometry GetMinimumRectangle()
        {
            if (_inputGeom.IsEmpty)
            {
                return _inputGeom.Factory.CreatePolygon();
            }
            if (_isConvex)
            {
                return ComputeConvex(_inputGeom);
            }
            var convexGeom = (new ConvexHull(_inputGeom)).GetConvexHull();
            return ComputeConvex(convexGeom);
        }

        private Geometry ComputeConvex(Geometry convexGeom)
        {
            //System.out.println("Input = " + geom);
            Coordinate[] convexHullPts;
            if (convexGeom is Polygon cp)
                convexHullPts = cp.ExteriorRing.Coordinates;
            else
                convexHullPts = convexGeom.Coordinates;

            // special cases for lines or points or degenerate rings
            if (convexHullPts.Length == 0)
            {
            }
            else if (convexHullPts.Length == 1)
            {
                return _inputGeom.Factory.CreatePoint(convexHullPts[0].Copy());
            }
            else if (convexHullPts.Length == 2 || convexHullPts.Length == 3)
            {
                //-- Min rectangle is a line. Use the diagonal of the extent
                return ComputeMaximumLine(convexHullPts, _inputGeom.Factory);
            }
            //TODO: ensure ring is CW
            return ComputeConvexRing(convexHullPts);
        }

        /// <summary>
        /// Computes the minimum-area rectangle for a convex ring of {@link Coordinate}s.
        /// <para/>
        /// This algorithm uses the "dual rotating calipers" technique.
        /// Performance is linear in the number of segments.
        /// </summary>
        /// <param name="ring">The convex ring to scan</param>
        private Polygon ComputeConvexRing(Coordinate[] ring)
        {
            // Assert: ring is oriented CW

            double minRectangleArea = double.MaxValue;
            int minRectangleBaseIndex = -1;
            int minRectangleDiamIndex = -1;
            int minRectangleLeftIndex = -1;
            int minRectangleRightIndex = -1;

            //-- start at vertex after first one
            int diameterIndex = 1;
            int leftSideIndex = 1;
            int rightSideIndex = -1; // initialized once first diameter is found

            var segBase = new LineSegment();
            var segDiam = new LineSegment();
            // for each segment, find the next vertex which is at maximum distance
            for (int i = 0; i < ring.Length - 1; i++)
            {
                segBase.P0 = ring[i];
                segBase.P1 = ring[i + 1];
                diameterIndex = FindFurthestVertex(ring, segBase, diameterIndex, 0);

                var diamPt = ring[diameterIndex];
                var diamBasePt = segBase.Project(diamPt);
                segDiam.P0 = diamBasePt;
                segDiam.P1 = diamPt;

                leftSideIndex = FindFurthestVertex(ring, segDiam, leftSideIndex, 1);

                //-- init the max right index
                if (i == 0)
                {
                    rightSideIndex = diameterIndex;
                }
                rightSideIndex = FindFurthestVertex(ring, segDiam, rightSideIndex, -1);

                double rectWidth = segDiam.DistancePerpendicular(ring[leftSideIndex])
                    + segDiam.DistancePerpendicular(ring[rightSideIndex]);
                double rectArea = segDiam.Length * rectWidth;

                if (rectArea < minRectangleArea)
                {
                    minRectangleArea = rectArea;
                    minRectangleBaseIndex = i;
                    minRectangleDiamIndex = diameterIndex;
                    minRectangleLeftIndex = leftSideIndex;
                    minRectangleRightIndex = rightSideIndex;
                }
            }
            return Rectangle.CreateFromSidePts(
                ring[minRectangleBaseIndex], ring[minRectangleBaseIndex + 1],
                ring[minRectangleDiamIndex],
                ring[minRectangleLeftIndex], ring[minRectangleRightIndex],
                _inputGeom.Factory);
        }

        private int FindFurthestVertex(Coordinate[] pts, LineSegment baseSeg, int startIndex, int orient)
        {
            double maxDistance = OrientedDistance(baseSeg, pts[startIndex], orient);
            double nextDistance = maxDistance;
            int maxIndex = startIndex;
            int nextIndex = maxIndex;
            //-- rotate "caliper" while distance from base segment is non-decreasing
            while (IsFurtherOrEqual(nextDistance, maxDistance, orient))
            {
                maxDistance = nextDistance;
                maxIndex = nextIndex;

                nextIndex = NextIndex(pts, maxIndex);
                if (nextIndex == startIndex)
                    break;
                nextDistance = OrientedDistance(baseSeg, pts[nextIndex], orient);
            }
            return maxIndex;
        }

        private bool IsFurtherOrEqual(double d1, double d2, int orient)
        {
            switch (orient)
            {
                case 0: return Math.Abs(d1) >= Math.Abs(d2);
                case 1: return d1 >= d2;
                case -1: return d1 <= d2;
            }
            throw new ArgumentException($"Invalid orientation index: {orient}", nameof(orient));
        }

        private static double OrientedDistance(LineSegment seg, Coordinate p, int orient)
        {
            double dist = seg.DistancePerpendicularOriented(p);
            if (orient == 0)
            {
                return Math.Abs(dist);
            }
            return dist;
        }

        private static int NextIndex(Coordinate[] ring, int index)
        {
            index++;
            if (index >= ring.Length - 1) index = 0;
            return index;
        }

        /// <summary>
        /// Creates a line of maximum extent from the provided vertices
        /// </summary>
        /// <param name="pts">The vertices</param>
        /// <param name="factory">The geometry factory</param>
        /// <returns>The line of maximum extent</returns>
        private static LineString ComputeMaximumLine(Coordinate[] pts, GeometryFactory factory)
        {
            //-- find max and min pts for X and Y
            Coordinate ptMinX = null;
            Coordinate ptMaxX = null;
            Coordinate ptMinY = null;
            Coordinate ptMaxY = null;
            foreach (var p in pts)
            {
                if (ptMinX == null || p.X < ptMinX.X) ptMinX = p;
                if (ptMaxX == null || p.X > ptMaxX.X) ptMaxX = p;
                if (ptMinY == null || p.Y < ptMinY.Y) ptMinY = p;
                if (ptMaxY == null || p.Y > ptMaxY.Y) ptMaxY = p;
            }
            var p0 = ptMinX;
            var p1 = ptMaxX;
            //-- line is vertical - use Y pts
            if (p0.X == p1.X)
            {
                p0 = ptMinY;
                p1 = ptMaxY;
            }
            return factory.CreateLineString(new Coordinate[] { p0.Copy(), p1.Copy() });
        }
    }
}
