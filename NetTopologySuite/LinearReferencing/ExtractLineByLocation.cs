using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Extracts the subline of a linear <see cref="Geometry{TCoordinate}" /> between
    /// two <see cref="LinearLocation{TCoordinate}" />s on the line.
    /// </summary>
    public class ExtractLineByLocation<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Computes the subline of a <see cref="ILineString{TCoordinate}" /> between
        /// two LineStringLocations on the line.
        /// If the start location is after the end location,
        /// the computed geometry is reversed.
        /// </summary>
        /// <param name="line">The line to use as the baseline.</param>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <returns>The extracted subline.</returns>
        public static IGeometry<TCoordinate> Extract(IGeometry<TCoordinate> line, LinearLocation<TCoordinate> start, LinearLocation<TCoordinate> end)
        {
            ExtractLineByLocation<TCoordinate> ls = new ExtractLineByLocation<TCoordinate>(line);
            return ls.Extract(start, end);
        }

        private readonly IGeometry<TCoordinate> _line = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractLineByLocation{TCoordinate}"/> class.
        /// </summary>
        public ExtractLineByLocation(IGeometry<TCoordinate> line)
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
        public IGeometry<TCoordinate> Extract(LinearLocation<TCoordinate> start, LinearLocation<TCoordinate> end)
        {
            if (end.CompareTo(start) < 0)
            {
                return reverse(computeLinear(end, start));
            }

            return computeLinear(start, end);
        }

        private static IGeometry<TCoordinate> reverse(IGeometry<TCoordinate> linear)
        {
            if (linear is ILineString<TCoordinate>)
            {
                return (linear as ILineString<TCoordinate>).Reverse();
            }
            if (linear is IMultiLineString<TCoordinate>)
            {
                return (linear as IMultiLineString<TCoordinate>).Reverse();
            }

            Assert.ShouldNeverReachHere("non-linear geometry encountered");
            return null;
        }

        /// <summary>
        /// Assumes input is valid 
        /// (e.g. <paramref name="start" /> minor or equals to <paramref name="end" />).
        /// </summary>
        private IGeometry<TCoordinate> computeLinear(LinearLocation<TCoordinate> start, LinearLocation<TCoordinate> end)
        {
            LinearGeometryBuilder<TCoordinate> builder = new LinearGeometryBuilder<TCoordinate>(_line.Factory);
            builder.FixInvalidLines = true;

            if (!start.IsVertex)
            {
                builder.Add(start.GetCoordinate(_line));
            }

            LinearIterator<TCoordinate> it = new LinearIterator<TCoordinate>(_line, start);

            foreach (LinearIterator<TCoordinate>.LinearElement element in it)
            {
                Int32 compare = end.CompareLocationValues(element.ComponentIndex, element.VertexIndex, 0.0);

                if (compare < 0)
                {
                    break;
                }

                TCoordinate pt = element.SegmentStart;
                builder.Add(pt);
                
                if (element.IsEndOfLine)
                {
                    builder.EndLine();
                }
            }

            if (!end.IsVertex)
            {
                builder.Add(end.GetCoordinate(_line));
            }

            return builder.GetGeometry();
        }

        ///// <summary>
        ///// Assumes input is valid 
        ///// (e.g. <paramref name="start" /> minor or equals to <paramref name="end" />).
        ///// </summary>
        //private ILineString computeLine(LinearLocation<TCoordinate> start, LinearLocation<TCoordinate> end)
        //{
        //    ICoordinate[] coordinates = _line.Coordinates;
        //    CoordinateList newCoordinates = new CoordinateList();

        //    Int32 startSegmentIndex = start.SegmentIndex;
        //    if (start.SegmentFraction > 0.0)
        //    {
        //        startSegmentIndex += 1;
        //    }
        //    Int32 lastSegmentIndex = end.SegmentIndex;
        //    if (end.SegmentFraction == 1.0)
        //    {
        //        lastSegmentIndex += 1;
        //    }
        //    if (lastSegmentIndex >= coordinates.Length)
        //    {
        //        lastSegmentIndex = coordinates.Length - 1;
        //    }
        //    // not needed - LinearLocation values should always be correct
        //    // Assert.IsTrue(end.SegmentFraction <= 1.0, "invalid segment fraction value");

        //    if (!start.IsVertex)
        //    {
        //        newCoordinates.Add(start.GetCoordinate(_line));
        //    }
        //    for (Int32 i = startSegmentIndex; i <= lastSegmentIndex; i++)
        //    {
        //        newCoordinates.Add(coordinates[i]);
        //    }
        //    if (!end.IsVertex)
        //    {
        //        newCoordinates.Add(end.GetCoordinate(_line));
        //    }

        //    // ensure there is at least one coordinate in the result
        //    if (newCoordinates.Count <= 0)
        //    {
        //        newCoordinates.Add(start.GetCoordinate(_line));
        //    }

        //    ICoordinate[] newCoordinateArray = newCoordinates.ToCoordinateArray();

        //    /*
        //     * Ensure there is enough coordinates to build a valid line.
        //     * Make a 2-point line with duplicate coordinates, if necessary.
        //     * There will always be at least one coordinate in the coordList.
        //     */
        //    if (newCoordinateArray.Length <= 1)
        //    {
        //        newCoordinateArray = new ICoordinate[] {newCoordinateArray[0], newCoordinateArray[0]};
        //    }

        //    return _line.Factory.CreateLineString(newCoordinateArray);
        //}
    }
}