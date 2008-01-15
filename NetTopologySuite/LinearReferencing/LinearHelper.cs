using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    internal static class LinearHelper
    {

        internal static ILineString<TCoordinate> GetLine<TCoordinate>(IGeometry<TCoordinate> linear, Int32 lineIndex)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            ILineString<TCoordinate> line = linear as ILineString<TCoordinate>;

            if (line == null)
            {
                IMultiLineString<TCoordinate> multiLine = linear as IMultiLineString<TCoordinate>;
                Debug.Assert(multiLine != null);
                line = multiLine[lineIndex];
            }

            Debug.Assert(line != null);
            return line;
        }

        internal static Int32 GetLineCount<TCoordinate>(IGeometry<TCoordinate> linear)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            if (linear is IMultiLineString)
            {
                IMultiLineString multiLine = linear as IMultiLineString;
                return multiLine.Count;
            }
            else
            {
                return 1;
            }
        }

        internal static double GetLength<TCoordinate>(IGeometry<TCoordinate> linear)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            if (linear is IMultiLineString)
            {
                IMultiLineString multiLine = linear as IMultiLineString;
                return multiLine.Length;
            }
            else
            {
                ILineString line = linear as ILineString;
                return line.Length;
            }
        }
    }
}
