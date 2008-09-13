using System;
using System.Collections.Generic;
using System.ComponentModel;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack;
using NPack.Interfaces;
#if NETCF
using BitConverter = GisSharpBlog.NetTopologySuite.Utilities;
#endif

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;
    using IBufferedVectorFactory = IVectorFactory<DoubleComponent, BufferedCoordinate2D>;

    public class BufferedCoordinate2DFactory
        : IBufferedCoordFactory, IVectorBuffer<BufferedCoordinate2D, DoubleComponent>,
          IBufferedVectorFactory<BufferedCoordinate2D, DoubleComponent>,
          ILinearFactory<DoubleComponent, BufferedCoordinate2D, Matrix3>
    {
        public static readonly Int32 MaximumBitResolution = 52;
        private static readonly IComparer<Pair<Double>> _valueComparer
            = new LexicographicComparer();

        private static readonly IComparer<BufferedCoordinate2D> _coordComparer
            = new LexicographicCoordinateComparer((LexicographicComparer)_valueComparer);
        private readonly ManagedVectorBuffer<BufferedCoordinate2D, DoubleComponent> _coordinates;
        private readonly IDictionary<Pair<Double>, Int32> _lexicographicVertexIndex;
        private readonly IDictionary<Triple<Double>, Int32> _lexicographicHomogeneousVertexIndex;
        private Int32 _bitResolution;
        private Int64 _mask = unchecked((Int64)0xFFFFFFFFFFFFFFFF);
        private readonly Int32[] _ordinateIndexTable = new Int32[4];
        private readonly IMatrixOperations<DoubleComponent, BufferedCoordinate2D, Matrix3> _ops;
        private readonly YieldingSpinLock _spinLock = new YieldingSpinLock();

        public BufferedCoordinate2DFactory()
            : this(MaximumBitResolution) { }

        public BufferedCoordinate2DFactory(Int32 bitResolution)
        {
            _bitResolution = bitResolution;
            _lexicographicVertexIndex = createLexicographicIndex();
            _lexicographicHomogeneousVertexIndex = createLexicographicHomogeneousIndex();
            _coordinates = new ManagedVectorBuffer<BufferedCoordinate2D, DoubleComponent>(2, true, this);
            initializeOrdinateIndexTable();
            _ops = new ClrMatrixOperations<DoubleComponent, BufferedCoordinate2D, Matrix3>(this);
        }

        public IVectorBuffer<BufferedCoordinate2D, DoubleComponent> VectorBuffer
        {
            get { return this; }
        }

        internal IComparer<BufferedCoordinate2D> Comparer
        {
            get { return _coordComparer; }
        }

        internal IMatrixOperations<DoubleComponent, BufferedCoordinate2D, Matrix3> Ops
        {
            get { return _ops; }
        }

        #region IBufferedCoordFactory Members
        public Int32 BitResolution
        {
            get { return _bitResolution; }
            set
            {
                _bitResolution = value;
                Int32 shift = MaximumBitResolution - _bitResolution;
                _mask = unchecked((Int64)(0xFFFFFFFFFFFFFFFF << shift));
            }
        }

        public BufferedCoordinate2D Create(Double x, Double y)
        {
            return getVertexInternal(x, y);
        }

        public BufferedCoordinate2D Create(Double x, Double y, Double m)
        {
            throw new NotSupportedException("Coordinates with 'M' values currently not supported.");
        }

        public BufferedCoordinate2D Create(params Double[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            Int32 length = coordinates.Length;

            if (length == 0)
            {
                return new BufferedCoordinate2D();
            }

            if (length == 2)
            {
                return Create(coordinates[0], coordinates[1]);
            }

            if (length == 1)
            {
                throw new ArgumentException("Only one coordinate component was provided; " +
                                            "at least 2 are needed.");
            }

            if (length == 3)
            {
                throw new NotSupportedException("Coordinates with 'M' values currently " +
                                                "not supported.");
            }

            throw new ArgumentException("Too many components.");
        }

        public BufferedCoordinate2D Create3D(Double x, Double y, Double z)
        {
            throw new NotSupportedException("Only 2D coordinates are supported.");
        }

        public BufferedCoordinate2D Create3D(Double x, Double y, Double z, Double m)
        {
            throw new NotSupportedException("Only 2D coordinates are supported.");
        }

        public BufferedCoordinate2D Create3D(params Double[] coordinates)
        {
            throw new NotSupportedException("Only 2D coordinates are supported.");
        }

        public BufferedCoordinate2D Create(BufferedCoordinate2D coordinate)
        {
            if (coordinate.IsEmpty)
            {
                return new BufferedCoordinate2D();
            }
            if (ReferenceEquals(coordinate.Factory, this))
            {
                return coordinate;
            }
            return getVertexInternal(coordinate.X, coordinate.Y);
        }

        public BufferedCoordinate2D Create(ICoordinate coordinate)
        {
            if (coordinate is BufferedCoordinate2D)
            {
                return Create((BufferedCoordinate2D)coordinate);
            }

            return coordinate.IsEmpty
                ? new BufferedCoordinate2D()
                : Create(coordinate[Ordinates.X], coordinate[Ordinates.Y]);
        }

        public BufferedCoordinate2D Homogenize(BufferedCoordinate2D coordinate)
        {
            return BufferedCoordinate2D.Homogenize(coordinate);
        }

        public IEnumerable<BufferedCoordinate2D> Homogenize(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            foreach (BufferedCoordinate2D coordinate in coordinates)
            {
                yield return Homogenize(coordinate);
            }
        }

        public BufferedCoordinate2D Dehomogenize(BufferedCoordinate2D coordinate)
        {
            return BufferedCoordinate2D.Dehomogenize(coordinate);
        }

        public IEnumerable<BufferedCoordinate2D> Dehomogenize(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            foreach (BufferedCoordinate2D coordinate in coordinates)
            {
                yield return Dehomogenize(coordinate);
            }
        }

        #endregion

        #region ICoordinateFactory Members

        ICoordinate ICoordinateFactory.Create(Double x, Double y)
        {
            return Create(x, y);
        }

        ICoordinate ICoordinateFactory.Create(Double x, Double y, Double m)
        {
            return Create(x, y, m);
        }

        ICoordinate ICoordinateFactory.Create(params Double[] coordinates)
        {
            return Create(coordinates);
        }

        ICoordinate ICoordinateFactory.Create3D(Double x, Double y, Double z)
        {
            return Create3D(x, y, z);
        }

        ICoordinate ICoordinateFactory.Create3D(Double x, Double y, Double z, Double m)
        {
            return Create3D(x, y, z, m);
        }

        ICoordinate ICoordinateFactory.Create3D(params Double[] coordinates)
        {
            return Create3D(coordinates);
        }

        IAffineTransformMatrix<DoubleComponent> ICoordinateFactory.CreateTransform(ICoordinate scaleVector, ICoordinate rotationAxis, Double rotation, ICoordinate translateVector)
        {
            throw new NotImplementedException();
        }

        ICoordinate ICoordinateFactory.Homogenize(ICoordinate coordinate)
        {
            if (coordinate == null)
            {
                throw new ArgumentNullException("coordinate");
            }

            return Homogenize(getVertexInternal(coordinate[Ordinates.X], coordinate[Ordinates.Y]));
        }

        ICoordinate ICoordinateFactory.Dehomogenize(ICoordinate coordinate)
        {
            if (coordinate == null)
            {
                throw new ArgumentNullException("coordinate");
            }

            return Dehomogenize(getVertexInternal(coordinate[Ordinates.X], coordinate[Ordinates.Y]));
        }

        #endregion

        #region IVectorBuffer<BufferedCoordinate2D,DoubleComponent> Members

        Int32 IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Add(IVector<DoubleComponent> vector)
        {
            if (vector == null || vector.ComponentCount != 2)
            {
                throw new ArgumentException(
                    "A BufferedCoordinate2D requires exactly two components.");
            }

            Double x = (Double)vector[0];
            Double y = (Double)vector[1];

            BufferedCoordinate2D v = getVertexInternal(x, y);

            return v.Index;
        }

        Int32 IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Add(BufferedCoordinate2D vector)
        {
            if (isValidVertex(vector))
            {
                return vector.Index;
            }

            return getVertexInternal(vector.X, vector.Y).Index;
        }

        BufferedCoordinate2D IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Add(params DoubleComponent[] components)
        {
            if (components.Length != 2)
            {
                throw new ArgumentException(
                    "A BufferedCoordinate2D requires exactly two components.");
            }

            return getVertexInternal((Double)components[0], (Double)components[1]);
        }

        void IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Clear()
        {
            _coordinates.Clear();
        }

        Boolean IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Contains(IVector<DoubleComponent> item)
        {
            return _coordinates.Contains(item);
        }

        Boolean IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Contains(BufferedCoordinate2D item)
        {
            return _coordinates.Contains(item);
        }

        void IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.CopyTo(BufferedCoordinate2D[] array, Int32 startIndex, Int32 endIndex)
        {
            _coordinates.CopyTo(array, startIndex, endIndex);
        }

        Int32 IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Count
        {
            get { return _coordinates.Count; }
        }

        //IVectorFactory<BufferedCoordinate2D, DoubleComponent> IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Factory
        //{
        //    get { return _coordinates.Factory; }
        //}

        Boolean IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.IsReadOnly
        {
            get { return _coordinates.IsReadOnly; }
        }

        Int32 IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.MaximumSize
        {
            get
            {
                return _coordinates.MaximumSize;
            }
            set
            {
                _coordinates.MaximumSize = value;
            }
        }

        void IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Remove(Int32 index)
        {
            throw new NotImplementedException();
            //_coordinates.Remove(index); - dangerous
        }

        event EventHandler IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.SizeIncreased
        {
            add { _coordinates.SizeIncreased += value; }
            remove { _coordinates.SizeIncreased -= value; }
        }

        event CancelEventHandler IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.SizeIncreasing
        {
            add { _coordinates.SizeIncreasing += value; }
            remove { _coordinates.SizeIncreasing -= value; }
        }

        event EventHandler<VectorOperationEventArgs<BufferedCoordinate2D, DoubleComponent>> IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.VectorChanged
        {
            add { _coordinates.VectorChanged += value; }
            remove { _coordinates.VectorChanged -= value; }
        }

        Int32 IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.VectorLength
        {
            get { return 2; }
        }

        BufferedCoordinate2D IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.this[Int32 index]
        {
            get
            {
                return _coordinates[index];
            }
            set
            {
                // coordinate normally immutable except during precision snap-to
                // there are potential use-cases where you would not just set the ordinates
                // but the potential dangers of using the setter by mistake currently
                // outweigh a use-case I cannot yet identify
                throw new NotSupportedException();
            }
        }

        #endregion

        #region IEnumerable<BufferedCoordinate2D> Members

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            return _coordinates.GetEnumerator();
        }

        #endregion

        #region IBufferedVectorFactory<BufferedCoordinate2D,DoubleComponent> Members

        public BufferedCoordinate2D CreateBufferedVector(IVectorBuffer<BufferedCoordinate2D, DoubleComponent> vectorBuffer, Int32 index)
        {
            if (!ReferenceEquals(_coordinates, vectorBuffer)
                && !ReferenceEquals(this, vectorBuffer))
            {
                throw new ArgumentException(
                    "The buffer must be this BufferedCoordinate2DFactory.");
            }

            return new BufferedCoordinate2D(this, index);
        }

        #endregion

        #region IMatrixFactory<DoubleComponent,Matrix3> Members

        public Matrix3 CreateMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            if (format == MatrixFormat.RowMajor)
            {
                checkCounts(rowCount, columnCount);
                return new Matrix3();
            }

            throw new ArgumentException("Only row-major matrixes are supported");
        }

        public Matrix3 CreateMatrix(Int32 rowCount, Int32 columnCount, IEnumerable<DoubleComponent> values)
        {
            checkCounts(rowCount, columnCount);

            return new Matrix3(Enumerable.ToArray(values));
        }

        public Matrix3 CreateMatrix(Int32 rowCount, Int32 columnCount)
        {
            checkCounts(rowCount, columnCount);

            return new Matrix3();
        }

        public Matrix3 CreateMatrix(Matrix3 matrix)
        {
            return matrix;
        }

        #endregion

        #region IVectorFactory<DoubleComponent,BufferedCoordinate2D> Members

        BufferedCoordinate2D IBufferedVectorFactory.CreateVector(IEnumerable<DoubleComponent> values)
        {
            Pair<DoubleComponent>? pair = Slice.GetPair(values);

            if (pair == null)
            {
                throw new ArgumentException("Must have at least two values.");
            }

            Pair<DoubleComponent> coord = pair.Value;

            return getVertexInternal((Double)coord.First, (Double)coord.Second);
        }

        BufferedCoordinate2D IBufferedVectorFactory.CreateVector(Int32 componentCount)
        {
            throw new NotSupportedException();
        }

        public BufferedCoordinate2D CreateVector(params Double[] components)
        {
            switch (components.Length)
            {
                case 2:
                    return CreateVector(components[0], components[1]);
                case 3:
                    return CreateVector(components[0], components[1], components[2]);
                default:
                    throw new ArgumentException(
                        "A BufferedCoordinate2D must have only 2 or 3 components.");
            }
        }

        public BufferedCoordinate2D CreateVector(Double a, Double b, Double c)
        {
            return getVertexInternal(a, b, c);
        }

        public BufferedCoordinate2D CreateVector(Double a, Double b)
        {
            return getVertexInternal(a, b);
        }

        public BufferedCoordinate2D CreateVector(params DoubleComponent[] components)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            return getVertexInternal(a.ToDouble(null), b.ToDouble(null), c.ToDouble(null));
        }

        public BufferedCoordinate2D CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return getVertexInternal(a.ToDouble(null), b.ToDouble(null));
        }


        #endregion

        internal Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            Int32 ordinateIndex = _ordinateIndexTable[(Int32)ordinate];
            return (Double)_coordinates[index, ordinateIndex];
        }

        internal BufferedCoordinate2D GetZero()
        {
            return getVertexInternal(0, 0);
        }

        internal BufferedCoordinate2D Add(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return _ops.Add(a, b);
            //return getVertexInternal(a.X + b.X, a.Y + b.Y);
        }

        internal BufferedCoordinate2D Add(BufferedCoordinate2D a, Double b)
        {
            return _ops.ScalarAdd(a, b);
            //return getVertexInternal(a.X + b, a.Y + b);
        }

        //internal static BufferedCoordinate2D Divide(BufferedCoordinate2D a, BufferedCoordinate2D b)
        //{
        //    throw new NotImplementedException();
        //}

        internal BufferedCoordinate2D Divide(BufferedCoordinate2D a, Double b)
        {
            return _ops.ScalarMultiply(a, 1 / b);
        }

        internal Double Distance(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            // the Euclidian norm over the vector difference
            return _ops.TwoNorm(_ops.Subtract(a, b));
        }

        internal BufferedCoordinate2D GetOne()
        {
            return getVertexInternal(1, 1);
        }

        internal Double Dot(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return (Double)_ops.Dot(a, b);
        }

        internal BufferedCoordinate2D Cross(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return _ops.Cross(a, b);
        }

        internal static Int32 Compare(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            Pair<Double> aValues = new Pair<Double>(a.X, a.Y);
            Pair<Double> bValues = new Pair<Double>(b.X, b.Y);
            return _valueComparer.Compare(aValues, bValues);
        }

        internal static Boolean GreaterThan(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return Compare(a, b) > 0;
        }

        internal static Boolean GreaterThanOrEqualTo(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return Compare(a, b) >= 0;
        }

        internal static Boolean LessThan(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return Compare(a, b) < 0;
        }

        internal static Boolean LessThanOrEqualTo(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return Compare(a, b) <= 0;
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private Boolean isValidVertex(BufferedCoordinate2D vector)
        {
            return ReferenceEquals(vector.Factory, this)
                   && vector.Index < _coordinates.Count
                   && vector.X == _coordinates[vector.Index].X
                   && vector.Y == _coordinates[vector.Index].Y;
        }

        private static IDictionary<Pair<Double>, Int32> createLexicographicIndex()
        {
            return new SortedDictionary<Pair<Double>, Int32>(
                new LexicographicComparer());
        }

        private static IDictionary<Triple<Double>, Int32> createLexicographicHomogeneousIndex()
        {
            return new SortedDictionary<Triple<Double>, Int32>(
                new LexicographicComparer());
        }

        private BufferedCoordinate2D getVertexInternal(Double x, Double y)
        {
            return getVertexInternal(x, y, 1);
        }

        private BufferedCoordinate2D getVertexInternal(Double x, Double y, Double w)
        {
            if (Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(w))
            {
                throw new InvalidOperationException("Vertex components can't be NaN.");
            }

            Int64 xBits = BitConverter.DoubleToInt64Bits(x);
            xBits &= _mask;
            x = BitConverter.Int64BitsToDouble(xBits);

            Int64 yBits = BitConverter.DoubleToInt64Bits(y);
            yBits &= _mask;
            y = BitConverter.Int64BitsToDouble(yBits);

            BufferedCoordinate2D v;

            _spinLock.Enter();

            if (w != 1.0)
            {
                Int64 wBits = BitConverter.DoubleToInt64Bits(w);
                wBits &= _mask;
                w = BitConverter.Int64BitsToDouble(wBits);

                v = findExisting(x, y, w) ?? addNew(x, y, w);
            }
            else
            {
                v = findExisting(x, y) ?? addNew(x, y);
            }

            _spinLock.Exit();

            return v;
        }

        private BufferedCoordinate2D? findExisting(Double x, Double y)
        {
            BufferedCoordinate2D? v = null;
            Int32 index;

            if (_lexicographicVertexIndex.TryGetValue(new Pair<Double>(x, y), out index))
            {
                v = new BufferedCoordinate2D(this, index);
            }

            return v;
        }

        private BufferedCoordinate2D? findExisting(Double x, Double y, Double w)
        {
            BufferedCoordinate2D? v = null;
            Int32 index;

            if (_lexicographicHomogeneousVertexIndex.TryGetValue(new Triple<Double>(x, y, w), out index))
            {
                v = new BufferedCoordinate2D(this, index);
            }

            return v;
        }

        private BufferedCoordinate2D addNew(Double x, Double y)
        {
            BufferedCoordinate2D coord = _coordinates.Add(x, y, 1);
            _lexicographicVertexIndex[new Pair<Double>(coord.X, coord.Y)] = coord.Index;
            return coord;
        }

        private BufferedCoordinate2D addNew(Double x, Double y, Double w)
        {
            BufferedCoordinate2D coord = _coordinates.Add(x, y, w);
            Triple<Double> values = new Triple<Double>(coord.X, coord.Y, coord[Ordinates.W]);
            _lexicographicHomogeneousVertexIndex[values] = coord.Index;
            return coord;
        }

        private void initializeOrdinateIndexTable()
        {
            _ordinateIndexTable[(Int32)Ordinates.X] = 0;
            _ordinateIndexTable[(Int32)Ordinates.Y] = 1;
            _ordinateIndexTable[(Int32)Ordinates.Z] = -1; // flag value to throw exception.
            _ordinateIndexTable[(Int32)Ordinates.W] = 2;
        }

        private static void checkCounts(Int32 rowCount, Int32 columnCount)
        {
            if (rowCount != 3)
            {
                throw new ArgumentOutOfRangeException("rowCount", rowCount, "Must be 3");
            }

            if (columnCount != 3)
            {
                throw new ArgumentOutOfRangeException("columnCount", columnCount, "Must be 3");
            }
        }

        private class LexicographicCoordinateComparer : IComparer<BufferedCoordinate2D>
        {
            private readonly LexicographicComparer _valueComparer;

            public LexicographicCoordinateComparer(LexicographicComparer valueComparer)
            {
                _valueComparer = valueComparer;
            }

            #region IComparer<BufferedCoordinate2D> Members

            public Int32 Compare(BufferedCoordinate2D a, BufferedCoordinate2D b)
            {
                return a.ComponentCount == 3
                    ? _valueComparer.Compare(new Triple<Double>(a.X, a.Y, a[Ordinates.W]),
                                             new Triple<Double>(b.X, b.Y, b[Ordinates.W]))
                    :_valueComparer.Compare(new Pair<Double>(a.X, a.Y), 
                                            new Pair<Double>(b.X, b.Y));
            }

            #endregion
        }

        private class LexicographicComparer : IComparer<Pair<Double>>, IComparer<Triple<Double>>
        {
            #region IComparer<Pair<Double>> Members

            public Int32 Compare(Pair<Double> v1, Pair<Double> v2)
            {
                Double v1_x = v1.First;
                Double v2_x = v2.First;

                if (v1_x < v2_x)
                {
                    return -1;
                }

                if (v1_x > v2_x)
                {
                    return 1;
                }

                // v1.First == v2.First
                return v1.Second.CompareTo(v2.Second);
            }

            #endregion

            #region IComparer<Triple<Double>> Members

            public Int32 Compare(Triple<Double> v1, Triple<Double> v2)
            {
                Int32 result = Compare(new Pair<Double>(v1.First, v2.First),
                                       new Pair<Double>(v1.Second, v2.Second));

                if(result != 0)
                {
                    return result;
                }

                return v1.Third.CompareTo(v2.Third);
            }

            #endregion
        }
    }
}
