using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Represents the location of a point on a Geometry.
    /// Maintains both the actual point location (which of course
    /// may not be exact) as well as information about the component
    /// and segment index where the point occurs.
    /// Locations inside area Geometrys will not have an associated segment index,
    /// so in this case the segment index will have the sentinel value of InsideArea.
    /// </summary>
    public class GeometryLocation
    {
        /// <summary>
        /// Special value of segment-index for locations inside area geometries. These
        /// locations do not have an associated segment index.
        /// </summary>
        public const Int32 InsideArea = -1;

        private IGeometry component = null;
        private Int32 segIndex;
        private ICoordinate pt = null;

        /// <summary>
        /// Constructs a GeometryLocation specifying a point on a point, as well as the 
        /// segment that the point is on (or InsideArea if the point is not on a segment).
        /// </summary>
        public GeometryLocation(IGeometry component, Int32 segIndex, ICoordinate pt)
        {
            this.component = component;
            this.segIndex = segIndex;
            this.pt = pt;
        }

        /// <summary> 
        /// Constructs a GeometryLocation specifying a point inside an area point.
        /// </summary>
        public GeometryLocation(IGeometry component, ICoordinate pt) : this(component, InsideArea, pt)
        {
        }

        /// <summary>
        /// Returns the point associated with this location.
        /// </summary>
        public IGeometry GeometryComponent
        {
            get { return component; }
        }

        /// <summary>
        /// Returns the segment index for this location. If the location is inside an
        /// area, the index will have the value InsideArea;
        /// </summary>
        public Int32 SegmentIndex
        {
            get { return segIndex; }
        }

        /// <summary>
        /// Returns the location.
        /// </summary>
        public ICoordinate Coordinate
        {
            get { return pt; }
        }

        /// <summary>
        /// Returns whether this GeometryLocation represents a point inside an area point.
        /// </summary>
        public Boolean IsInsideArea
        {
            get { return segIndex == InsideArea; }
        }
    }
}