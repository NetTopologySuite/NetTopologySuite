using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Determines the location of a subline along a linear <see cref="Geometry{TCoordinate}" />.
    /// The location is reported as a pair of <see cref="LinearLocation{TCoordinate}" />s.
    /// NOTE: Currently this algorithm is not guaranteed to
    /// return the correct substring in some situations where
    /// an endpoint of the test line occurs more than once in the input line.
    /// (However, the common case of a ring is always handled correctly).
    /// </summary>
    public class LocationIndexOfLine<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        public static Pair<LinearLocation<TCoordinate>> IndicesOf(IGeometry<TCoordinate> linearGeom,
                                                                  IGeometry<TCoordinate> subLine)
        {
            /*
             * MD - this algorithm has been extracted into a class
             * because it is intended to validate that the subline truly is a subline,
             * and also to use the internal vertex information to unambiguously locate the subline.
             */
            LocationIndexOfLine<TCoordinate> locater = new LocationIndexOfLine<TCoordinate>(linearGeom);
            return locater.IndexesOf(subLine);
        }

        private readonly IGeometry<TCoordinate> _linearGeom;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationIndexOfLine{TCoordinate}"/> class.
        /// </summary>
        /// <param name="linearGeom">The linear geom.</param>
        public LocationIndexOfLine(IGeometry<TCoordinate> linearGeom)
        {
            if (linearGeom == null)
            {
                throw new ArgumentNullException("linearGeom");
            }

            if (linearGeom.IsEmpty)
            {
                throw new ArgumentException("Parameter is an empty geometry.", "linearGeom");
            }

            if (!(linearGeom is ILineString || linearGeom is IMultiLineString))
            {
                throw new ArgumentException("Parameter must be an instance of ILineString or IMultiLineString");
            }

            _linearGeom = linearGeom;
        }

        public virtual Pair<LinearLocation<TCoordinate>> IndexesOf(IGeometry<TCoordinate> subLine)
        {
            ILineString<TCoordinate> startLine = getStartLine(subLine);
            TCoordinate startPt = Slice.GetFirst(startLine.Coordinates);

            ILineString<TCoordinate> lastLine = getEndLine(subLine);
            TCoordinate endPt = Slice.GetLast(lastLine.Coordinates);

            LocationIndexOfPoint<TCoordinate> locPt = new LocationIndexOfPoint<TCoordinate>(_linearGeom);
            LinearLocation<TCoordinate> subLineLoc0, subLineLoc1;

            subLineLoc0 = locPt.IndexOf(startPt);

            // check for case where subline is zero length
            if (LinearHelper.GetLength(subLine) == 0)
            {
                subLineLoc1 = subLineLoc0;
            }
            else
            {
                subLineLoc1 = locPt.IndexOfAfter(endPt, subLineLoc0);
            }

            return new Pair<LinearLocation<TCoordinate>>(subLineLoc0, subLineLoc1);
        }

        private static ILineString<TCoordinate> getStartLine(IGeometry<TCoordinate> lineOrMultiLine)
        {
            if (lineOrMultiLine is ILineString<TCoordinate>)
            {
                return lineOrMultiLine as ILineString<TCoordinate>;
            }
            else
            {
                IMultiLineString<TCoordinate> mutliLine = lineOrMultiLine as IMultiLineString<TCoordinate>;
                Debug.Assert(mutliLine != null);
                return Slice.GetFirst(mutliLine as IEnumerable<ILineString<TCoordinate>>);
            }
        }

        private static ILineString<TCoordinate> getEndLine(IGeometry<TCoordinate> lineOrMultiLine)
        {
            if (lineOrMultiLine is ILineString<TCoordinate>)
            {
                return lineOrMultiLine as ILineString<TCoordinate>;
            }
            else
            {
                IMultiLineString<TCoordinate> mutliLine = lineOrMultiLine as IMultiLineString<TCoordinate>;
                Debug.Assert(mutliLine != null);
                return Slice.GetLast(mutliLine as IEnumerable<ILineString<TCoordinate>>);
            }
        }
    }
}