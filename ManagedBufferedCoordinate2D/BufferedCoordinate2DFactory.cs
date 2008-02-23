using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;
using GeoAPI.DataStructures;

namespace NetTopologySuite.Coordinates
{
    public class BufferedCoordinate2DFactory
        : ICoordinateFactory<BufferedCoordinate2D>, IVectorBuffer<BufferedCoordinate2D, DoubleComponent>,
          IBufferedVectorFactory<BufferedCoordinate2D, DoubleComponent>
    {
        private readonly ManagedVectorBuffer<BufferedCoordinate2D, DoubleComponent> _coordinates;
        private readonly IList<BufferedCoordinate2D> _lexicographicVertexIndex = new List<BufferedCoordinate2D>();

        public BufferedCoordinate2DFactory()
        {
            _coordinates = new ManagedVectorBuffer<BufferedCoordinate2D, DoubleComponent>(2, true, this);
        }

        public IVectorBuffer<BufferedCoordinate2D, DoubleComponent> VectorBuffer
        {
            get { return _coordinates; }
        }

        #region ICoordinateFactory<BufferedCoordinate2D> Members

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
            throw new NotImplementedException();
        }

        ICoordinate ICoordinateFactory.Dehomogenize(ICoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVectorBuffer<BufferedCoordinate2D,DoubleComponent> Members

        public Int32 Add(IVector<DoubleComponent> vector)
        {
            Double x = (Double)vector[0];
            Double y = (Double)vector[1];

            BufferedCoordinate2D v = getVertexInternal(x, y);

            return v.Index;
        }

        public Int32 Add(BufferedCoordinate2D vector)
        {
            if (isValidVertex(vector))
            {
                return vector.Index;
            }
            else
            {
                return getVertexInternal(vector.X, vector.Y).Index;
            }
        }

        private Boolean isValidVertex(BufferedCoordinate2D vector)
        {
            return ReferenceEquals(vector.Factory, this)
                   && vector.Index < _coordinates.Count
                   && vector.X == _coordinates[vector.Index].X
                   && vector.Y == _coordinates[vector.Index].Y;
        }

        public BufferedCoordinate2D Add(params DoubleComponent[] components)
        {
            if (components.Length != 2)
            {
                throw new ArgumentException("A BufferedCoordinate2D can only have two components.");
            }

            return getVertexInternal((Double)components[0], (Double)components[1]);
        }

        public void Clear()
        {
            _coordinates.Clear();
        }

        public Boolean Contains(IVector<DoubleComponent> item)
        {
            return _coordinates.Contains(item);
        }

        public Boolean Contains(BufferedCoordinate2D item)
        {
            return _coordinates.Contains(item);
        }

        public void CopyTo(BufferedCoordinate2D[] array, Int32 startIndex, Int32 endIndex)
        {
            _coordinates.CopyTo(array, startIndex, endIndex);
        }

        public Int32 Count
        {
            get { return _coordinates.Count; }
        }

        public IVectorFactory<BufferedCoordinate2D, DoubleComponent> Factory
        {
            get { return _coordinates.Factory; }
        }

        public Boolean IsReadOnly
        {
            get { return _coordinates.IsReadOnly; }
        }

        public Int32 MaximumSize
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

        public void Remove(Int32 index)
        {
            _coordinates.Remove(index);
        }

        public event EventHandler SizeIncreased;

        public event CancelEventHandler SizeIncreasing;

        public event EventHandler<VectorOperationEventArgs<BufferedCoordinate2D, DoubleComponent>> VectorChanged;

        public Int32 VectorLength
        {
            get { return 2; }
        }

        public BufferedCoordinate2D this[Int32 index]
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
            throw new NotImplementedException();
        }

        #endregion

        #region IBufferedVectorFactory<BufferedCoordinate2D,DoubleComponent> Members

        public BufferedCoordinate2D CreateBufferedVector(IVectorBuffer<BufferedCoordinate2D, DoubleComponent> vectorBuffer, Int32 index)
        {
            if (!ReferenceEquals(_coordinates, vectorBuffer) && !ReferenceEquals(this, vectorBuffer))
            {
                throw new ArgumentException("The buffer must be this BufferedCoordinate2DFactory.");
            }

            return new BufferedCoordinate2D(this, index);
        }

