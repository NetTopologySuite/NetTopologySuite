using System;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// Builds packed array coordinate sequences. The array data type can be either
    /// double or float, and defaults to double.
    /// </summary>
    [Serializable]
    public class PackedCoordinateSequenceFactory : ICoordinateSequenceFactory
    {
        /// <summary>
        ///
        /// </summary>
        public enum PackedType
        {
            /// <summary>
            ///
            /// </summary>
            Double = 0,

            /// <summary>
            ///
            /// </summary>
            Float = 1,
        }

        public static readonly PackedCoordinateSequenceFactory DoubleFactory =
            new PackedCoordinateSequenceFactory(PackedType.Double);

        public static readonly PackedCoordinateSequenceFactory FloatFactory =
            new PackedCoordinateSequenceFactory(PackedType.Float);

        private PackedType type = PackedType.Double;

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
        ///
        /// </summary>
        public PackedType Type
        {
            get => type;
            set
            {
                if (value != PackedType.Double && value != PackedType.Float)
                    throw new ArgumentException("Unknown type " + value);
                this.type = value;
            }
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given array; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">Coordinates array, which may not be null nor contain null elements</param>
        /// <returns></returns>
        public ICoordinateSequence Create(Coordinate[] coordinates)
        {
            int dimension = 2;
            int measures = 0;
            if (coordinates != null && coordinates.Length > 1 && coordinates[0] != null)
            {
                var first = coordinates[0];
                dimension = Coordinates.Dimension(first);
                measures = Coordinates.Measures(first);
            }

            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(coordinates, dimension, measures);
            return new PackedFloatCoordinateSequence(coordinates, dimension, measures);
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given coordinate sequence; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            int dimension = coordSeq.Dimension;
            int measures = coordSeq.Measures;
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(coordSeq.ToCoordinateArray(), dimension, measures);
            return new PackedFloatCoordinateSequence(coordSeq.ToCoordinateArray(), dimension, measures);
        }

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and ordinates.
        /// For this to be useful, the <see cref="ICoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <param name="size">The number of coordinates</param>
        /// <param name="ordinates">
        /// The ordinates each coordinate has. <see cref="Ordinates.XY"/> is fix, <see cref="Ordinates.Z"/> and <see cref="Ordinates.M"/> can be set.
        /// </param>
        /// <returns></returns>
        public ICoordinateSequence Create(int size, Ordinates ordinates)
        {
            return Create(size, OrdinatesUtility.OrdinatesToDimension(ordinates));
        }

        // this is supposed to be the MAX ordinates that we can cope with, but we can actually cope
        // with even more than the 32 listed here.
        public Ordinates Ordinates => unchecked((Ordinates)0xFFFFFFFF);

        /// <summary>
        /// Create a packed coordinate sequence from the provided array. 
        /// </summary>
        /// <param name="packedCoordinates"></param>
        /// <param name="dimension"></param>
        /// <returns>Packaged coordinate seqeunce of the requested type</returns>
        public ICoordinateSequence Create(double[] packedCoordinates, int dimension)
        {
            return Create(packedCoordinates, dimension, 0);
        }

        /// <summary>
        /// Create a packed coordinate sequence from the provided array. 
        /// </summary>
        /// <param name="packedCoordinates"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        /// <returns>Packaged coordinate seqeunce of the requested type</returns>
        public ICoordinateSequence Create(double[] packedCoordinates, int dimension, int measures)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(packedCoordinates, dimension, measures);
            return new PackedFloatCoordinateSequence(packedCoordinates, dimension, measures);
        }

        /// <summary>
        /// Create a packed coordinate sequence from the provided array. 
        /// </summary>
        /// <param name="packedCoordinates"></param>
        /// <param name="dimension"></param>
        /// <returns>Packaged coordinate seqeunce of the requested type</returns>
        public ICoordinateSequence Create(float[] packedCoordinates, int dimension)
        {
            return Create(packedCoordinates, dimension, 0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packedCoordinates"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        /// <returns>Packaged coordinate seqeunce of the requested type</returns>
        public ICoordinateSequence Create(float[] packedCoordinates, int dimension, int measures)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(packedCoordinates, dimension, measures);
            else return new PackedFloatCoordinateSequence(packedCoordinates, dimension, measures);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(int size, int dimension)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(size, dimension, 0);
            else return new PackedFloatCoordinateSequence(size, dimension, 0);
        }

        /// <inheritdoc />
        public ICoordinateSequence Create(int size, int dimension, int measures)
        {
            if (type == PackedType.Double)
                return new PackedDoubleCoordinateSequence(size, dimension, measures);
            else return new PackedFloatCoordinateSequence(size, dimension, measures);
        }
    }
}
