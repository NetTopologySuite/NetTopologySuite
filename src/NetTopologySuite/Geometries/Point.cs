using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Represents a single point.
    /// <para/>
    /// A <c>Point</c> is topologically valid if and only if:
    /// <list type="bullet">
    /// <item><description>The coordinate which defines it if any) is a valid coordinate
    /// (i.e. does not have an <c>NaN</c> X- or Y-ordinate</description></item>
    /// </list>
    /// </summary>
    ///
    [Serializable]
    public class Point : Geometry, IPuntal
    {
        private static readonly Coordinate EmptyCoordinate = null;

        /// <summary>
        /// Represents an empty <c>Point</c>.
        /// </summary>
        public static readonly Point Empty = new GeometryFactory().CreatePoint(EmptyCoordinate);

        /// <summary>
        /// The <c>Coordinate</c> wrapped by this <c>Point</c>.
        /// </summary>
        private CoordinateSequence _coordinates;

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.Point;

        /// <summary>
        ///
        /// </summary>
        public CoordinateSequence CoordinateSequence => _coordinates;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="coordinate">The coordinate used for create this <see cref="Point" />.</param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(Coordinate coordinate) :
            this(GeometryFactory.Default.CoordinateSequenceFactory.Create(new Coordinate[] { coordinate } ),
            GeometryFactory.Default) { }

        /// <summary>
        /// Constructs a <c>Point</c> with the given coordinate.
        /// </summary>
        /// <param name="coordinates">
        /// Contains the single coordinate on which to base this <c>Point</c>,
        /// or <c>null</c> to create the empty point.
        /// </param>
        /// <param name="factory"></param>
        public Point(CoordinateSequence coordinates, GeometryFactory factory) : base(factory)
        {
            if (coordinates == null)
                coordinates = factory.CoordinateSequenceFactory.Create(new Coordinate[] { });
            NetTopologySuite.Utilities.Assert.IsTrue(coordinates.Count <= 1);
            _coordinates = coordinates;
        }

        /// <summary>
        ///
        /// </summary>
        public override Coordinate[] Coordinates => IsEmpty ? new Coordinate[] { } : new Coordinate[] { this.Coordinate };

        public override double[] GetOrdinates(Ordinate ordinate)
        {

            if (IsEmpty)
                return new double[0];

            var ordinateFlag = (Ordinates)(1 << (int)ordinate);
            if ((_coordinates.Ordinates & ordinateFlag) != ordinateFlag)
                return new[] {Coordinate.NullOrdinate};

            double val = _coordinates.TryGetOrdinateIndex(ordinate, out int ordinateIndex)
                ? _coordinates.GetOrdinate(0, ordinateIndex)
                : Coordinate.NullOrdinate;
            return new [] { val };
        }

        /// <summary>
        ///
        /// </summary>
        public override int NumPoints => IsEmpty ? 0 : 1;

        /// <summary>
        ///
        /// </summary>
        public override bool IsEmpty => _coordinates.Count == 0;

        /// <summary>
        ///
        /// </summary>
        public override Dimension Dimension => Dimension.Point;

        /// <summary>
        ///
        /// </summary>
        public override Dimension BoundaryDimension => Dimension.False;

        /// <summary>
        /// Gets a value indicating the x-ordinate of this point
        /// </summary>
        /// <remarks>
        /// Deviation from JTS: this implementation <b>does not</b> throw an exception
        /// when this property is accessed or set
        /// </remarks>
        public double X
        {
            get
            {
                return !IsEmpty ? CoordinateSequence.GetX(0) : Coordinate.NullOrdinate;
            }
            set
            {
                if (!IsEmpty) CoordinateSequence.SetX(0, value);
            }
        }

        /// <summary>
        /// Gets a value indicating the y-ordinate of this point
        /// </summary>
        /// <remarks>
        /// Deviation from JTS: this implementation <b>does not</b> throw an exception
        /// when this property is accessed or set
        /// </remarks>
        public double Y
        {
            get
            {
                return !IsEmpty ? CoordinateSequence.GetY(0) : Coordinate.NullOrdinate;
            }
            set
            {
                if (!IsEmpty) CoordinateSequence.SetY(0, value);
            }
        }

        /// <inheritdoc/>
        public override Coordinate Coordinate => _coordinates.Count != 0 ? _coordinates.GetCoordinate(0) : null;

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"Point"</returns>
        public override string GeometryType => Geometry.TypeNamePoint;

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.Point;

        /// <summary>
        ///Gets the boundary of this geometry.
        ///Zero-dimensional geometries have no boundary by definition,
        ///so an empty GeometryCollection is returned.
        /// </summary>
        public override Geometry Boundary => Factory.CreateGeometryCollection();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override Envelope ComputeEnvelopeInternal()
        {
            return new Envelope(_coordinates);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            if (IsEmpty && other.IsEmpty)
                return true;
            if (IsEmpty != other.IsEmpty)
                return false;

            return Factory.CoordinateEqualityComparer.Equals(other.Coordinate, Coordinate, tolerance);
        }


        /// <inheritdoc />
        public override void Apply(ICoordinateFilter filter)
        {
            if (IsEmpty)
                return;
            filter.Filter(Coordinate);
        }

        /// <inheritdoc />
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (IsEmpty)
                return;
            filter.Filter(_coordinates, 0);
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <inheritdoc />
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (_coordinates.Count == 0)
            {
                return;
            }

            filter.Filter(_coordinates);

            if (filter.GeometryChanged)
            {
                GeometryChanged();
            }
        }

        /// <inheritdoc />
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        /// <inheritdoc />
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()
        {
            var coordinates = _coordinates.Copy();
            return new Point(coordinates, Factory);
        }

        /// <inheritdoc cref="Geometry.Reverse"/>
        [Obsolete("Call Geometry.Reverse()")]
