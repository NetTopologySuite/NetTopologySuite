using System;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// The internal representation of a list of coordinates inside a Geometry.
    /// <para>
    /// This allows Geometries to store their
    /// points using something other than the NTS <see cref="Coordinate"/> class.
    /// For example, a storage-efficient implementation
    /// might store coordinate sequences as an array of x's
    /// and an array of y's.
    /// Or a custom coordinate class might support extra attributes like M-values.
    /// </para>
    /// <para>
    /// Implementing a custom coordinate storage structure
    /// requires implementing the <see cref="CoordinateSequence"/> and
    /// <see cref="CoordinateSequenceFactory"/> abstract classes.
    /// To use the custom CoordinateSequence, create a
    /// new <see cref="GeometryFactory"/> parameterized by the CoordinateSequenceFactory
    /// The <see cref="GeometryFactory"/> can then be used to create new <see cref="Geometry"/>s.
    /// The new Geometries will use the custom CoordinateSequence implementation.
    /// </para>
    /// </summary>
    ///// <remarks>
    ///// For an example see <see cref="ExtendedCoordinateSample"/>
    ///// </remarks>
    ///// <seealso cref="NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory"/>
    ///// <seealso cref="NetTopologySuite.Geometries.Implementation.ExtendedCoordinateExample"/>
    ///// <seealso cref="NetTopologySuite.Geometries.Implementation.PackedCoordinateSequenceFactory"/>
    [Serializable]
    public abstract class CoordinateSequence
    {
        private readonly Ordinates? _ordinates;

        private readonly int _zIndex = -1;

        private readonly int _mIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSequence"/> class.
        /// </summary>
        /// <param name="count">The value for <see cref="Count"/>.</param>
        /// <param name="dimension">The value for <see cref="Dimension"/>.</param>
        /// <param name="measures">The value for <see cref="Measures"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any argument is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="dimension"/> and <paramref name="measures"/> specify fewer
        /// than two (2) spatial dimensions.
        /// </exception>
        protected CoordinateSequence(int count, int dimension, int measures)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must be non-negative.");
            }

            if (dimension < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Must be non-negative");
            }

            if (measures < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(measures), measures, "Must be non-negative");
            }

            if (dimension - measures < 2)
            {
                throw new ArgumentException("Must have at least two spatial dimensions.");
            }

            Count = count;
            Dimension = dimension;
            Measures = measures;

            // Z is the first non-measure dimension after X and Y, if any.
            if (dimension - measures > 2)
            {
                _zIndex = 2;
            }

            if (measures > 0)
            {
                // JTS convention defines measures to immediately follow spatial dimensions.
                _mIndex = dimension - measures;
            }

            // treat the Ordinates enum as safely as we can, and only set it for XY, XYZ, XYM, and
            // XYZM sequences.  we can't unambiguously talk about any other kinds of sequences,
            // especially (but not exclusively) those with measures > 1, because Ordinate.Ordinate4
            // is not defined as either a spatial or measure dimension.
            switch (dimension)
            {
                case 2:
                    _ordinates = Ordinates.XY;
                    break;

                case 3 when measures == 0:
                    _ordinates = Ordinates.XYZ;
                    break;

                case 3 when measures == 1:
                    _ordinates = Ordinates.XYM;
                    break;

                case 4 when measures == 1:
                    _ordinates = Ordinates.XYZM;
                    break;
            }
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// <para>
        /// This total includes any measures, indicated by non-zero <see cref="Measures"/>.
        /// </para>
        /// </summary>
        public int Dimension { get; }

        /// <summary>
        /// Gets the number of measures included in <see cref="Dimension"/> for each coordinate for this
        /// sequence.
        /// </summary>
        /// <remarks>
        /// For a measured coordinate sequence a non-zero value is returned.
        /// <list type="Bullet">
        /// <item>For <see cref="Geometries.Ordinates.XY"/> sequence measures is zero</item>
        /// <item>For <see cref="Geometries.Ordinates.XYM"/> sequence measure is one</item>
        /// <item>For <see cref="Geometries.Ordinates.XYZ"/> sequence measure is zero</item>
        /// <item>For <see cref="Geometries.Ordinates.XYZM"/> sequence measure is one</item>
        /// <item>Values greater than one are supported</item>
        /// </list>
        /// </remarks>
        public int Measures { get; }

        /// <summary>
        /// Gets the kind of ordinates this sequence supplies.
        /// </summary>
        public Ordinates Ordinates
        {
            get
            {
                if (!_ordinates.HasValue)
                {
                    ThrowForUnusualSequence();
                }

                return _ordinates.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets a value indicating if <see cref="GetZ(int)"/> is supported.
        /// </summary>
        public bool HasZ => _zIndex >= 0;

        public int ZOrdinateIndex => HasZ ? _zIndex : throw new InvalidOperationException("Z ordinate is not available.");

        /// <summary>
        /// Gets a value indicating if <see cref="GetM(int)"/> is supported.
        /// </summary>
        public bool HasM => _mIndex >= 0;

        public int MOrdinateIndex => HasM ? _mIndex : throw new InvalidOperationException("M ordinate is not available.");

        /// <summary>
        /// Creates a coordinate for use in this sequence.
        /// </summary>
        /// <remarks>
        /// The coordinate is created supporting the same number of <see cref="Dimension"/> and <see cref="Measures"/>
        /// as this sequence and is suitable for use with <see cref="GetCoordinate(int, Coordinate)"/>.
        /// </remarks>
        /// <returns>A coordinate for use with this sequence</returns>
        public virtual Coordinate CreateCoordinate() => Coordinates.Create(Dimension, Measures);

        /// <summary>
        /// Returns (possibly a copy of) the ith Coordinate in this collection.
        /// Whether or not the Coordinate returned is the actual underlying
        /// Coordinate or merely a copy depends on the implementation.
        /// Note that in the future the semantics of this method may change
        /// to guarantee that the Coordinate returned is always a copy. Callers are
        /// advised not to assume that they can modify a CoordinateSequence by
        /// modifying the Coordinate returned by this method.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual Coordinate GetCoordinate(int i) => GetCoordinateCopy(i);

        /// <summary>
        /// Returns a copy of the i'th coordinate in this sequence.
        /// This method optimizes the situation where the caller is
        /// going to make a copy anyway - if the implementation
        /// has already created a new Coordinate object, no further copy is needed.
        /// </summary>
        /// <param name="i">The index of the coordinate to retrieve.</param>
        /// <returns>A copy of the i'th coordinate in the sequence</returns>
        public virtual Coordinate GetCoordinateCopy(int i)
        {
            var result = CreateCoordinate();
            GetCoordinate(i, result);
            return result;
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// At least the first two dimensions <b>must</b> be copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public virtual void GetCoordinate(int index, Coordinate coord)
        {
            if (coord == null)
            {
                throw new ArgumentNullException(nameof(coord));
            }

            coord.X = GetX(index);
            coord.Y = GetY(index);
            if (HasZ)
            {
                coord.Z = GetZ(index);
            }

            if (HasM)
            {
                coord.M = GetM(index);
            }
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The value of the X ordinate in the index'th coordinate.</returns>
        public virtual double GetX(int index) => GetOrdinate(index, 0);

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The value of the Y ordinate in the index'th coordinate.</returns>
        public virtual double GetY(int index) => GetOrdinate(index, 1);

        /// <summary>
        /// Returns ordinate Z of the specified coordinate if available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Z ordinate in the index'th coordinate, or
        /// <see cref="Coordinate.NullOrdinate"/> if not defined.
        /// </returns>
        public virtual double GetZ(int index) => _zIndex < 0 ? Coordinate.NullOrdinate : GetOrdinate(index, _zIndex);

        /// <summary>
        /// Returns ordinate M of the specified coordinate if available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the M ordinate in the index'th coordinate, or
        /// <see cref="Coordinate.NullOrdinate"/> if not defined.
        /// </returns>
        public virtual double GetM(int index) => _mIndex < 0 ? Coordinate.NullOrdinate : GetOrdinate(index, _mIndex);

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// <para/>
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure
        /// values as described by <see cref="Dimension"/> and <see cref="Measures"/>.
        /// </summary>
        /// <remarks>
        /// If the sequence does not provide value for the required ordinate, the implementation <b>must not</b> throw an exception, it should return <see cref="Coordinate.NullOrdinate"/>.
        /// </remarks>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns>The ordinate value, or <see cref="Coordinate.NullOrdinate"/> if the sequence does not provide values for <paramref name="ordinateIndex"/>"/></returns>
        public abstract double GetOrdinate(int index, int ordinateIndex);

        /// <summary>
        /// Gets a value indicating the number of coordinates in this sequence.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <remarks>
        /// If the sequence can't store the ordinate value, the implementation <b>must not</b> throw an exception, it should simply ignore the call.
        /// </remarks>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>
        public abstract void SetOrdinate(int index, int ordinateIndex, double value);

        /// <summary>
        /// Returns (possibly copies of) the Coordinates in this collection.
        /// Whether or not the Coordinates returned are the actual underlying
        /// Coordinates or merely copies depends on the implementation. Note that
        /// if this implementation does not store its data as an array of Coordinates,
        /// this method will incur a performance penalty because the array needs to
        /// be built from scratch.
        /// </summary>
        /// <returns></returns>
        public virtual Coordinate[] ToCoordinateArray()
        {
            var result = new Coordinate[Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GetCoordinate(i);
            }

            return result;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public virtual Envelope ExpandEnvelope(Envelope env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            for (int i = 0; i < Count; i++)
            {
                env.ExpandToInclude(GetX(i), GetY(i));
            }

            return env;
        }

        /// <summary>
        /// Returns a deep copy of this collection.
        /// </summary>
        /// <returns>A copy of the coordinate sequence containing copies of all points</returns>
        public abstract CoordinateSequence Copy();

        public virtual CoordinateSequence Reversed() => new ReversedCoordinateSequence(Copy() ?? throw new InvalidOperationException("Cannot reverse a coordinate sequence whose Copy() method returns null."));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForUnusualSequence() => throw new InvalidOperationException("Ordinate(s) enum methods are only supported on XY, XYZ, XYM, and XYZM sequences.");

        [Serializable]
        private sealed class ReversedCoordinateSequence : CoordinateSequence
        {
            private readonly CoordinateSequence _inner;

            public ReversedCoordinateSequence(CoordinateSequence inner)
                : base(inner.Count, inner.Dimension, inner.Measures)
            {
                _inner = inner;
            }

            public override Coordinate CreateCoordinate() => _inner.CreateCoordinate();

            public override Coordinate GetCoordinate(int i) => _inner.GetCoordinate(Count - i - 1);

            public override Coordinate GetCoordinateCopy(int i) => _inner.GetCoordinateCopy(Count - i - 1);

            public override void GetCoordinate(int index, Coordinate coord) => _inner.GetCoordinate(Count - index - 1, coord);

            public override double GetX(int index) => _inner.GetX(Count - index - 1);

            public override double GetY(int index) => _inner.GetY(Count - index - 1);

            public override double GetZ(int index) => _inner.GetZ(Count - index - 1);

            public override double GetM(int index) => _inner.GetM(Count - index - 1);

            public override double GetOrdinate(int index, int ordinateIndex) => _inner.GetOrdinate(Count - index - 1, ordinateIndex);

            public override void SetOrdinate(int index, int ordinateIndex, double value) => _inner.SetOrdinate(Count - index - 1, ordinateIndex, value);

            public override Coordinate[] ToCoordinateArray()
            {
                // can't rely on _inner.ToCoordinateArray(), because it might return a reference to
                // an underlying array.
                var result = new Coordinate[Count];
                for (int i = 0, j = result.Length - 1; i < result.Length; i++, j--)
                {
                    result[i] = _inner.GetCoordinate(j);
                }

                return result;
            }

            public override Envelope ExpandEnvelope(Envelope env) => _inner.ExpandEnvelope(env);

            public override CoordinateSequence Copy() => new ReversedCoordinateSequence(_inner.Copy());

            public override CoordinateSequence Reversed() => _inner.Copy();
        }
    }
}
