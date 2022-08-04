using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Detects invalid coverage topology where ring segments interact.
    /// The inputs to <see cref="ProcessIntersections(ISegmentString, int, ISegmentString, int)"/>
    /// must be <see cref="CoverageRing"/>s.
    /// If an invalid situation is detected the input target segment is
    /// marked invalid using {@link CoverageRing#markInvalid(int)}.
    /// </summary>
    internal class InvalidSegmentDetector : ISegmentIntersector
    {
        private readonly double _distanceTol;

        ///// <summary>
        ///// Creates an invalid segment detector.
        ///// </summary>
        //public InvalidSegmentDetector()
        //{
        //}

        public InvalidSegmentDetector(double distanceTol)
        {
            _distanceTol = distanceTol;
        }

        /// <summary>
        /// Process interacting segments.
        /// The input order is important.
        /// The adjacent segment is first, the target is second.
        /// The inputs must be <see cref="CoverageRing"/>s.
        /// </summary>
        public void ProcessIntersections(ISegmentString ssAdj, int iAdj, ISegmentString ssTarget, int iTarget)
        {
            // note the source of the edges is important
            var target = (CoverageRing)ssTarget;
            var adj = (CoverageRing)ssAdj;

            //-- skip target segments with known status
            if (target.IsKnown(iTarget)) return;

            var t0 = target.Coordinates[iTarget];
            var t1 = target.Coordinates[iTarget + 1];
            var adj0 = adj.Coordinates[iAdj];
            var adj1 = adj.Coordinates[iAdj + 1];
            /*
            System.out.println("checking target= " + WKTWriter.toLineString(t0, t1)
             + "   adj= " + WKTWriter.toLineString(adj0, adj1));
            //*/

            //-- skip zero-length segments
            if (t0.Equals2D(t1) || adj0.Equals2D(adj1))
                return;

            /*
            //-- skip segments beyond distance tolerance
            if (distanceTol < Distance.segmentToSegment(t0, t1, adj0, adj1)) {
              return;
            } 
            */

            bool isInvalid = IsInvalid(t0, t1, adj0, adj1, adj, iAdj);
            if (isInvalid)
            {
                target.MarkInvalid(iTarget);
            }
        }

        private bool IsInvalid(Coordinate tgt0, Coordinate tgt1,
            Coordinate adj0, Coordinate adj1, CoverageRing adj, int indexAdj)
        {

            //-- segments that are collinear (but not matching) or are interior are invalid
            if (IsCollinearOrInterior(tgt0, tgt1, adj0, adj1, adj, indexAdj))
                return true;

            //-- segments which are nearly parallel for a significant length are invalid
            if (_distanceTol > 0 && IsNearlyParallel(tgt0, tgt1, adj0, adj1, _distanceTol))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the segments are collinear, or if the target segment
        /// intersects the interior of the adjacent ring.
        /// Segments which are collinear must be non-equal and hence invalid,
        /// since matching segments have already been marked as valid and
        /// are not passed to this code. 
        /// </summary>
        private bool IsCollinearOrInterior(Coordinate tgt0, Coordinate tgt1,
            Coordinate adj0, Coordinate adj1, CoverageRing adj, int indexAdj)
        {
            var li = new RobustLineIntersector();
            li.ComputeIntersection(tgt0, tgt1, adj0, adj1);

            //-- segments do not interact
            if (!li.HasIntersection)
                return false;

            //-- If the segments are collinear, they do not match, so are invalid.
            if (li.IntersectionNum == 2)
            {
                //TODO: assert segments are not equal?
                return true;
            }

            //-- target segment crosses, or segments touch at non-endpoint
            if (li.IsProper || li.IsInteriorIntersection())
            {
                return true;
            }

            /*
             * At this point the segments have a single intersection point 
             * which is an endpoint of both segments.
             * 
             * Check if the target segment lies in the interior of the adj ring.
             */
            var intVertex = li.GetIntersection(0);
            bool isInterior = IsInteriorSegment(intVertex, tgt0, tgt1, adj, indexAdj);
            return isInterior;
        }

        private bool IsInteriorSegment(Coordinate intVertex, Coordinate tgt0, Coordinate tgt1,
            CoverageRing adj, int indexAdj)
        {
            //-- find target segment endpoint which is not the intersection point
            var tgtEnd = intVertex.Equals2D(tgt0) ? tgt1 : tgt0;

            //-- find adjacent-ring vertices on either side of intersection vertex
            var adjPrev = adj.FindVertexPrev(indexAdj, intVertex);
            var adjNext = adj.FindVertexNext(indexAdj, intVertex);
            //-- if needed, re-orient corner to have interior on right
            if (!adj.IsInteriorOnRight)
            {
                var temp = adjPrev;
                adjPrev = adjNext;
                adjNext = temp;
            }

            bool isInterior = PolygonNodeTopology.IsInteriorSegment(intVertex, adjPrev, adjNext, tgtEnd);
            return isInterior;
        }

        private static bool IsNearlyParallel(Coordinate p00, Coordinate p01,
            Coordinate p10, Coordinate p11, double distanceTol)
        {
            var line0 = new LineSegment(p00, p01);
            var line1 = new LineSegment(p10, p11);
            var proj0 = line0.Project(line1);
            if (proj0 == null)
                return false;
            var proj1 = line1.Project(line0);
            if (proj1 == null)
                return false;

            if (proj0.Length <= distanceTol
                || proj1.Length <= distanceTol)
                return false;

            if (proj0.P0.Distance(proj1.P1) < proj0.P0.Distance(proj1.P0))
            {
                proj1.Reverse();
            }
            return proj0.P0.Distance(proj1.P0) <= distanceTol
                && proj0.P1.Distance(proj1.P1) <= distanceTol;
        }

        public bool IsDone => false; // process all intersections

    }
}
