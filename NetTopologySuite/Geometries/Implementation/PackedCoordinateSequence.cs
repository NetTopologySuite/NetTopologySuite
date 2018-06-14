using System;
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
using System.Runtime.Serialization;
#endif
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public abstract class PackedCoordinateSequence : ICoordinateSequence
    {
        /// <summary>
        /// A soft reference to the Coordinate[] representation of this sequence.
        /// Makes repeated coordinate array accesses more efficient.
        /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
        [NonSerialized]
#endif
        protected WeakReference CoordRef;

        /// <summary>
        /// The dimensions of the coordinates hold in the packed array
        /// </summary>
        private int _dimension;

        /// <summary>
        /// The ordinates
        /// </summary>
        private Ordinates _ordinates;

        private static Ordinates DimensionToOrdinate(int dimension)
        {
            switch (dimension)
            {
                case 2:
                    return Ordinates.XY;
                case 3:
                    return Ordinates.XYZ;
                case 4:
                    return Ordinates.XYZM;
                default:
                    var flag = Ordinates.None;
                    for (int i = 3; i < dimension; i++)
                        flag |= (Ordinates) (1 << i);
                    return Ordinates.XY | flag;
            }
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension
        {
            get => _dimension;
            protected set
            {
                _dimension = value;
                _ordinates = DimensionToOrdinate(_dimension);
            }
        }

        public Ordinates Ordinates => _ordinates;

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
        public abstract int Count { get; }

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
        public Coordinate GetCoordinate(int i)
        {
            var arr = GetCachedCoords();
            if(arr != null)
                 return arr[i];
            return GetCoordinateInternal(i);
        }

        /// <summary>
        /// Returns a copy of the i'th coordinate in this sequence.
        /// This method optimizes the situation where the caller is
        /// going to make a copy anyway - if the implementation
        /// has already created a new Coordinate object, no further copy is needed.
        /// </summary>
        /// <param name="i">The index of the coordinate to retrieve.</param>
        /// <returns>
        /// A copy of the i'th coordinate in the sequence
        /// </returns>
        public Coordinate GetCoordinateCopy(int i)
        {
            return GetCoordinateInternal(i);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="i">The index of the coordinate to copy.</param>
        /// <param name="c">A Coordinate to receive the value.</param>
        public void GetCoordinate(int i, Coordinate c)
        {
            c.X = GetOrdinate(i, Ordinate.X);
            c.Y = GetOrdinate(i, Ordinate.Y);
        }

        /// <summary>
        /// Returns (possibly copies of) the Coordinates in this collection.
        /// Whether or not the Coordinates returned are the actual underlying
        /// Coordinates or merely copies depends on the implementation.
        /// Note that if this implementation does not store its data as an array of Coordinates,
        /// this method will incur a performance penalty because the array needs to
        /// be built from scratch.
        /// </summary>
        /// <returns></returns>
        public Coordinate[] ToCoordinateArray()
        {
            var arr = GetCachedCoords();
            // testing - never cache
            if (arr != null)
                return arr;

            arr = new Coordinate[Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = GetCoordinateInternal(i);

            CoordRef = new WeakReference(arr);
            return arr;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Coordinate[] GetCachedCoords()
        {
            var localCoordRef = CoordRef;
            if (localCoordRef != null)
            {
                var arr = (Coordinate[]) localCoordRef.Target;
                if (arr != null)
                    return arr;

                CoordRef = null;
                return null;
            }
            return null;
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public double GetX(int index)
        {
            return GetOrdinate(index, Ordinate.X);
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public double GetY(int index)
        {
            return GetOrdinate(index, Ordinate.Y);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public abstract double GetOrdinate(int index, Ordinate ordinate);

        /// <summary>
        /// Sets the first ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetX(int index, double value)
        {
            CoordRef = null;
            SetOrdinate(index, Ordinate.X, value);
        }

        /// <summary>
        /// Sets the second ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetY(int index, double value)
        {
            CoordRef = null;
            SetOrdinate(index, Ordinate.Y, value);
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinate">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked.
        /// If it is larger than the dimension a meaningless value may be returned.
        /// </remarks>
        public abstract void SetOrdinate(int index, Ordinate ordinate, double value);

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return CoordinateSequences.ToString(this);
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract Coordinate GetCoordinateInternal(int index);

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        [Obsolete("Use Copy()")]
        public abstract object Clone();

        public abstract ICoordinateSequence Copy();

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public abstract Envelope ExpandEnvelope(Envelope env);

        public abstract ICoordinateSequence Reversed();

#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
        [OnDeserialized]
        private void OnDeserialization(StreamingContext context)
        {
            CoordRef = null;
        }
#endif
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on doubles.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class PackedDoubleCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private readonly double[] _coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="dimensions"></param>
        public PackedDoubleCoordinateSequence(double[] coords, int dimensions)
        {
            if (dimensions < 2)
                throw new ArgumentException("Must have at least 2 dimensions");

            if (coords.Length % dimensions != 0)
                throw new ArgumentException("Packed array does not contain " +
                    "an integral number of coordinates");

            Dimension = dimensions;
            _coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimensions"></param>
        public PackedDoubleCoordinateSequence(float[] coordinates, int dimensions)
        {
            _coords = new double[coordinates.Length];
            Dimension = dimensions;
            for (int i = 0; i < coordinates.Length; i++)
                _coords[i] = coordinates[i];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        public PackedDoubleCoordinateSequence(Coordinate[] coordinates, int dimension)
        {
            if (coordinates == null)
                coordinates = new Coordinate[0];
            Dimension = dimension;

            _coords = new double[coordinates.Length * Dimension];
            for (int i = 0; i < coordinates.Length; i++)
            {
                _coords[i * Dimension] = coordinates[i].X;
                if (Dimension >= 2)
                    _coords[i * Dimension + 1] = coordinates[i].Y;
                if (Dimension >= 3)
                    _coords[i * Dimension + 2] = coordinates[i].Z;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        public PackedDoubleCoordinateSequence(Coordinate[] coordinates) : this(coordinates, 3) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        public PackedDoubleCoordinateSequence(int size, int dimension)
        {
            Dimension = dimension;
            _coords = new double[size * Dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Coordinate GetCoordinateInternal(int index)
        {
            double x = _coords[index * Dimension];
            double y = _coords[index * Dimension + 1];
            double z = Dimension == 2 ? Coordinate.NullOrdinate : _coords[index * Dimension + 2];
            return new Coordinate(x, y, z);
        }

        /// <summary>
        /// Gets the underlying array containing the coordinate values.
        /// </summary>
        /// <returns>The array of coordinate values</returns>
        public double[] GetRawCoordinates()
        {
            return _coords;
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
        public override int Count => _coords.Length / Dimension;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        [Obsolete]
        public override object Clone()
        {
            return Copy();

        }

        /// <inheritdoc cref="ICoordinateSequence.Copy"/>
        public override ICoordinateSequence Copy()
        {
            double[] clone = new double[_coords.Length];
            Array.Copy(_coords, clone, _coords.Length);
            return new PackedDoubleCoordinateSequence(clone, Dimension);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <remarks>
        /// Beware, for performance reasons the ordinate index is not checked, if
        /// it's over dimensions you may not get an exception but a meaningless
        /// value.
        /// </remarks>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, Ordinate ordinate)
        {
            return _coords[index * Dimension + (int) ordinate];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinate">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked.
        /// If it is larger than the dimension a meaningless value may be returned.
        /// </remarks>
        public override void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            CoordRef = null;
            _coords[index * Dimension + (int) ordinate] = value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override Envelope ExpandEnvelope(Envelope env)
        {
            int dim = Dimension;
            for (int i = 0; i < _coords.Length; i += dim)
                env.ExpandToInclude(_coords[i], _coords[i + 1]);
            return env;
        }

        public override ICoordinateSequence Reversed()
        {
            int dim = Dimension;
            double[] coords = new double[_coords.Length];
            int j = Count;
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(_coords, i * dim * sizeof(double), coords, --j * dim * sizeof(double), dim * sizeof(double));
            }
            return new PackedDoubleCoordinateSequence(coords, dim);
        }
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on floats.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class PackedFloatCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private readonly float[] _coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="dimensions"></param>
        public PackedFloatCoordinateSequence(float[] coords, int dimensions)
        {
            if (dimensions < 2)
                throw new ArgumentException("Must have at least 2 dimensions");

            if (coords.Length % dimensions != 0)
                throw new ArgumentException("Packed array does not contain " +
                    "an integral number of coordinates");

            Dimension = dimensions;
            _coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimensions"></param>
        public PackedFloatCoordinateSequence(double[] coordinates, int dimensions)
        {
            _coords = new float[coordinates.Length];
            Dimension = dimensions;
            for (int i = 0; i < coordinates.Length; i++)
                _coords[i] = (float) coordinates[i];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        public PackedFloatCoordinateSequence(Coordinate[] coordinates, int dimension)
        {
            if (coordinates == null)
                coordinates = new Coordinate[0];
            Dimension = dimension;

            _coords = new float[coordinates.Length * Dimension];
            for (int i = 0; i < coordinates.Length; i++)
            {
                _coords[i * Dimension] = (float) coordinates[i].X;
                if (Dimension >= 2)
                    _coords[i * Dimension + 1] = (float) coordinates[i].Y;
                if (Dimension >= 3)
                _coords[i * Dimension + 2] = (float) coordinates[i].Z;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        public PackedFloatCoordinateSequence(int size, int dimension)
        {
            Dimension = dimension;
            _coords = new float[size * Dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Coordinate GetCoordinateInternal(int index)
        {
            double x = _coords[index * Dimension];
            double y = _coords[index * Dimension + 1];
            double z = Dimension == 2 ? 0.0 : _coords[index * Dimension + 2];
            return new Coordinate(x, y, z);
        }

        /// <summary>
        /// Gets the underlying array containing the coordinate values.
        /// </summary>
        /// <returns>The array of coordinate values</returns>
        public float[] GetRawCoordinates()
        {
            return _coords;
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
        public override int Count => _coords.Length / Dimension;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        [Obsolete]
        public override object Clone()
        {
            return Copy();

        }

        /// <inheritdoc cref="ICoordinateSequence.Copy"/>
        public override ICoordinateSequence Copy()
        {
            float[] clone = new float[_coords.Length];
            Array.Copy(_coords, clone, _coords.Length);
            return new PackedFloatCoordinateSequence(clone, Dimension);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <remarks>
        /// Beware, for performance reasons the ordinate index is not checked, if
        /// it's over dimensions you may not get an exception but a meaningless
        /// value.
        /// </remarks>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, Ordinate ordinate)
        {
            return _coords[index * Dimension + (int) ordinate];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinate">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked:
        /// if it is over dimensions you may not get an exception but a meaningless value.
        /// </remarks>
        public override void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            CoordRef = null;
            _coords[index * Dimension + (int) ordinate] = (float) value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override Envelope ExpandEnvelope(Envelope env)
        {
        for (int i = 0; i < _coords.Length; i += Dimension )
            env.ExpandToInclude(_coords[i], _coords[i + 1]);
        return env;
        }

        public override ICoordinateSequence Reversed()
        {
            int dim = Dimension;
            float[] coords = new float[_coords.Length];
            int j = Count;
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(_coords, i * dim * sizeof(float), coords, --j * dim * sizeof(float), dim * sizeof(float));
            }
            return new PackedDoubleCoordinateSequence(coords, dim);
        }

    }
}
