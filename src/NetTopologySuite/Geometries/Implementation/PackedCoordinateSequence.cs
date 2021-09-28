using System;
using System.Runtime.Serialization;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// </summary>
    [Serializable]
    public abstract class PackedCoordinateSequence : CoordinateSequence
    {
        /// <summary>
        /// A soft reference to the Coordinate[] representation of this sequence.
        /// Makes repeated coordinate array accesses more efficient.
        /// </summary>
        [NonSerialized]
        protected WeakReference CoordRef;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="count">The number of <see cref="Coordinate"/>s in the sequence.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">the number of measure-ordinates each {<see cref="Coordinate"/> in this sequence has.</param>
        protected PackedCoordinateSequence(int count, int dimension, int measures)
            : base(count, dimension, measures)
        {
        }

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
        public sealed override Coordinate GetCoordinate(int i)
        {
            var arr = GetCachedCoords();
            if(arr != null)
                 return arr[i];
            return GetCoordinateCopy(i);
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
        public sealed override Coordinate[] ToCoordinateArray()
        {
            var arr = GetCachedCoords();
            // testing - never cache
            if (arr != null)
                return arr;

            arr = new Coordinate[Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = GetCoordinateCopy(i);

            CoordRef = new WeakReference(arr);
            return arr;
        }

        /// <summary>
        /// Releases the weak reference to the coordinate array.
        /// </summary>
        /// <remarks>
        /// This is necessary if you directly modify the array from <c>GetRawCoordinates</c>.
        /// </remarks>
        public void ReleaseCoordinateArray()
        {
            CoordRef = null;
        }

        private Coordinate[] GetCachedCoords()
        {
            var localCoordRef = CoordRef;
            if (localCoordRef != null)
            {
                var arr = (Coordinate[]) localCoordRef.Target;
                if (arr != null && localCoordRef.IsAlive)
                    return arr;

                CoordRef = null;
                return null;
            }
            return null;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return CoordinateSequences.ToString(this);
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index">The coordinate index</param>
        /// <returns>The <see cref="Coordinate"/> at the given index</returns>
        [Obsolete("Unused.  This will be removed in a future release.")]
        protected virtual Coordinate GetCoordinateInternal(int index)
        {
            // DEVIATION: JTS can't create Coordinate instances other than XY, XYZ, XYM, or XYZM.
            // we can, and in fact our base method is able to do so without our help, so instead of
            // channeling everything through this method, we just leave the inherited behavior alone
            // and keep this around to avoid a breaking change.
            return this.GetCoordinateCopy(index);
        }

        private protected static int GetDimensionAndMeasures(Coordinate[] coords, out int measures)
        {
            int dimension;
            (_, dimension, measures) = CoordinateSequenceFactory.GetCommonSequenceParameters(coords);
            return dimension;
        }

        [OnDeserialized]
        private void OnDeserialization(StreamingContext context)
        {
            CoordRef = null;
        }
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on doubles.
    /// </summary>
    [Serializable]
    public class PackedDoubleCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private readonly double[] _coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <c>double</c> values that contains the ordinate values of the sequence.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedDoubleCoordinateSequence(double[] coords, int dimension, int measures)
            : base(coords?.Length / dimension ?? 0, dimension, measures)
        {
            if (coords == null)
                coords = new double[0];

            if (coords.Length % dimension != 0)
                throw new ArgumentException("Packed array does not contain " +
                    "an integral number of coordinates");

            _coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <c>float</c> values that contains the ordinate values of the sequence.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedDoubleCoordinateSequence(float[] coords, int dimension, int measures)
            : base(coords?.Length / dimension ?? 0, dimension, measures)
        {
            if (coords == null)
            {
                _coords = new double[0];
                return;
            }
            _coords = new double[coords.Length];
            for (int i = 0; i < coords.Length; i++)
                _coords[i] = coords[i];
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <see cref="Coordinate"/>s.</param>
        public PackedDoubleCoordinateSequence(Coordinate[] coords)
            : this(coords, GetDimensionAndMeasures(coords, out int measures), measures)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <see cref="Coordinate"/>s.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public PackedDoubleCoordinateSequence(Coordinate[] coords, int dimension)
            : this(coords, GetDimensionAndMeasures(coords, out int measures), measures)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <see cref="Coordinate"/>s.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedDoubleCoordinateSequence(Coordinate[] coords, int dimension, int measures)
            : this(coords, dimension, measures, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size">The number of coordinates in this sequence</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedDoubleCoordinateSequence(int size, int dimension, int measures)
            : base(size, dimension, measures)
        {
            _coords = new double[size * Dimension];
        }

        internal PackedDoubleCoordinateSequence(Coordinate[] coords, int dimension, int measures, bool dimensionAndMeasuresCameFromCoords)
            : base(coords?.Length ?? 0, dimension, measures)
        {
            if (coords == null)
                coords = Array.Empty<Coordinate>();

            _coords = new double[coords.Length * Dimension];
            if (coords.Length == 0)
            {
                return;
            }

            if (dimensionAndMeasuresCameFromCoords)
            {
                for (int i = 0; i < coords.Length; i++)
                {
                    int offset = i * dimension;

                    var coord = coords[i];
                    for (int dim = 0; dim < dimension; dim++)
                    {
                        _coords[offset + dim] = coord[dim];
                    }
                }
            }
            else
            {
                _coords.AsSpan().Fill(double.NaN);

                int spatial = dimension - measures;
                for (int i = 0; i < coords.Length; i++)
                {
                    int offset = i * dimension;

                    var coord = coords[i];
                    int coordDimension = Coordinates.Dimension(coord);
                    int coordMeasures = Coordinates.Measures(coord);
                    int coordSpatial = coordDimension - coordMeasures;

                    for (int dim = 0, dimEnd = Math.Min(spatial, coordSpatial); dim < dimEnd; dim++)
                    {
                        _coords[offset + dim] = coord[dim];
                    }

                    for (int measure = 0, measureEnd = Math.Min(measures, coordMeasures); measure < measureEnd; measure++)
                    {
                        _coords[offset + spatial + measure] = coord[coordSpatial + measure];
                    }
                }
            }
        }

        /// <summary>
        /// Gets the underlying array containing the coordinate values.
        /// </summary>
        /// <returns>The array of coordinate values</returns>
        public double[] GetRawCoordinates()
        {
            return _coords;
        }

        /// <inheritdoc cref="CoordinateSequence.Copy"/>
        public override CoordinateSequence Copy()
        {
            return new PackedDoubleCoordinateSequence((double[])_coords.Clone(), Dimension, Measures);
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
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            return _coords[index * Dimension + ordinateIndex];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked.
        /// If it is larger than the dimension a meaningless value may be returned.
        /// </remarks>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            CoordRef = null;
            _coords[index * Dimension + ordinateIndex] = value;
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
            for (int i = 1; i < _coords.Length; i += dim)
                env.ExpandToInclude(_coords[i - 1], _coords[i]);
            return env;
        }

        public override CoordinateSequence Reversed()
        {
            int dim = Dimension;
            double[] coords = new double[_coords.Length];
            int j = Count;
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(_coords, i * dim * sizeof(double), coords, --j * dim * sizeof(double), dim * sizeof(double));
            }
            return new PackedDoubleCoordinateSequence(coords, dim, Measures);
        }
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on floats.
    /// </summary>
    [Serializable]
    public class PackedFloatCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private readonly float[] _coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <c>float</c> values that contains the ordinate values of the sequence.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedFloatCoordinateSequence(float[] coords, int dimension, int measures)
            : base(coords?.Length / dimension ?? 0, dimension, measures)
        {
            if (coords == null)
                coords = new float[0];

            if (coords.Length % dimension != 0)
                throw new ArgumentException("Packed array does not contain " +
                    "an integral number of coordinates");

            _coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <c>double</c> values that contains the ordinate values of the sequence.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedFloatCoordinateSequence(double[] coords, int dimension, int measures)
            : base(coords?.Length / dimension ?? 0, dimension, measures)
        {
            if (coords == null)
            {
                _coords = new float[0];
                return;
            }

            _coords = new float[coords.Length];
            for (int i = 0; i < coords.Length; i++)
                _coords[i] = (float)coords[i];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <see cref="Coordinate"/>s.</param>
        public PackedFloatCoordinateSequence(Coordinate[] coords)
            : this(coords, GetDimensionAndMeasures(coords, out int measures), measures)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <see cref="Coordinate"/>s.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public PackedFloatCoordinateSequence(Coordinate[] coords, int dimension)
            : this(coords, GetDimensionAndMeasures(coords, out int measures), measures)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords">An array of <see cref="Coordinate"/>s.</param>
        /// <param name="dimension">The total number of ordinates that make up a <see cref="Coordinate"/> in this sequence.</param>
        /// <param name="measures">The number of measure-ordinates each <see cref="Coordinate"/> in this sequence has.</param>
        public PackedFloatCoordinateSequence(Coordinate[] coords, int dimension, int measures)
            : this(coords, dimension, measures, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedFloatCoordinateSequence(int size, int dimension, int measures)
            : base(size, dimension, measures)
        {
            _coords = new float[size * Dimension];
        }

        internal PackedFloatCoordinateSequence(Coordinate[] coords, int dimension, int measures, bool dimensionAndMeasuresCameFromCoords)
            : base(coords?.Length ?? 0, dimension, measures)
        {
            if (coords == null)
                coords = Array.Empty<Coordinate>();

            _coords = new float[coords.Length * Dimension];
            if (coords.Length == 0)
            {
                return;
            }

            if (dimensionAndMeasuresCameFromCoords)
            {
                for (int i = 0; i < coords.Length; i++)
                {
                    int offset = i * dimension;

                    var coord = coords[i];
                    for (int dim = 0; dim < dimension; dim++)
                    {
                        _coords[offset + dim] = (float)coord[dim];
                    }
                }
            }
            else
            {
                _coords.AsSpan().Fill(float.NaN);

                int spatial = dimension - measures;
                for (int i = 0; i < coords.Length; i++)
                {
                    int offset = i * dimension;

                    var coord = coords[i];
                    int coordDimension = Coordinates.Dimension(coord);
                    int coordMeasures = Coordinates.Measures(coord);
                    int coordSpatial = coordDimension - coordMeasures;

                    for (int dim = 0, dimEnd = Math.Min(spatial, coordSpatial); dim < dimEnd; dim++)
                    {
                        _coords[offset + dim] = (float)coord[dim];
                    }

                    for (int measure = 0, measureEnd = Math.Min(measures, coordMeasures); measure < measureEnd; measure++)
                    {
                        _coords[offset + spatial + measure] = (float)coord[coordSpatial + measure];
                    }
                }
            }
        }

        /// <summary>
        /// Gets the underlying array containing the coordinate values.
        /// </summary>
        /// <returns>The array of coordinate values</returns>
        public float[] GetRawCoordinates()
        {
            return _coords;
        }

        /// <inheritdoc cref="CoordinateSequence.Copy"/>
        public override CoordinateSequence Copy()
        {
            return new PackedFloatCoordinateSequence((float[])_coords.Clone(), Dimension, Measures);
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
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            return _coords[index * Dimension + ordinateIndex];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked:
        /// if it is over dimensions you may not get an exception but a meaningless value.
        /// </remarks>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            CoordRef = null;
            _coords[index * Dimension + ordinateIndex] = (float) value;
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

        public override CoordinateSequence Reversed()
        {
            int dim = Dimension;
            float[] coords = new float[_coords.Length];
            int j = Count;
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(_coords, i * dim * sizeof(float), coords, --j * dim * sizeof(float), dim * sizeof(float));
            }
            return new PackedDoubleCoordinateSequence(coords, dim, Measures);
        }
    }
}
