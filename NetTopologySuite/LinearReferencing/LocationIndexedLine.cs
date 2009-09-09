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
    /// using <see cref="LinearLocation{TCoordinate}" />s as the index.
    /// </summary>
    public class LocationIndexedLine<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _linearGeom;

        /// <summary>
        /// Constructs an object which allows linear referencing along
        /// a given linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        public LocationIndexedLine(IGeometry<TCoordinate> linearGeom)
        {
            if (!(_linearGeom is ILineString || _linearGeom is IMultiLineString))
            {
                throw new ArgumentException("Input geometry must be linear", "linearGeom");
            }

            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Returns the index of the start of the line.
        /// </summary>
        public LinearLocation<TCoordinate> StartIndex
        {
            get { return new LinearLocation<TCoordinate>(); }
        }

        /// <summary>
        /// Returns the index of the end of the line.
        /// </summary>
        public LinearLocation<TCoordinate> EndIndex
        {
            get { return LinearLocation<TCoordinate>.GetEndLocation(_linearGeom); }
        }

        /// <summary>
        /// Computes the <typeparamref name="TCoordinate"/> for the point on the line at the given index.
        /// If the <paramref name="index" /> is out of range,
        /// the first or last point on the line will be returned.
        /// </summary>
        /// <param name="index">The index of the desired point.</param>
        /// <returns>The <typeparamref name="TCoordinate"/> at the given index.</returns>
        public TCoordinate ExtractPoint(LinearLocation<TCoordinate> index)
        {
            return index.GetCoordinate(_linearGeom);
        }

        /**
         * Computes the {@link Coordinate} for the point
         * on the line at the given index, offset by the given distance.
         * If the index is out of range the first or last point on the
         * line will be returned.
         * The computed point is offset to the left of the line if the offset distance is
         * positive, to the right if negative.
         * 
         * The Z-ordinate of the computed point will be interpolated from
         * the Z-ordinates of the line segment containing it, if they exist.
         *
         * @param index the index of the desired point
         * @param offsetDistance the distance the point is offset from the segment
         *    (positive is to the left, negative is to the right)
         * @return the Coordinate at the given index
         */
        ///<summary>
        ///</summary>
        ///<param name="index"></param>
        ///<param name="offsetDistance"></param>
        ///<returns></returns>
        public TCoordinate ExtractPoint(LinearLocation<TCoordinate> index, Double offsetDistance)
        {
            return index.GetSegment(_linearGeom).PointAlongOffset(_linearGeom.Coordinates.CoordinateFactory, index.SegmentFraction, offsetDistance);
        }

        /// <summary>
        /// Computes the <see cref="ILineString{TCoordinate}" /> for the interval
        /// on the line between the given indices.
        /// </summary>
        /// <param name="startIndex">The index of the start of the interval.</param>
        /// <param name="endIndex">The index of the end of the interval.</param>
        /// <returns>The linear interval between the indices.</returns>
        public IGeometry<TCoordinate> ExtractLine(LinearLocation<TCoordinate> startIndex,
                                                  LinearLocation<TCoordinate> endIndex)
        {
            return ExtractLineByLocation<TCoordinate>.Extract(_linearGeom, startIndex, endIndex);
        }

        /// <summary>
        /// Computes the index for a given point on the line.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project" /> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The index of the point.</returns>
        public LinearLocation<TCoordinate> IndexOf(TCoordinate pt)
        {
            return LocationIndexOfPoint<TCoordinate>.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Computes the indices for a subline of the line.
        /// (The subline must conform to the line; that is,
        /// all vertices in the subline (except possibly the first and last)
        /// must be vertices of the line and occcur in the same order).
        /// </summary>
        /// <param name="subLine">A subLine of the line.</param>
        /// <returns>A pair of indices for the start and end of the subline.</returns>
        public Pair<LinearLocation<TCoordinate>> IndicesOf(IGeometry<TCoordinate> subLine)
        {
            return LocationIndexOfLine<TCoordinate>.IndicesOf(_linearGeom, subLine);
        }

        /// <summary>
        /// Computes the index for the closest point on the line to the given point.
        /// If more than one point has the closest distance the first one along the line is returned.
        /// (The point does not necessarily have to lie precisely on the line.)
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The index of the point.</returns>
        public LinearLocation<TCoordinate> Project(TCoordinate pt)
        {
            return LocationIndexOfPoint<TCoordinate>.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Tests whether an index is in the valid index range for the line.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><see langword="true"/> if the index is in the valid range.</returns>
        public Boolean IsValidIndex(LinearLocation<TCoordinate> index)
        {
            return index.IsValid(_linearGeom);
        }

        /// <summary>
        /// Computes a valid index for this line by clamping 
        /// the given index to the valid range of index values.
        /// </summary>
        /// <param name="index">The index value to clamp to a valid value.</param>
        /// <returns>A valid index value.</returns>
        public LinearLocation<TCoordinate> ClampIndex(LinearLocation<TCoordinate> index)
        {
            return index.Clamp(_linearGeom);
        }
    }
}