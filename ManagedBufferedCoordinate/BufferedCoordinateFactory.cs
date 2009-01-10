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
    using ITypedVectorFactory = IVectorFactory<DoubleComponent, BufferedCoordinate>;
    using ITypedBufferedVectorFactory = IBufferedVectorFactory<DoubleComponent, BufferedCoordinate>;
    using ITypedVectorBuffer = IVectorBuffer<DoubleComponent, BufferedCoordinate>;
    using IMatrixFactoryD = IMatrixFactory<DoubleComponent>;
    using IVectorD = IVector<DoubleComponent>;
    using IVectorFactoryD = IVectorFactory<DoubleComponent>;

    public class BufferedCoordinateFactory
        : IBufferedCoordFactory,
          ITypedVectorBuffer,
          ITypedBufferedVectorFactory,
          ILinearFactory<DoubleComponent, BufferedCoordinate, BufferedMatrix>,
          ILinearFactory<DoubleComponent>
    {

        public struct BufferedCoordinateContext
        {
            private readonly Boolean _hasZ;
            private readonly Boolean _hasW;

            internal BufferedCoordinateContext(Boolean hasZ, Boolean isHomogeneous)
            {
                _hasZ = hasZ;
                _hasW = isHomogeneous;
            }

            public Boolean HasZ { get { return _hasZ; } }
            public Boolean IsHomogeneous { get { return _hasW; } }
        }

        private static readonly Object _nonHomogeneous2DContext
            = new BufferedCoordinateContext(false, false);

        private static readonly Object _nonHomogeneous3DContext
            = new BufferedCoordinateContext(true, false);

        private static readonly Object _homogeneous2DContext
            = new BufferedCoordinateContext(false, true);

        //public static readonly Int32 MaximumBitResolution = 53;
        //private static readonly IComparer<Pair<Double>> _valueComparer
        //    = new LexicographicComparer();

        //private static readonly IComparer<BufferedCoordinate> _coordComparer
        //    = new LexicographicCoordinateComparer((LexicographicComparer)_valueComparer);
        private readonly ManagedVectorBuffer<DoubleComponent, BufferedCoordinate> _coordinates;
        //private Int32 _bitResolution;
        //private Int64 _mask = unchecked((Int64)0xFFFFFFFFFFFFFFFF);
        private readonly PrecisionModel _precisionModel;
        //private readonly Int32[] _ordinateIndexTable = new Int32[4];
        private readonly IMatrixOperations<DoubleComponent, BufferedCoordinate, BufferedMatrix> _ops;
        private readonly YieldingSpinLock _spinLock = new YieldingSpinLock();

        class VertexIndex : IVectorIndex<DoubleComponent>
        {
            private readonly IDictionary<Pair<Double>, Int32> _2Index =
                new Dictionary<Pair<Double>, Int32>();
            private readonly IDictionary<Triple<Double>, Int32> _3Index =
                new Dictionary<Triple<Double>, Int32>();

            public void Add(DoubleComponent v0, DoubleComponent v1, Int32 id)
            {
                _2Index[new Pair<Double>((Double)v0, (Double)v1)] = id;
            }

            public void Add(DoubleComponent[] components, Int32 start, Int32 id)
            {
                throw new NotSupportedException();
            }

            public void Add(DoubleComponent v0, DoubleComponent v1, DoubleComponent v2, Int32 id)
            {
                _3Index[new Triple<Double>((Double)v0, (Double)v1, (Double)v2)] = id;
            }

            #region Implementation of IVectorIndex<DoubleComponent>

            public Boolean Query(DoubleComponent v0, DoubleComponent v1, out Int32 id)
            {
                return _2Index.TryGetValue(new Pair<Double>((Double)v0, (Double)v1), out id);
            }

            public Boolean Query(DoubleComponent v0, DoubleComponent v1, DoubleComponent v2, out Int32 id)
            {
                return _3Index.TryGetValue(new Triple<Double>((Double)v0, (Double)v1, (Double)v2), out id);
            }

            public Boolean Query(out Int32 id, params DoubleComponent[] components)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public BufferedCoordinateFactory()
            : this(null) { }

        public BufferedCoordinateFactory(Double scale)
            : this(new PrecisionModel(null, scale)) { }

        public BufferedCoordinateFactory(PrecisionModelType type)
            : this(new PrecisionModel(null, type)) { }

        public BufferedCoordinateFactory(IPrecisionModel precisionModel)
        {
            _precisionModel = new PrecisionModel(this, precisionModel);
            //_lexicographicVertexIndex = createLexicographicIndex();
            //_lexicographicHomogeneousVertexIndex = createLexicographicHomogeneousIndex();
            _coordinates = new ManagedVectorBuffer<DoubleComponent, BufferedCoordinate>(this);
            _coordinates.Index = new VertexIndex();
            //initializeOrdinateIndexTable();
            _ops = new ClrMatrixOperations<DoubleComponent, BufferedCoordinate, BufferedMatrix>(this);
        }

        public ITypedVectorBuffer VectorBuffer
        {
            get { return this; }
        }

        //internal static IComparer<BufferedCoordinate> Comparer
        //{
        //    get { return _coordComparer; }
        //}

        internal IMatrixOperations<DoubleComponent, BufferedCoordinate, BufferedMatrix> Ops
        {
            get { return _ops; }
        }

        #region IBufferedCoordFactory Members

        public BufferedCoordinate Create(Double x, Double y)
        {
            return getVertexInternal(x, y);
        }

        public BufferedCoordinate Create(Double x, Double y, Double w)
        {
            return getVertexInternal(x, y, w);
        }

        public BufferedCoordinate Create(params Double[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            Int32 length = coordinates.Length;

            switch (length)
            {
                case 0:
                    return new BufferedCoordinate();
                case 1:
                    throw new ArgumentException("Only one coordinate component was provided; " +
                                                "at least 2 are needed.");
                case 2:
                    return Create(coordinates[0], coordinates[1]);
                case 3:
                    return Create(coordinates[0], coordinates[1], coordinates[2]);
                default:
                    throw new ArgumentException("Too many components.");
            }
        }

        public BufferedCoordinate Create3D(Double x, Double y, Double z)
        {
            return getVertexInternal3D(x, y, z);
        }

        public BufferedCoordinate Create3D(Double x, Double y, Double z, Double w)
        {
            return getVertexInternal(x, y, z, w);
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

            if (ReferenceEquals(coordinate.BufferedCoordinateFactory, this))
            {
                return coordinate;
            }

            Double x, y, w;
            coordinate.GetComponents(out x, out y, out w);
            return getVertexInternal(x, y, w);
        }

        public BufferedCoordinate Create(ICoordinate coordinate)
        {
            if (coordinate is BufferedCoordinate)
            {
                return Create((BufferedCoordinate)coordinate);
            }

            ICoordinate2D coordinate2D = coordinate as ICoordinate2D;

            if (coordinate2D == null)
            {
                return coordinate.IsEmpty
                           ? new BufferedCoordinate()
                           : Create(coordinate[Ordinates.X], coordinate[Ordinates.Y], coordinate[Ordinates.W]);
            }

            if (coordinate.IsEmpty)
            {
                return new BufferedCoordinate();
            }

            Double x, y, w;
            coordinate2D.GetComponents(out x, out y, out w);
            return Create(x, y, w);
        }

        public BufferedCoordinate Create3D(BufferedCoordinate coordinate)
        {
            if (coordinate.IsEmpty)
            {
                return new BufferedCoordinate();
            }

            if (ReferenceEquals(coordinate.BufferedCoordinateFactory, this))
            {
                return coordinate;
            }

            Double x, y, z, w;
            coordinate.GetComponents(out x, out y, out z, out w);
            return getVertexInternal(x, y, z, w);
        }

        public BufferedCoordinate Create3D(ICoordinate coordinate)
        {
            if (coordinate is BufferedCoordinate)
            {
                return Create3D((BufferedCoordinate)coordinate);
            }

            ICoordinate3D coordinate3D = coordinate as ICoordinate3D;

            if (coordinate3D == null)
            {
                return coordinate.IsEmpty
                    ? new BufferedCoordinate()
                    : Create(coordinate[Ordinates.X],
                             coordinate[Ordinates.Y],
                             coordinate[Ordinates.Z],
                             coordinate[Ordinates.W]);

            }

            if (coordinate.IsEmpty)
            {
                return new BufferedCoordinate();
            }

            Double x, y, z, w;
            coordinate3D.GetComponents(out x, out y, out z, out w);
            return Create(x, y, z, w);
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

        public BufferedCoordinate Add(Object context, params DoubleComponent[] components)
        {
            throw new System.NotImplementedException();
        }

        Int32 ITypedVectorBuffer.Add(IVectorD vector)
        {
            return ((ITypedVectorBuffer)this).Add(vector, _nonHomogeneous2DContext);
        }

        Int32 ITypedVectorBuffer.Add(IVectorD vector, Object context)
        {
            if (vector == null)
            {
                throw new ArgumentNullException("vector");
            }

            Int32 componentCount = vector.ComponentCount;

            if (componentCount < 2 || componentCount > 4)
            {
                throw new ArgumentException("A BufferedCoordinate requires " +
                                            "2, 3 or 4 components.");
            }

            Double x = (Double)vector[0];
            Double y = (Double)vector[1];

            Double z, w;

            getZW(vector, context, out z, out w);

            BufferedCoordinate v = getVertexInternal(x, y, z, w);

            return v.Index;
        }

        Int32 ITypedVectorBuffer.Add(BufferedCoordinate vector)
        {
            if (isValidVertex(vector))
            {
                return vector.Index;
            }

            return getVertexInternal(vector.X, vector.Y).Index;
        }

        public BufferedCoordinate Add(DoubleComponent v0, DoubleComponent v1)
        {
            return Add(v0, v1, _nonHomogeneous2DContext);
        }

        public BufferedCoordinate Add(DoubleComponent v0, DoubleComponent v1, DoubleComponent v2)
        {
            return Add(v0, v1, v2, _nonHomogeneous2DContext);
        }

        public BufferedCoordinate Add(DoubleComponent v0, DoubleComponent v1, Object context)
        {
            return getVertexInternal((Double)v0, (Double)v1, Double.NaN, Double.NaN);
        }

        public BufferedCoordinate Add(DoubleComponent v0, DoubleComponent v1, DoubleComponent v2, Object context)
        {
            Double z, w;
            getZW(v2, context, out z, out w);
            return getVertexInternal((Double)v0, (Double)v1, z, w);
        }

        BufferedCoordinate ITypedVectorBuffer.Add(params DoubleComponent[] components)
        {
            throw new NotImplementedException("Fix this");
            if (components.Length != 2)
            {
                throw new ArgumentException(
                    "A BufferedCoordinate requires exactly two components.");
            }

            return getVertexInternal((Double)components[0], (Double)components[1]);
        }

        void ITypedVectorBuffer.Clear()
        {
            _coordinates.Clear();
        }

        Boolean ITypedVectorBuffer.Contains(IVectorD item)
        {
            return _coordinates.Contains(item);
        }

        Boolean ITypedVectorBuffer.Contains(BufferedCoordinate item)
        {
            return _coordinates.Contains(item);
        }

        void ITypedVectorBuffer.CopyTo(BufferedCoordinate[] array, Int32 startIndex, Int32 endIndex)
        {
            _coordinates.CopyTo(array, startIndex, endIndex);
        }

        Int32 ITypedVectorBuffer.Count
        {
            get { return _coordinates.Count; }
        }

        public Boolean Find(DoubleComponent v0, DoubleComponent v1, out Int32 id)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Find(DoubleComponent v0, DoubleComponent v1, DoubleComponent v2, out Int32 id)
        {
            throw new System.NotImplementedException();
        }

        //IVectorFactory<DoubleComponent, BufferedCoordinate> ITypedVectorBuffer.BufferedCoordinateFactory
        //{
        //    get { return _coordinates.BufferedCoordinateFactory; }
        //}

        public Int32 GetVectorLength(Int32 index)
        {
            return _coordinates.GetVectorLength(index);
        }

        Boolean ITypedVectorBuffer.IsReadOnly
        {
            get { return _coordinates.IsReadOnly; }
        }

        Int32 ITypedVectorBuffer.MaximumSize
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

        void ITypedVectorBuffer.Remove(Int32 index)
        {
            throw new NotImplementedException();
            //_coordinates.Remove(index); - dangerous
        }

        event EventHandler ITypedVectorBuffer.SizeIncreased
        {
            add { _coordinates.SizeIncreased += value; }
            remove { _coordinates.SizeIncreased -= value; }
        }

        event CancelEventHandler ITypedVectorBuffer.SizeIncreasing
        {
            add { _coordinates.SizeIncreasing += value; }
            remove { _coordinates.SizeIncreasing -= value; }
        }

        event EventHandler<VectorOperationEventArgs<DoubleComponent, BufferedCoordinate>> ITypedVectorBuffer.VectorChanged
        {
            add { _coordinates.VectorChanged += value; }
            remove { _coordinates.VectorChanged -= value; }
        }

        public VectorComparison ComparisonMode
        {
            get { return _coordinates.ComparisonMode; }
            set { _coordinates.ComparisonMode = value; }
        }

        public Int32 Compare(BufferedCoordinate a, BufferedCoordinate b, VectorComparison type)
        {
            return _coordinates.Compare(a, b, type);
        }

        public IVectorIndex<DoubleComponent> Index
        {
            get { return _coordinates.Index; }
            set { _coordinates.Index = value; }
        }

        BufferedCoordinate ITypedVectorBuffer.this[Int32 index]
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

        #region IBufferedVectorFactory<BufferedCoordinate, DoubleComponent> Members

        public BufferedCoordinate CreateBufferedVector(ITypedVectorBuffer vectorBuffer, Int32 index)
        {
            return CreateBufferedVector(vectorBuffer, index, _nonHomogeneous2DContext);
        }

        public BufferedCoordinate CreateBufferedVector(ITypedVectorBuffer vectorBuffer,
                                                       Int32 index,
                                                       Object context)
        {
            if (!ReferenceEquals(_coordinates, vectorBuffer) &&
                !ReferenceEquals(this, vectorBuffer))
            {
                throw new ArgumentException("The buffer must be this " +
                                            "BufferedCoordinateFactory.");
            }

            BufferedCoordinateContext typedContext = (BufferedCoordinateContext)context;
            return new BufferedCoordinate(this, index, typedContext.HasZ, typedContext.IsHomogeneous);
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

        #region IMatrixFactoryD Members
        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(Int32 rowCount, Int32 columnCount, IEnumerable<DoubleComponent> values)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(IMatrix<DoubleComponent> matrix)
        {
            throw new System.NotImplementedException();
        }

        ITransformMatrix<DoubleComponent> IMatrixFactoryD.CreateTransformMatrix(Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        ITransformMatrix<DoubleComponent> IMatrixFactoryD.CreateTransformMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        IAffineTransformMatrix<DoubleComponent> IMatrixFactoryD.CreateAffineMatrix(Int32 rank)
        {
            throw new System.NotImplementedException();
        }

        IAffineTransformMatrix<DoubleComponent> IMatrixFactoryD.CreateAffineMatrix(MatrixFormat format, Int32 rank)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(Int32 rowCount, Int32 columnCount)
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
                    //return CreateVector(components[0], components[1], components[2], components[3]);
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("A BufferedCoordinate must " +
                                                "have only 2, 3 " + /* "or 4 " + */
                                                "components.");
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
            return getVertexInternal((Double)a, (Double)b, (Double)c);
        }

        public BufferedCoordinate CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return getVertexInternal((Double)a, (Double)b);
        }
        #endregion

        #region IBufferedVectorFactory Members

        BufferedCoordinate ITypedVectorFactory.CreateVector(IEnumerable<DoubleComponent> values)
        {
            Pair<DoubleComponent>? pair = Slice.GetPair(values);

            if (pair == null)
            {
                throw new ArgumentException("Must have at least two values.");
            }

            Pair<DoubleComponent> coord = pair.Value;

            return getVertexInternal((Double)coord.First, (Double)coord.Second);
        }

        BufferedCoordinate ITypedVectorFactory.CreateVector(Int32 componentCount)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IVectorFactoryD Members
        IVectorD IVectorFactoryD.CreateVector(Int32 componentCount)
        {
            return CreateVector(componentCount);
        }

        IVectorD IVectorFactoryD.CreateVector(IEnumerable<DoubleComponent> values)
        {
            return CreateVector(Enumerable.ToArray(values));
        }

        IVectorD IVectorFactoryD.CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return CreateVector(a, b);
        }

        IVectorD IVectorFactoryD.CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            return CreateVector(a, b, c);
        }

        IVectorD IVectorFactoryD.CreateVector(params DoubleComponent[] components)
        {
            return CreateVector(components);
        }

        IVectorD IVectorFactoryD.CreateVector(Double a, Double b)
        {
            return CreateVector(a, b);
        }

        IVectorD IVectorFactoryD.CreateVector(Double a, Double b, Double c)
        {
            return CreateVector(a, b, c);
        }

        IVectorD IVectorFactoryD.CreateVector(params Double[] components)
        {
            return CreateVector(components);
        }
        #endregion

        internal Double GetOrdinate(Int32 index, Int32 ordinate)
        {
            return (Double)_coordinates[index, ordinate];
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

        public Int32 Compare(BufferedCoordinate a, BufferedCoordinate b)
        {
            return _coordinates.Compare(a, b);
        }

        internal Boolean GreaterThan(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) > 0;
        }

        internal Boolean GreaterThanOrEqualTo(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) >= 0;
        }

        internal Boolean LessThan(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) < 0;
        }

        internal Boolean LessThanOrEqualTo(BufferedCoordinate a, BufferedCoordinate b)
        {
            return Compare(a, b) <= 0;
        }

        internal void GetComponents(Int32 id, out DoubleComponent x, out DoubleComponent y, out DoubleComponent w)
        {
            _coordinates.GetVectorComponents(id, out x, out y, out w);
        }

        internal void GetComponents(Int32 id, out DoubleComponent x, out DoubleComponent y, out DoubleComponent z, out DoubleComponent w)
        {
            _coordinates.GetVectorComponents(id, out x, out y, out z, out w);
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private Boolean isValidVertex(BufferedCoordinate vector)
        {
            return ReferenceEquals(vector.BufferedCoordinateFactory, this)
                   && vector.Index < _coordinates.Count
                   && vector.X == _coordinates[vector.Index].X
                   && vector.Y == _coordinates[vector.Index].Y;
        }

        private static void getZW(IVectorD vector,
                                  Object context,
                                  out Double z,
                                  out Double w)
        {
            BufferedCoordinateContext? typedContext = (BufferedCoordinateContext?)context;

            z = Double.NaN;
            w = Double.NaN;

            if (typedContext == null)
            {
                return;
            }

            if (typedContext.Value.HasZ)
            {
                z = (Double)vector[2];

                if (typedContext.Value.IsHomogeneous)
                {
                    w = (Double)vector[3];
                }
            }
            else if (typedContext.Value.IsHomogeneous)
            {
                w = (Double)vector[2];
            }
        }

        private void getZW(DoubleComponent component, Object context, out Double z, out Double w)
        {
            BufferedCoordinateContext? typedContext = (BufferedCoordinateContext?)context;

            z = Double.NaN;
            w = Double.NaN;

            if (typedContext == null)
            {
                return;
            }

            if (typedContext.Value.HasZ)
            {
                z = (Double)component;
            }
            else if (typedContext.Value.IsHomogeneous)
            {
                w = (Double)component;
            }
        }

        private BufferedCoordinate getVertexInternal(Double x, Double y)
        {
            return getVertexInternal(x, y, 1);
        }

        private BufferedCoordinate getVertexInternal(Double x, Double y, Double w)
        {
            return getVertexInternal(x, y, Double.NaN, w);
        }

        private BufferedCoordinate getVertexInternal3D(Double x, Double y, Double z)
        {
            return getVertexInternal(x, y, z, 1);
        }

        private BufferedCoordinate getVertexInternal(Double x, Double y, Double z, Double w)
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

            if (!Double.IsNaN(z))
            {
                z = _precisionModel.MakePrecise(z);

                if (w != 1.0)
                {
                    throw new NotImplementedException("3D homogenous points not supported.");
                    //v = findExisting(x, y, z, w) ?? addNew(x, y, z, w);
                }
                else
                {
                    v = findExisting(x, y, z, true) ?? addNew(x, y, z, true);
                }
            }
            else
            {
                if (w != 1.0)
                {
                    v = findExisting(x, y, w, false) ?? addNew(x, y, w, false);
                }
                else
                {
                    v = findExisting(x, y) ?? addNew(x, y);
                }
            }

            _spinLock.Exit();

            return v;
        }

        private static void checkCounts(Int32 rowCount, Int32 columnCount)
        {
            if (rowCount != 3 || rowCount != 4)
            {
                throw new ArgumentOutOfRangeException("rowCount", rowCount, "Must be 3 or 4");
            }

            if (columnCount != 3 || columnCount != 4)
            {
                throw new ArgumentOutOfRangeException("columnCount", columnCount, "Must be 3 or 4");
            }
        }

        private BufferedCoordinate? findExisting(Double v0, Double v1)
        {
            BufferedCoordinate? v = null;
            Int32 id;

            if (_coordinates.Find(v0, v1, out id))
            {
                v = new BufferedCoordinate(this, id, false, false);
            }

            return v;
        }

        private BufferedCoordinate? findExisting(Double v0, Double v1, Double v2, Boolean is3D)
        {
            BufferedCoordinate? v = null;
            Int32 id;

            if (_coordinates.Find(v0, v1, v2, out id))
            {
                v = new BufferedCoordinate(this, id, is3D, !is3D);
            }

            return v;
        }

        private BufferedCoordinate addNew(Double v0, Double v1)
        {
            BufferedCoordinate coord = _coordinates.Add(v0, v1, 1);
            _coordinates.Index.Add(v0, v1, coord.Index);
            //_lexicographicVertexIndex[new Pair<Double>(v0, v1)] = coord.Index;
            return coord;
        }

        private BufferedCoordinate addNew(Double v0, Double v1, Double v2, Boolean is3D)
        {
            Object context = is3D ? _nonHomogeneous3DContext : _homogeneous2DContext;
            BufferedCoordinate coord = _coordinates.Add(v0, v1, v2, context);
            _coordinates.Index.Add(v0, v1, v2, coord.Index);
            //Triple<Double> values = new Triple<Double>(v0, v1, v2);
            //_lexicographicHomogeneousVertexIndex[values] = coord.Index;
            return coord;
        }

        //private BufferedCoordinate addNew3D(Double x, Double y, Double z)
        //{
        //    BufferedCoordinate coord = _coordinates.Add(x, y, z);
        //    //Triple<Double> values = new Triple<Double>(coord.X, coord.Y, coord[Ordinates.W]);
        //    //_lexicographicHomogeneousVertexIndex[values] = coord.Index;
        //    return coord;
        //}

        //private BufferedCoordinate addNew3D(Double x, Double y, Double z, Double w)
        //{
        //    BufferedCoordinate coord = _coordinates.Add(x, y, w);
        //    Triple<Double> values = new Triple<Double>(coord.X, coord.Y, coord[Ordinates.W]);
        //    _lexicographicHomogeneousVertexIndex[values] = coord.Index;
        //    return coord;
        //}

        //private static IDictionary<Pair<Double>, Int32> createLexicographicIndex()
        //{
        //    return new SortedDictionary<Pair<Double>, Int32>(
        //        new LexicographicComparer());
        //}

        //private static IDictionary<Triple<Double>, Int32> createLexicographicHomogeneousIndex()
        //{
        //    return new SortedDictionary<Triple<Double>, Int32>(
        //        new LexicographicComparer());
        //}

        //private void initializeOrdinateIndexTable()
        //{
        //    _ordinateIndexTable[(Int32)Ordinates.X] = 0;
        //    _ordinateIndexTable[(Int32)Ordinates.Y] = 1;
        //    _ordinateIndexTable[(Int32)Ordinates.Z] = -1; // flag value to throw exception.
        //    _ordinateIndexTable[(Int32)Ordinates.W] = 2;
        //}

        //private class LexicographicCoordinateComparer : IComparer<BufferedCoordinate>
        //{
        //    private readonly LexicographicComparer _valueComparer;

        //    public LexicographicCoordinateComparer(LexicographicComparer valueComparer)
        //    {
        //        _valueComparer = valueComparer;
        //    }

        //    #region IComparer<BufferedCoordinate> Members

        //    public Int32 Compare(BufferedCoordinate a, BufferedCoordinate b)
        //    {
        //        switch (a.ComponentCount)
        //        {
        //            case 2:
        //                return _valueComparer.Compare(new Pair<Double>(a.X, a.Y),
        //                                              new Pair<Double>(b.X, b.Y));
        //            case 3:
        //                return _valueComparer.Compare(new Triple<Double>(a.X, a.Y, a[Ordinates.W]),
        //                                              new Triple<Double>(b.X, b.Y, b[Ordinates.W]));
        //            case 4:
        //                break;
        //        }

        //        throw new NotSupportedException("Vector dimension not supported: " + a.ComponentCount);
        //    }

        //    #endregion
        //}

        //private class LexicographicComparer : IComparer<Pair<Double>>, IComparer<Triple<Double>>
        //{
        //    #region IComparer<Pair<Double>> Members

        //    public Int32 Compare(Pair<Double> v1, Pair<Double> v2)
        //    {
        //        Double v1_x = v1.First;
        //        Double v2_x = v2.First;

        //        if (v1_x < v2_x)
        //        {
        //            return -1;
        //        }

        //        if (v1_x > v2_x)
        //        {
        //            return 1;
        //        }

        //        // v1.First == v2.First
        //        return v1.Second.CompareTo(v2.Second);
        //    }

        //    #endregion

        //    #region IComparer<Triple<Double>> Members

        //    public Int32 Compare(Triple<Double> v1, Triple<Double> v2)
        //    {
        //        Int32 result = Compare(new Pair<Double>(v1.First, v2.First),
        //                               new Pair<Double>(v1.Second, v2.Second));

        //        if (result != 0)
        //        {
        //            return result;
        //        }

        //        return v1.Third.CompareTo(v2.Third);
        //    }

        //    #endregion
        //}

        #region IBufferedCoordFactory<BufferedCoordinate> Members


        public BufferedCoordinate Create(IVector<DoubleComponent> coordinate)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinateFactory Members


        ICoordinate ICoordinateFactory.Create(IVector<DoubleComponent> coordinate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
