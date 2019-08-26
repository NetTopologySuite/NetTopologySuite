using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Supports linear referencing along a linear <see cref="Geometry" />
    /// using <see cref="LinearLocation" />s as the index.
    /// </summary>
    public class LocationIndexedLine
    {
        private readonly Geometry _linearGeom;

        /// <summary>
        /// Constructs an object which allows linear referencing along
        /// a given linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to reference alo</param>
        public LocationIndexedLine(Geometry linearGeom)
        {
            if (!CheckGeometryType(linearGeom))
                throw new ArgumentException("Input geometry must be linear", "linearGeom");
            _linearGeom = linearGeom;
        }

        private static bool CheckGeometryType(Geometry linearGeometry)
        {
            return ((linearGeometry is LineString || linearGeometry is MultiLineString));
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" />for the point on the line at the given index.
        /// If the <paramref name="index" /> is out of range,
        /// the first or last point on the line will be returned.
        /// </summary>
        /// <remarks>
        /// The Z-ordinate of the computed point will be interpolated from
        /// the Z-ordinates of the line segment containing it, if they exist.
        /// </remarks>
        /// <param name="index">The index of the desired point.</param>
        /// <returns>The <see cref="Coordinate" /> at the given index.</returns>
        public Coordinate ExtractPoint(LinearLocation index)
        {
            return index.GetCoordinate(_linearGeom);
        }

        /// <summary>
        /// Computes the <see cref="Coordinate"/> for the point
        /// on the line at the given index, offset by the given distance.
        /// If the index is out of range the first or last point on the
        /// line will be returned.<para/>
        /// The computed point is offset to the left of the line if the offset distance is
        /// positive, to the right if negative.<para/>
        /// The Z-ordinate of the computed point will be interpolated from
        /// the Z-ordinates of the line segment containing it, if they exist.
        /// </summary>
        /// <param name="index">The index of the desired point</param>
        /// <param name="offsetDistance">The distance the point is offset from the segment
        /// (positive is to the left, negative is to the right)</param>
        /// <returns>The Coordinate at the given index</returns>
        public Coordinate ExtractPoint(LinearLocation index, double offsetDistance)
        {
            var indexLow = index.ToLowest(_linearGeom);
            return indexLow.GetSegment(_linearGeom).PointAlongOffset(indexLow.SegmentFraction, offsetDistance);
        }

        /// <summary>
        /// Computes the <see cref="Coordinate"/> for the point on the line at the given index, offset by the given distance.
        /// </summary>
        /// <remarks>
        /// If the index is out of range the first or last point on the line will be returned.
        /// The computed point is offset to the left of the line if the offset distance is
        /// positive, to the right if negative.
        /// The Z-ordinate of the computed point will be interpolated from the Z-ordinates of the line segment containing it, if they exist.
        /// </remarks>
        /// <param name="index">The index of the desired point</param>
        /// <param name="offsetDistance">The distance the point is offset from the segment (positive is to the left, negative is to the right)</param>
        /// <returns>The Coordinate at the given index</returns>
        public Coordinate ExtractPoint(double index, double offsetDistance)
        {
            var loc = LengthLocationMap.GetLocation(_linearGeom, index);
            return loc.GetSegment(_linearGeom).PointAlongOffset(loc.SegmentFraction, offsetDistance);
        }
        /// <summary>
        /// Computes the <see cref="LineString" /> for the interval
        /// on the line between the given indices.
        /// If the start location is after the end location,
        /// the computed linear geometry has reverse orientation to the input line.
        /// </summary>
        /// <param name="startIndex">The index of the start of the interval.</param>
        /// <param name="endIndex">The index of the end of the interval.</param>
        /// <returns>The linear interval between the indices.</returns>
        public Geometry ExtractLine(LinearLocation startIndex, LinearLocation endIndex)
        {
            return ExtractLineByLocation.Extract(_linearGeom, startIndex, endIndex);
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
        /// <seealso cref="Project(Coordinate)"/>
        public LinearLocation IndexOf(Coordinate pt)
        {
            return LocationIndexOfPoint.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Computes the indices for a subline of the line.
        /// (The subline must conform to the line; that is,
        /// all vertices in the subline (except possibly the first and last)
        /// must be vertices of the line and occur in the same order).
        /// </summary>
        /// <param name="subLine">A subLine of the line.</param>
        /// <returns>A pair of indices for the start and end of the subline.</returns>
        public LinearLocation[] IndicesOf(Geometry subLine)
        {
            return LocationIndexOfLine.IndicesOf(_linearGeom, subLine);
        }

        /// <summary>
        /// Finds the index for a point on the line which is greater than the given index.
        /// If no such index exists, returns <paramref name="minIndex" />.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be used to determine all indexes for
        /// a point which occurs more than once on a non-simple line.
        /// It can also be used to disambiguate cases where the given point lies
        /// slightly off the line and is equidistant from two different
        /// points on the line.
        /// </para>
        /// <para>
        /// The supplied point does not <i>necessarily</i> have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project"/> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </para>
        /// </remarks>
        /// <param name="pt">A point on the line</param>
        /// <param name="minIndex">The value the returned index must be greater than</param>
        /// <returns>The index of the point greater than the given minimum index</returns>
        /// <seealso cref="Project(Coordinate)"/>
        public LinearLocation IndexOfAfter(Coordinate pt, LinearLocation minIndex)
        {
            return LocationIndexOfPoint.IndexOfAfter(_linearGeom, pt, minIndex);
        }

        /// <summary>
        /// Computes the index for the closest point on the line to the given point.
        /// If more than one point has the closest distance the first one along the line is returned.
        /// (The point does not necessarily have to lie precisely on the line.)
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The index of the point.</returns>
        public LinearLocation Project(Coordinate pt)
        {
            return LocationIndexOfPoint.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Returns the index of the start of the line.
        /// </summary>
        public LinearLocation StartIndex => new LinearLocation();

        /// <summary>
        /// Returns the index of the end of the line.
        /// </summary>
        public LinearLocation EndIndex => LinearLocation.GetEndLocation(_linearGeom);

        /// <summary>
        /// Tests whether an index is in the valid index range for the line.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><c>true</c> if the index is in the valid range.</returns>
        public bool IsValidIndex(LinearLocation index)
        {
            return index.IsValid(_linearGeom);
        }

        /// <summary>
        /// Computes a valid index for this line by clamping
        /// the given index to the valid range of index values.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>A valid index value.</returns>
        public LinearLocation ClampIndex(LinearLocation index)
        {
            var loc = (LinearLocation)index.Copy();
            loc.Clamp(_linearGeom);
            return loc;
        }
    }
}
