using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Extracts the subline of a linear <see cref="Geometry" /> between
    /// two <see cref="LinearLocation" />s on the line.
    /// </summary>
    public class ExtractLineByLocation
    {
        /// <summary>
        /// Computes the subline of a <see cref="LineString" /> between
        /// two LineStringLocations on the line.
        /// If the start location is after the end location,
        /// the computed geometry is reversed.
        /// </summary>
        /// <param name="line">The line to use as the baseline.</param>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <returns>The extracted subline.</returns>
        public static IGeometry Extract(IGeometry line, LinearLocation start, LinearLocation end)
        {
            ExtractLineByLocation ls = new ExtractLineByLocation(line);
            return ls.Extract(start, end);
        }

        private IGeometry line = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExtractLineByLocation"/> class.
        /// </summary>
        /// <param name="line"></param>
        public ExtractLineByLocation(IGeometry line)
        {
            this.line = line;
        }

        /// <summary>
        /// Extracts a subline of the input.
        /// If <paramref name="end" /> is minor that <paramref name="start" />,
        /// the linear geometry computed will be reversed.
        /// </summary>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <returns>A linear geometry.</returns>
        public IGeometry Extract(LinearLocation start, LinearLocation end)
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
        private IGeometry Reverse(IGeometry linear)
        {
            if (linear is ILineString)
            return ((ILineString) linear).Reverse();
            if (linear is IMultiLineString)
            return ((IMultiLineString) linear).Reverse();
            Assert.ShouldNeverReachHere("non-linear geometry encountered");
            return null;
        }

        /// <summary>
        /// Assumes input is valid 
        /// (e.g. <paramref name="start" /> minor or equals to <paramref name="end" />).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private ILineString ComputeLine(LinearLocation start, LinearLocation end)
        {
            ICoordinate[] coordinates = line.Coordinates;
            CoordinateList newCoordinates = new CoordinateList();

            int startSegmentIndex = start.SegmentIndex;
            if (start.SegmentFraction > 0.0)
                startSegmentIndex += 1;
            int lastSegmentIndex = end.SegmentIndex;
            if (end.SegmentFraction == 1.0)
                lastSegmentIndex += 1;
            if (lastSegmentIndex >= coordinates.Length)
                lastSegmentIndex = coordinates.Length - 1;
            // not needed - LinearLocation values should always be correct
            // Assert.IsTrue(end.SegmentFraction <= 1.0, "invalid segment fraction value");

            if (!start.IsVertex)
                newCoordinates.Add(start.GetCoordinate(line));
            for (int i = startSegmentIndex; i <= lastSegmentIndex; i++)
                newCoordinates.Add(coordinates[i]);            
            if (!end.IsVertex)
                newCoordinates.Add(end.GetCoordinate(line));

            // ensure there is at least one coordinate in the result
            if (newCoordinates.Count <= 0)
                newCoordinates.Add(start.GetCoordinate(line));

            ICoordinate[] newCoordinateArray = newCoordinates.ToCoordinateArray();

            /*
             * Ensure there is enough coordinates to build a valid line.
             * Make a 2-point line with duplicate coordinates, if necessary.
             * There will always be at least one coordinate in the coordList.
             */
            if (newCoordinateArray.Length <= 1)
                newCoordinateArray = new ICoordinate[] { newCoordinateArray[0], newCoordinateArray[0] };
            
            return line.Factory.CreateLineString(newCoordinateArray);
        }

        /// <summary>
        /// Assumes input is valid 
        /// (e.g. <paramref name="start" /> minor or equals to <paramref name="end" />).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private IGeometry ComputeLinear(LinearLocation start, LinearLocation end)
        {
            LinearGeometryBuilder builder = new LinearGeometryBuilder(line.Factory);
            builder.FixInvalidLines = true;

            if (!start.IsVertex)
                builder.Add(start.GetCoordinate(line));

            LinearIterator it = new LinearIterator(line, start);
            foreach (LinearIterator.LinearElement element in it)
            {
                int compare = end.CompareLocationValues(element.ComponentIndex, element.VertexIndex, 0.0);
                if (compare < 0)
                    break;

                ICoordinate pt = element.SegmentStart;
                builder.Add(pt);
                if (element.IsEndOfLine)
                    builder.EndLine();
            }            
            
            if (!end.IsVertex)
                builder.Add(end.GetCoordinate(line));

            return builder.GetGeometry();
        }
    }
}
