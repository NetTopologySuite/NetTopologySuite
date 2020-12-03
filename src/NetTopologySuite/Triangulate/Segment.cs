using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate
{

    /// <summary>
    /// Models a constraint segment in a triangulation.
    /// A constraint segment is an oriented straight line segment between a start point
    /// and an end point.
    /// </summary>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    ///
    public class Segment
    {
        private readonly LineSegment _ls;

        /// <summary>
        /// Creates a new instance for the given ordinates.
        /// </summary>
        public Segment(double x1, double y1, double z1, double x2, double y2, double z2)
            :this(new CoordinateZ(x1, y1, z1), new CoordinateZ(x2, y2, z2))
        {
        }

        /// <summary>
        /// Creates a new instance for the given ordinates,  with associated external data.
        /// </summary>
        public Segment(double x1, double y1, double z1, double x2, double y2, double z2, object data)
            : this(new CoordinateZ(x1, y1, z1), new CoordinateZ(x2, y2, z2), data)
        {
        }

        /// <summary>
        /// Creates a new instance for the given points, with associated external data.
        /// </summary>
        /// <param name="p0">the start point</param>
        /// <param name="p1">the end point</param>
        /// <param name="data">an external data object</param>
        public Segment(Coordinate p0, Coordinate p1, object data)
        {
            _ls = new LineSegment(p0, p1);
            this.Data = data;
        }

        /// <summary>
        /// Creates a new instance for the given points.
        /// </summary>
        /// <param name="p0">the start point</param>
        /// <param name="p1">the end point</param>
        public Segment(Coordinate p0, Coordinate p1)
        {
            _ls = new LineSegment(p0, p1);
        }

        /// <summary>
        /// Gets the start coordinate of the segment
        /// </summary>
        /// <remarks>a Coordinate</remarks>
        public Coordinate Start => _ls.GetCoordinate(0);

        /// <summary>
        /// Gets the end coordinate of the segment
        /// </summary>
        /// <remarks>a Coordinate</remarks>
        public Coordinate End => _ls.GetCoordinate(1);

        /// <summary>
        /// Gets the start X ordinate of the segment
        /// </summary>
        /// <remarks>the X ordinate value</remarks>
        public double StartX
        {
            get
            {
                var p = _ls.GetCoordinate(0);
                return p.X;
            }
        }

        /// <summary>
        /// Gets the start Y ordinate of the segment
        /// </summary>
        /// <remarks>the Y ordinate value</remarks>
        public double StartY
        {
            get
            {
                var p = _ls.GetCoordinate(0);
                return p.Y;
            }
        }

        /// <summary>
        /// Gets the start Z ordinate of the segment
        /// </summary>
        /// <remarks>the Z ordinate value</remarks>
        public double StartZ
        {
            get
            {
                var p = _ls.GetCoordinate(0);
                return p.Z;
            }
        }

        /// <summary>
        /// Gets the end X ordinate of the segment
        /// </summary>
        /// <remarks>the X ordinate value</remarks>
        public double EndX
        {
            get
            {
                var p = _ls.GetCoordinate(1);
                return p.X;
            }
        }

        /// <summary>
        /// Gets the end Y ordinate of the segment
        /// </summary>
        /// <remarks>he Y ordinate value</remarks>
        public double EndY
        {
            get
            {
                var p = _ls.GetCoordinate(1);
                return p.Y;
            }
        }

        /// <summary>
        /// Gets the end Z ordinate of the segment
        /// </summary>
        /// <remarks>the Z ordinate value</remarks>
        public double EndZ
        {
            get
            {
                var p = _ls.GetCoordinate(1);
                return p.Z;
            }
        }

        /// <summary>
        /// Gets a <tt>LineSegment</tt> modelling this segment.
        /// </summary>
        /// <remarks>a LineSegment</remarks>
        public LineSegment LineSegment => _ls;

        /// <summary>
        /// Gets or sets the external data associated with this segment
        /// </summary>
        /// <remarks>a data object</remarks>
        public object Data { get; set; }

        /// <summary>
        /// Determines whether two segments are topologically equal.
        /// I.e. equal up to orientation.
        /// </summary>
        /// <param name="s">a segment</param>
        /// <returns>true if the segments are topologically equal</returns>
        public bool EqualsTopologically(Segment s)
        {
            return _ls.EqualsTopologically(s.LineSegment);
        }

        /// <summary>
        /// Computes the intersection point between this segment and another one.
        /// </summary>
        /// <param name="s">a segment</param>
        /// <returns>the intersection point, or <c>null</c> if there is none</returns>
        public Coordinate Intersection(Segment s)
        {
            return _ls.Intersection(s.LineSegment);
        }

        /// <summary>
        /// Computes a string representation of this segment.
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString()
        {
            return _ls.ToString();
        }
    }
}
