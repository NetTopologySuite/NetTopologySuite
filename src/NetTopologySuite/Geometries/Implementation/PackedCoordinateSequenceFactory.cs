using System;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// Builds packed array coordinate sequences.
    /// The array data type can be either
    /// <c>double</c> or <c>float</c>,
    /// and defaults to <c>double</c>.
    /// </summary>
    [Serializable]
    public class PackedCoordinateSequenceFactory : CoordinateSequenceFactory
    {
        /// <summary>
        /// An enumeration of valid type codes
        /// </summary>
        public enum PackedType
        {
            /// <summary>
            /// Type code for arrays of type <c>double</c>.
            /// </summary>
            Double = 0,

            /// <summary>
            /// Type code for arrays of type <c>float</c>.
            /// </summary>
            Float = 1,
        }

        /// <summary>
        /// A factory creating <see cref="PackedType.Double"/> coordinate sequences
        /// </summary>
        public static readonly PackedCoordinateSequenceFactory DoubleFactory =
            new PackedCoordinateSequenceFactory(PackedType.Double);

        /// <summary>
        /// A factory creating <see cref="PackedType.Float"/> coordinate sequences
        /// </summary>
        public static readonly PackedCoordinateSequenceFactory FloatFactory =
            new PackedCoordinateSequenceFactory(PackedType.Float);

        private PackedType _type = PackedType.Double;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class,
        /// using double values.
        /// </summary>
        public PackedCoordinateSequenceFactory() : this(PackedType.Double) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public PackedCoordinateSequenceFactory(PackedType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the type of packed coordinate sequence this factory builds, either
        /// <see cref="PackedType.Float"/> or <see cref="PackedType.Double"/>
        /// </summary>
        /// <value>The type of packed array built.</value>
        public PackedType Type
        {
            get => _type;
            set
            {
                if (value != PackedType.Double && value != PackedType.Float)
                    throw new ArgumentException("Unknown type " + value);
                this._type = value;
            }
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given array; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">Coordinates array, which may not be null nor contain null elements</param>
        /// <returns></returns>
        public override CoordinateSequence Create(Coordinate[] coordinates)
        {
            // DEVIATION: JTS just uses the first coordinate, which can be lossy.
            (_, int dimension, int measures) = GetCommonSequenceParameters(coordinates);
            if (_type == PackedType.Double)
                return new PackedDoubleCoordinateSequence(coordinates, dimension, measures, true);
            return new PackedFloatCoordinateSequence(coordinates, dimension, measures, true);
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given coordinate sequence; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns></returns>
        public override CoordinateSequence Create(CoordinateSequence coordSeq)
        {
            int dimension = coordSeq.Dimension;
            int measures = coordSeq.Measures;
            if (_type == PackedType.Double)
                return new PackedDoubleCoordinateSequence(coordSeq.ToCoordinateArray(), dimension, measures, true);
            return new PackedFloatCoordinateSequence(coordSeq.ToCoordinateArray(), dimension, measures, true);
        }

        /// <summary>
        /// Creates a packed coordinate sequence of type <see cref="Type"/> from the provided <c>double</c> array
        /// using the provided dimension and a measure of <c>0</c>.
        /// </summary>
        /// <param name="packedCoordinates">The array containing coordinate values</param>
        /// <param name="dimension">The coordinate dimension</param>
        /// <returns>A packed coordinate sequence of <see cref="Type"/></returns>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public CoordinateSequence Create(double[] packedCoordinates, int dimension)
        {
            return Create(packedCoordinates, dimension, 0);
        }

        /// <summary>
        /// Creates a packed coordinate sequence of type <see cref="Type"/> from the provided <c>double</c> array
        /// using the provided dimension and a measure of <c>0</c>.
        /// </summary>
        /// <param name="packedCoordinates">The array containing coordinate values</param>
        /// <param name="dimension">The coordinate dimension</param>
        /// <param name="measures">The coordinate measure count</param>
        /// <returns>A packed coordinate sequence of <see cref="Type"/></returns>
        public CoordinateSequence Create(double[] packedCoordinates, int dimension, int measures)
        {
            if (_type == PackedType.Double)
                return new PackedDoubleCoordinateSequence(packedCoordinates, dimension, measures);
            return new PackedFloatCoordinateSequence(packedCoordinates, dimension, measures);
        }

        /// <summary>
        /// Creates a packed coordinate sequence of type <see cref="Type"/> from the provided <c>float</c> array
        /// using the provided dimension and a measure of <c>0</c>.
        /// </summary>
        /// <param name="packedCoordinates">The array containing coordinate values</param>
        /// <param name="dimension">The coordinate dimension</param>
        /// <returns>A packed coordinate sequence of <see cref="Type"/></returns>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public CoordinateSequence Create(float[] packedCoordinates, int dimension)
        {
            return Create(packedCoordinates, dimension, 0);
        }

        /// <summary>
        /// Creates a packed coordinate sequence of type <see cref="Type"/> from the provided <c>float</c> array
        /// using the provided dimension and a measure of <c>0</c>.
        /// </summary>
        /// <param name="packedCoordinates">The array containing coordinate values</param>
        /// <param name="dimension">The coordinate dimension</param>
        /// <param name="measures">The coordinate measure count</param>
        /// <returns>A packed coordinate sequence of <see cref="Type"/></returns>
        public CoordinateSequence Create(float[] packedCoordinates, int dimension, int measures)
        {
            if (_type == PackedType.Double)
                return new PackedDoubleCoordinateSequence(packedCoordinates, dimension, measures);
            return new PackedFloatCoordinateSequence(packedCoordinates, dimension, measures);
        }

        /// <inheritdoc />
        public override CoordinateSequence Create(int size, int dimension, int measures)
        {
            if (_type == PackedType.Double)
                return new PackedDoubleCoordinateSequence(size, dimension, measures);
            return new PackedFloatCoordinateSequence(size, dimension, measures);
        }
    }
}
