using System;
using System.Collections.Generic;
using NetTopologySuite.Operation;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Models an OGC-style <c>LineString</c>
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
    /// <para>A <c>LineString</c> must have either 0 or <see cref="MinimumValidSize"/> or more points.
    /// If these conditions are not met, the constructors throw an <see cref="ArgumentException"/>.
    /// </para>
    /// </remarks>
    [Serializable]
    public class LineString : Geometry, ILineal
    {
        /// <summary>
        /// The minimum number of vertices allowed in a valid non-empty linestring.
        /// Empty linestrings with 0 vertices are also valid.
        /// </summary>
        public const int MinimumValidSize = 2;

        /// <summary>
        /// Represents an empty <c>LineString</c>.
        /// </summary>
        public static readonly LineString Empty = new GeometryFactory().CreateLineString(new Coordinate[] { });

        /// <summary>
        /// The points of this <c>LineString</c>.
        /// </summary>
        private CoordinateSequence _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineString"/> class.
        /// </summary>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        /// <param name="points">The coordinates used for create this <see cref="LineString" />.</param>
        /// <exception cref="ArgumentException">If too few points are provided</exception>
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
        public LineString(CoordinateSequence points, GeometryFactory factory)
            : base(factory)
        {
            if (points == null)
                points = factory.CoordinateSequenceFactory.Create(new Coordinate[] { });
            if (points.Count == 1)
                throw new ArgumentException($"Invalid number of points in LineString (found {points.Count} - must be 0 or >= {MinimumValidSize})");
            _points = points;
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.LineString;

        /// <summary>
        /// Returns an array containing the values of all the vertices for
        /// this geometry.
        /// </summary>
        /// <remarks>
        /// If the geometry is a composite, the array will contain all the vertices
        /// for the components, in the order in which the components occur in the geometry.
        /// <para>
        /// In general, the array cannot be assumed to be the actual internal
        /// storage for the vertices.  Thus modifying the array
        /// may not modify the geometry itself.
        /// Use the <see cref="Geometries.CoordinateSequence.SetOrdinate(int, int, double)"/> method
        /// (possibly on the components) to modify the underlying data.
        /// If the coordinates are modified,
        /// <see cref="Geometry.GeometryChanged"/> must be called afterwards.
        /// </para>
        /// </remarks>
        /// <returns>The vertices of this <c>Geometry</c>.</returns>
        /// <seealso cref="Geometry.GeometryChanged"/>
        /// <seealso cref="Geometries.CoordinateSequence.SetOrdinate(int, int, double)"/>
        /// <seealso cref="Geometries.CoordinateSequence.SetOrdinate(int, Ordinate, double)"/>
        public override Coordinate[] Coordinates => _points.ToCoordinateArray();

        /// <summary>
        /// Gets an array of <see cref="double"/> ordinate values
        /// </summary>
        /// <param name="ordinate">The ordinate index</param>
        /// <returns>An array of ordinate values</returns>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty)
                return new double[0];

            var ordinateFlag = (Ordinates)(1 << (int)ordinate);
            if ((_points.Ordinates & ordinateFlag) != ordinateFlag)
                return CreateArray(_points.Count, Coordinate.NullOrdinate);

            return CreateArray(_points, ordinate);
        }

        /// <summary>
        ///
        /// </summary>
        public CoordinateSequence CoordinateSequence => _points;

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
        /// Gets 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Point GetPointN(int n)
        {
            return Factory.CreatePoint(_points.GetCoordinate(n));
        }

        /// <summary>
        /// Gets a value indicating the start point of this <c>LINESTRING</c>
        /// </summary>
        public Point StartPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return GetPointN(0);
            }
        }

        /// <summary>
        /// Gets a value indicating the end point of this <c>LINESTRING</c>
        /// </summary>
        public Point EndPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return GetPointN(NumPoints - 1);
            }
        }

        /// <summary>
        /// Gets a value indicating if this <c>LINESTRING</c> is closed.
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
        /// Gets a value indicating if this <c>LINESTRING</c> forms a ring.
        /// </summary>
        public bool IsRing => IsClosed && IsSimple;

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"LineString"</returns>
        public override string GeometryType => Geometry.TypeNameLineString;

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.LineString;

        /// <summary>
        /// Returns the length of this <c>LineString</c>
        /// </summary>
        /// <returns>The length of the polygon.</returns>
        public override double Length => Algorithm.Length.OfLine(_points);

        /// <summary>
        /// Returns the boundary, or an empty geometry of appropriate dimension
        /// if this <c>Geometry</c> is empty.
        /// For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <c>Geometry</c>.</returns>
        public override Geometry Boundary => (new BoundaryOp(this)).GetBoundary();

        /// <summary>
        /// Creates a <see cref="LineString" /> whose coordinates are in the reverse order of this objects.
        /// </summary>
        /// <returns>A <see cref="LineString" /> with coordinates in the reverse order.</returns>
        [Obsolete("Call Geometry.Reverse()")]
