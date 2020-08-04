using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Extracts the subline of a linear <see cref="Geometry" /> between
    /// two <see cref="LinearLocation" />s on the line.
    /// </summary>
    public class ExtractLineByLocation
    {
        /// <summary>
        /// Computes the subline of a <see cref="LineString" /> between
        /// two <see cref="LinearLocation"/>s on the line.
        /// If the start location is after the end location,
        /// the computed linear geometry has reverse orientation to the input line.
        /// </summary>
        /// <param name="line">The line to use as the baseline.</param>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <returns>The extracted subline.</returns>
        public static Geometry Extract(Geometry line, LinearLocation start, LinearLocation end)
        {
            var ls = new ExtractLineByLocation(line);
            return ls.Extract(start, end);
        }

        private readonly Geometry _line;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractLineByLocation"/> class.
        /// </summary>
        /// <param name="line"></param>
        public ExtractLineByLocation(Geometry line)
        {
            _line = line;
        }

        /// <summary>
        /// Extracts a subline of the input.
        /// If <paramref name="end" /> is minor that <paramref name="start" />,
        /// the linear geometry computed will be reversed.
        /// </summary>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <returns>A linear geometry.</returns>
        public Geometry Extract(LinearLocation start, LinearLocation end)
        {
            if (end.CompareTo(start) < 0)
                return Reverse(ComputeLinear(end, start));
            return ComputeLinear(start, end);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="linear"></param>
        /// <returns></returns>
        private static Geometry Reverse(Geometry linear)
        {
            if (linear is ILineal)
                return linear.Reverse();

            Assert.ShouldNeverReachHere("non-linear geometry encountered");
            return null;
        }

        ///// <summary>
        ///// Assumes input is valid
        ///// (e.g. <paramref name="start" /> minor or equals to <paramref name="end" />).
        ///// </summary>
        ///// <param name="start"></param>
        ///// <param name="end"></param>
        ///// <returns></returns>
        //private LineString ComputeLine(LinearLocation start, LinearLocation end)
        //{
        //    var coordinates = _line.Coordinates;
        //    var newCoordinates = new CoordinateList();

        //    var startSegmentIndex = start.SegmentIndex;
        //    if (start.SegmentFraction > 0.0)
        //        startSegmentIndex += 1;
        //    var lastSegmentIndex = end.SegmentIndex;
        //    if (end.SegmentFraction == 1.0)
        //        lastSegmentIndex += 1;
        //    if (lastSegmentIndex >= coordinates.Length)
        //        lastSegmentIndex = coordinates.Length - 1;
        //    // not needed - LinearLocation values should always be correct
        //    // Assert.IsTrue(end.SegmentFraction <= 1.0, "invalid segment fraction value");

        //    if (!start.IsVertex)
        //        newCoordinates.Add(start.GetCoordinate(_line));
        //    for (var i = startSegmentIndex; i <= lastSegmentIndex; i++)
        //        newCoordinates.Add(coordinates[i]);
        //    if (!end.IsVertex)
        //        newCoordinates.Add(end.GetCoordinate(_line));

        //    // ensure there is at least one coordinate in the result
        //    if (newCoordinates.Count <= 0)
        //        newCoordinates.Add(start.GetCoordinate(_line));

        //    var newCoordinateArray = newCoordinates.ToCoordinateArray();

        //    /*
        //     * Ensure there is enough coordinates to build a valid line.
        //     * Make a 2-point line with duplicate coordinates, if necessary.
        //     * There will always be at least one coordinate in the coordList.
        //     */
        //    if (newCoordinateArray.Length <= 1)
        //        newCoordinateArray = new[] { newCoordinateArray[0], newCoordinateArray[0] };

        //    return _line.Factory.CreateLineString(newCoordinateArray);
        //}

        /// <summary>
        /// Assumes input is valid
        /// (e.g. <paramref name="start" /> minor or equals to <paramref name="end" />).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private Geometry ComputeLinear(LinearLocation start, LinearLocation end)
        {
            var builder = new LinearGeometryBuilder(_line.Factory);
            builder.FixInvalidLines = true;

            if (!start.IsVertex)
                builder.Add(start.GetCoordinate(_line));

            for (var it = new LinearIterator(_line, start); it.HasNext(); it.Next())
            {
                if (end.CompareLocationValues(it.ComponentIndex, it.VertexIndex, 0.0) < 0)
                    break;

                var pt = it.SegmentStart;
                builder.Add(pt);
                if (it.IsEndOfLine)
                    builder.EndLine();
            }

            if (!end.IsVertex)
                builder.Add(end.GetCoordinate(_line));

            return builder.GetGeometry();
        }
    }
}
