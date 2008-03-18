using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;
using GeoAPI.DataStructures;
#if NETCF
using BitConverter = GisSharpBlog.NetTopologySuite.Utilities;
#endif

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;

    public class BufferedCoordinate2DFactory
        : IBufferedCoordFactory, IVectorBuffer<BufferedCoordinate2D, DoubleComponent>,
          IBufferedVectorFactory<BufferedCoordinate2D, DoubleComponent>
    {
        public static readonly Int32 MaximumBitResolution = 52;
        private static readonly IComparer<Pair<Double>> _comparer 
            = new LexicographicComparer();
        private readonly ManagedVectorBuffer<BufferedCoordinate2D, DoubleComponent> _coordinates;
        private readonly IDictionary<Pair<Double>, Int32> _lexicographicVertexIndex;
        private Int32 _bitResolution;
        private Int64 _mask = unchecked((Int64)0xFFFFFFFFFFFFFFFF);
        private readonly Int32[] _ordinateIndexTable = new Int32[4];

        public BufferedCoordinate2DFactory()
            : this(MaximumBitResolution) { }

        public BufferedCoordinate2DFactory(Int32 bitResolution)
        {
            _bitResolution = bitResolution;
            _lexicographicVertexIndex = createLexicographicIndex();
            _coordinates = new ManagedVectorBuffer<BufferedCoordinate2D, DoubleComponent>(2, true, this);
            initializeOrdinateIndexTable();
        }

        public Int32 BitResolution
        {
            get { return _bitResolution; }
            set 
            {
                _bitResolution = value;
                Int32 shift = MaximumBitResolution - _bitResolution;
                _mask = unchecked((Int64) (0xFFFFFFFFFFFFFFFF << shift));
            }
        }

        public IVectorBuffer<BufferedCoordinate2D, DoubleComponent> VectorBuffer
        {
            get { return _coordinates; }
        }

        #region IBufferedCoordFactory Members

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

            if (coordinates.Length == 2)
            {
                return Create(coordinates[0], coordinates[1]);
            }

            throw new NotSupportedException("Coordinates with 'M' values currently not supported.");
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
            if (ReferenceEquals(coordinate.Factory, this))
            {
                return coordinate;
            }
            else
            {
                return getVertexInternal(coordinate.X, coordinate.Y);
            }
        }

        public BufferedCoordinate2D Create(ICoordinate coordinate)
        {
            if (coordinate is BufferedCoordinate2D)
            {
                return Create((BufferedCoordinate2D) coordinate);
            }

            return coordinate.IsEmpty 
                ? new BufferedCoordinate2D() 
                : Create(coordinate[Ordinates.X], coordinate[Ordinates.Y]);
        }

        public AffineTransformMatrix<BufferedCoordinate2D> CreateTransform(BufferedCoordinate2D scaleVector, Double rotation, BufferedCoordinate2D translateVector)
        {
            throw new NotImplementedException();
        }

        public AffineTransformMatrix<BufferedCoordinate2D> CreateTransform(BufferedCoordinate2D scaleVector, BufferedCoordinate2D rotationAxis, Double rotation, BufferedCoordinate2D translateVector)
        {
            throw new NotImplementedException();
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
                    "A BufferedCoordinate2D can only have two components.");
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

        IVectorFactory<BufferedCoordinate2D, DoubleComponent> IVectorBuffer<BufferedCoordinate2D, DoubleComponent>.Factory
        {
            get { return _coordinates.Factory; }
        }

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
            _coordinates.Remove(index);
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
                _coordinates[index] = value;
            }
        }

        #endregion

        #region IEnumerable<BufferedCoordinate2D> Members

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            foreach (BufferedCoordinate2D coordinate in _coordinates)
            {
                yield return coordinate;
            }
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

        internal Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            try
            {
                Int32 ordinateIndex = _ordinateIndexTable[(Int32) ordinate];
                return (Double) _coordinates[index, ordinateIndex];
            }
            catch(ArgumentOutOfRangeException)
            {
                throw new NotSupportedException("Ordinate not supported: " + ordinate);
            }
        }

        internal BufferedCoordinate2D GetZero()
        {
            return getVertexInternal(0, 0);
        }

        internal BufferedCoordinate2D Add(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return getVertexInternal(a.X + b.X, a.Y + b.Y);
        }

        internal static BufferedCoordinate2D Divide(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotSupportedException();
        }

        internal BufferedCoordinate2D Divide(BufferedCoordinate2D a, Double b)
        {
            return getVertexInternal(a.X / b, a.Y / b);
        }

        internal BufferedCoordinate2D GetOne()
        {
            return getVertexInternal(1, 1);
        }

        internal static BufferedCoordinate2D Multiply(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException("Cross-product not implemented");
        }

        internal static Int32 Compare(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            Pair<Double> aValues = new Pair<Double>(a.X, a.Y);
            Pair<Double> bValues = new Pair<Double>(b.X, b.Y);
            return _comparer.Compare(aValues, bValues);
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

        private BufferedCoordinate2D getVertexInternal(Double x, Double y)
        {
            Int64 xBits = BitConverter.DoubleToInt64Bits(x);
            xBits &= _mask;
            x = BitConverter.Int64BitsToDouble(xBits);

            Int64 yBits = BitConverter.DoubleToInt64Bits(y);
            yBits &= _mask;
            y = BitConverter.Int64BitsToDouble(yBits);

            BufferedCoordinate2D v = findExisting(x, y) ?? addNew(x, y);

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

        private BufferedCoordinate2D addNew(Double x, Double y)
        {
            BufferedCoordinate2D coord = _coordinates.Add(x, y, 1);
            _lexicographicVertexIndex[new Pair<Double>(coord.X, coord.Y)] = coord.Index;
            return coord;
        }

        private void initializeOrdinateIndexTable()
        {
            _ordinateIndexTable[(Int32)Ordinates.X] = 0;
            _ordinateIndexTable[(Int32)Ordinates.Y] = 1;
            _ordinateIndexTable[(Int32)Ordinates.Z] = -1; // flag value to throw exception.
            _ordinateIndexTable[(Int32)Ordinates.W] = 2;
        }

        class LexicographicComparer : IComparer<Pair<Double>>
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
                else
                {
                    if (v1_x > v2_x)
                    {
                        return 1;
                    }
                    else // v1.First == v2.First
                    {
                        return v1.Second.CompareTo(v2.Second);
                    }
                }
            }

            #endregion
        }
    }
}
