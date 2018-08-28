using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Operation;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Models an OGC-style <code>LineString</code>
    /// </summary>
    /// <remarks>
    /// A LineString consists of a sequence of two or more vertices,
    /// along with all points along the linearly-interpolated curves
    /// (line segments) between each
    /// pair of consecutive vertices.
    /// Consecutive vertices may be equal.
    /// The line segments in the line may intersect each other (in other words,
    /// the <c>LineString</c> may "curl back" in itself and self-intersect.
    /// <c>LineString</c>s with exactly two identical points are invalid.
    /// <para>A <c>LineString</c> must have either 0 or 2 or more points.
    /// If these conditions are not met, the constructors throw an <see cref="ArgumentException"/>.
    /// </para>
    /// </remarks>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class LineString : Geometry, ILineString
    {

        /// <summary>
        /// Represents an empty <c>LineString</c>.
        /// </summary>
        public static readonly ILineString Empty = new GeometryFactory().CreateLineString(new Coordinate[] { });

        /// <summary>
        /// The points of this <c>LineString</c>.
        /// </summary>
        private ICoordinateSequence _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineString"/> class.
        /// </summary>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        /// <param name="points">The coordinates used for create this <see cref="LineString" />.</param>
        /// <exception cref="ArgumentException">If too few points are provided</exception>
        //[Obsolete("Use GeometryFactory instead")]
        public LineString(Coordinate[] points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineString"/> class.
        /// </summary>
        /// <param name="points">
        /// The points of the <c>LineString</c>, or <c>null</c>
        /// to create the empty point. Consecutive points may not be equal.
        /// </param>
        /// <param name="factory"></param>
        /// <exception cref="ArgumentException">If too few points are provided</exception>
        public LineString(ICoordinateSequence points, IGeometryFactory factory)
            : base(factory)
        {
            if (points == null)
                points = factory.CoordinateSequenceFactory.Create(new Coordinate[] { });
            if (points.Count == 1)
                throw new ArgumentException("Invalid number of points in LineString (found "
                                            + points.Count + " - must be 0 or >= 2)");
            _points = points;
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        protected override SortIndexValue SortIndex => SortIndexValue.LineString;

        /// <summary>
        ///
        /// </summary>
        public override Coordinate[] Coordinates => _points.ToCoordinateArray();

        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty)
                return new double[0];

            var ordinateFlag = OrdinatesUtility.ToOrdinatesFlag(ordinate);
            if ((_points.Ordinates & ordinateFlag) != ordinateFlag)
                return CreateArray(_points.Count, Coordinate.NullOrdinate);

            return CreateArray(_points, ordinate);
        }

        /// <summary>
        ///
        /// </summary>
        public ICoordinateSequence CoordinateSequence => _points;

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Coordinate GetCoordinateN(int n)
        {
            return _points.GetCoordinate(n);
        }

        /// <summary>
        ///
        /// </summary>
        public override Coordinate Coordinate
        {
            get
            {
                if (IsEmpty) return null;
                return _points.GetCoordinate(0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override Dimension Dimension => Dimension.Curve;

        /// <summary>
        ///
        /// </summary>
        public override Dimension BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return Dimension.False;
                }
                return Dimension.Point;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override bool IsEmpty => _points.Count == 0;

        /// <summary>
        ///
        /// </summary>
        public override int NumPoints => _points.Count;

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public IPoint GetPointN(int n)
        {
            return Factory.CreatePoint(_points.GetCoordinate(n));
        }

        /// <summary>
        ///
        /// </summary>
        public IPoint StartPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return GetPointN(0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public IPoint EndPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return GetPointN(NumPoints - 1);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public virtual bool IsClosed
        {
            get
            {
                if (IsEmpty)
                    return false;
                return GetCoordinateN(0).Equals2D(GetCoordinateN(NumPoints - 1));
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsRing => IsClosed && IsSimple;

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"LineString"</returns>
        public override string GeometryType => "LineString";

        public override OgcGeometryType OgcGeometryType => OgcGeometryType.LineString;

        /// <summary>
        /// Returns the length of this <c>LineString</c>
        /// </summary>
        /// <returns>The length of the polygon.</returns>
        public override double Length => Algorithm.Length.OfLine(_points);

        ///// <summary>
        /////
        ///// </summary>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return (new IsSimpleOp()).IsSimple(this);
        //    }
        //}

        public override IGeometry Boundary => (new BoundaryOp(this)).GetBoundary();

        /// <summary>
        /// Creates a <see cref="LineString" /> whose coordinates are in the reverse order of this objects.
        /// </summary>
        /// <returns>A <see cref="LineString" /> with coordinates in the reverse order.</returns>
        public override IGeometry Reverse()
        {
            /*
            var seq = (ICoordinateSequence)_points.Copy();
            CoordinateSequences.Reverse(seq);
             */
            var seq = _points.Reversed();
            return Factory.CreateLineString(seq);
        }

        //ILineString ILineString.Reverse()
        //{
        //    return (ILineString)Reverse();
        //}

        /// <summary>
        /// Returns true if the given point is a vertex of this <c>LineString</c>.
        /// </summary>
        /// <param name="pt">The <c>Coordinate</c> to check.</param>
        /// <returns><c>true</c> if <c>pt</c> is one of this <c>LineString</c>'s vertices.</returns>
        public bool IsCoordinate(Coordinate pt)
        {
            for (int i = 0; i < _points.Count; i++)
                if (_points.GetCoordinate(i).Equals(pt))
                    return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override Envelope ComputeEnvelopeInternal()
        {
            if (IsEmpty)
                return new Envelope();

            //Convert to array, then access array directly, to avoid the function-call overhead
            //of calling Getter millions of times. ToArray may be inefficient for
            //non-BasicCoordinateSequence CoordinateSequences. [Jon Aquino]
            var coordinates = _points.ToCoordinateArray();
            double minx = coordinates[0].X;
            double miny = coordinates[0].Y;
            double maxx = coordinates[0].X;
            double maxy = coordinates[0].Y;
            for (int i = 1; i < coordinates.Length; i++)
            {
                minx = minx < coordinates[i].X ? minx : coordinates[i].X;
                maxx = maxx > coordinates[i].X ? maxx : coordinates[i].X;
                miny = miny < coordinates[i].Y ? miny : coordinates[i].Y;
                maxy = maxy > coordinates[i].Y ? maxy : coordinates[i].Y;
            }
            return new Envelope(minx, maxx, miny, maxy);
        }

        //[Obsolete]
        //internal override int GetHashCodeInternal(int baseValue, Func<int, int> operation)
        //{
        //    if (!IsEmpty)
        //        baseValue = _points.GetHashCode(baseValue, operation);
        //    return baseValue;
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(IGeometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;

            var otherLineString = (ILineString)other;
            if (_points.Count != otherLineString.NumPoints)
                return false;

            for (int i = 0; i < _points.Count; i++)
                if (!Equal(_points.GetCoordinate(i), otherLineString.GetCoordinateN(i), tolerance))
                    return false;
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(ICoordinateFilter filter)
        {
            for (int i = 0; i < _points.Count; i++)
                filter.Filter(_points.GetCoordinate(i));
        }

        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (_points.Count == 0)
                return;
            for (int i = 0; i < _points.Count; i++)
            {
                filter.Filter(_points, i);
                if (filter.Done)
                    break;
            }
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
        }

        /// <summary>
        /// Creates and returns a full copy of this object.
        /// (including all coordinates contained by it).
        /// </summary>
        /// <returns>A copy of this instance</returns>
        [Obsolete("Use Copy()")]
        public override object Clone()
        {
            return Copy();
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override IGeometry CopyInternal()

        {
            var points = _points.Copy();
            return new LineString(points, Factory);
        }

        /// <summary>
        /// Normalizes a <c>LineString</c>.  A normalized <c>LineString</c>
        /// has the first point which is not equal to it's reflected point
        /// less than the reflected point.
        /// </summary>
        public override void Normalize()
        {
            for (int i = 0; i < _points.Count / 2; i++)
            {
                int j = _points.Count - 1 - i;
                // skip equal points on both ends
                if (!_points.GetCoordinate(i).Equals(_points.GetCoordinate(j)))
                {
                    if (_points.GetCoordinate(i).CompareTo(_points.GetCoordinate(j)) > 0)
                    {
                        var copy = _points.Copy();
                        CoordinateSequences.Reverse(copy);
                        _points = copy;
                    }
                    return;
                }
            }
        }

        protected override bool IsEquivalentClass(IGeometry other)
        {
            return other is ILineString;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object o)
        {
            Assert.IsTrue(o is ILineString);

            var line = (ILineString)o;
            // MD - optimized implementation
            int i = 0;
            int j = 0;
            while (i < _points.Count && j < line.CoordinateSequence.Count)
            {
                int comparison = _points.GetCoordinate(i).CompareTo(line.CoordinateSequence.GetCoordinate(j));
                if (comparison != 0)
                    return comparison;
                i++;
                j++;
            }
            if (i < _points.Count)
                return 1;
            if (j < line.CoordinateSequence.Count)
                return -1;
            return 0;
        }

        protected internal override int CompareToSameClass(object o, IComparer<ICoordinateSequence> comp)
        {
            Assert.IsTrue(o is ILineString);
            var line = (LineString)o;
            return comp.Compare(_points, line.CoordinateSequence);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */
        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Coordinate this[int n]
        {
            get => _points.GetCoordinate(n);
            set
            {
                _points.SetOrdinate(n, Ordinate.X, value.X);
                _points.SetOrdinate(n, Ordinate.Y, value.Y);
                _points.SetOrdinate(n, Ordinate.Z, value.Z);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public int Count => _points.Count;

        /// <summary>
        /// Returns the value of the angle between the <see cref="StartPoint" />
        /// and the <see cref="EndPoint" />.
        /// </summary>
        /// <remarks>
        /// Use <see cref="AngleUtility"/> for a more precise computation.
        /// </remarks>
        [Obsolete("Use AngleUtility")]
        public double Angle
        {
            get
            {
                double deltaX = EndPoint.X - StartPoint.X;
                double deltaY = EndPoint.Y - StartPoint.Y;
                double length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                double angleRAD = Math.Asin(Math.Abs(EndPoint.Y - StartPoint.Y) / length);
                double angle = (angleRAD * 180) / Math.PI;

                if (((StartPoint.X < EndPoint.X) && (StartPoint.Y > EndPoint.Y)) ||
                     ((StartPoint.X > EndPoint.X) && (StartPoint.Y < EndPoint.Y)))
                    angle = 360 - angle;
                return angle;
            }
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