#pragma warning disable 809
        public override Geometry Reverse()
        {
            return base.Reverse();
        }
#pragma warning restore 809

        /// <summary>
        /// The actual implementation of the <see cref="Geometry.Reverse"/> function for <c>LINESTRING</c>s.
        /// </summary>
        /// <returns>A reversed geometry</returns>
        protected override Geometry ReverseInternal()
        {
            var seq = _points.Copy();
            CoordinateSequences.Reverse(seq);
            return Factory.CreateLineString(seq);
        }

        /// <summary>
        /// Returns true if the given point is a vertex of this <c>LineString</c>.
        /// </summary>
        /// <param name="pt">The <c>Coordinate</c> to check.</param>
        /// <returns><c>true</c> if <c>pt</c> is one of this <c>LineString</c>'s vertices.</returns>
        public virtual bool IsCoordinate(Coordinate pt)
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
            return new Envelope(_points);

            /*
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
             */
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

            var otherLineString = (LineString)other;
            if (_points.Count != otherLineString.NumPoints)
                return false;

            var cec = Factory.CoordinateEqualityComparer;
            for (int i = 0; i < _points.Count; i++)
            {
                if (!cec.Equals(_points.GetCoordinate(i), otherLineString.GetCoordinateN(i), tolerance))
                    return false;
            }

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

        /// <summary>
        /// Performs an operation on the coordinates in this <c>Geometry</c>'s <see cref="Geometries.CoordinateSequence"/>s.
        /// </summary>
        /// <remarks>
        /// If the filter reports that a coordinate value has been changed,
        /// <see cref="Geometry.GeometryChanged"/> will be called automatically.
        /// </remarks>
        /// <param name="filter">The filter to apply</param>
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

        /// <inheritdoc />
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (_points.Count == 0)
            {
                return;
            }

            filter.Filter(_points);

            if (filter.GeometryChanged)
            {
                GeometryChanged();
            }
        }

        /// <summary>
        /// Performs an operation with or on this <c>Geometry</c> and its
        /// subelement <c>Geometry</c>s (if any).
        /// Only GeometryCollections and subclasses
        /// have subelement Geometry's.
        /// </summary>
        /// <param name="filter">
        /// The filter to apply to this <c>Geometry</c> (and
        /// its children, if it is a <c>GeometryCollection</c>).
        /// </param>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        /// <summary>
        /// Performs an operation with or on this Geometry and its
        /// component Geometry's. Only GeometryCollections and
        /// Polygons have component Geometry's; for Polygons they are the LinearRings
        /// of the shell and holes.
        /// </summary>
        /// <param name="filter">The filter to apply to this <c>Geometry</c>.</param>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()

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

        /// <inheritdoc cref="Geometry.IsEquivalentClass"/>
        protected override bool IsEquivalentClass(Geometry other)
        {
            return other is LineString;
        }

        /// <inheritdoc cref="Geometry.CompareToSameClass(object)"/>
        protected internal override int CompareToSameClass(object o)
        {
            Assert.IsTrue(o is LineString);

            var line = (LineString)o;
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

        /// <inheritdoc cref="Geometry.CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            Assert.IsTrue(o is LineString);
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
                _points.SetOrdinate(n, 0, value.X);
                _points.SetOrdinate(n, 1, value.Y);
                _points.SetOrdinate(n, 2, value.Z);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public int Count => _points.Count;

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
