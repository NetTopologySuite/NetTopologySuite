using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Functions to compute topological information
    /// about nodes (ring intersections) in polygonal geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    static class PolygonNode
    {
        /// <summary>
        /// Check if the edges at a node between two rings (or one ring) cross.
        /// The node is topologically valid if the ring edges do not cross.
        /// This function assumes that the edges are not collinear. 
        /// </summary>
        /// <param name="nodePt">The node location</param>
        /// <param name="a0">The previous edge endpoint in a ring</param>
        /// <param name="a1">The next edge endpoint in a ring</param>
        /// <param name="b0">The previous edge endpoint in the other ring</param>
        /// <param name="b1">The next edge endpoint in the other ring</param>
        /// <returns>
        /// <c>true</c> if the edges cross at the node
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
             * Find positions of b0 and b1.  
             * If they are the same they do not cross the other edge
             */
            bool isBetween0 = IsBetween(nodePt, b0, aLo, aHi);
            bool isBetween1 = IsBetween(nodePt, b1, aLo, aHi);

            return isBetween0 != isBetween1;
        }

        /// <summary>
        /// Tests whether an edge node-b lies in the interior or exterior
        /// of a corner of a ring given by a0-node-a1.
        /// The ring interior is assumed to be on the right of the corner (a CW ring).
        /// The edge must not be collinear with the corner segments.
        /// </summary>
        /// <param name="nodePt">The node location</param>
        /// <param name="a0">The first vertex of the corner</param>
        /// <param name="a1">The second vertex of the corner</param>
        /// <param name="b">The destination vertex of the edge</param>
        /// <returns><c>true</c> if the edge is interior to the ring corner</returns>
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
            bool isInterior = (isBetweenVal && isInteriorBetween)
                || (!isBetweenVal && !isInteriorBetween);
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

        private static Quadrant Quadrant(Coordinate origin, Coordinate p)
        {
            double dx = p.X - origin.X;
            double dy = p.Y - origin.Y;
            return new Quadrant(dx, dy);
        }

    }

}
