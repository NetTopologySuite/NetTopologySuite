using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Builds packed array coordinate sequences. The array data type can be either
    /// Double or float, and defaults to Double.
    /// </summary>
    public class PackedCoordinateSequenceFactory : ICoordinateSequenceFactory
    {
        public enum PackedType
        {
            Double = 0,
            Float = 1,
        }

        public static readonly PackedCoordinateSequenceFactory DoubleFactory =
            new PackedCoordinateSequenceFactory(PackedType.Double);

        public static readonly PackedCoordinateSequenceFactory FloatFactory =
            new PackedCoordinateSequenceFactory(PackedType.Float);

        private PackedType type = PackedType.Double;
        private Int32 dimension = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class, 
        /// using Double values.
        /// </summary>
        public PackedCoordinateSequenceFactory() : this(PackedType.Double) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public PackedCoordinateSequenceFactory(PackedType type) : this(type, 3) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class.
        /// </summary>
        public PackedCoordinateSequenceFactory(PackedType type, Int32 dimension)
        {
            Type = type;
            Dimension = dimension;
        }

        public PackedType Type
        {
            get { return type; }
            set
            {
                if (value != PackedType.Double && value != PackedType.Float)
                {
                    throw new ArgumentException("Unknown type " + value);
                }

                type = value;
            }
        }

        public Int32 Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given array; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">Coordinates array, which may not be null nor contain null elements</param>
        public ICoordinateSequence Create(ICoordinate[] coordinates)
        {
            if (type == PackedType.Double)
            {
                return new PackedDoubleCoordinateSequence(coordinates, dimension);
            }
            else
            {
                return new PackedFloatCoordinateSequence(coordinates, dimension);
            }
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given coordinate sequence; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            if (type == PackedType.Double)
            {
                return new PackedDoubleCoordinateSequence(coordSeq.ToCoordinateArray(), dimension);
            }
            else
            {
                return new PackedFloatCoordinateSequence(coordSeq.ToCoordinateArray(), dimension);
            }
        }

        public ICoordinateSequence Create(Double[] packedCoordinates, Int32 dimension)
        {
            if (type == PackedType.Double)
            {
                return new PackedDoubleCoordinateSequence(packedCoordinates, dimension);
            }
            else
            {
                return new PackedFloatCoordinateSequence(packedCoordinates, dimension);
            }
        }

        public ICoordinateSequence Create(float[] packedCoordinates, Int32 dimension)
        {
            if (type == PackedType.Double)
            {
                return new PackedDoubleCoordinateSequence(packedCoordinates, dimension);
            }
            else
            {
                return new PackedFloatCoordinateSequence(packedCoordinates, dimension);
            }
        }

        public ICoordinateSequence Create(Int32 size, Int32 dimension)
        {
            if (type == PackedType.Double)
            {
                return new PackedDoubleCoordinateSequence(size, dimension);
            }
            else
            {
                return new PackedFloatCoordinateSequence(size, dimension);
            }
        }
    }
}