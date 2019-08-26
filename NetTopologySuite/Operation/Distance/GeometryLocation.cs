using System.Globalization;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Represents the location of a point on a Geometry.
    /// Maintains both the actual point location
    /// (which may not be exact, if the point is not a vertex)
    /// as well as information about the component
    /// and segment index where the point occurs.
    /// Locations inside area Geometrys will not have an associated segment index,
    /// so in this case the segment index will have the sentinel value of <see cref="InsideArea"/>.
    /// </summary>
    public class GeometryLocation
    {
        /// <summary>
        /// A special value of segmentIndex used for locations inside area geometries.
        /// These locations are not located on a segment,
        /// and thus do not have an associated segment index.
        /// </summary>
        public const int InsideArea = -1;

        private readonly Geometry _component;
        private readonly int _segIndex;
        private readonly Coordinate _pt;

        /// <summary>
        /// Constructs a GeometryLocation specifying a point on a point, as well as the
        /// segment that the point is on (or <see cref="InsideArea"/> if the point is not on a segment).
        /// </summary>
        /// <param name="component">The component of the geometry containing the point</param>
        /// <param name="segIndex">The segment index of the location, or <see cref="InsideArea"/></param>
        /// <param name="pt">The coordinate of the location</param>
        public GeometryLocation(Geometry component, int segIndex, Coordinate pt)
        {
            _component = component;
            _segIndex = segIndex;
            _pt = pt;
        }

        /// <summary>
        /// Constructs a GeometryLocation specifying a point inside an area point.
        /// </summary>
        /// <param name="component">The component of the geometry containing the point</param>
        /// <param name="pt">The coordinate of the location</param>
        public GeometryLocation(Geometry component, Coordinate pt) : this(component, InsideArea, pt) { }

        /// <summary>
        /// Returns the geometry component on (or in) which this location occurs.
        /// </summary>
        public Geometry GeometryComponent => _component;

        /// <summary>
        /// Returns the segment index for this location. If the location is inside an
        /// area, the index will have the value <see cref="InsideArea"/>.
        /// </summary>
        public int SegmentIndex => _segIndex;

        /// <summary>
        /// Returns the <see cref="Coordinate"/> of this location.
        /// </summary>
        public Coordinate Coordinate => _pt;

        /// <summary>
        /// Tests whether this location represents a point inside an area geometry.
        /// </summary>
        public bool IsInsideArea => _segIndex == InsideArea;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{_component.GeometryType}[{_segIndex.ToString(CultureInfo.InvariantCulture)}]-{WKTWriter.ToPoint(_pt)}";
        }
    }
}