#pragma warning disable 809
        public override Geometry Reverse()
        {
            return base.Reverse();
        }
#pragma warning restore 809

        /// <summary>
        /// The actual implementation of the <see cref="Geometry.Reverse"/> function for <c>POINT</c>s.
        /// </summary>
        /// <returns>A reversed geometry</returns>
        protected override Geometry ReverseInternal()
        {
            return Factory.CreatePoint(_coordinates.Copy());
        }

        /// <inheritdoc cref="Normalize"/>
        public override void Normalize() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object other)
        {
            var point = (Point)  other;
            return Coordinate.CompareTo(point.Coordinate);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object other, IComparer<CoordinateSequence> comparer)
        {
            return comparer.Compare(CoordinateSequence, ((Point) other).CoordinateSequence);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> set to </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(double x, double y, double z) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(new Coordinate[] { new CoordinateZ(x, y, z) }), DefaultFactory) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> set to </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(double x, double y)
            : this(DefaultFactory.CoordinateSequenceFactory.Create(new Coordinate[] { new Coordinate(x, y) }), DefaultFactory) { }

        /// <summary>
        ///
        /// </summary>
        public double Z
        {
            get
            {
                if (CoordinateSequence.Count == 0)
                    throw new ArgumentOutOfRangeException("Z called on empty Point");
                return CoordinateSequence.GetZ(0);
            }
            set
            {
                if (CoordinateSequence.Count == 0)
                    throw new ArgumentOutOfRangeException("Z called on empty Point");
                CoordinateSequence.SetZ(0, value);
            }
        }

        /* END ADDED BY MPAUL42: monoGIS team */

        public double M
        {
            get
            {
                if (CoordinateSequence.Count == 0)
                    throw new ArgumentOutOfRangeException("M called on empty Point");
                return CoordinateSequence.GetM(0);
            }
            set
            {
                if (CoordinateSequence.Count == 0)
                    throw new ArgumentOutOfRangeException("Z called on empty Point");
                CoordinateSequence.SetM(0, value);
            }
        }
    }
}
