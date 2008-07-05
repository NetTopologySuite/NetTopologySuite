using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{

    /// <summary>
    /// 
    /// </summary>
    public class LengthIndexOfPoint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearGeom"></param>
        /// <param name="inputPt"></param>
        /// <returns></returns>
        public static double IndexOf(IGeometry linearGeom, ICoordinate inputPt)
        {
            LengthIndexOfPoint locater = new LengthIndexOfPoint(linearGeom);
            return locater.IndexOf(inputPt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearGeom"></param>
        /// <param name="inputPt"></param>
        /// <param name="minIndex"></param>
        /// <returns></returns>
        public static double IndexOfAfter(IGeometry linearGeom, ICoordinate inputPt, double minIndex)
        {
            LengthIndexOfPoint locater = new LengthIndexOfPoint(linearGeom);
            return locater.IndexOfAfter(inputPt, minIndex);
        }

        private IGeometry linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthIndexOfPoint"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LengthIndexOfPoint(IGeometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }

        /// <summary>
        /// Find the nearest location along a linear {@link Geometry} to a given point.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <returns>The location of the nearest point.</returns>
        public double IndexOf(ICoordinate inputPt)
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
        public double IndexOfAfter(ICoordinate inputPt, double minIndex)
        {
            if (minIndex < 0.0) return IndexOf(inputPt);

            // sanity check for minIndex at or past end of line
            double endIndex = linearGeom.Length;
            if (endIndex < minIndex)
                return endIndex;

            double closestAfter = IndexOfFromStart(inputPt, minIndex);

            /*
             * Return the minDistanceLocation found.
             * This will not be null, since it was initialized to minLocation
             */
            Assert.IsTrue(closestAfter > minIndex, "computed index is before specified minimum index");
            return closestAfter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPt"></param>
        /// <param name="minIndex"></param>
        /// <returns></returns>
        private double IndexOfFromStart(ICoordinate inputPt, double minIndex)
        {
            double minDistance = Double.MaxValue;

            double ptMeasure = minIndex;
            double segmentStartMeasure = 0.0;

            LineSegment seg = new LineSegment();
            foreach(LinearIterator.LinearElement element in new LinearIterator(linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    seg.P0 = element.SegmentStart;
                    seg.P1 = element.SegmentEnd;
                    double segDistance = seg.Distance(inputPt);
                    double segMeasureToPt = SegmentNearestMeasure(seg, inputPt, segmentStartMeasure);
                    if (segDistance < minDistance && segMeasureToPt > minIndex)
                    {
                        ptMeasure = segMeasureToPt;
                        minDistance = segDistance;
                    }
                    segmentStartMeasure += seg.Length;
                }                
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
        private double SegmentNearestMeasure(LineSegment seg, ICoordinate inputPt, double segmentStartMeasure)
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
