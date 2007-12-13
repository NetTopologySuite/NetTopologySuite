using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Supports linear referencing along a linear <see cref="Geometry{TCoordinate}" />
    /// using the length along the line as the index.
    /// </summary>
    /// <remarks>
    /// Negative length values are taken as measured in the reverse direction
    /// from the end of the geometry.
    /// Out-of-range index values are handled by clamping
    /// them to the valid range of values.
    /// Non-simple lines (i.e. which loop back to cross or touch
    /// themselves) are supported.
    /// </remarks>
    public class LengthIndexedLine<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _linearGeom = null;

        /// <summary>
        /// Constructs an object which allows a linear <see cref="Geometry{TCoordinate}" />
        /// to be linearly referenced using length as an index.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to reference along.</param>
        public LengthIndexedLine(IGeometry<TCoordinate> linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Computes the <typeparamref name="TCoordinate"/> for the point
        /// on the line at the given index.
        /// If the index is out of range the first or last point on the
        /// line will be returned.
        /// </summary>
        /// <param name="index">The index of the desired point.</param>
        /// <returns>The <typeparamref name="TCoordinate" /> at the given index.</returns>
        public TCoordinate ExtractPoint(Double index)
        {
            LinearLocation<TCoordinate> loc = LengthLocationMap<TCoordinate>.GetLocation(_linearGeom, index);
            return loc.GetCoordinate(_linearGeom);
        }

        /// <summary>
        /// Computes the <see cref="ILineString{TCoordinate}" /> for the interval
        /// on the line between the given indices.
        /// If the <paramref name="endIndex" /> lies before the <paramref name="startIndex" />,
        /// the computed geometry is reversed.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public IGeometry ExtractLine(Double startIndex, Double endIndex)
        {
            LocationIndexedLine lil = new LocationIndexedLine(_linearGeom);
            LinearLocation<TCoordinate> startLoc = locationOf(startIndex);
            LinearLocation<TCoordinate> endLoc = locationOf(endIndex);
            return ExtractLineByLocation<TCoordinate>.Extract(_linearGeom, startLoc, endLoc);
        }

        /// <summary>
        /// Computes the minimum index for a point on the line.
        /// If the line is not simple (i.e. loops back on itself)
        /// a single point may have more than one possible index.
        /// In this case, the smallest index is returned.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project" /> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The minimum index of the point.</returns>
        public Double IndexOf(TCoordinate pt)
        {
            return LengthIndexOfPoint<TCoordinate>.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Finds the index for a point on the line
        /// which is greater than the given index.
        /// If no such index exists, returns <paramref name="minIndex" />.
        /// This method can be used to determine all indexes for
        /// a point which occurs more than once on a non-simple line.
        /// It can also be used to disambiguate cases where the given point lies
        /// slightly off the line and is equidistant from two different
        /// points on the line.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project" /> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <param name="minIndex">The value the returned index must be greater than.</param>
        /// <returns>The index of the point greater than the given minimum index.</returns>
        public Double IndexOfAfter(TCoordinate pt, Double minIndex)
        {
            return LengthIndexOfPoint<TCoordinate>.IndexOfAfter(_linearGeom, pt, minIndex);
        }

        /// <summary>
        /// Computes the indices for a subline of the line.
        /// (The subline must conform to the line; that is,
        /// all vertices in the subline (except possibly the first and last)
        /// must be vertices of the line and occcur in the same order).
        /// </summary>
        /// <param name="subLine">A subLine of the line.</param>
        /// <returns>A pair of indices for the start and end of the subline..</returns>
        public Double[] IndicesOf(IGeometry<TCoordinate> subLine)
        {
            Pair<LinearLocation<TCoordinate>> locIndex
                = LocationIndexOfLine<TCoordinate>.IndicesOf(_linearGeom, subLine);

            Double[] index = new Double[]
                {
                    LengthLocationMap<TCoordinate>.GetLength(_linearGeom, locIndex.First),
                    LengthLocationMap<TCoordinate>.GetLength(_linearGeom, locIndex.Second),
                };

            return index;
        }

        /// <summary>
        /// Computes the index for the closest point on the line to the given point.
        /// If more than one point has the closest distance the first one along the line is returned.
        /// (The point does not necessarily have to lie precisely on the line.)
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Double Project(TCoordinate pt)
        {
            return LengthIndexOfPoint<TCoordinate>.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Returns the index of the start of the line.
        /// </summary>
        public Double StartIndex
        {
            get { return 0; }
        }

        /// <summary>
        /// Returns the index of the end of the line.
        /// </summary>
        public Double EndIndex
        {
            get { return _linearGeom.Length; }
        }

        /// <summary>
        /// Tests whether an index is in the valid index range for the line.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><see langword="true"/> if the index is in the valid range.</returns>
        public Boolean IsValidIndex(Double index)
        {
            return (index >= StartIndex && index <= EndIndex);
        }

        /// <summary>
        /// Computes a valid index for this line
        /// by clamping the given index to the valid range of index values
        /// </summary>
        /// <param name="index"></param>
        /// <returns>A valid index value.</returns>
        public Double ClampIndex(Double index)
        {
            if (index < StartIndex)
            {
                return StartIndex;
            }

            if (index > EndIndex)
            {
                return EndIndex;
            }

            return index;
        }

        private LinearLocation<TCoordinate> locationOf(Double index)
        {
            return LengthLocationMap<TCoordinate>.GetLocation(_linearGeom, index);
        }
    }
}