        #endregion

        internal Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            if (ordinate == Ordinates.X)
            {
                return (Double)_coordinates[index, 0];
            }
            else if (ordinate == Ordinates.Y)
            {
                return (Double)_coordinates[index, 1];
            }
            else
            {
                throw new NotSupportedException("Ordinate not supported: " + ordinate);
            }
        }

        internal ICoordinate GetZero()
        {
            return getVertexInternal(0, 0);
        }

        internal BufferedCoordinate2D Add(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            return getVertexInternal(a.X + b.X, a.Y + b.Y);
        }

        internal BufferedCoordinate2D Divide(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException();
        }

        internal BufferedCoordinate2D GetOne()
        {
            return getVertexInternal(1, 1);
        }

        internal BufferedCoordinate2D Multiply(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException();
        }

        internal Boolean GreaterThan(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException();
        }

        internal Boolean GreaterThanOrEqualTo(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException();
        }

        internal Boolean LessThan(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException();
        }

        internal Boolean LessThanOrEqualTo(BufferedCoordinate2D a, BufferedCoordinate2D b)
        {
            throw new NotImplementedException();
        }

        internal BufferedCoordinate2D Divide(BufferedCoordinate2D a, Double b)
        {
            return getVertexInternal(a.X / b, a.Y / b);
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private BufferedCoordinate2D getVertexInternal(Double x, Double y)
        {
            Int32 index;
            BufferedCoordinate2D v;

            if (findExisting(x, y, out index))
            {
                v = _coordinates[index];
            }
            else
            {
                v = addNew(x, y, index);
            }

            return v;
        }

        private Boolean findExisting(Double x, Double y, out Int32 index)
        {
            Pair<Double> coord = new Pair<Double>(x, y);
            Pair<Int32> range = new Pair<Int32>(0, _lexicographicVertexIndex.Count);
            return findExisting(coord, range, out index);
        }

        private Boolean findExisting(Pair<Double> coord, Pair<Int32> range, out Int32 index)
        {
            Double x = coord.First;
            Double y = coord.Second;

            Int32 indexStart = range.First;
            Int32 indexEnd = range.Second;

            Debug.Assert(indexStart <= indexEnd);

            if (indexEnd == indexStart)
            {
                index = indexEnd;

                if (index >= _coordinates.Count)
                {
                    return false;
                }

                BufferedCoordinate2D vertex = _coordinates[index];

                if (vertex.X == x && vertex.Y == y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            Int32 midPoint = (indexEnd + indexStart) / 2;

            BufferedCoordinate2D midVertex = _coordinates[midPoint];

            Int32 compareResult = compareLexicographically(
                new Pair<Double>(midVertex.X, midVertex.Y), coord);

            if (compareResult < 0)
            {
                if (midPoint == indexStart)
                {
                    index = midPoint;
                    return false;
                }
                else
                {
                    return findExisting(coord, new Pair<Int32>(indexStart, midPoint - 1), out index);
                }
            }
            else
            {
                if (compareResult > 0)
                {
                    Pair<Int32> newRange = new Pair<Int32>((midPoint + 1 + indexEnd) / 2, indexEnd);
                    return findExisting(coord, newRange, out index);
                }
                else // compareResult == 0
                {
                    index = midVertex.Index;
                    return true;
                }
            }
        }

        private static Int32 compareLexicographically(Pair<Double> v1, Pair<Double> v2)
        {
            if (v1.First < v2.First)
            {
                return -1;
            }
            else
            {
                if (v1.First > v2.First)
                {
                    return 1;
                }
                else // v1.First == v2.First
                {
                    return v1.Second.CompareTo(v2.Second);
                }
            }
        }

        private BufferedCoordinate2D addNew(Double x, Double y, Int32 index)
        {
            BufferedCoordinate2D coord = _coordinates.Add(x, y, 1);
            _lexicographicVertexIndex.Insert(index, coord);
            return coord;
        }

        private BufferedCoordinate2D import(BufferedCoordinate2D coord)
        {
            if (!ReferenceEquals(coord.Factory, this))
            {
                return getVertexInternal(coord.X, coord.Y);
            }

            return coord;
        }
    }
}
