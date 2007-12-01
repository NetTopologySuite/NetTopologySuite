using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    public class LengthIndexOfPoint<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        public static Double IndexOf(IGeometry<TCoordinate> linearGeometry, TCoordinate inputPt)
        {
            LengthIndexOfPoint<TCoordinate> locater = new LengthIndexOfPoint<TCoordinate>(linearGeometry);
            return locater.IndexOf(inputPt);
        }

        public static Double IndexOfAfter(IGeometry<TCoordinate> linearGeometry, ICoordinate inputPt, Double minIndex)
        {
            LengthIndexOfPoint<TCoordinate> locater = new LengthIndexOfPoint<TCoordinate>(linearGeometry);
            return locater.IndexOfAfter(inputPt, minIndex);
        }

        private readonly ICurve<TCoordinate> _linearGeometry;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthIndexOfPoint{TCoordinate}"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LengthIndexOfPoint(IGeometry<TCoordinate> linearGeom)
        {
            this._linearGeometry = linearGeom;
        }

        /// <summary>
        /// Find the nearest location along a linear {@link Geometry} to a given point.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <returns>The location of the nearest point.</returns>
        public Double IndexOf(ICoordinate inputPt)
        {
            return IndexOfFromStart(inputPt, -1.0);
        }

        /// <summary>
        /// Finds the nearest index along the linear <see cref="Geometry{TCoordinate}" />
        /// to a given <typeparamref name="TCoordinate"/> after the specified minimum index.
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
        public Double IndexOfAfter(ICoordinate inputPt, Double minIndex)
        {
            if (minIndex < 0.0)
            {
                return IndexOf(inputPt);
            }

            // sanity check for minIndex at or past end of line
            Double endIndex = _linearGeometry.Length;

            if (endIndex < minIndex)
            {
                return endIndex;
            }

            Double closestAfter = IndexOfFromStart(inputPt, minIndex);

            /*
             * Return the minDistanceLocation found.
             * This will not be null, since it was initialized to minLocation
             */
            Assert.IsTrue(closestAfter > minIndex, "computed index is before specified minimum index");
            return closestAfter;
        }

        private Double IndexOfFromStart(TCoordinate inputPt, Double minIndex)
        {
            Double minDistance = Double.MaxValue;

            Double ptMeasure = minIndex;
            Double segmentStartMeasure = 0.0;

            LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>();

            foreach (LinearIterator.LinearElement element in new LinearIterator(_linearGeometry))
            {
                if (!element.IsEndOfLine)
                {
                    seg.P0 = element.SegmentStart;
                    seg.P1 = element.SegmentEnd;
                    Double segDistance = seg.Distance(inputPt);
                    Double segMeasureToPt = SegmentNearestMeasure(seg, inputPt, segmentStartMeasure);
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

        private Double SegmentNearestMeasure(LineSegment seg, ICoordinate inputPt, Double segmentStartMeasure)
        {
            // found new minimum, so compute location distance of point
            Double projFactor = seg.ProjectionFactor(inputPt);
            if (projFactor <= 0.0)
            {
                return segmentStartMeasure;
            }
            if (projFactor <= 1.0)
            {
                return segmentStartMeasure + projFactor*seg.Length;
            }
            // ASSERT: projFactor > 1.0
            return segmentStartMeasure + seg.Length;
        }
    }
}