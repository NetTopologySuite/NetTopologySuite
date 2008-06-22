using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Computes the <see cref="LinearLocation{TCoordinate}" /> for a given length
    /// along a linear <see cref="Geometry{TCoordinate}" />
    /// Negative lengths are measured in reverse from end of the linear geometry.
    /// Out-of-range values are clamped.
    /// </summary>
    public class LengthLocationMap<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Computes the <see cref="LinearLocation{TCoordinate}" /> for a
        /// given length along a linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="length">The length index of the location.</param>
        /// <returns>The <see cref="LinearLocation{TCoordinate}" /> for the length.</returns>
        public static LinearLocation<TCoordinate> GetLocation(IGeometry<TCoordinate> linearGeom, Double length)
        {
            LengthLocationMap<TCoordinate> locater = new LengthLocationMap<TCoordinate>(linearGeom);
            return locater.GetLocation(length);
        }

        /// <summary>
        /// Computes the length for a given <see cref="LinearLocation{TCoordinate}" />
        /// on a linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to use.</param>
        /// <param name="loc">The <see cref="LinearLocation{TCoordinate}" /> index of the location.</param>
        /// <returns>The length for the <see cref="LinearLocation{TCoordinate}" />.</returns>
        public static Double GetLength(IGeometry<TCoordinate> linearGeom, LinearLocation<TCoordinate> loc)
        {
            LengthLocationMap<TCoordinate> locater = new LengthLocationMap<TCoordinate>(linearGeom);
            return locater.GetLength(loc);
        }

        private readonly IGeometry<TCoordinate> _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthLocationMap{TCoordinate}"/> class.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        public LengthLocationMap(IGeometry<TCoordinate> linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Compute the <see cref="LinearLocation{TCoordinate}" /> corresponding to a length.
        /// Negative lengths are measured in reverse from end of the linear geometry.
        /// Out-of-range values are clamped.
        /// </summary>
        /// <param name="length">The length index.</param>
        /// <returns>The corresponding <see cref="LinearLocation{TCoordinate}" />.</returns>
        public LinearLocation<TCoordinate> GetLocation(Double length)
        {
            Double forwardLength = length;

            if (length < 0.0)
            {
                Double lineLen = LinearHelper.GetLength(_linearGeom);
                forwardLength = lineLen + length;
            }

            return GetLocationForward(forwardLength);
        }

        private LinearLocation<TCoordinate> GetLocationForward(Double length)
        {
            if (length <= 0.0)
            {
                return new LinearLocation<TCoordinate>();
            }

            Double totalLength = 0.0;

            foreach (LinearIterator<TCoordinate>.LinearElement element in new LinearIterator<TCoordinate>(_linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    TCoordinate p0 = element.SegmentStart;
                    TCoordinate p1 = element.SegmentEnd;

                    Double segLen = p1.Distance(p0);

                    // length falls in this segment
                    if (totalLength + segLen > length)
                    {
                        Double frac = (length - totalLength)/segLen;
                        Int32 compIndex = element.ComponentIndex;
                        Int32 segIndex = element.VertexIndex;
                        return new LinearLocation<TCoordinate>(compIndex, segIndex, frac);
                    }

                    totalLength += segLen;
                }
            }
            // length is longer than line - return end location
            return LinearLocation<TCoordinate>.GetEndLocation(_linearGeom);
        }

        public Double GetLength(LinearLocation<TCoordinate> loc)
        {
            Double totalLength = 0.0;

            foreach (LinearIterator<TCoordinate>.LinearElement element in new LinearIterator<TCoordinate>(_linearGeom))
            {
                if (!element.IsEndOfLine)
                {
                    TCoordinate p0 = element.SegmentStart;
                    TCoordinate p1 = element.SegmentEnd;

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