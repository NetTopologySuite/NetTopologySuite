using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    public class BufferedCoordinate2DSequence : ICoordinateSequence<BufferedCoordinate2D>
    {
        private readonly IVectorBuffer<BufferedCoordinate2D, DoubleComponent> _buffer;
        private readonly BufferedCoordinate2DSequenceFactory _factory;
        private readonly List<Int32> _sequence;

        internal BufferedCoordinate2DSequence(BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
            : this(8, factory, buffer) { }

        internal BufferedCoordinate2DSequence(Int32 size, BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
        {
            _factory = factory;
            _buffer = buffer;
            _sequence = new List<Int32>(size);
        }

        #region ICoordinateSequence<BufferedCoordinate2D> Members

        public ICoordinateSequenceFactory<BufferedCoordinate2D> CoordinateSequenceFactory
        {
            get { return _factory; }
        }

        public BufferedCoordinate2D[] ToArray()
        {
            BufferedCoordinate2D[] array = new BufferedCoordinate2D[Count];

            for (Int32 i = 0; i < Count; i++)
            {
                Int32 coordIndex = _sequence[i];
                BufferedCoordinate2D coord = _buffer[coordIndex];
                array[i] = coord;
            }

            return array;
        }

        public void Add(IEnumerable<BufferedCoordinate2D> coordinates, Boolean allowRepeated, Boolean reverse)
        {
            throw new NotImplementedException();
        }

        public void Add(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> AddSequence(ICoordinateSequence<BufferedCoordinate2D> sequence)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate2D> AsSet()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _sequence.Clear();
        }

        public void CloseRing()
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public Int32 CompareTo(ICoordinateSequence<BufferedCoordinate2D> other)
        {
            throw new NotImplementedException();
        }

        public ICoordinateFactory<BufferedCoordinate2D> CoordinateFactory
        {
            get { throw new NotImplementedException(); }
        }

        public void CopyTo(BufferedCoordinate2D[] array, Int32 arrayIndex)
        {
            throw new NotImplementedException();
        }

        public Int32 Count
        {
            get { return _sequence.Count; }
        }

        public Int32 Dimension
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean Equals(ICoordinateSequence<BufferedCoordinate2D> other)
        {
            throw new NotImplementedException();
        }

        public IExtents<BufferedCoordinate2D> ExpandEnvelope(IExtents<BufferedCoordinate2D> extents)
        {
            IExtents<BufferedCoordinate2D> expanded = extents;
            expanded.ExpandToInclude(this);
            return expanded;
        }

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Boolean HasRepeatedCoordinates
        {
            get { throw new NotImplementedException(); }
        }

        public Int32 IncreasingDirection
        {
            get { throw new NotImplementedException(); }
        }

        public Int32 IndexOf(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public void Insert(Int32 index, BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D this[Int32 index]
        {
            get
            {
                checkIndex(index, "index");
                return _buffer[_sequence[index]];
            }
            set
            {
                checkWritable();
                checkIndex(index, "index");

                _sequence[index] = _buffer.Add(value);
            }
        }

        public Boolean IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public Double this[Int32 index, Ordinates ordinate]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public BufferedCoordinate2D Maximum()
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> Merge(ICoordinateSequence<BufferedCoordinate2D> other)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D Minimum()
        {
            throw new NotImplementedException();
        }

        public Boolean Remove(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(Int32 index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reverses the coordinates in a sequence in-place.
        /// </summary>
        public void Reverse()
        {
            Int32 last = Count - 1;
            Int32 mid = last / 2;

            for (Int32 i = 0; i <= mid; i++)
            {
                swap(i, last - i);
            }
        }

        public ICoordinateSequence<BufferedCoordinate2D> Reversed
        {
            get { throw new NotImplementedException(); }
        }

        public void Scroll(BufferedCoordinate2D coordinateToBecomeFirst)
        {
            throw new NotImplementedException();
        }

        public void Scroll(Int32 indexToBecomeFirst)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> Slice(Int32 startIndex, Int32 endIndex)
        {
            throw new NotImplementedException();
        }

        public void Sort(Int32 start, Int32 end, IComparer<BufferedCoordinate2D> comparer)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> WithoutDuplicatePoints()
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> WithoutRepeatedPoints()
        {
            throw new NotImplementedException();
        }

        public event EventHandler SequenceChanged;

        #endregion

        IExtents ICoordinateSequence.ExpandEnvelope(IExtents env)
        {
            throw new NotImplementedException();
        }

        ICoordinateSequence ICoordinateSequence.Merge(ICoordinateSequence other)
        {
            throw new NotImplementedException();
        }

        ICoordinate[] ICoordinateSequence.ToArray()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        ICoordinate ICoordinateSequence.this[Int32 index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        Object IList.this[Int32 index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        Int32 IList.Add(Object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(Object value)
        {
            throw new NotImplementedException();
        }

        Boolean IList.Contains(Object value)
        {
            throw new NotImplementedException();
        }

        Int32 IList.IndexOf(Object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(Int32 index, Object value)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, Int32 index)
        {
            throw new NotImplementedException();
        }

        Boolean ICollection.IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        Object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        Object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Swaps two coordinates in a sequence.
        /// </summary>
        private void swap(Int32 i, Int32 j)
        {
            //checkIndex(i, "i");
            //checkIndex(j, "j");

            if (i == j)
            {
                return;
            }

            Int32 temp = _sequence[i];
            _sequence[i] = _sequence[j];
            _sequence[j] = temp;
        }

        private void checkWritable()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(
                    "The coordinate sequence is read-only.");
            }
        }

        private void checkIndex(Int32 index, String parameterName)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(parameterName, index,
                                                      "Index must be between 0 and Count - 1.");
            }
        }

        #region ICoordinateSequence<BufferedCoordinate2D> Members


        public ICoordinateSequence<BufferedCoordinate2D> Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinateSequence Members


        ICoordinateSequence ICoordinateSequence.Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}