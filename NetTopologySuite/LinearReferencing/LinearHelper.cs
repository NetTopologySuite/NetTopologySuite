using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    internal static class LinearHelper
    {
        internal static ILineString<TCoordinate> GetLine<TCoordinate>(IGeometry<TCoordinate> linear, 
                                                                      Int32 lineIndex)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
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
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IMultiLineString multiLine = linear as IMultiLineString;

            return multiLine != null
                       ? multiLine.Count
                       : 1;
        }

        internal static Double GetLength<TCoordinate>(IGeometry<TCoordinate> linear)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Double length = Double.NaN;

            IMultiLineString multiLine = linear as IMultiLineString;

            if (multiLine != null)
            {
                length = multiLine.Length;
            }

            ILineString line = linear as ILineString;

            if (line != null)
            {
                length = line.Length;   
            }

            if (Double.IsNaN(length))
            {
                throw new ArgumentException("Parameter is not a linear geometry.");
            }

            return length;
        }
    }
}
