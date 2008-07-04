using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation" /> of the point
    /// on a linear <see cref="Geometry" />nearest a given <see cref="Coordinate"/>.
    /// The nearest point is not necessarily unique; this class
    /// always computes the nearest point closest to the start of the geometry.
    /// </summary>
    public class LocationIndexOfPoint
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearGeom"></param>
        /// <param name="inputPt"></param>
        /// <returns></returns>
        public static LinearLocation IndexOf(IGeometry linearGeom, ICoordinate inputPt)
        {
            LocationIndexOfPoint locater = new LocationIndexOfPoint(linearGeom);
            return locater.IndexOf(inputPt);
        }

        private IGeometry linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LocationIndexOfPoint"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LocationIndexOfPoint(IGeometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }
        
        /// <summary>     
        /// Find the nearest location along a linear {@link Geometry} to a given point.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <returns>The location of the nearest point.</returns>
        public LinearLocation IndexOf(ICoordinate inputPt)
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
        public LinearLocation IndexOfAfter(ICoordinate inputPt, LinearLocation minIndex)
        {
            if (minIndex == null) 
                return IndexOf(inputPt);

            // sanity check for minLocation at or past end of line
            LinearLocation endLoc = LinearLocation.GetEndLocation(linearGeom);
            if (endLoc.CompareTo(minIndex) <= 0)
                return endLoc;

            LinearLocation closestAfter = IndexOfFromStart(inputPt, minIndex);

            /*
             * Return the minDistanceLocation found.
             * This will not be null, since it was initialized to minLocation
             */
            Assert.IsTrue(closestAfter.CompareTo(minIndex) >= 0, "computed location is before specified minimum location");
            return closestAfter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPt"></param>
        /// <param name="minIndex"></param>
        /// <returns></returns>
        private LinearLocation IndexOfFromStart(ICoordinate inputPt, LinearLocation minIndex)
        {
            double minDistance = Double.MaxValue;
            int minComponentIndex = 0;
            int minSegmentIndex = 0;
            double minFrac = -1.0;

            LineSegment seg = new LineSegment();
            foreach (LinearIterator.LinearElement element in new LinearIterator(linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    seg.P0 = element.SegmentStart;
                    seg.P1 = element.SegmentEnd;
                    double segDistance = seg.Distance(inputPt);
                    double segFrac = SegmentFraction(seg, inputPt);

                    int candidateComponentIndex = element.ComponentIndex;
                    int candidateSegmentIndex = element.VertexIndex;
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

            LinearLocation loc = new LinearLocation(minComponentIndex, minSegmentIndex, minFrac);
            return loc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="inputPt"></param>
        /// <returns></returns>
        public static double SegmentFraction(LineSegment seg, ICoordinate inputPt)
        {
            double segFrac = seg.ProjectionFactor(inputPt);
            if (segFrac < 0.0)
                segFrac = 0.0;
            else if (segFrac > 1.0)
                segFrac = 1.0;
            return segFrac;
        }
    }
}
