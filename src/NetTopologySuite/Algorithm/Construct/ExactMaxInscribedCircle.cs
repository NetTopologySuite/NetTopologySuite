using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Computes the Maximum Inscribed Circle for some kinds of convex polygons.
    /// It determines the circle center point by computing Voronoi node points
    /// and testing them for distance to generating edges.
    /// This is more precise than iterated approximation,
    /// and faster for small polygons (such as triangles and convex quadrilaterals).
    /// </summary>
    /// <author>Martin Davis</author>
    internal static class ExactMaxInscribedCircle
    {
        /// <summary>
        /// Tests whether a given geometry is supported by this class.
        /// Currently only triangles and convex quadrilaterals are supported.
        /// </summary>
        public static bool IsSupported(Geometry geom)
        {
            if (!IsSimplePolygon(geom))
                return false;
            var polygon = (Polygon)geom;
            if (IsTriangle(polygon))
                return true;
            if (IsQuadrilateral(polygon) && IsConvex(polygon))
                return true;
            return false;
        }

        private static bool IsSimplePolygon(Geometry geom)
        {
            return geom is Polygon polygon && polygon.NumInteriorRings == 0;
        }

        private static bool IsTriangle(Polygon polygon) => polygon.NumPoints == 4;

        private static bool IsQuadrilateral(Polygon polygon) => polygon.NumPoints == 5;

        public static Coordinate[] ComputeRadius(Polygon polygon)
        {
            var ring = polygon.ExteriorRing.Coordinates;
            if (ring.Length == 4)
                return ComputeTriangle(ring);
            if (ring.Length == 5)
                return ComputeConvexQuadrilateral(ring);
            throw new System.ArgumentException("Input must be a triangle or convex quadrilateral");
        }

        private static Coordinate[] ComputeTriangle(Coordinate[] ring)
        {
            var center = Triangle.InCentre(ring[0], ring[1], ring[2]);
            var seg = new LineSegment(ring[0], ring[1]);
            var radius = seg.Project(center);
            return new[] { center, radius };
        }

        /// <summary>
        /// The Voronoi nodes of a convex polygon occur at the intersection point
        /// of two bisectors of each triplet of edges.
        /// The Maximum Inscribed Circle center is the node
        /// with the farthest distance from the generating edges.
        /// For a quadrilateral there are 4 distinct edge triplets,
        /// at each edge with its adjacent edges.
        /// </summary>
        private static Coordinate[] ComputeConvexQuadrilateral(Coordinate[] ring)
        {
            var ringCW = OrientClockwise(ring);

            double diameter = CoordinateArrays.Envelope(ringCW).Diameter;
            //-- expand diameter for robustness
            double diamWithTolerance = 2 * diameter;

            //-- compute corner bisectors
            var bisector = ComputeBisectors(ringCW, diamWithTolerance);
            //-- compute nodes and find interior one farthest from sides
            double maxDist = -1;
            Coordinate center = null;
            Coordinate radius = null;
            for (int i = 0; i < 4; i++)
            {
                var b1 = bisector[i];
                int i2 = (i + 1) % 4;
                var b2 = bisector[i2];

                var nodePt = b1.Intersection(b2);
                //-- if bisector segments don't intersect node is outside polygon
                if (nodePt == null)
                    continue;

                //-- only interior nodes are considered
                if (!IsPointInConvexRing(ringCW, nodePt))
                    continue;

                //-- check if node is further than current max center
                var r = NearestEdgePt(ringCW, nodePt);
                double dist = nodePt.Distance(r);
                if (maxDist < 0 || dist > maxDist)
                {
                    center = nodePt;
                    radius = r;
                    maxDist = dist;
                }
            }
            return new[] { center, radius };
        }

        private static Coordinate[] OrientClockwise(Coordinate[] ring)
        {
            bool isCCW = Orientation.IsCCW(ring);
            if (!isCCW) return ring;
            var copy = (Coordinate[])ring.Clone();
            CoordinateArrays.Reverse(copy);
            return copy;
        }

        private static LineSegment[] ComputeBisectors(Coordinate[] ptsCW, double diameter)
        {
            var bisector = new LineSegment[4];
            for (int i = 0; i < 4; i++)
            {
                bisector[i] = ComputeConvexBisector(ptsCW, i, diameter);
            }
            return bisector;
        }

        private static Coordinate NearestEdgePt(Coordinate[] ring, Coordinate pt)
        {
            Coordinate nearestPt = null;
            double minDist = -1;
            for (int i = 0; i < ring.Length - 1; i++)
            {
                var edge = new LineSegment(ring[i], ring[i + 1]);
                var r = edge.ClosestPoint(pt);
                double dist = pt.Distance(r);
                if (minDist < 0 || dist < minDist)
                {
                    minDist = dist;
                    nearestPt = r;
                }
            }
            return nearestPt;
        }

        private static LineSegment ComputeConvexBisector(Coordinate[] pts, int index, double len)
        {
            var basePt = pts[index];
            int iPrev = index == 0 ? pts.Length - 2 : index - 1;
            int iNext = index >= pts.Length ? 0 : index + 1;
            var pPrev = pts[iPrev];
            var pNext = pts[iNext];

            //-- this should never happen, since only convex quads are handled
            if (IsConcave(pPrev, basePt, pNext))
                throw new System.InvalidOperationException("Input is not convex");

            double bisectAng = AngleUtility.Bisector(pPrev, basePt, pNext);
            var endPt = AngleUtility.Project(basePt, bisectAng, len);
            return new LineSegment(basePt.Copy(), endPt);
        }

        private static bool IsConvex(Polygon polygon)
        {
            var shell = polygon.ExteriorRing;
            return IsConvex(shell.CoordinateSequence);
        }

        private static bool IsConvex(CoordinateSequence ring)
        {
            //-- A ring cannot be all concave, so if it has a consistent
            //-- orientation it must be convex.
            int n = ring.Count;
            if (n < 4)
                return false;
            //-- triangles must be convex
            if (n == 4)
                return true;
            //-- check for all convex or collinear angles
            OrientationIndex ringOrient = OrientationIndex.None;
            for (int i = 0; i < n - 1; i++)
            {
                int i1 = i + 1;
                int i2 = (i1 >= n - 1) ? 1 : i1 + 1;
                var orient = Orientation.Index(ring.GetCoordinate(i),
                    ring.GetCoordinate(i1), ring.GetCoordinate(i2));
                if (orient == OrientationIndex.Collinear)
                    continue;
                if (ringOrient == OrientationIndex.None)
                {
                    ringOrient = orient;
                }
                else if (orient != ringOrient)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsConcave(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            return Orientation.Index(p0, p1, p2) == OrientationIndex.CounterClockwise;
        }

        private static bool IsPointInConvexRing(Coordinate[] ringCW, Coordinate p)
        {
            for (int i = 0; i < ringCW.Length - 1; i++)
            {
                var p0 = ringCW[i];
                var p1 = ringCW[i + 1];
                var orient = Orientation.Index(p0, p1, p);
                if (orient == OrientationIndex.CounterClockwise)
                    return false;
            }
            return true;
        }
    }
}
