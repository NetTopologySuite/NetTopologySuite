using NetTopologySuite.Geometries;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Supports linear referencing along a linear <see cref="Geometry" />
    /// using the length along the line as the index.
    /// Negative length values are taken as measured in the reverse direction
    /// from the end of the geometry.
    /// Out-of-range index values are handled by clamping
    /// them to the valid range of values.
    /// Non-simple lines (i.e. which loop back to cross or touch
    /// themselves) are supported.
    /// </summary>
    public class LengthIndexedLine
    {
        private readonly Geometry _linearGeom;

        /// <summary>
        /// Constructs an object which allows a linear <see cref="Geometry" />
        /// to be linearly referenced using length as an index.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to reference along.</param>
        public LengthIndexedLine(Geometry linearGeom)
        {
            _linearGeom = linearGeom;
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" /> for the point
        /// on the line at the given index.
        /// If the index is out of range the first or last point on the
        /// line will be returned.
        /// </summary>
        /// <remarks>
        /// The Z-ordinate of the computed point will be interpolated from
        /// the Z-ordinates of the line segment containing it, if they exist.
        /// </remarks>
        /// <param name="index">The index of the desired point.</param>
        /// <returns>The <see cref="Coordinate" /> at the given index.</returns>
        public Coordinate ExtractPoint(double index)
        {
            var loc = LengthLocationMap.GetLocation(_linearGeom, index);
            return loc.GetCoordinate(_linearGeom);
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
            var locLow = loc.ToLowest(_linearGeom);
            return locLow.GetSegment(_linearGeom).PointAlongOffset(locLow.SegmentFraction, offsetDistance);
        }

        /// <summary>
        /// Computes the <see cref="LineString" /> for the interval
        /// on the line between the given indices.
        /// If the <paramref name="endIndex" /> lies before the <paramref name="startIndex" />,
        /// the computed geometry is reversed.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public Geometry ExtractLine(double startIndex, double endIndex)
        {
            double startIndex2 = ClampIndex(startIndex);
            double endIndex2 = ClampIndex(endIndex);
            // if extracted line is zero-length, resolve start lower as well to ensure they are equal
            bool resolveStartLower = startIndex2 == endIndex2;
            var startLoc = LocationOf(startIndex2, resolveStartLower);
            //    LinearLocation endLoc = locationOf(endIndex2, true);
            //    LinearLocation startLoc = locationOf(startIndex2);
            var endLoc = LocationOf(endIndex2);
            return ExtractLineByLocation.Extract(_linearGeom, startLoc, endLoc);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private LinearLocation LocationOf(double index)
        {
            return LengthLocationMap.GetLocation(_linearGeom, index);
        }

        private LinearLocation LocationOf(double index, bool resolveLower)
        {
            return LengthLocationMap.GetLocation(_linearGeom, index, resolveLower);
        }

        /// <summary>
        /// Computes the minimum index for a point on the line.
        /// If the line is not simple (i.e. loops back on itself)
        /// a single point may have more than one possible index.
        /// In this case, the smallest index is returned.
        /// The supplied point does not necessarily have to lie precisely
        /// on the line, but if it is far from the line the accuracy and
        /// performance of this function is not guaranteed.
        /// Use <see cref="Project(Coordinate)"/> to compute a guaranteed result for points
        /// which may be far from the line.
        /// </summary>
        /// <param name="pt">A point on the line.</param>
        /// <returns>The minimum index of the point.</returns>
        /// <seealso cref="Project(Coordinate)"/>
        public double IndexOf(Coordinate pt)
        {
            return LengthIndexOfPoint.IndexOf(_linearGeom, pt);
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
        /// <seealso cref="Project(Coordinate)"/>
        public double IndexOfAfter(Coordinate pt, double minIndex)
        {
            return LengthIndexOfPoint.IndexOfAfter(_linearGeom, pt, minIndex);
        }

        /// <summary>
        /// Computes the indices for a subline of the line.
        /// (The subline must conform to the line; that is,
        /// all vertices in the subline (except possibly the first and last)
        /// must be vertices of the line and occur in the same order).
        /// </summary>
        /// <param name="subLine">A subLine of the line.</param>
        /// <returns>A pair of indices for the start and end of the subline..</returns>
        public double[] IndicesOf(Geometry subLine)
        {
            var locIndex = LocationIndexOfLine.IndicesOf(_linearGeom, subLine);
            double[] index =
            {
                LengthLocationMap.GetLength(_linearGeom, locIndex[0]),
                LengthLocationMap.GetLength(_linearGeom, locIndex[1])
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
        public double Project(Coordinate pt)
        {
            return LengthIndexOfPoint.IndexOf(_linearGeom, pt);
        }

        /// <summary>
        /// Returns the index of the start of the line.
        /// </summary>
        public double StartIndex => 0;

        /// <summary>
        /// Returns the index of the end of the line.
        /// </summary>
        public double EndIndex => _linearGeom.Length;

        /// <summary>
        /// Tests whether an index is in the valid index range for the line.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns><c>true</c> if the index is in the valid range.</returns>
        public bool IsValidIndex(double index)
        {
            return (index >= StartIndex && index <= EndIndex);
        }

        /// <summary>
        /// Computes a valid index for this line
        /// by clamping the given index to the valid range of index values
        /// </summary>
        /// <returns>A valid index value</returns>
        public double ClampIndex(double index)
        {
            double posIndex = PositiveIndex(index);
            double startIndex = StartIndex;
            if (posIndex < startIndex) return startIndex;

            double endIndex = EndIndex;
            if (posIndex > endIndex) return endIndex;

            return posIndex;
        }

        private double PositiveIndex(double index)
        {
            if (index >= 0.0) return index;
            return _linearGeom.Length + index;
        }
    }
}
