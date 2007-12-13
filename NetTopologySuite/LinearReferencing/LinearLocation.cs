using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;
using GeoAPI.Coordinates;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Represents a location along a <see cref="ILineString{TCoordinate}" /> 
    /// or <see cref="IMultiLineString{TCoordinate}" />.
    /// </summary>
    /// <remarks>
    /// The referenced geometry is not maintained within this location, 
    /// but must be provided for operations which require it.
    /// Various methods are provided to manipulate the location value
    /// and query the geometry it references.
    /// </remarks>
    public struct LinearLocation<TCoordinate> : IEquatable<LinearLocation<TCoordinate>>, IComparable<LinearLocation<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Gets a location which refers to the end of a linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linear">The linear geometry.</param>
        /// <returns>A new <c>LinearLocation</c>.</returns>
        public static LinearLocation<TCoordinate> GetEndLocation(IGeometry<TCoordinate> linear)
        {
            if (!(linear is ILineString<TCoordinate> || linear is IMultiLineString<TCoordinate>))
            {
                string message = String.Format("Expected {0} or {1}, but was {2}",
                                               typeof(ILineString<TCoordinate>), typeof(IMultiLineString<TCoordinate>),
                                               linear.GetType());

                throw new ArgumentException(message, "linear");
            }

            return setToEnd(linear);
        }

        /// <summary>
        /// Computes the <typeparamref name="TCoordinate"/> of a point a given fraction
        /// along the line segment <c>(p0, p1)</c>.
        /// If the fraction is greater than 1.0 the last
        /// point of the segment is returned.
        /// If the fraction is less than or equal to 0.0 the first point
        /// of the segment is returned.
        /// </summary>
        /// <param name="p0">The first point of the line segment.</param>
        /// <param name="p1">The last point of the line segment.</param>
        /// <param name="fraction">The length to the desired point.</param>
        public static TCoordinate PointAlongSegmentByFraction(TCoordinate p0, TCoordinate p1, Double fraction)
        {
            if (fraction <= 0.0)
            {
                return p0;
            }

            if (fraction >= 1.0)
            {
                return p1;
            }

            Double x = (p1[Ordinates.X] - p0[Ordinates.X]) * fraction + p0[Ordinates.X];
            Double y = (p1[Ordinates.Y] - p0[Ordinates.Y]) * fraction + p0[Ordinates.Y];
            return new TCoordinate(x, y);
        }

        private readonly Int32 _componentIndex;
        private readonly Int32 _segmentIndex;
        private readonly Double _segmentFraction;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation{TCoordinate}"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        public LinearLocation(Int32 segmentIndex, Double segmentFraction) :
            this(0, segmentIndex, segmentFraction) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation{TCoordinate}"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="componentIndex">Index of the component.</param>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        public LinearLocation(Int32 componentIndex, Int32 segmentIndex, Double segmentFraction)
        {
            _componentIndex = componentIndex;
            _segmentIndex = segmentIndex;
            _segmentFraction = segmentFraction;

            // Ensures the individual values are locally valid.
            // Does not ensure that the indexes are valid for
            // a particular linear geometry.

            if (_segmentFraction < 0.0)
            {
                _segmentFraction = 0.0;
            }

            if (_segmentFraction > 1.0)
            {
                _segmentFraction = 1.0;
            }

            if (_componentIndex < 0)
            {
                _componentIndex = 0;
                _segmentIndex = 0;
                _segmentFraction = 0.0;
            }

            if (_segmentIndex < 0)
            {
                _segmentIndex = 0;
                _segmentFraction = 0.0;
            }

            if (_segmentFraction == 1.0)
            {
                _segmentFraction = 0.0;
                _segmentIndex += 1;
            }
        }

        /// <summary>
        /// Ensures the indexes are valid for a given linear <see cref="IGeometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linear">A linear geometry.</param>
        public LinearLocation<TCoordinate> Clamp(IGeometry<TCoordinate> linear)
        {
            ILineString line = linear as ILineString;

            if (line == null)
            {
                IMultiLineString multiLine = linear as IMultiLineString;

                Debug.Assert(multiLine != null);

                if (_componentIndex >= multiLine.Count)
                {
                    return setToEnd(linear);
                }
                else
                {
                    line = multiLine[_componentIndex];
                }
            }

            if (_segmentIndex >= linear.PointCount)
            {
                return new LinearLocation<TCoordinate>(_componentIndex, line.PointCount - 1, 1.0);
            }

            return this;
        }

        /// <summary>
        /// Snaps the value of this location to
        /// the nearest vertex on the given linear <see cref="IGeometry{TCoordinate}" />,
        /// if the vertex is closer than <paramref name="minDistance" />.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <param name="minDistance">The minimum allowable distance to a vertex.</param>
        public LinearLocation<TCoordinate> SnapToVertex(IGeometry<TCoordinate> linearGeom, Double minDistance)
        {
            if (_segmentFraction <= 0.0 || _segmentFraction >= 1.0)
            {
                return this;
            }

            Double segLen = GetSegmentLength(linearGeom);
            Double lenToStart = _segmentFraction * segLen;
            Double lenToEnd = segLen - lenToStart;

            Double newFraction = _segmentFraction;

            if (lenToStart <= lenToEnd && lenToStart < minDistance)
            {
                newFraction = 0.0;
            }
            else if (lenToEnd <= lenToStart && lenToEnd < minDistance)
            {
                newFraction = 1.0;
            }

            return new LinearLocation<TCoordinate>(_componentIndex, _segmentIndex, newFraction);
        }

        /// <summary>
        /// Gets the length of the segment in the given
        /// Geometry containing this location.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns>The length of the segment.</returns>
        public Double GetSegmentLength(IGeometry<TCoordinate> linearGeom)
        {
            ILineString<TCoordinate> lineComp = getLine(linearGeom, _componentIndex);

            // ensure segment index is valid
            Int32 segIndex = _segmentIndex;
            Int32 pointCount = lineComp.PointCount;

            if (_segmentIndex >= pointCount - 1)
            {
                segIndex = pointCount - 2;
            }

            TCoordinate p0 = lineComp.Coordinates[segIndex];
            TCoordinate p1 = lineComp.Coordinates[segIndex + 1];

            return p0.Distance(p1);
        }

        /// <summary>
        /// Creates a new <see cref="LinearLocation{TCoordinate}"/> with 
        /// the value of this location to moved to refer the end of a linear geometry.
        /// </summary>
        /// <param name="linear">The linear geometry to create a location for.</param>
        private static LinearLocation<TCoordinate> setToEnd(IGeometry<TCoordinate> linear)
        {
            Int32 componentIndex = getLineCount(linear) - 1;
            ILineString<TCoordinate> line = getLine(linear, componentIndex);

            Int32 segmentIndex = line.PointCount - 1;
            Double segmentFraction = 1.0;

            return new LinearLocation<TCoordinate>(componentIndex, segmentIndex, segmentFraction);
        }

        /// <summary>
        /// Gets the component index for this location.
        /// </summary>
        public Int32 ComponentIndex
        {
            get { return _componentIndex; }
        }

        /// <summary>
        /// Gets the segment index for this location.
        /// </summary>
        public Int32 SegmentIndex
        {
            get { return _segmentIndex; }
        }

        /// <summary>
        /// Gets the segment fraction for this location.
        /// </summary>
        public Double SegmentFraction
        {
            get { return _segmentFraction; }
        }

        /// <summary>
        /// Tests whether this location refers to a vertex:
        /// returns <see langword="true"/> if the location is a vertex.
        /// </summary>        
        public Boolean IsVertex
        {
            get { return _segmentFraction <= 0.0 || _segmentFraction >= 1.0; }
        }

        /// <summary>
        /// Gets the <typeparamref name="TCoordinate"/> along the
        /// given linear <see cref="Geometry{TCoordinate}" /> which is
        /// referenced by this location.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns>The <typeparamref name="TCoordinate"/> at the location.</returns>
        public TCoordinate GetCoordinate(IGeometry<TCoordinate> linearGeom)
        {
            ILineString<TCoordinate> lineComp = getLine(linearGeom, _componentIndex);
            TCoordinate p0 = lineComp.Coordinates[_segmentIndex];
            
            if (_segmentIndex >= lineComp.PointCount - 1)
            {
                return p0;
            }

            TCoordinate p1 = lineComp.Coordinates[_segmentIndex + 1];

            return PointAlongSegmentByFraction(p0, p1, _segmentFraction);
        }

        /// <summary>
        /// Tests whether this location refers to a valid
        /// location on the given linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns><see langword="true"/> if this location is valid.</returns>
        public Boolean IsValid(IGeometry<TCoordinate> linearGeom)
        {
            Int32 lineCount = getLineCount(linearGeom);

            if (_componentIndex < 0 || _componentIndex >= lineCount)
            {
                return false;
            }

            ILineString<TCoordinate> lineComp = getLine(linearGeom, _componentIndex);

            if (_segmentIndex < 0 || _segmentIndex > lineComp.PointCount)
            {
                return false;
            }

            if (_segmentIndex == lineComp.PointCount && _segmentFraction != 0.0)
            {
                return false;
            }

            if (_segmentFraction < 0.0 || _segmentFraction > 1.0)
            {
                return false;
            }

            return true;
        }

        #region IEquatable<LinearLocation<TCoordinate>> Members

        public Boolean Equals(LinearLocation<TCoordinate> other)
        {
            return other._componentIndex == _componentIndex &&
                   other._segmentFraction == _segmentFraction &&
                   other._segmentIndex == _segmentIndex;
        }

        #endregion

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="LinearLocation{TCoordinate}"/> being compared to.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this 
        /// <see cref="LinearLocation{TCoordinate}"/> is less than, equal to, 
        /// or greater than the specified <see cref="LinearLocation{TCoordinate}"/>.
        /// </returns>
        public Int32 CompareTo(LinearLocation<TCoordinate> other)
        {
            // compare component indices
            if (_componentIndex < other.ComponentIndex) { return -1; }
            if (_componentIndex > other.ComponentIndex) { return 1; }

            // compare segments
            if (_segmentIndex < other.SegmentIndex) { return -1; }
            if (_segmentIndex > other.SegmentIndex) { return 1; }

            // same segment, so compare segment fraction
            if (_segmentFraction < other.SegmentFraction) { return -1; }
            if (_segmentFraction > other.SegmentFraction) { return 1; }

            // same location
            return 0;
        }

        /// <summary>
        /// Compares this object with the specified index values for order.
        /// </summary>
        /// <param name="componentIndex">The component index.</param>
        /// <param name="segmentIndex">The segment index.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <see cref="LinearLocation{TCoordinate}"/>
        /// is less than, equal to, or greater than the specified locationValues.
        /// </returns>
        public Int32 CompareLocationValues(Int32 componentIndex, Int32 segmentIndex, Double segmentFraction)
        {
            // compare component indices
            if (_componentIndex < componentIndex) { return -1; }
            if (_componentIndex > componentIndex) { return 1; }

            // compare segments
            if (_segmentIndex < segmentIndex) { return -1; }
            if (_segmentIndex > segmentIndex) { return 1; }

            // same segment, so compare segment fraction
            if (_segmentFraction < segmentFraction) { return -1; }
            if (_segmentFraction > segmentFraction) { return 1; }

            // same location
            return 0;
        }

        /// <summary>
        /// Compares two sets of location values for order.
        /// </summary>
        /// <param name="componentIndex0">The first component index.</param>
        /// <param name="segmentIndex0">The first segment index.</param>
        /// <param name="segmentFraction0">The first segment fraction.</param>
        /// <param name="componentIndex1">The second component index.</param>
        /// <param name="segmentIndex1">The second segment index.</param>
        /// <param name="segmentFraction1">The second segment fraction.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer
        /// as the first set of location values is less than, equal to, 
        /// or greater than the second set of locationValues.
        /// </returns>
        public static Int32 CompareLocationValues(
            Int32 componentIndex0, Int32 segmentIndex0, Double segmentFraction0,
            Int32 componentIndex1, Int32 segmentIndex1, Double segmentFraction1)
        {
            // compare component indices
            if (componentIndex0 < componentIndex1) { return -1; }
            if (componentIndex0 > componentIndex1) { return 1; }

            // compare segments
            if (segmentIndex0 < segmentIndex1) { return -1; }
            if (segmentIndex0 > segmentIndex1) { return 1; }

            // same segment, so compare segment fraction
            if (segmentFraction0 < segmentFraction1) { return -1; }
            if (segmentFraction0 > segmentFraction1) { return 1; }

            // same location
            return 0;
        }

        private static ILineString<TCoordinate> getLine(IGeometry<TCoordinate> linearGeometry, Int32 lineIndex)
        {
            ILineString<TCoordinate> line = linearGeometry as ILineString<TCoordinate>;

            if (line == null)
            {
                IMultiLineString<TCoordinate> multiLine = linearGeometry as IMultiLineString<TCoordinate>;
                Debug.Assert(multiLine != null);
                line = multiLine[lineIndex];
            }

            Debug.Assert(line != null);
            return line;
        }

        private static Int32 getLineCount(IGeometry<TCoordinate> linear)
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
    }
}