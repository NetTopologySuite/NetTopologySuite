using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
#if !HAS_SYSTEM_ICLONEABLE
using ICloneable = GeoAPI.ICloneable;
#endif
namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Represents a location along a <see cref="ILineString" /> or <see cref="IMultiLineString" />.<br/>
    /// The referenced geometry is not maintained within this location,
    /// but must be provided for operations which require it.
    /// Various methods are provided to manipulate the location value
    /// and query the geometry it references.
    /// </summary>
    public class LinearLocation : IComparable<LinearLocation>, IComparable, ICloneable
    {
        /// <summary>
        /// Gets a location which refers to the end of a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linear">The linear geometry.</param>
        /// <returns>A new <c>LinearLocation</c>.</returns>
        public static LinearLocation GetEndLocation(IGeometry linear)
        {
            if (!(linear is ILineString || linear is IMultiLineString))
            {
                var message = String.Format("Expected {0} or {1}, but was {2}",
                    typeof(ILineString), typeof(IMultiLineString), linear.GetType());
                throw new ArgumentException(message, "linear");
            }
            var loc = new LinearLocation();
            loc.SetToEnd(linear);
            return loc;
        }
        /// <summary>
        /// Computes the <see cref="Coordinate" /> of a point a given fraction
        /// along the line segment <c>(p0, p1)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the fraction is greater than 1.0 the last
        /// point of the segment is returned.</para>
        /// <para>If the fraction is less than or equal to 0.0 the first point
        /// of the segment is returned.</para>
        /// <para>
        /// The Z ordinate is interpolated from the Z-ordinates of the given points,
        /// if they are specified.</para>
        /// </remarks>
        /// <param name="p0">The first point of the line segment.</param>
        /// <param name="p1">The last point of the line segment.</param>
        /// <param name="fraction">The length to the desired point.</param>
        /// <returns></returns>
        public static Coordinate PointAlongSegmentByFraction(Coordinate p0, Coordinate p1, double fraction)
        {
            if (fraction <= 0.0) return p0;
            if (fraction >= 1.0) return p1;
            var x = (p1.X - p0.X) * fraction + p0.X;
            var y = (p1.Y - p0.Y) * fraction + p0.Y;
            // interpolate Z value. If either input Z is NaN, result z will be NaN as well.
            var z = (p1.Z - p0.Z) * fraction + p0.Z;
            return new Coordinate(x, y, z);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        public LinearLocation() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        public LinearLocation(int segmentIndex, double segmentFraction) :
            this(0, segmentIndex, segmentFraction) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="componentIndex">Index of the component.</param>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        public LinearLocation(int componentIndex, int segmentIndex, double segmentFraction) :
            this(componentIndex, segmentIndex, segmentFraction, true) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="componentIndex">Index of the component.</param>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        /// <param name="normalize">If <c>true</c>, ensures the individual values are locally valid.</param>
        private LinearLocation(int componentIndex, int segmentIndex, double segmentFraction, bool normalize)
        {
            ComponentIndex = componentIndex;
            SegmentIndex = segmentIndex;
            SegmentFraction = segmentFraction;
            if (normalize)
                Normalize();
        }
        /// <summary>
        /// Creates a new location equal to a given one.
        /// </summary>
        /// <param name="loc">A linear location</param>
        public LinearLocation(LinearLocation loc)
        {
            ComponentIndex = loc.ComponentIndex;
            SegmentIndex = loc.SegmentIndex;
            SegmentFraction = loc.SegmentFraction;
        }
        /// <summary>
        /// Ensures the individual values are locally valid.
        /// Does not ensure that the indexes are valid for
        /// a particular linear geometry.
        /// </summary>
        private void Normalize()
        {
            if (SegmentFraction < 0.0)
                SegmentFraction = 0.0;
            if (SegmentFraction > 1.0)
                SegmentFraction = 1.0;
            if (ComponentIndex < 0)
            {
                ComponentIndex = 0;
                SegmentIndex = 0;
                SegmentFraction = 0.0;
            }
            if (SegmentIndex < 0)
            {
                SegmentIndex = 0;
                SegmentFraction = 0.0;
            }
            if (SegmentFraction == 1.0)
            {
                SegmentFraction = 0.0;
                SegmentIndex += 1;
            }
        }
        /// <summary>
        /// Ensures the indexes are valid for a given linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linear">A linear geometry.</param>
        public void Clamp(IGeometry linear)
        {
            if (ComponentIndex >= linear.NumGeometries)
            {
                SetToEnd(linear);
                return;
            }
            if (SegmentIndex >= linear.NumPoints)
            {
                var line = (ILineString)linear.GetGeometryN(ComponentIndex);
                SegmentIndex = line.NumPoints - 1;
                SegmentFraction = 1.0;
            }
        }
        /// <summary>
        /// Snaps the value of this location to
        /// the nearest vertex on the given linear <see cref="Geometry" />,
        /// if the vertex is closer than <paramref name="minDistance" />.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <param name="minDistance">The minimum allowable distance to a vertex.</param>
        public void SnapToVertex(IGeometry linearGeom, double minDistance)
        {
            if (SegmentFraction <= 0.0 || SegmentFraction >= 1.0)
                return;
            var segLen = GetSegmentLength(linearGeom);
            var lenToStart = SegmentFraction * segLen;
            var lenToEnd = segLen - lenToStart;
            if (lenToStart <= lenToEnd && lenToStart < minDistance)
                SegmentFraction = 0.0;
            else if (lenToEnd <= lenToStart && lenToEnd < minDistance)
                SegmentFraction = 1.0;
        }
        /// <summary>
        /// Gets the length of the segment in the given
        /// Geometry containing this location.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns>The length of the segment.</returns>
        public double GetSegmentLength(IGeometry linearGeom)
        {
            var lineComp = (ILineString)linearGeom.GetGeometryN(ComponentIndex);
            // ensure segment index is valid
            var segIndex = SegmentIndex;
            if (SegmentIndex >= lineComp.NumPoints - 1)
                segIndex = lineComp.NumPoints - 2;
            var p0 = lineComp.GetCoordinateN(segIndex);
            var p1 = lineComp.GetCoordinateN(segIndex + 1);
            return p0.Distance(p1);
        }
        /// <summary>
        /// Sets the value of this location to
        /// refer to the end of a linear geometry.
        /// </summary>
        /// <param name="linear">The linear geometry to use to set the end.</param>
        public void SetToEnd(IGeometry linear)
        {
            ComponentIndex = linear.NumGeometries - 1;
            var lastLine = (ILineString)linear.GetGeometryN(ComponentIndex);
            SegmentIndex = lastLine.NumPoints - 1;
            SegmentFraction = 1.0;
        }
        /// <summary>
        /// Gets the component index for this location.
        /// </summary>
        public int ComponentIndex { get; private set; }
        /// <summary>
        /// Gets the segment index for this location.
        /// </summary>
        public int SegmentIndex { get; private set; }
        /// <summary>
        /// Gets the segment fraction for this location.
        /// </summary>
        public double SegmentFraction { get; private set; }
        /// <summary>
        /// Tests whether this location refers to a vertex:
        /// returns <c>true</c> if the location is a vertex.
        /// </summary>
        public bool IsVertex => SegmentFraction <= 0.0 || SegmentFraction >= 1.0;
        /// <summary>
        /// Gets the <see cref="Coordinate" /> along the
        /// given linear <see cref="Geometry" /> which is
        /// referenced by this location.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns>The <see cref="Coordinate" /> at the location.</returns>
        public Coordinate GetCoordinate(IGeometry linearGeom)
        {
            var lineComp = (ILineString)linearGeom.GetGeometryN(ComponentIndex);
            var p0 = lineComp.GetCoordinateN(SegmentIndex);
            if (SegmentIndex >= lineComp.NumPoints - 1)
                return p0;
            var p1 = lineComp.GetCoordinateN(SegmentIndex + 1);
            return PointAlongSegmentByFraction(p0, p1, SegmentFraction);
        }
        ///<summary>
        /// Gets a <see cref="LineSegment"/> representing the segment of the given linear <see cref="IGeometry"/> which contains this location.
        ///</summary>
        /// <param name="linearGeom">A linear geometry</param>
        /// <returns>the <c>LineSegment</c> containing the location</returns>
        public LineSegment GetSegment(IGeometry linearGeom)
        {
            var lineComp = (ILineString)linearGeom.GetGeometryN(ComponentIndex);
            var p0 = lineComp.GetCoordinateN(SegmentIndex);
            // check for endpoint - return last segment of the line if so
            if (SegmentIndex >= lineComp.NumPoints - 1)
            {
                var prev = lineComp.GetCoordinateN(lineComp.NumPoints - 2);
                return new LineSegment(prev, p0);
            }
            var p1 = lineComp.GetCoordinateN(SegmentIndex + 1);
            return new LineSegment(p0, p1);
        }
        /// <summary>
        /// Tests whether this location refers to a valid
        /// location on the given linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns><c>true</c> if this location is valid.</returns>
        public bool IsValid(IGeometry linearGeom)
        {
            if (ComponentIndex < 0 || ComponentIndex >= linearGeom.NumGeometries)
                return false;
            var lineComp = (ILineString)linearGeom.GetGeometryN(ComponentIndex);
            if (SegmentIndex < 0 || SegmentIndex > lineComp.NumPoints)
                return false;
            if (SegmentIndex == lineComp.NumPoints && SegmentFraction != 0.0)
                return false;
            if (SegmentFraction < 0.0 || SegmentFraction > 1.0)
                return false;
            return true;
        }
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">
        /// The <c>LineStringLocation</c> with which this
        /// <c>Coordinate</c> is being compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this
        /// <c>LineStringLocation</c> is less than, equal to,
        /// or greater than the specified <c>LineStringLocation</c>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="obj" /> is not the same type as this instance.
        /// </exception>
        public int CompareTo(object obj)
        {
            var other = (LinearLocation)obj;
            return CompareTo(other);
        }
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// The <c>LineStringLocation</c> with which this
        /// <c>Coordinate</c> is being compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this
        /// <c>LineStringLocation</c> is less than, equal to,
        /// or greater than the specified <c>LineStringLocation</c>.
        /// </returns>
        public int CompareTo(LinearLocation other)
        {
            // compare component indices
            if (ComponentIndex < other.ComponentIndex)
                return -1;
            if (ComponentIndex > other.ComponentIndex)
                return 1;
            // compare segments
            if (SegmentIndex < other.SegmentIndex)
                return -1;
            if (SegmentIndex > other.SegmentIndex)
                return 1;
            // same segment, so compare segment fraction
            if (double.IsNaN(SegmentFraction) && double.IsNaN(other.SegmentFraction))
                return 0;
            if (SegmentFraction < other.SegmentFraction)
                return -1;
            if (SegmentFraction > other.SegmentFraction)
                return 1;
            // same location
            return 0;
        }
        /// <summary>
        /// Compares this object with the specified index values for order.
        /// </summary>
        /// <param name="componentIndex1">The component index.</param>
        /// <param name="segmentIndex1">The segment index.</param>
        /// <param name="segmentFraction1">The segment fraction.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>LineStringLocation</c>
        /// is less than, equal to, or greater than the specified locationValues.
        /// </returns>
        public int CompareLocationValues(int componentIndex1, int segmentIndex1, double segmentFraction1)
        {
            // compare component indices
            if (ComponentIndex < componentIndex1)
                return -1;
            if (ComponentIndex > componentIndex1)
                return 1;
            // compare segments
            if (SegmentIndex < segmentIndex1)
                return -1;
            if (SegmentIndex > segmentIndex1)
                return 1;
            // same segment, so compare segment fraction
            if (SegmentFraction < segmentFraction1)
                return -1;
            if (SegmentFraction > segmentFraction1)
                return 1;
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
        public static int CompareLocationValues(
            int componentIndex0, int segmentIndex0, double segmentFraction0,
            int componentIndex1, int segmentIndex1, double segmentFraction1)
        {
            // compare component indices
            if (componentIndex0 < componentIndex1)
                return -1;
            if (componentIndex0 > componentIndex1)
                return 1;
            // compare segments
            if (segmentIndex0 < segmentIndex1)
                return -1;
            if (segmentIndex0 > segmentIndex1)
                return 1;
            // same segment, so compare segment fraction
            if (segmentFraction0 < segmentFraction1)
                return -1;
            if (segmentFraction0 > segmentFraction1)
                return 1;
            // same location
            return 0;
        }
        ///<summary>
        /// Tests whether two locations are on the same segment in the parent <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="loc">A location on the same geometry</param>
        /// <returns><c>true</c> if the locations are on the same segment of the parent geometry</returns>
        public bool IsOnSameSegment(LinearLocation loc)
        {
            if (ComponentIndex != loc.ComponentIndex)
                return false;
            if (SegmentIndex == loc.SegmentIndex)
                return true;
            if (loc.SegmentIndex - SegmentIndex == 1 &&
                loc.SegmentFraction == 0.0)
                return true;
            if (SegmentIndex - loc.SegmentIndex == 1 &&
                SegmentFraction == 0.0)
                return true;
            return false;
        }
        /// <summary>
        /// Tests whether this location is an endpoint of
        /// the linear component it refers to.
        /// </summary>
        /// <param name="linearGeom">The linear geometry referenced by this location</param>
        /// <returns>True if the location is a component endpoint</returns>
        public bool IsEndpoint(IGeometry linearGeom)
        {
            var lineComp = (ILineString)linearGeom.GetGeometryN(ComponentIndex);
            // check for endpoint
            var nseg = lineComp.NumPoints - 1;
            return SegmentIndex >= nseg ||
                (SegmentIndex == nseg && SegmentFraction >= 1.0);
        }
        /// <summary>
        /// Converts a linear location to the lowest equivalent location index.
        /// The lowest index has the lowest possible component and segment indices.
        /// Specifically:
        /// * if the location point is an endpoint, a location value is returned as (nseg-1, 1.0)
        /// * if the location point is ambiguous (i.e. an endpoint and a startpoint), the lowest endpoint location is returned
        /// If the location index is already the lowest possible value, the original location is returned.
        /// </summary>
        /// <param name="linearGeom">The linear geometry referenced by this location.</param>
        /// <returns>The lowest equivalent location.</returns>
        public LinearLocation ToLowest(IGeometry linearGeom)
        {
            // TODO: compute lowest component index
            var lineComp = linearGeom.GetGeometryN(ComponentIndex);
            var nseg = lineComp.NumPoints - 1;
            // if not an endpoint can be returned directly
            if (SegmentIndex < nseg)
                return this;
            return new LinearLocation(ComponentIndex, nseg, 1.0, false);
        }
        /// <summary>
        /// Copies this location.
        /// </summary>
        /// <returns>A copy of this location.</returns>
        [Obsolete("Use Copy()")]
        public object Clone()
        {
            return Copy();
        }
        public LinearLocation Copy()
        {
            return new LinearLocation(SegmentIndex, SegmentFraction);
        }
    }
}
