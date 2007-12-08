using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation{TCoordinate}" /> of the point
    /// on a linear <see cref="Geometry{TCoordinate}" />nearest a given <typeparamref name="TCoordinate"/>.
    /// The nearest point is not necessarily unique; this class
    /// always computes the nearest point closest to the start of the geometry.
    /// </summary>
    public class LocationIndexOfPoint<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        public static LinearLocation<TCoordinate> IndexOf(IGeometry<TCoordinate> linearGeom, TCoordinate inputPt)
        {
            LocationIndexOfPoint<TCoordinate> locater = new LocationIndexOfPoint<TCoordinate>(linearGeom);
            return locater.IndexOf(inputPt);
        }

        private readonly IGeometry<TCoordinate> _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationIndexOfPoint{TCoordinate}"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LocationIndexOfPoint(IGeometry<TCoordinate> linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>     
        /// Find the nearest location along a linear {@link Geometry} to a given point.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <returns>The location of the nearest point.</returns>
        public LinearLocation<TCoordinate> IndexOf(TCoordinate inputPt)
        {
            return indexOfFromStart(inputPt, null);
        }

        /// <summary>
        /// Find the nearest <see cref="LinearLocation{TCoordinate}" /> 
        /// along the linear <see cref="Geometry{TCoordinate}" /> 
        /// to a given <see cref="Geometry{TCoordinate}" /> 
        /// after the specified minimum <see cref="LinearLocation{TCoordinate}" />.
        /// </summary>
        /// <param name="inputPt">The coordinate to locate.</param>
        /// <param name="minIndex">The minimum location for the point location.</param>
        /// <returns>The location of the nearest point.</returns>
        /// <remarks>
        /// If possible the location returned will be strictly greater 
        /// than the <paramref name="minIndex" />. If this is not possible, 
        /// the value returned will equal <paramref name="minIndex" />.
        /// (An example where this is not possible is when <paramref name="minIndex" /> = [end of line] ).
        /// </remarks>
        public LinearLocation<TCoordinate> IndexOfAfter(TCoordinate inputPt, LinearLocation<TCoordinate>? minIndex)
        {
            if (minIndex == null)
            {
                return IndexOf(inputPt);
            }

            LinearLocation<TCoordinate> minIndexValue = minIndex.Value;

            // sanity check for minLocation at or past end of line
            LinearLocation<TCoordinate> endLoc = LinearLocation<TCoordinate>.GetEndLocation(_linearGeom);

            if (endLoc.CompareTo(minIndexValue) <= 0)
            {
                return endLoc;
            }

            LinearLocation<TCoordinate> closestAfter = indexOfFromStart(inputPt, minIndex);

            /*
             * Return the minDistanceLocation found.
             * This will not be null, since it was initialized to minLocation
             */
            Assert.IsTrue(closestAfter.CompareTo(minIndexValue) >= 0,
                          "computed location is before specified minimum location");

            return closestAfter;
        }

        private LinearLocation<TCoordinate> indexOfFromStart(TCoordinate inputPt, LinearLocation<TCoordinate>? minIndex)
        {
            Double minDistance = Double.MaxValue;
            Int32 minComponentIndex = 0;
            Int32 minSegmentIndex = 0;
            Double minFrac = -1.0;

            LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>();

            foreach (LinearIterator<TCoordinate>.LinearElement element in new LinearIterator<TCoordinate>(_linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    seg.P0 = element.SegmentStart;
                    seg.P1 = element.SegmentEnd;
                    Double segDistance = seg.Distance(inputPt);
                    Double segFrac = segmentFraction(seg, inputPt);

                    Int32 candidateComponentIndex = element.ComponentIndex;
                    Int32 candidateSegmentIndex = element.VertexIndex;
                    if (segDistance < minDistance)
                    {
                        // ensure after minLocation, if any                        
                        if (minIndex == null ||
                            minIndex.Value.CompareLocationValues(candidateComponentIndex, candidateSegmentIndex, segFrac) < 0)
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

            LinearLocation<TCoordinate> loc = new LinearLocation<TCoordinate>(minComponentIndex, minSegmentIndex, minFrac);
            return loc;
        }

        public static Double segmentFraction(LineSegment<TCoordinate> seg, TCoordinate inputPt)
        {
            Double segFrac = seg.ProjectionFactor(inputPt);

            if (segFrac < 0.0)
            {
                segFrac = 0.0;
            }
            else if (segFrac > 1.0)
            {
                segFrac = 1.0;
            }

            return segFrac;
        }
    }
}