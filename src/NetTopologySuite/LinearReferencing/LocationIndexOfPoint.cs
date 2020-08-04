using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation" /> of the point
    /// on a linear <see cref="Geometry" />nearest a given <see cref="Coordinate"/>.
    /// The nearest point is not necessarily unique; this class
    /// always computes the nearest point closest
    /// to the start of the geometry.
    /// </summary>
    public class LocationIndexOfPoint
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="linearGeom"></param>
        /// <param name="inputPt"></param>
        /// <returns></returns>
        public static LinearLocation IndexOf(Geometry linearGeom, Coordinate inputPt)
        {
            var locater = new LocationIndexOfPoint(linearGeom);
            return locater.IndexOf(inputPt);
        }

        public static LinearLocation IndexOfAfter(Geometry linearGeom, Coordinate inputPt, LinearLocation minIndex)
        {
            var locater = new LocationIndexOfPoint(linearGeom);
            return locater.IndexOfAfter(inputPt, minIndex);
        }

        private readonly Geometry _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationIndexOfPoint"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LocationIndexOfPoint(Geometry linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Find the nearest location along a linear <see cref="Geometry" /> to a given point.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <returns>The location of the nearest point.</returns>
        public LinearLocation IndexOf(Coordinate inputPt)
        {
            return IndexOfFromStart(inputPt, null);
        }

        /// <summary>
        /// Find the nearest <see cref="LinearLocation" /> along the linear <see cref="Geometry" />
        /// to a given <see cref="Geometry" /> after the specified minimum <see cref="LinearLocation" />.
        /// If possible the location returned will be strictly greater than the <paramref name="minIndex" />.
        /// If this is not possible, the value returned will equal <paramref name="minIndex" />.
        /// (An example where this is not possible is when <paramref name="minIndex" /> = [end of line] ).
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <param name="minIndex">The minimum location for the point location.</param>
        /// <returns>The location of the nearest point.</returns>
        public LinearLocation IndexOfAfter(Coordinate inputPt, LinearLocation minIndex)
        {
            if (minIndex == null)
                return IndexOf(inputPt);

            // sanity check for minLocation at or past end of line
            var endLoc = LinearLocation.GetEndLocation(_linearGeom);
            if (endLoc.CompareTo(minIndex) <= 0)
                return endLoc;

            var closestAfter = IndexOfFromStart(inputPt, minIndex);

            /*
             * Return the minDistanceLocation found.
             * This will not be null, since it was initialized to minLocation
             */
            Assert.IsTrue(closestAfter.CompareTo(minIndex) >= 0, "computed location is before specified minimum location");
            return closestAfter;
        }

        private LinearLocation IndexOfFromStart(Coordinate inputPt, LinearLocation minIndex)
        {
            double minDistance = double.MaxValue;
            int minComponentIndex = 0;
            int minSegmentIndex = 0;
            double minFrac = -1.0;

            var seg = new LineSegment();
            for (var it = new LinearIterator(_linearGeom);
                 it.HasNext(); it.Next())
            {
                if (!it.IsEndOfLine)
                {
                    seg.P0 = it.SegmentStart;
                    seg.P1 = it.SegmentEnd;
                    double segDistance = seg.Distance(inputPt);
                    double segFrac = seg.SegmentFraction(inputPt);

                    int candidateComponentIndex = it.ComponentIndex;
                    int candidateSegmentIndex = it.VertexIndex;
                    if (segDistance < minDistance)
                    {
                        // ensure after minLocation, if any
                        if (minIndex == null ||
                            minIndex.CompareLocationValues(candidateComponentIndex, candidateSegmentIndex, segFrac) < 0)
                        {
                            // otherwise, save this as new minimum
                            minComponentIndex = candidateComponentIndex;
                            minSegmentIndex = candidateSegmentIndex;
                            minFrac = segFrac;
                            minDistance = segDistance;
                        }
                    }
                }
            }

            if (minDistance == double.MaxValue)
            {
                // no minimum was found past minLocation, so return it
                return new LinearLocation(minIndex);
            }
            // otherwise, return computed location
            var loc = new LinearLocation(minComponentIndex, minSegmentIndex, minFrac);
            return loc;
        }

        /*
        /// <summary>
        /// Computes the fraction of distance (in <c>[0.0, 1.0]</c>) that a point occurs along a line segment.
        /// </summary>
        /// <remarks>If the point is beyond either ends of the line segment, the closest fractional value (<c>0.0</c> or <c>1.0</c>) is returned.</remarks>
        /// <param name="seg">The line segment to use</param>
        /// <param name="inputPt">The point</param>
        /// <returns>The fraction along the line segment the point occurs</returns>
        public static double SegmentFraction(LineSegment seg, Coordinate inputPt)
        {
            double segFrac = seg.ProjectionFactor(inputPt);
            if (segFrac < 0.0)
                segFrac = 0.0;
            else if (segFrac > 1.0)
                segFrac = 1.0;
            return segFrac;
        }
        */
    }
}