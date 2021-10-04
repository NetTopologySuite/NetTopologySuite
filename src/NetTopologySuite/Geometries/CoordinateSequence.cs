using System;

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
    /// <seealso cref="Implementation.CoordinateArraySequenceFactory"/>
    ///// <seealso cref="NetTopologySuite.Geometries.Implementation.ExtendedCoordinateExample"/>
    /// <seealso cref="Implementation.PackedCoordinateSequenceFactory"/>
    [Serializable]
    public abstract class CoordinateSequence
    {
        private readonly int _zIndex;

        private readonly int _mIndex;

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

            Spatial = dimension - measures;
            if (Spatial < 2)
            {
                throw new ArgumentException("Must have at least two spatial dimensions.");
            }

            Count = count;
            Dimension = dimension;
            Measures = measures;

            // Ordinates flags support only up to 16 spatial dimensions and 16 measures.  If we're
            // given something with more, then we can still handle them perfectly fine (like JTS),
            // just our Ordinate / Ordinates stuff will be limited to the first 16.
            int spatialFlags = (1 << Math.Min(Spatial, 16)) - 1;
            int measureFlags = (1 << Math.Min(measures, 16)) - 1;
            Ordinates = (Ordinates)(spatialFlags | (measureFlags << 16));

            // cache indexes for these named ordinates.
            TryGetOrdinateIndex(Ordinate.Z, out _zIndex);
            TryGetOrdinateIndex(Ordinate.M, out _mIndex);
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
        /// <list type="bullet">
        /// <item><description>For <see cref="Geometries.Ordinates.XY"/> sequence measures is zero</description></item>
        /// <item><description>For <see cref="Geometries.Ordinates.XYM"/> sequence measure is one</description></item>
        /// <item><description>For <see cref="Geometries.Ordinates.XYZ"/> sequence measure is zero</description></item>
        /// <item><description>For <see cref="Geometries.Ordinates.XYZM"/> sequence measure is one</description></item>
        /// <item><description>Values greater than one are supported</description></item>
        /// </list>
        /// </remarks>
        public int Measures { get; }

        /// <summary>
        /// Gets the number of non-measure dimensions included in <see cref="Dimension"/> for each
        /// coordinate for this sequence.
        /// <para>
        /// Equivalent to <c>Dimension - Measures</c>.
        /// </para>
        /// </summary>
        public int Spatial { get; }

        /// <summary>
        /// Gets the kind of ordinates this sequence supplies.
        /// </summary>
        public Ordinates Ordinates { get; }

        /// <summary>
        /// Gets a value indicating if <see cref="GetZ(int)"/> is supported.
        /// </summary>
        public bool HasZ => _zIndex >= 0;

        /// <summary>
        /// Gets a value indicating if <see cref="GetM(int)"/> is supported.
        /// </summary>
        public bool HasM => _mIndex >= 0;

        /// <summary>
        /// Gets the index of the Z ordinate (for use with <see cref="GetOrdinate(int, int)"/> or
        /// <see cref="SetOrdinate(int, int, double)"/>), or -1 if <see cref="HasZ"/> is
        /// <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// It's just a cache for <see cref="TryGetOrdinateIndex"/> with <see cref="Ordinate.Z"/>.
        /// </remarks>
        public int ZOrdinateIndex => _zIndex;

        /// <summary>
        /// Gets the index of the M ordinate (for use with <see cref="GetOrdinate(int, int)"/> or
        /// <see cref="SetOrdinate(int, int, double)"/>), or -1 if <see cref="HasM"/> is
        /// <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// It's just a cache for <see cref="TryGetOrdinateIndex"/> with <see cref="Ordinate.M"/>.
        /// </remarks>
        public int MOrdinateIndex => _mIndex;

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

            for (int dim = 0; dim < Dimension; dim++)
            {
                coord[dim] = GetOrdinate(index, dim);
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
        /// Sets ordinate X (0) of the specified coordinate to the specified value.
        /// </summary>
        /// <param name="index">
        /// The index of the coordinate whose X value to set.
        /// </param>
        /// <param name="value">
        /// The value to set the coordinate's X value to.
        /// </param>
        public virtual void SetX(int index, double value) => SetOrdinate(index, 0, value);

        /// <summary>
        /// Sets ordinate Y (1) of the specified coordinate to the specified value.
        /// </summary>
        /// <param name="index">
        /// The index of the coordinate whose Y value to set.
        /// </param>
        /// <param name="value">
        /// The value to set the coordinate's Y value to.
        /// </param>
        public virtual void SetY(int index, double value) => SetOrdinate(index, 1, value);

        /// <summary>
        /// Sets ordinate Z of the specified coordinate to the specified value if present.
        /// </summary>
        /// <param name="index">
        /// The index of the coordinate whose Z value to set if present.
        /// </param>
        /// <param name="value">
        /// The value to set the coordinate's Z value to if present.
        /// </param>
        public virtual void SetZ(int index, double value)
        {
            int zIndex = _zIndex;
            if (zIndex >= 0)
            {
                SetOrdinate(index, zIndex, value);
            }
        }

        /// <summary>
        /// Sets ordinate M of the specified coordinate to the specified value if present.
        /// </summary>
        /// <param name="index">
        /// The index of the coordinate whose M value to set if present.
        /// </param>
        /// <param name="value">
        /// The value to set the coordinate's M value to if present.
        /// </param>
        public virtual void SetM(int index, double value)
        {
            int mIndex = _mIndex;
            if (mIndex >= 0)
            {
                SetOrdinate(index, mIndex, value);
            }
        }

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
        /// Returns the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate value to get.</param>
        /// <returns>The ordinate value, or <see cref="Coordinate.NullOrdinate"/> if the sequence does not provide values for <paramref name="ordinate"/>"/></returns>
        public double GetOrdinate(int index, Ordinate ordinate)
        {
            return TryGetOrdinateIndex(ordinate, out int ordinateIndex)
                ? GetOrdinate(index, ordinateIndex)
                : Coordinate.NullOrdinate;
        }

        /// <summary>
        /// Gets a value indicating the first <c>Coordinate</c> in this sequence.<br/>
        /// For <c>LineString</c>s e.g. this is the starting point.
        /// </summary>
        /// <returns>First <c>Coordinate</c> in sequence or <c>null</c> if empty.</returns>
        public Coordinate First
        {
            get => Count > 0 ? GetCoordinate(0) : null;
        }

        /// <summary>
        /// Gets a value indicating the last <c>Coordinate</c> in this sequence.<br/>
        /// For <c>LineString</c>s e.g. this is the ending point.
        /// </summary>
        /// <returns>Last <c>Coordinate</c> in sequence or <c>null</c> if empty.</returns>
        public Coordinate Last
        {
            get => Count > 0 ? GetCoordinate(Count - 1) : null;
        }

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
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate value to set.</param>
        /// <param name="value">The new ordinate value.</param>
        public void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            if (TryGetOrdinateIndex(ordinate, out int ordinateIndex))
            {
                SetOrdinate(index, ordinateIndex, value);
            }
        }

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

        /// <summary>
        /// Returns a reversed copy of this <see cref="CoordinateSequence"/>.
        /// </summary>
        /// <returns>
        /// A reversed copy of this <see cref="CoordinateSequence"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="Copy"/> returned <see langword="null"/>.
        /// </exception>
        public virtual CoordinateSequence Reversed()
        {
            var copy = Copy() ?? throw new InvalidOperationException("Cannot reverse a coordinate sequence whose Copy() method returns null.");
            return new ReversedCoordinateSequence(copy);
        }

        /// <summary>
        /// Retrieves the index at which this sequence stores a particular <see cref="Ordinate"/>'s
        /// values, if that ordinate is present in <see cref="Ordinates"/>.
        /// </summary>
        /// <param name="ordinate">
        /// The <see cref="Ordinate"/> value whose index to retrieve.
        /// </param>
        /// <param name="ordinateIndex">
        /// When this method returns, contains the index of the requested ordinate, if the ordinate
        /// is present in this sequence; otherwise, -1.  This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this sequence contains <paramref name="ordinate"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetOrdinateIndex(Ordinate ordinate, out int ordinateIndex)
        {
            // use uint instead of int for comparisons, in order to handle negatives more gracefully
            if ((uint)ordinate < (uint)Ordinate.Measure1)
            {
                if ((uint)ordinate < (uint)Spatial)
                {
                    ordinateIndex = (int)ordinate;
                    return true;
                }
            }
            else if ((uint)ordinate <= (uint)Ordinate.Measure16)
            {
                ordinate -= Ordinate.Measure1;
                if ((uint)ordinate < (uint)Measures)
                {
                    ordinateIndex = (int)ordinate + Spatial;
                    return true;
                }
            }

            ordinateIndex = -1;
            return false;
        }

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
