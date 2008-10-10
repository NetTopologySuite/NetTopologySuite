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
#if DOTNET35
using System.Linq;
#endif

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate>;
    using IBufferedVectorFactory = IVectorFactory<DoubleComponent, BufferedCoordinate>;

    public class BufferedCoordinateFactory
        : IBufferedCoordFactory, IVectorBuffer<DoubleComponent, BufferedCoordinate>,
          IBufferedVectorFactory<DoubleComponent, BufferedCoordinate>,
          ILinearFactory<DoubleComponent, BufferedCoordinate, BufferedMatrix>,
          ILinearFactory<DoubleComponent>
    {
        //public static readonly Int32 MaximumBitResolution = 53;
        private static readonly IComparer<Pair<Double>> _valueComparer
            = new LexicographicComparer();

        private static readonly IComparer<BufferedCoordinate> _coordComparer
            = new LexicographicCoordinateComparer((LexicographicComparer)_valueComparer);
        private readonly ManagedVectorBuffer<DoubleComponent, BufferedCoordinate> _coordinates;
        private readonly IDictionary<Pair<Double>, Int32> _lexicographicVertexIndex;
        private readonly IDictionary<Triple<Double>, Int32> _lexicographicHomogeneousVertexIndex;
        //private Int32 _bitResolution;
        //private Int64 _mask = unchecked((Int64)0xFFFFFFFFFFFFFFFF);
        private readonly PrecisionModel _precisionModel;
        private readonly Int32[] _ordinateIndexTable = new Int32[4];
        private readonly IMatrixOperations<DoubleComponent, BufferedCoordinate, BufferedMatrix> _ops;
        private readonly YieldingSpinLock _spinLock = new YieldingSpinLock();

        public BufferedCoordinateFactory()
            : this(null) { }

        public BufferedCoordinateFactory(Double scale)
            : this(new PrecisionModel(null, scale)) { }

        public BufferedCoordinateFactory(PrecisionModelType type)
            : this(new PrecisionModel(null, type)) { }

        public BufferedCoordinateFactory(IPrecisionModel precisionModel)
        {
            _precisionModel = new PrecisionModel(this, precisionModel);
            _lexicographicVertexIndex = createLexicographicIndex();
            _lexicographicHomogeneousVertexIndex = createLexicographicHomogeneousIndex();
            _coordinates = new ManagedVectorBuffer<DoubleComponent, BufferedCoordinate>(this);
            initializeOrdinateIndexTable();
            _ops = new ClrMatrixOperations<DoubleComponent, BufferedCoordinate, BufferedMatrix>(this);
        }

        public IVectorBuffer<DoubleComponent, BufferedCoordinate> VectorBuffer
        {
            get { return this; }
        }

        internal static IComparer<BufferedCoordinate> Comparer
        {
            get { return _coordComparer; }
        }

        internal IMatrixOperations<DoubleComponent, BufferedCoordinate, BufferedMatrix> Ops
        {
            get { return _ops; }
        }

        #region IBufferedCoordFactory Members

        public BufferedCoordinate Create(Double x, Double y)
        {
            return getVertexInternal(x, y);
        }

        public BufferedCoordinate Create(Double x, Double y, Double m)
        {
            throw new NotSupportedException("Coordinates with 'M' values currently not supported.");
        }

        public BufferedCoordinate Create(params Double[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            Int32 length = coordinates.Length;

            if (length == 0)
            {
                return new BufferedCoordinate();
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

        public BufferedCoordinate Create3D(Double x, Double y, Double z)
        {
            return getVertexInternal(x, y, z);
        }

        public BufferedCoordinate Create3D(Double x, Double y, Double z, Double m)
        {
            throw new NotSupportedException("Coordinates with 'M' values currently " +
                                            "not supported.");
        }

        public BufferedCoordinate Create3D(params Double[] coordinates)
        {
            throw new NotSupportedException("Only 2D or 3D coordinates are supported.");
        }

        public BufferedCoordinate Create(BufferedCoordinate coordinate)
        {
            if (coordinate.IsEmpty)
            {
                return new BufferedCoordinate();
            }
            if (ReferenceEquals(coordinate.Factory, this))
            {
                return coordinate;
            }
            return getVertexInternal(coordinate.X, coordinate.Y);
        }

        public BufferedCoordinate Create(ICoordinate coordinate)
        {
            if (coordinate is BufferedCoordinate)
            {
                return Create((BufferedCoordinate)coordinate);
            }

            return coordinate.IsEmpty
                ? new BufferedCoordinate()
                : Create(coordinate[Ordinates.X], coordinate[Ordinates.Y]);
        }

        public BufferedCoordinate Homogenize(BufferedCoordinate coordinate)
        {
            return BufferedCoordinate.Homogenize(coordinate);
        }

        public IEnumerable<BufferedCoordinate> Homogenize(IEnumerable<BufferedCoordinate> coordinates)
        {
            foreach (BufferedCoordinate coordinate in coordinates)
            {
                yield return Homogenize(coordinate);
            }
        }

        public BufferedCoordinate Dehomogenize(BufferedCoordinate coordinate)
        {
            return BufferedCoordinate.Dehomogenize(coordinate);
        }

        public IEnumerable<BufferedCoordinate> Dehomogenize(IEnumerable<BufferedCoordinate> coordinates)
        {
            foreach (BufferedCoordinate coordinate in coordinates)
            {
                yield return Dehomogenize(coordinate);
            }
        }

        public IPrecisionModel<BufferedCoordinate> PrecisionModel
        {
            get { return _precisionModel; }
        }

        public IPrecisionModel<BufferedCoordinate> CreatePrecisionModel(Double scale)
        {
            return new PrecisionModel(this, scale);
        }

        public IPrecisionModel<BufferedCoordinate> CreatePrecisionModel(PrecisionModelType type)
        {
            return new PrecisionModel(this, type);
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

        IPrecisionModel ICoordinateFactory.PrecisionModel
        {
            get { return PrecisionModel; }
        }

        IPrecisionModel ICoordinateFactory.CreatePrecisionModel(PrecisionModelType type)
        {
            return CreatePrecisionModel(type);
        }

        IPrecisionModel ICoordinateFactory.CreatePrecisionModel(Double scale)
        {
            return CreatePrecisionModel(scale);
        }

        #endregion

        #region IVectorBuffer<BufferedCoordinate,DoubleComponent> Members

        Int32 IVectorBuffer<DoubleComponent, BufferedCoordinate>.Add(IVector<DoubleComponent> vector)
        {
            if (vector == null || vector.ComponentCount != 2)
            {
                throw new ArgumentException(
                    "A BufferedCoordinate requires exactly two components.");
            }

            Double x = (Double)vector[0];
            Double y = (Double)vector[1];

            BufferedCoordinate v = getVertexInternal(x, y);

            return v.Index;
        }

        Int32 IVectorBuffer<DoubleComponent, BufferedCoordinate>.Add(BufferedCoordinate vector)
        {
            if (isValidVertex(vector))
            {
                return vector.Index;
            }

            return getVertexInternal(vector.X, vector.Y).Index;
        }

        BufferedCoordinate IVectorBuffer<DoubleComponent, BufferedCoordinate>.Add(params DoubleComponent[] components)
        {
            if (components.Length != 2)
            {
                throw new ArgumentException(
                    "A BufferedCoordinate requires exactly two components.");
            }

            return getVertexInternal((Double)components[0], (Double)components[1]);
        }

        void IVectorBuffer<DoubleComponent, BufferedCoordinate>.Clear()
        {
            _coordinates.Clear();
        }

        Boolean IVectorBuffer<DoubleComponent, BufferedCoordinate>.Contains(IVector<DoubleComponent> item)
        {
            return _coordinates.Contains(item);
        }

        Boolean IVectorBuffer<DoubleComponent, BufferedCoordinate>.Contains(BufferedCoordinate item)
        {
            return _coordinates.Contains(item);
        }

        void IVectorBuffer<DoubleComponent, BufferedCoordinate>.CopyTo(BufferedCoordinate[] array, Int32 startIndex, Int32 endIndex)
        {
            _coordinates.CopyTo(array, startIndex, endIndex);
        }

        Int32 IVectorBuffer<DoubleComponent, BufferedCoordinate>.Count
        {
            get { return _coordinates.Count; }
        }

        //IVectorFactory<DoubleComponent, BufferedCoordinate> IVectorBuffer<DoubleComponent, BufferedCoordinate>.Factory
        //{
        //    get { return _coordinates.Factory; }
        //}

        public Int32 GetVectorLength(Int32 index)
        {
            throw new System.NotImplementedException();
        }

        Boolean IVectorBuffer<DoubleComponent, BufferedCoordinate>.IsReadOnly
        {
            get { return _coordinates.IsReadOnly; }
        }

        Int32 IVectorBuffer<DoubleComponent, BufferedCoordinate>.MaximumSize
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

        void IVectorBuffer<DoubleComponent, BufferedCoordinate>.Remove(Int32 index)
        {
            throw new NotImplementedException();
            //_coordinates.Remove(index); - dangerous
        }

        event EventHandler IVectorBuffer<DoubleComponent, BufferedCoordinate>.SizeIncreased
        {
            add { _coordinates.SizeIncreased += value; }
            remove { _coordinates.SizeIncreased -= value; }
        }

        event CancelEventHandler IVectorBuffer<DoubleComponent, BufferedCoordinate>.SizeIncreasing
        {
            add { _coordinates.SizeIncreasing += value; }
            remove { _coordinates.SizeIncreasing -= value; }
        }

        event EventHandler<VectorOperationEventArgs<DoubleComponent, BufferedCoordinate>> IVectorBuffer<DoubleComponent, BufferedCoordinate>.VectorChanged
        {
            add { _coordinates.VectorChanged += value; }
            remove { _coordinates.VectorChanged -= value; }
        }

        BufferedCoordinate IVectorBuffer<DoubleComponent, BufferedCoordinate>.this[Int32 index]
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

        #region IEnumerable<BufferedCoordinate> Members

        public IEnumerator<BufferedCoordinate> GetEnumerator()
        {
            return _coordinates.GetEnumerator();
        }

        #endregion

        #region IBufferedVectorFactory<BufferedCoordinate,DoubleComponent> Members

        public BufferedCoordinate CreateBufferedVector(IVectorBuffer<DoubleComponent, BufferedCoordinate> vectorBuffer, Int32 index)
        {
            if (!ReferenceEquals(_coordinates, vectorBuffer)
                && !ReferenceEquals(this, vectorBuffer))
            {
                throw new ArgumentException(
                    "The buffer must be this BufferedCoordinateFactory.");
            }

            return new BufferedCoordinate(this, index);
        }

        #endregion

        #region IMatrixFactory<DoubleComponent,BufferedMatrix> Members

        public BufferedMatrix CreateMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            if (format == MatrixFormat.RowMajor)
            {
                checkCounts(rowCount, columnCount);
                return new BufferedMatrix();
            }

            throw new ArgumentException("Only row-major matrixes are supported");
        }

        public BufferedMatrix CreateMatrix(Int32 rowCount, Int32 columnCount, IEnumerable<DoubleComponent> values)
        {
            checkCounts(rowCount, columnCount);

            return new BufferedMatrix(this, Enumerable.ToArray(values));
        }

        public BufferedMatrix CreateMatrix(Int32 rowCount, Int32 columnCount)
        {
            checkCounts(rowCount, columnCount);

            return new BufferedMatrix();
        }

        public BufferedMatrix CreateMatrix(BufferedMatrix matrix)
        {
            return matrix;
        }

        #endregion

        #region IMatrixFactory<DoubleComponent> Members
        IMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateMatrix(Int32 rowCount, Int32 columnCount, IEnumerable<DoubleComponent> values)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateMatrix(IMatrix<DoubleComponent> matrix)
        {
            throw new System.NotImplementedException();
        }

        ITransformMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateTransformMatrix(Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        ITransformMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateTransformMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        IAffineTransformMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateAffineMatrix(Int32 rank)
        {
            throw new System.NotImplementedException();
        }

        IAffineTransformMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateAffineMatrix(MatrixFormat format, Int32 rank)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactory<DoubleComponent>.CreateMatrix(Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IVectorFactory<DoubleComponent,BufferedCoordinate> Members

        public BufferedCoordinate CreateVector(params Double[] components)
        {
            switch (components.Length)
            {
                case 2:
                    return CreateVector(components[0], components[1]);
                case 3:
                    return CreateVector(components[0], components[1], components[2]);
                case 4:
                    return CreateVector(components[0], components[1], components[2]);
                default:
                    throw new ArgumentException("A BufferedCoordinate must " +
                                                "have only 2, 3 or 4 components.");
            }
        }

        public BufferedCoordinate CreateVector(Double a, Double b, Double c)
        {
            return getVertexInternal(a, b, c);
        }

        public BufferedCoordinate CreateVector(Double a, Double b)
        {
            return getVertexInternal(a, b);
        }

        public BufferedCoordinate CreateVector(params DoubleComponent[] components)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            return getVertexInternal(a.ToDouble(null), b.ToDouble(null), c.ToDouble(null));
        }

        public BufferedCoordinate CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return getVertexInternal(a.ToDouble(null), b.ToDouble(null));
        }
        #endregion

        #region IBufferedVectorFactory Members

        BufferedCoordinate IBufferedVectorFactory.CreateVector(IEnumerable<DoubleComponent> values)
        {
            Pair<DoubleComponent>? pair = Slice.GetPair(values);

            if (pair == null)
            {
                throw new ArgumentException("Must have at least two values.");
            }

            Pair<DoubleComponent> coord = pair.Value;

            return getVertexInternal((Double)coord.First, (Double)coord.Second);
        }

        BufferedCoordinate IBufferedVectorFactory.CreateVector(Int32 componentCount)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IVectorFactory<DoubleComponent> Members
        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(Int32 componentCount)
        {
            return CreateVector(componentCount);
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(IEnumerable<DoubleComponent> values)
        {
            return CreateVector(Enumerable.ToArray(values));
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return CreateVector(a, b);
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            return CreateVector(a, b, c);
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(params DoubleComponent[] components)
        {
            return CreateVector(components);
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(double a, double b)
        {
            return CreateVector(a, b);
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(double a, double b, double c)
        {
            return CreateVector(a, b, c);
        }

        IVector<DoubleComponent> IVectorFactory<DoubleComponent>.CreateVector(params double[] components)
        {
            return CreateVector(components);
        }
        #endregion

        internal Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            Int32 ordinateIndex = _ordinateIndexTable[(Int32)ordinate];
            return (Double)_coordinates[index, ordinateIndex];
        }

        internal BufferedCoordinate GetZero()
        {
            return getVertexInternal(0, 0);
        }

        internal BufferedCoordinate Add(BufferedCoordinate a, BufferedCoordinate b)
        {
            return _ops.Add(a, b);
            //return getVertexInternal(a.X + b.X, a.Y + b.Y);
        }

        internal BufferedCoordinate Add(BufferedCoordinate a, Double b)
        {
            return _ops.ScalarAdd(a, b);
            //return getVertexInternal(a.X + b, a.Y + b);
        }

        //internal static BufferedCoordinate Divide(BufferedCoordinate a, BufferedCoordinate b)
        //{
        //    throw new NotImplementedException();
        //}

        internal BufferedCoordinate Divide(BufferedCoordinate a, Double b)
        {
            return _ops.ScalarMultiply(a, 1 / b);
        }

        internal Double Distance(BufferedCoordinate a, BufferedCoordinate b)
        {
            // the Euclidian norm over the vector difference
            return _ops.TwoNorm(_ops.Subtract(a, b));
        }

        internal BufferedCoordinate GetOne()
        {
            return getVertexInternal(1, 1);
        }

        internal Double Dot(BufferedCoordinate a, BufferedCoordinate b)
        {
            return (Double)_ops.Dot(a, b);
        }

        internal BufferedCoordinate Cross(BufferedCoordinate a, BufferedCoordinate b)
        {
            return _ops.Cross(a, b);
        }

        internal static Int32 Compare(BufferedCoordinate a, BufferedCoordinate b)
        {
            Pair<Double> aValues = new Pair<Double>(a.X, a.Y);
            Pair<Double> bValues = new Pair<Double>(b.X, b.Y);
            return _valueComparer.Compare(aValues, bValues);
        }

        internal static Boolean GreaterThan(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) > 0;
        }

        internal static Boolean GreaterThanOrEqualTo(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) >= 0;
        }

        internal static Boolean LessThan(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) < 0;
        }

        internal static Boolean LessThanOrEqualTo(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) <= 0;
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private Boolean isValidVertex(BufferedCoordinate vector)
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

        private BufferedCoordinate getVertexInternal(Double x, Double y)
        {
            return getVertexInternal(x, y, 1);
        }

        private BufferedCoordinate getVertexInternal(Double x, Double y, Double w)
        {
            if (Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(w))
            {
                throw new InvalidOperationException("Vertex components can't be NaN.");
            }

            x = _precisionModel.MakePrecise(x);
            y = _precisionModel.MakePrecise(y);

            BufferedCoordinate v;

            // TODO: locking the entire read/write is too pessimistic, and serializes 
            // multiple readers - need to get a lock which allows this, or change the 
            // design to put the factory index into a load state and then freeze it, 
            // or allow some kind of user-chosen transaction policy (like databases)
            _spinLock.Enter();

            if (w != 1.0)
            {
                w = _precisionModel.MakePrecise(w);

                v = findExisting(x, y, w) ?? addNew(x, y, w);
            }
            else
            {
                v = findExisting(x, y) ?? addNew(x, y);
            }

            _spinLock.Exit();

            return v;
        }

        private BufferedCoordinate? findExisting(Double x, Double y)
        {
            BufferedCoordinate? v = null;
            Int32 index;

            if (_lexicographicVertexIndex.TryGetValue(new Pair<Double>(x, y), out index))
            {
                v = new BufferedCoordinate(this, index);
            }

            return v;
        }

        private BufferedCoordinate? findExisting(Double x, Double y, Double w)
        {
            BufferedCoordinate? v = null;
            Int32 index;

            if (_lexicographicHomogeneousVertexIndex.TryGetValue(new Triple<Double>(x, y, w), out index))
            {
                v = new BufferedCoordinate(this, index);
            }

            return v;
        }

        private BufferedCoordinate addNew(Double x, Double y)
        {
            BufferedCoordinate coord = _coordinates.Add(x, y, 1);
            _lexicographicVertexIndex[new Pair<Double>(coord.X, coord.Y)] = coord.Index;
            return coord;
        }

        private BufferedCoordinate addNew(Double x, Double y, Double w)
        {
            BufferedCoordinate coord = _coordinates.Add(x, y, w);
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

        private class LexicographicCoordinateComparer : IComparer<BufferedCoordinate>
        {
            private readonly LexicographicComparer _valueComparer;

            public LexicographicCoordinateComparer(LexicographicComparer valueComparer)
            {
                _valueComparer = valueComparer;
            }

            #region IComparer<BufferedCoordinate> Members

            public Int32 Compare(BufferedCoordinate a, BufferedCoordinate b)
            {
                return a.ComponentCount == 3
                    ? _valueComparer.Compare(new Triple<Double>(a.X, a.Y, a[Ordinates.W]),
                                             new Triple<Double>(b.X, b.Y, b[Ordinates.W]))
                    : _valueComparer.Compare(new Pair<Double>(a.X, a.Y),
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

                if (result != 0)
                {
                    return result;
                }

                return v1.Third.CompareTo(v2.Third);
            }

            #endregion
        }
    }
}
