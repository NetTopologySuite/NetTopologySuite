using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation" /> for a given length
    /// along a linear <see cref="Geometry" />
    /// Negative lengths are measured in reverse from end of the linear geometry.
    /// Out-of-range values are clamped.
    /// </summary>
    public class LengthLocationMap
    {
        /// <summary>
        /// Computes the <see cref="LinearLocation" /> for a
        /// given length along a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="length">The length index of the location.</param>
        /// <returns>The <see cref="LinearLocation" /> for the length.</returns>
        public static LinearLocation GetLocation(IGeometry linearGeom, Double length)
        {
            LengthLocationMap locater = new LengthLocationMap(linearGeom);
            return locater.GetLocation(length);
        }

        /// <summary>
        /// Computes the length for a given <see cref="LinearLocation" />
        /// on a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="loc">The <see cref="LinearLocation" /> index of the location.</param>
        /// <returns>The length for the <see cref="LinearLocation" />.</returns>
        public static Double GetLength(IGeometry linearGeom, LinearLocation loc)
        {
            LengthLocationMap locater = new LengthLocationMap(linearGeom);
            return locater.GetLength(loc);
        }

        private IGeometry linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthLocationMap"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LengthLocationMap(IGeometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }

        /// <summary>
        /// Compute the <see cref="LinearLocation" /> corresponding to a length.
        /// Negative lengths are measured in reverse from end of the linear geometry.
        /// Out-of-range values are clamped.
        /// </summary>
        /// <param name="length">The length index.</param>
        /// <returns>The corresponding <see cref="LinearLocation" />.</returns>
        public LinearLocation GetLocation(Double length)
        {
            Double forwardLength = length;
            if (length < 0.0)
            {
                Double lineLen = linearGeom.Length;
                forwardLength = lineLen + length;
            }
            return GetLocationForward(forwardLength);
        }

        private LinearLocation GetLocationForward(Double length)
        {
            if (length <= 0.0)
            {
                return new LinearLocation();
            }

            Double totalLength = 0.0;

            foreach (LinearIterator.LinearElement element in new LinearIterator(linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    ICoordinate p0 = element.SegmentStart;
                    ICoordinate p1 = element.SegmentEnd;
                    Double segLen = p1.Distance(p0);
                    // length falls in this segment
                    if (totalLength + segLen > length)
                    {
                        Double frac = (length - totalLength)/segLen;
                        Int32 compIndex = element.ComponentIndex;
                        Int32 segIndex = element.VertexIndex;
                        return new LinearLocation(compIndex, segIndex, frac);
                    }
                    totalLength += segLen;
                }
            }
            // length is longer than line - return end location
            return LinearLocation.GetEndLocation(linearGeom);
        }

        public Double GetLength(LinearLocation loc)
        {
            Double totalLength = 0.0;

            foreach (LinearIterator.LinearElement element in new LinearIterator(linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    ICoordinate p0 = element.SegmentStart;
                    ICoordinate p1 = element.SegmentEnd;
                    Double segLen = p1.Distance(p0);
                    // length falls in this segment
                    if (loc.ComponentIndex == element.ComponentIndex && loc.SegmentIndex == element.VertexIndex)
                    {
                        return totalLength + segLen*loc.SegmentFraction;
                    }
                    totalLength += segLen;
                }
            }
            return totalLength;
        }
    }
}