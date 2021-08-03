using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Contains information about the nature and location of
    /// a <see cref="Geometry" /> validation error.
    /// </summary>
    public enum TopologyValidationErrors
    {
        /// <summary>
        /// </summary>
        NoInvalidIntersection = -1,

        /// <summary>
        /// Indicates that a hole of a polygon lies partially
        /// or completely in the exterior of the shell.
        /// </summary>
        HoleOutsideShell = 2,

        /// <summary>
        /// Indicates that a hole lies
        /// in the interior of another hole in the same polygon.
        /// </summary>
        NestedHoles = 3,

        /// <summary>
        /// Indicates that the interior of a polygon is disjoint
        /// (often caused by set of contiguous holes splitting
        /// the polygon into two parts).
        /// </summary>
        DisconnectedInteriors = 4,

        /// <summary>
        /// Indicates that two rings of a polygonal geometry intersect.
        /// </summary>
        SelfIntersection = 5,

        /// <summary>
        /// Indicates that a ring self-intersects.
        /// </summary>
        RingSelfIntersection = 6,

        /// <summary>
        /// Indicates that a polygon component of a
        /// <see cref="MultiPolygon" /> lies inside another polygonal component.
        /// </summary>
        NestedShells = 7,

        /// <summary>
        /// Indicates that a polygonal geometry
        /// contains two rings which are identical.
        /// </summary>
        DuplicateRings = 8,

        /// <summary>
        /// Indicates that either:
        /// - A <see cref="LineString" /> contains a single point.
        /// - A <see cref="LinearRing" /> contains 2 or 3 points.
        /// </summary>
        TooFewPoints = 9,

        /// <summary>
        /// Indicates that the <c>X</c> or <c>Y</c> ordinate of
        /// a <see cref="Coordinate" /> is not a valid
        /// numeric value (e.g. <see cref="double.NaN" />).
        /// </summary>
        InvalidCoordinate = 10,

        /// <summary>
        /// Indicates that a ring is not correctly closed
        /// (the first and the last coordinate are different).
        /// </summary>
        RingNotClosed = 11,
    }

    /// <summary>
    /// Contains information about the nature and location of a <c>Geometry</c>
    /// validation error.
    /// </summary>
    public class TopologyValidationError
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Added readonly!

        /// <summary>
        /// These messages must synch up with the indexes above
        /// </summary>
        private static readonly string[] errMsg =
        {
            "Topology Validation Error",
            "Repeated Point",
            "Hole lies outside shell",
            "Holes are nested",
            "Interior is disconnected",
            "Self-intersection",
            "Ring Self-intersection",
            "Nested shells",
            "Duplicate Rings",
            "Too few points in geometry component",
            "Invalid Coordinate"
        };

        private readonly TopologyValidationErrors errorType;
        private readonly Coordinate pt;

        /// <summary>
        ///
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="pt"></param>
        public TopologyValidationError(TopologyValidationErrors errorType, Coordinate pt)
        {
            this.errorType = errorType;
            if(pt != null)
                this.pt = (Coordinate) pt.Copy();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="errorType"></param>
        public TopologyValidationError(TopologyValidationErrors errorType) : this(errorType, null) { }

        /// <summary>
        ///
        /// </summary>
        public Coordinate Coordinate => pt;

        /// <summary>
        ///
        /// </summary>
        public TopologyValidationErrors ErrorType => errorType;

        /// <summary>
        ///
        /// </summary>
        public string Message => errMsg[(int) errorType];

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Message + " at or near point " + pt;
        }
    }
}
