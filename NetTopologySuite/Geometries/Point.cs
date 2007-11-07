using System;
using System.Diagnostics;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>Point</c>.
    /// </summary>
    [Serializable]
    public class Point : Geometry, IPoint
    {
        private static readonly ICoordinate emptyCoordinate = null;

        /// <summary>
        /// Represents an empty <c>Point</c>.
        /// </summary>
        public static readonly IPoint Empty = new GeometryFactory().CreatePoint(emptyCoordinate);

        /// <summary>  
        /// The <c>Coordinate</c> wrapped by this <c>Point</c>.
        /// </summary>
        private ICoordinateSequence coordinates;

        /// <summary>
        /// 
        /// </summary>
        public ICoordinateSequence CoordinateSequence
        {
            get { return coordinates; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Point"/> class.
        /// </summary>
        /// <param name="coordinate">The coordinate used for create this <see cref="Point" />.</param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(ICoordinate coordinate) :
            this(GeometryFactory.Default.CoordinateSequenceFactory.Create(new ICoordinate[] {coordinate}),
                 GeometryFactory.Default) {}

        /// <summary>
        /// Constructs a <c>Point</c> with the given coordinate.
        /// </summary>
        /// <param name="coordinates">
        /// Contains the single coordinate on which to base this <c>Point</c>,
        /// or <c>null</c> to create the empty point.
        /// </param>
        public Point(ICoordinateSequence coordinates, IGeometryFactory factory) : base(factory)
        {
            if (coordinates == null)
            {
                coordinates = factory.CoordinateSequenceFactory.Create(new ICoordinate[] {});
            }

            Debug.Assert(coordinates.Count <= 1);
            this.coordinates = (ICoordinateSequence) coordinates;
        }

        public override ICoordinate[] Coordinates
        {
            get { return IsEmpty ? new ICoordinate[] {} : new ICoordinate[] {Coordinate}; }
        }

        public override Int32 NumPoints
        {
            get { return IsEmpty ? 0 : 1; }
        }

        public override Boolean IsEmpty
        {
            get { return Coordinate == null; }
        }

        public override Boolean IsSimple
        {
            get { return true; }
        }

        public override Boolean IsValid
        {
            get { return true; }
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Point; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.False; }
        }
       
        public Double X
        {
            get
            {
                if (Coordinate == null)
                {
                    throw new ArgumentOutOfRangeException("X called on empty Point");
                }

                return Coordinate.X;
            }
            set { Coordinate.X = value; }
        }
      
        public Double Y
        {
            get
            {
                if (Coordinate == null)
                {
                    throw new ArgumentOutOfRangeException("Y called on empty Point");
                }

                return Coordinate.Y;
            }
            set { Coordinate.Y = value; }
        }

        public override ICoordinate Coordinate
        {
            get { return coordinates.Count != 0 ? coordinates.GetCoordinate(0) : null; }
        }

        public override string GeometryType
        {
            get { return "Point"; }
        }

        public override IGeometry Boundary
        {
            get { return Factory.CreateGeometryCollection(null); }
        }

        protected override IExtents ComputeEnvelopeInternal()
        {
            if (IsEmpty)
            {
                return new Extents();
            }

            return new Extents(Coordinate.X, Coordinate.X, Coordinate.Y, Coordinate.Y);
        }

        public override Boolean EqualsExact(IGeometry other, Double tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            if (IsEmpty && other.IsEmpty)
            {
                return true;
            }

            return Equal(((IPoint) other).Coordinate, Coordinate, tolerance);
        }

        public override void Apply(ICoordinateFilter filter)
        {
            if (IsEmpty)
            {
                return;
            }

            filter.Filter((Coordinate) Coordinate);
        }

        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
        }

        public override object Clone()
        {
            Point p = (Point) base.Clone();
            p.coordinates = (ICoordinateSequence) coordinates.Clone();
            return p;
        }

        public override void Normalize() {}

        protected internal override Int32 CompareToSameClass(object other)
        {
            Point point = (Point) other;
            return Coordinate.CompareTo(point.Coordinate);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Point"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> set to </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(Double x, Double y, Double z) :
            this(
            DefaultFactory.CoordinateSequenceFactory.Create(new ICoordinate[] {new Coordinate(x, y, z)}), DefaultFactory
            ) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Point"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> set to </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(Double x, Double y)
            : this(
                DefaultFactory.CoordinateSequenceFactory.Create(new ICoordinate[] {new Coordinate(x, y)}),
                DefaultFactory) {}
     
        public Double Z
        {
            get
            {
                if (Coordinate == null)
                {
                    throw new ArgumentOutOfRangeException("Z called on empty Point");
                }

                return Coordinate.Z;
            }
            set { Coordinate.Z = value; }
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}