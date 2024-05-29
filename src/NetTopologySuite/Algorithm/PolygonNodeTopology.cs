using NetTopologySuite.Geometries;
using static System.Collections.Specialized.BitVector32;
using System.Net.NetworkInformation;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions to compute topological information
    /// about nodes (ring intersections) in polygonal geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    static class PolygonNodeTopology
    {
        /// <summary>
        /// Check if four segments at a node cross.
        /// Typically the segments lie in two different rings, or different sections of one ring.
        /// The node is topologically valid if the rings do not cross.
        /// If any segments are collinear, the test returns false.
        /// </summary>
        /// <param name="nodePt">The node location</param>
        /// <param name="a0">The previous segment endpoint in a ring</param>
        /// <param name="a1">The next segment endpoint in a ring</param>
        /// <param name="b0">The previous segment endpoint in the other ring</param>
        /// <param name="b1">The next segment endpoint in the other ring</param>
        /// <returns>
        /// <c>true</c> if the rings cross at the node
        /// </returns>
        public static bool IsCrossing(Coordinate nodePt, Coordinate a0, Coordinate a1, Coordinate b0, Coordinate b1)
        {
            var aLo = a0;
            var aHi = a1;
            if (IsAngleGreater(nodePt, aLo, aHi))
            {
                aLo = a1;
                aHi = a0;
            }
            /*
            boolean isBetween0 = isBetween(nodePt, b0, aLo, aHi);
            boolean isBetween1 = isBetween(nodePt, b1, aLo, aHi);

            return isBetween0 != isBetween1;
            */

            /*
             * Find positions of b0 and b1.  
             * The edges cross if the positions are different.
             * If any edge is collinear they are reported as not crossing
             */
            int compBetween0 = CompareBetween(nodePt, b0, aLo, aHi);
            if (compBetween0 == 0) return false;
            int compBetween1 = CompareBetween(nodePt, b1, aLo, aHi);
            if (compBetween1 == 0) return false;

            return compBetween0 != compBetween1;

        }

        /// <summary>
        /// Tests whether an segment node-b lies in the interior or exterior
        /// of a corner of a ring formed by the two segments a0-node-a1.
        /// The ring interior is assumed to be on the right of the corner
        /// (i.e. a CW shell or CCW hole).
        /// The test segment must not be collinear with the corner segments.
        /// </summary>
        /// <param name="nodePt">The node location</param>
        /// <param name="a0">The first vertex of the corner</param>
        /// <param name="a1">The second vertex of the corner</param>
        /// <param name="b">The other vertex of the test segment</param>
        /// <returns><c>true</c> if the segment is interior to the ring corner</returns>
        public static bool IsInteriorSegment(Coordinate nodePt, Coordinate a0, Coordinate a1, Coordinate b)
        {
            var aLo = a0;
            var aHi = a1;
            bool isInteriorBetween = true;
            if (IsAngleGreater(nodePt, aLo, aHi))
            {
                aLo = a1;
                aHi = a0;
                isInteriorBetween = false;
            }
            bool isBetweenVal = IsBetween(nodePt, b, aLo, aHi);
            bool isInterior = isBetweenVal && isInteriorBetween
                || !isBetweenVal && !isInteriorBetween;
            return isInterior;
        }

        /// <summary>
        /// Tests if an edge p is between edges e0 and e1,
        /// where the edges all originate at a common origin.
        /// The "inside" of e0 and e1 is the arc which does not include the origin.
        /// The edges are assumed to be distinct (non-collinear).
        /// </summary>
        /// <param name="origin">the origin</param>
        /// <param name="p">the destination point of edge p</param>
        /// <param name="e0">the destination point of edge e0</param>
        /// <param name="e1">the destination point of edge e1</param>
        /// <returns><c>true</c> if p is between e0 and e1</returns>
        private static bool IsBetween(Coordinate origin, Coordinate p, Coordinate e0, Coordinate e1)
        {
            bool isGreater0 = IsAngleGreater(origin, p, e0);
            if (!isGreater0) return false;
            bool isGreater1 = IsAngleGreater(origin, p, e1);
            return !isGreater1;
        }

        /// <summary>
        /// Compares whether an edge p is between or outside the edges e0 and e1,
        /// where the edges all originate at a common origin.
        /// The "inside" of e0 and e1 is the arc which does not include
        /// the positive X-axis at the origin.
        /// If p is collinear with an edge 0 is returned.
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="p">The destination point of edge p</param>
        /// <param name="e0">The destination point of edge e0</param>
        /// <param name="e1">The destination point of edge e1</param>
        /// <returns>A negative integer, zero or positive integer as the vector P lies outside, collinear with, or inside the vectors E0 and E1</returns>
        private static int CompareBetween(Coordinate origin, Coordinate p, Coordinate e0, Coordinate e1)
        {
            int comp0 = CompareAngle(origin, p, e0);
            if (comp0 == 0) return 0;
            int comp1 = CompareAngle(origin, p, e1);
            if (comp1 == 0) return 0;
            if (comp0 > 0 && comp1 < 0) return 1;
            return -1;
        }

        /// <summary>
        /// Tests if the angle with the origin of a vector P is greater than that of the
        /// vector Q.
        /// </summary>
        /// <param name="origin">The origin of the vectors</param>
        /// <param name="p">The endpoint of the vector P</param>
        /// <param name="q">The endpoint of the vector Q</param>
        /// <returns><c>true</c> if vector P has angle greater than Q</returns>
        private static bool IsAngleGreater(Coordinate origin, Coordinate p, Coordinate q)
        {
            var quadrantP = Quadrant(origin, p);
            var quadrantQ = Quadrant(origin, q);

            /*
             * If the vectors are in different quadrants, 
             * that determines the ordering
             */
            if (quadrantP > quadrantQ) return true;
            if (quadrantP < quadrantQ) return false;

            //--- vectors are in the same quadrant
            // Check relative orientation of vectors
            // P > Q if it is CCW of Q
            var orient = Orientation.Index(origin, q, p);
            return orient == OrientationIndex.CounterClockwise;
        }

        /// <summary>
        /// Compares the angles of two vectors
        /// relative to the positive X-axis at their origin.
        /// Angles increase CCW from the X-axis.
        /// </summary>
        /// <param name="origin">The origin of the vectors</param>
        /// <param name="p">The endpoint of the vector P</param>
        /// <param name="q">The endpoint of the vector Q</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this vector P has angle less than, equal to, or greater than vector Q
        /// </returns>
        public static int CompareAngle(Coordinate origin, Coordinate p, Coordinate q)
        {
            var quadrantP = Quadrant(origin, p);
            var quadrantQ = Quadrant(origin, q);

            /*
             * If the vectors are in different quadrants, 
             * that determines the ordering
             */
            if (quadrantP > quadrantQ) return 1;
            if (quadrantP < quadrantQ) return -1;

            //--- vectors are in the same quadrant
            // Check relative orientation of vectors
            // P > Q if it is CCW of Q
            var orient = Orientation.Index(origin, q, p);
            switch (orient)
            {
                case OrientationIndex.CounterClockwise: return 1;
                case OrientationIndex.Clockwise: return -1;
                default: return 0;
            }
        }


        private static Quadrant Quadrant(Coordinate origin, Coordinate p)
        {
            double dx = p.X - origin.X;
            double dy = p.Y - origin.Y;
            return new Quadrant(dx, dy);
        }

    }

}
