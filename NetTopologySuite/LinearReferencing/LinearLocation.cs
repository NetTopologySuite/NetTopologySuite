using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Represents a location along a <see cref="LineString" /> or <see cref="MultiLineString" />.
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
            if (! (linear is ILineString || linear is IMultiLineString))
            {
                string message = String.Format("Expected {0} or {1}, but was {2}",
                                               typeof (ILineString), typeof (IMultiLineString), linear.GetType());
                throw new ArgumentException(message, "linear");
            }
            LinearLocation loc = new LinearLocation();
            loc.SetToEnd(linear);
            return loc;
        }

        /// <summary>
        /// Computes the <see cref="Coordinate" /> of a point a given fraction
        /// along the line segment <c>(p0, p1)</c>.
        /// If the fraction is greater than 1.0 the last
        /// point of the segment is returned.
        /// If the fraction is less than or equal to 0.0 the first point
        /// of the segment is returned.
        /// </summary>
        /// <param name="p0">The first point of the line segment.</param>
        /// <param name="p1">The last point of the line segment.</param>
        /// <param name="fraction">The length to the desired point.</param>
        public static ICoordinate PointAlongSegmentByFraction(ICoordinate p0, ICoordinate p1, Double fraction)
        {
            if (fraction <= 0.0)
            {
                return p0;
            }
            if (fraction >= 1.0)
            {
                return p1;
            }

            Double x = (p1.X - p0.X)*fraction + p0.X;
            Double y = (p1.Y - p0.Y)*fraction + p0.Y;
            return new Coordinate(x, y);
        }

        private Int32 componentIndex = 0;
        private Int32 segmentIndex = 0;
        private Double segmentFraction = 0.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        public LinearLocation() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        public LinearLocation(Int32 segmentIndex, Double segmentFraction) :
            this(0, segmentIndex, segmentFraction) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearLocation"/> class:
        /// creates a location referring to the start of a linear geometry.
        /// </summary>
        /// <param name="componentIndex">Index of the component.</param>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="segmentFraction">The segment fraction.</param>
        public LinearLocation(Int32 componentIndex, Int32 segmentIndex, Double segmentFraction)
        {
            this.componentIndex = componentIndex;
            this.segmentIndex = segmentIndex;
            this.segmentFraction = segmentFraction;
            Normalize();
        }

        /// <summary>
        /// Ensures the individual values are locally valid.
        /// Does not ensure that the indexes are valid for
        /// a particular linear geometry.
        /// </summary>
        private void Normalize()
        {
            if (segmentFraction < 0.0)
            {
                segmentFraction = 0.0;
            }

            if (segmentFraction > 1.0)
            {
                segmentFraction = 1.0;
            }

            if (componentIndex < 0)
            {
                componentIndex = 0;
                segmentIndex = 0;
                segmentFraction = 0.0;
            }

            if (segmentIndex < 0)
            {
                segmentIndex = 0;
                segmentFraction = 0.0;
            }

            if (segmentFraction == 1.0)
            {
                segmentFraction = 0.0;
                segmentIndex += 1;
            }
        }

        /// <summary>
        /// Ensures the indexes are valid for a given linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linear">A linear geometry.</param>
        public void Clamp(IGeometry linear)
        {
            if (componentIndex >= linear.NumGeometries)
            {
                SetToEnd(linear);
                return;
            }

            if (segmentIndex >= linear.NumPoints)
            {
                ILineString line = (ILineString) linear.GetGeometryN(componentIndex);
                segmentIndex = line.NumPoints - 1;
                segmentFraction = 1.0;
            }
        }

        /// <summary>
        /// Snaps the value of this location to
        /// the nearest vertex on the given linear <see cref="Geometry" />,
        /// if the vertex is closer than <paramref name="minDistance" />.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <param name="minDistance">The minimum allowable distance to a vertex.</param>
        public void SnapToVertex(IGeometry linearGeom, Double minDistance)
        {
            if (segmentFraction <= 0.0 || segmentFraction >= 1.0)
            {
                return;
            }

            Double segLen = GetSegmentLength(linearGeom);
            Double lenToStart = segmentFraction*segLen;
            Double lenToEnd = segLen - lenToStart;

            if (lenToStart <= lenToEnd && lenToStart < minDistance)
            {
                segmentFraction = 0.0;
            }
            else if (lenToEnd <= lenToStart && lenToEnd < minDistance)
            {
                segmentFraction = 1.0;
            }
        }

        /// <summary>
        /// Gets the length of the segment in the given
        /// Geometry containing this location.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns>The length of the segment.</returns>
        public Double GetSegmentLength(IGeometry linearGeom)
        {
            ILineString lineComp = (ILineString) linearGeom.GetGeometryN(componentIndex);

            // ensure segment index is valid
            Int32 segIndex = segmentIndex;
            if (segmentIndex >= lineComp.NumPoints - 1)
            {
                segIndex = lineComp.NumPoints - 2;
            }

            ICoordinate p0 = lineComp.GetCoordinateN(segIndex);
            ICoordinate p1 = lineComp.GetCoordinateN(segIndex + 1);
            return p0.Distance(p1);
        }

        /// <summary>
        /// Sets the value of this location to
        /// refer the end of a linear geometry.
        /// </summary>
        /// <param name="linear">The linear geometry to set.</param>
        public void SetToEnd(IGeometry linear)
        {
            componentIndex = linear.NumGeometries - 1;
            ILineString lastLine = (ILineString) linear.GetGeometryN(componentIndex);
            segmentIndex = lastLine.NumPoints - 1;
            segmentFraction = 1.0;
        }

        /// <summary>
        /// Gets the component index for this location.
        /// </summary>
        public Int32 ComponentIndex
        {
            get { return componentIndex; }
        }

        /// <summary>
        /// Gets the segment index for this location.
        /// </summary>
        public Int32 SegmentIndex
        {
            get { return segmentIndex; }
        }

        /// <summary>
        /// Gets the segment fraction for this location.
        /// </summary>
        public Double SegmentFraction
        {
            get { return segmentFraction; }
        }

        /// <summary>
        /// Tests whether this location refers to a vertex:
        /// returns <c>true</c> if the location is a vertex.
        /// </summary>        
        public Boolean IsVertex
        {
            get { return segmentFraction <= 0.0 || segmentFraction >= 1.0; }
        }

        /// <summary>
        /// Gets the <see cref="Coordinate" /> along the
        /// given linear <see cref="Geometry" /> which is
        /// referenced by this location.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns>The <see cref="Coordinate" /> at the location.</returns>
        public ICoordinate GetCoordinate(IGeometry linearGeom)
        {
            ILineString lineComp = (ILineString) linearGeom.GetGeometryN(componentIndex);
            ICoordinate p0 = lineComp.GetCoordinateN(segmentIndex);
            if (segmentIndex >= lineComp.NumPoints - 1)
            {
                return p0;
            }
            ICoordinate p1 = lineComp.GetCoordinateN(segmentIndex + 1);
            return PointAlongSegmentByFraction(p0, p1, segmentFraction);
        }

        /// <summary>
        /// Tests whether this location refers to a valid
        /// location on the given linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">A linear geometry.</param>
        /// <returns><c>true</c> if this location is valid.</returns>
        public Boolean IsValid(IGeometry linearGeom)
        {
            if (componentIndex < 0 || componentIndex >= linearGeom.NumGeometries)
            {
                return false;
            }
            ILineString lineComp = (ILineString) linearGeom.GetGeometryN(componentIndex);
            if (segmentIndex < 0 || segmentIndex > lineComp.NumGeometries)
            {
                return false;
            }
            if (segmentIndex == lineComp.NumGeometries && segmentFraction != 0.0)
            {
                return false;
            }
            if (segmentFraction < 0.0 || segmentFraction > 1.0)
            {
                return false;
            }
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
        /// <exception cref="System.ArgumentException">
        /// <paramref name="obj" /> is not the same type as this instance. 
        /// </exception>
        public Int32 CompareTo(object obj)
        {
            LinearLocation other = (LinearLocation) obj;
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
        public Int32 CompareTo(LinearLocation other)
        {
            // compare component indices
            if (componentIndex < other.ComponentIndex)
            {
                return -1;
            }
            if (componentIndex > other.ComponentIndex)
            {
                return 1;
            }
            // compare segments
            if (segmentIndex < other.SegmentIndex)
            {
                return -1;
            }
            if (segmentIndex > other.SegmentIndex)
            {
                return 1;
            }
            // same segment, so compare segment fraction
            if (segmentFraction < other.SegmentFraction)
            {
                return -1;
            }
            if (segmentFraction > other.SegmentFraction)
            {
                return 1;
            }
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
        public Int32 CompareLocationValues(Int32 componentIndex1, Int32 segmentIndex1, Double segmentFraction1)
        {
            // compare component indices
            if (componentIndex < componentIndex1)
            {
                return -1;
            }
            if (componentIndex > componentIndex1)
            {
                return 1;
            }
            // compare segments
            if (segmentIndex < segmentIndex1)
            {
                return -1;
            }
            if (segmentIndex > segmentIndex1)
            {
                return 1;
            }
            // same segment, so compare segment fraction
            if (segmentFraction < segmentFraction1)
            {
                return -1;
            }
            if (segmentFraction > segmentFraction1)
            {
                return 1;
            }
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
            if (componentIndex0 < componentIndex1)
            {
                return -1;
            }
            if (componentIndex0 > componentIndex1)
            {
                return 1;
            }
            // compare segments
            if (segmentIndex0 < segmentIndex1)
            {
                return -1;
            }
            if (segmentIndex0 > segmentIndex1)
            {
                return 1;
            }
            // same segment, so compare segment fraction
            if (segmentFraction0 < segmentFraction1)
            {
                return -1;
            }
            if (segmentFraction0 > segmentFraction1)
            {
                return 1;
            }
            // same location
            return 0;
        }

        /// <summary>
        /// Copies this location.
        /// </summary>
        /// <returns>A copy of this location.</returns>
        public object Clone()
        {
            return new LinearLocation(segmentIndex, segmentFraction);
        }
    }
}