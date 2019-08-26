using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    ///
    /// </summary>
    public class LengthIndexOfPoint
    {
        public static double IndexOf(Geometry linearGeom, Coordinate inputPt)
        {
            var locater = new LengthIndexOfPoint(linearGeom);
            return locater.IndexOf(inputPt);
        }

        public static double IndexOfAfter(Geometry linearGeom, Coordinate inputPt, double minIndex)
        {
            var locater = new LengthIndexOfPoint(linearGeom);
            return locater.IndexOfAfter(inputPt, minIndex);
        }

        private readonly Geometry _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthIndexOfPoint"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LengthIndexOfPoint(Geometry linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Find the nearest location along a linear <see cref="Geometry"/> to a given point.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <returns>The location of the nearest point.</returns>
        public double IndexOf(Coordinate inputPt)
        {
            return IndexOfFromStart(inputPt, -1.0);
        }

        /// <summary>
        /// Finds the nearest index along the linear <see cref="Geometry" />
        /// to a given <see cref="Coordinate"/> after the specified minimum index.
        /// If possible the location returned will be strictly
        /// greater than the <paramref name="minIndex" />.
        /// If this is not possible, the value returned
        /// will equal <paramref name="minIndex" />.
        /// (An example where this is not possible is when
        /// <paramref name="minIndex" /> = [end of line] ).
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <param name="minIndex">The minimum location for the point location.</param>
        /// <returns>The location of the nearest point.</returns>
        public double IndexOfAfter(Coordinate inputPt, double minIndex)
        {
            if (minIndex < 0.0) return IndexOf(inputPt);

            // sanity check for minIndex at or past end of line
            double endIndex = _linearGeom.Length;
            if (endIndex < minIndex)
                return endIndex;

            double closestAfter = IndexOfFromStart(inputPt, minIndex);

            /*
             * Return the minDistanceLocation found.
             */
            Assert.IsTrue(closestAfter >= minIndex, "computed index is before specified minimum index");
            return closestAfter;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputPt"></param>
        /// <param name="minIndex"></param>
        /// <returns></returns>
        private double IndexOfFromStart(Coordinate inputPt, double minIndex)
        {
            double minDistance = double.MaxValue;

            double ptMeasure = minIndex;
            double segmentStartMeasure = 0.0;

            var seg = new LineSegment();
            var it = new LinearIterator(_linearGeom);

            while (it.HasNext())
            {
                if (!it.IsEndOfLine)
                {
                    seg.P0 = it.SegmentStart;
                    seg.P1 = it.SegmentEnd;
                    double segDistance = seg.Distance(inputPt);
                    double segMeasureToPt = SegmentNearestMeasure(seg, inputPt, segmentStartMeasure);
                    if (segDistance < minDistance
                        && segMeasureToPt > minIndex)
                    {
                        ptMeasure = segMeasureToPt;
                        minDistance = segDistance;
                    }
                    segmentStartMeasure += seg.Length;
                }
                it.Next();
            }
            return ptMeasure;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="inputPt"></param>
        /// <param name="segmentStartMeasure"></param>
        /// <returns></returns>
        private static double SegmentNearestMeasure(LineSegment seg, Coordinate inputPt, double segmentStartMeasure)
        {
            // found new minimum, so compute location distance of point
            double projFactor = seg.ProjectionFactor(inputPt);
            if (projFactor <= 0.0)
                return segmentStartMeasure;
            if (projFactor <= 1.0)
                return segmentStartMeasure + projFactor * seg.Length;
            // ASSERT: projFactor > 1.0
            return segmentStartMeasure + seg.Length;
        }
    }
}