using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate2D>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate2D>;

    public class BufferedCoordinate2DSequence : IBufferedCoordSequence
    {
        private readonly IVectorBuffer<BufferedCoordinate2D, DoubleComponent> _buffer;
        private readonly BufferedCoordinate2DSequenceFactory _factory;
        private readonly List<Int32> _sequence;
        private Boolean _isFrozen;
        private Int32 _max = -1;
        private Int32 _min = -1;

        internal BufferedCoordinate2DSequence(BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
            : this(0, factory, buffer) { }

        internal BufferedCoordinate2DSequence(Int32 size, BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
        {
            _factory = factory;
            _buffer = buffer;
            _sequence = new List<Int32>(Math.Max(size, 8));

            for (Int32 i = 0; i < size; i++)
            {
                _sequence.Add(-1);
            }
        }

        #region IBufferedCoordSequence Members

        public IBufferedCoordSequenceFactory CoordinateSequenceFactory
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

        public void Add(BufferedCoordinate2D item)
        {
            checkFrozen();
            _sequence.Add(item.Index);
            OnSequenceChanged();
        }

        public void AddRange(IEnumerable<BufferedCoordinate2D> coordinates,
                             Boolean allowRepeated, 
                             Boolean reverse)
        {
            checkFrozen();

            if (reverse)
            {
                coordinates = Enumerable.Reverse(coordinates);
            }

            Int32 lastIndex = -1;

            foreach (BufferedCoordinate2D coordinate in coordinates)
            {
                if (allowRepeated)
                {
                    if (lastIndex == -1)
                    {
                        lastIndex = coordinate.Index;
                    }
                    else if (lastIndex == coordinate.Index)
                    {
                        continue;
                    }
                }

                _sequence.Add(coordinate.Index);
            }

            OnSequenceChanged();
        }

        public void AddRange(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            checkFrozen();

            foreach (BufferedCoordinate2D coordinate in coordinates)
            {
                _sequence.Add(coordinate.Index);
            }

            OnSequenceChanged();
        }

        public IBufferedCoordSequence AddSequence(IBufferedCoordSequence sequence)
        {
            if (sequence == null) throw new ArgumentNullException("sequence");

            checkFrozen();

            _sequence.Capacity = Math.Max(_sequence.Capacity,
                sequence.Count - (_sequence.Capacity - Count));

            BufferedCoordinate2DSequence buf2DSeq = sequence as BufferedCoordinate2DSequence;

            if (sequence == null)
            {
                foreach (BufferedCoordinate2D coordinate2D in sequence)
                {
                    Add(coordinate2D);
                }
            }
            else
            {
                _sequence.AddRange(buf2DSeq._sequence);
            }

            OnSequenceChanged();
            return this;
        }

        public ISet<BufferedCoordinate2D> AsSet()
        {
            return new BufferedCoordinate2DSet(this, _factory, _buffer);
        }

        public void Clear()
        {
            _sequence.Clear();
            OnSequenceChanged();
        }

        public IBufferedCoordSequence Clone()
        {
            BufferedCoordinate2DSequence clone
                = new BufferedCoordinate2DSequence(_factory, _buffer);

            clone._sequence.AddRange(_sequence);

            return clone;
        }

        public void CloseRing()
        {
            checkFrozen();

            if (Count == 0)
            {
                return;
            }

            if (_sequence[0] != _sequence[Count - 1])
            {
                _sequence.Add(_sequence[0]);
                OnSequenceChanged();
            }
        }

        public Boolean Contains(BufferedCoordinate2D item)
        {
            Int32 coordIndex = item.Index;
            return _sequence.Contains(coordIndex);
        }

        public Int32 CompareTo(IBufferedCoordSequence other)
        {
            Int32 size1 = Count;
            Int32 size2 = other.Count;

            Int32 dim1 = (Int32)Dimension;
            Int32 dim2 = (Int32)other.Dimension;

            // lower dimension is less than higher
            if (dim1 < dim2) return -1;
            if (dim1 > dim2) return 1;

            // lexicographic ordering of point sequences
            Int32 i = 0;

            while (i < size1 && i < size2)
            {
                Int32 ptComp = this[i].CompareTo(other[i]);

                if (ptComp != 0)
                {
                    return ptComp;
                }

                i++;
            }

            if (i < size1) return 1;
            if (i < size2) return -1;

            return 0;
        }

        public IBufferedCoordFactory CoordinateFactory
        {
            get { return _factory.CoordinateFactory; }
        }

        public void CopyTo(BufferedCoordinate2D[] array, Int32 arrayIndex)
        {
            checkCopyToParameters(array, arrayIndex, "arrayIndex");

            for (Int32 index = 0; index < Count; index++)
            {
                array[index + arrayIndex] = this[index];
            }
        }

        public Int32 Count
        {
            get { return _sequence.Count; }
        }

        public CoordinateDimensions Dimension
        {
            get { return CoordinateDimensions.Two; }
        }

        public Boolean Equals(IBufferedCoordSequence other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (Count != other.Count)
            {
                return false;
            }

            BufferedCoordinate2DSequence buf2DSeq = other as BufferedCoordinate2DSequence;

            Int32 count = Count;

            if (ReferenceEquals(buf2DSeq, null))
            {
                for (Int32 index = 0; index < count; index++)
                {
                    if (!this[index].Equals(other[index]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (Int32 index = 0; index < count; index++)
                {
                    if (_sequence[index] != buf2DSeq._sequence[index])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IExtents<BufferedCoordinate2D> ExpandExtents(
            IExtents<BufferedCoordinate2D> extents)
        {
            IExtents<BufferedCoordinate2D> expanded = extents;
            expanded.ExpandToInclude(this);
            return expanded;
        }

        public void Freeze()
        {
            _isFrozen = true;
        }

        public Boolean IsFrozen
        {
            get { return _isFrozen; }
        }

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            foreach (Int32 index in _sequence)
            {
                yield return _buffer[index];
            }
        }

        public Boolean HasRepeatedCoordinates
        {
            get
            {
                Int32 lastValue = -1;

                for (int index = 0; index < Count; index++)
                {
                    Int32 currentValue = _sequence[index];

                    if (lastValue == currentValue)
                    {
                        return true;
                    }

                    lastValue = currentValue;
                }

                return false;
            }
        }

        public Int32 IncreasingDirection
        {
            get
            {
                Int32 midPoint = Count / 2;

                for (Int32 index = 0; index < midPoint; index++)
                {
                    Int32 j = Count - 1 - index;

                    // reflecting about midpoint, compare the coordinates
                    Int32 comp = this[index].CompareTo(this[j]);

                    if (comp != 0)
                    {
                        return comp;
                    }
                }

                // must be a palindrome - defined to be in positive direction
                return 1;
            }
        }

        public Int32 IndexOf(BufferedCoordinate2D item)
        {
            Int32 coordIndex = item.Index;
            return _sequence.IndexOf(coordIndex);
        }

        public void Insert(Int32 index, BufferedCoordinate2D item)
        {
            checkFrozen();
            _sequence.Insert(index, item.Index);
            OnSequenceChanged();
        }

        public BufferedCoordinate2D this[Int32 index]
        {
            get
            {
                checkIndex(index, "index");
                Int32 bufferIndex = _sequence[index];

                return bufferIndex < 0 
                    ? new BufferedCoordinate2D() 
                    : _buffer[bufferIndex];
            }
            set
            {
                checkFrozen();
                checkIndex(index, "index");

                _sequence[index] = _buffer.Add(value);
                OnSequenceChanged();
            }
        }

        public Boolean IsFixedSize
        {
            get { return IsFrozen; }
        }

        public Boolean IsReadOnly
        {
            get { return IsFrozen; }
        }

        public Double this[Int32 index, Ordinates ordinate]
        {
            get
            {
                checkIndex(index, "index");
                checkOrdinate(ordinate);

                return this[index][ordinate];
            }
            set
            {
                checkFrozen();
                checkIndex(index, "index");
                checkOrdinate(ordinate);

                throw new NotImplementedException();
                //onSequenceChanged();
            }
        }

        public BufferedCoordinate2D Maximum()
        {
            if (_max < 0)
            {
                Int32 maxIndex = -1;
                BufferedCoordinate2D maxCoord = new BufferedCoordinate2D();

                for (int i = 0; i < Count; i++)
                {
                    BufferedCoordinate2D current = this[i];

                    if (maxCoord.IsEmpty || current.GreaterThan(maxCoord))
                    {
                        maxIndex = i;
                        maxCoord = current;
                    }
                }

                _max = maxIndex;
            }

            return this[_max];
        }

        public IBufferedCoordSequence Merge(IBufferedCoordSequence other)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D Minimum()
        {
            if (_min < 0)
            {
                Int32 minIndex = -1;
                BufferedCoordinate2D? minCoord = null;

                for (int i = 0; i < Count; i++)
                {
                    BufferedCoordinate2D current = this[i];

                    if (minCoord == null || current.LessThan(minCoord.Value))
                    {
                        minIndex = i;
                        minCoord = current;
                    }
                }

                _min = minIndex;
            }

            return this[_min];
        }

        public Boolean Remove(BufferedCoordinate2D item)
        {
            checkFrozen();
            Boolean result = _sequence.Remove(item.Index);
            OnSequenceChanged();
            return result;
        }

        public void RemoveAt(Int32 index)
        {
            checkFrozen();
            _sequence.RemoveAt(index);
            OnSequenceChanged();
        }

        /// <summary>
        /// Reverses the coordinates in a sequence in-place.
        /// </summary>
        public void Reverse()
        {
            checkFrozen();

            Int32 last = Count - 1;
            Int32 mid = last / 2;

            for (Int32 i = 0; i <= mid; i++)
            {
                swap(i, last - i);
            }

            OnSequenceChanged();
        }

        public IBufferedCoordSequence Reversed
        {
            get
            {
                BufferedCoordinate2DSequence reversed = Clone() as BufferedCoordinate2DSequence;
                reversed.Reverse();
                return reversed;
            }
        }

        public void Scroll(BufferedCoordinate2D coordinateToBecomeFirst)
        {
            checkFrozen();
            OnSequenceChanged();
            throw new NotImplementedException();
        }

        public void Scroll(Int32 indexToBecomeFirst)
        {
            checkFrozen();
            OnSequenceChanged();
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Slice(Int32 startIndex, Int32 endIndex)
        {
            checkIndexes(endIndex, startIndex);

            BufferedCoordinate2DSequence seq = new BufferedCoordinate2DSequence(_factory, _buffer);

            for (Int32 i = startIndex; i < endIndex; i++)
            {
                seq._sequence.Add(_sequence[i]);
            }

            return seq;
        }

        public void Sort(Int32 startIndex, Int32 endIndex, IComparer<BufferedCoordinate2D> comparer)
        {
            checkFrozen();

            checkIndexes(endIndex, startIndex);

            if (startIndex == endIndex)
            {
                return;
            }

            List<BufferedCoordinate2D> coords = new List<BufferedCoordinate2D>(endIndex - startIndex);

            for (Int32 i = startIndex; i <= endIndex; i++)
            {
                coords.Add(this[i]);
            }

            coords.Sort(comparer);

            for (Int32 i = startIndex; i <= endIndex; i++)
            {
                this[i] = coords[i];
            }

            OnSequenceChanged();
        }

        public IBufferedCoordSequence WithoutDuplicatePoints()
        {
            Int32[] indexes = _sequence.ToArray();
            Array.Sort(indexes, 0, indexes.Length);
            List<Int32> duplicates = new List<Int32>(32);

            Int32 lastIndex = -1;

            foreach (Int32 index in indexes)
            {
                if (index == lastIndex)
                {
                    duplicates.Add(index);
                }

                lastIndex = index;
            }

            LinkedList<Int32> coordsToFix = new LinkedList<Int32>(_sequence);

            foreach (Int32 duplicate in duplicates)
            {
                coordsToFix.Remove(duplicate);
            }

            BufferedCoordinate2DSequence noDupes = new BufferedCoordinate2DSequence(_factory, _buffer);

            noDupes._sequence.AddRange(coordsToFix);

            return noDupes;
        }

        public IBufferedCoordSequence WithoutRepeatedPoints()
        {
            return _factory.Create(this, false, true);
        }

        public event EventHandler SequenceChanged;

        #endregion

        IExtents ICoordinateSequence.ExpandExtents(IExtents extents)
        {
            IExtents expanded = extents;
            expanded.ExpandToInclude(this);
            return expanded;
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
            return GetEnumerator();
        }

        ICoordinate ICoordinateSequence.this[Int32 index]
        {
            get { throw new NotImplementedException(); }
            set
            {
                checkFrozen();
                throw new NotImplementedException();
                OnSequenceChanged();
            }
        }

        Object IList.this[Int32 index]
        {
            get { throw new NotImplementedException(); }
            set
            {
                checkFrozen();
                throw new NotImplementedException();
                OnSequenceChanged();
            }
        }

        Int32 IList.Add(Object value)
        {
            checkFrozen();

            if (value is BufferedCoordinate2D)
            {
                BufferedCoordinate2D coord = (BufferedCoordinate2D)value;
                Add(coord); // OnSequenceChanged() called here
                return _sequence.Count - 1;
            }

            throw new ArgumentException("Parameter must be a BufferedCoordinate2D.");
        }

        void IList.Remove(Object value)
        {
            checkFrozen();
            throw new NotImplementedException();
            OnSequenceChanged();
        }

        Boolean IList.Contains(Object value)
        {
            if (!(value is BufferedCoordinate2D))
	        {
        		 return false;
	        }

            return Contains((BufferedCoordinate2D) value);
        }

        Int32 IList.IndexOf(Object value)
        {
            if (!(value is BufferedCoordinate2D))
            {
                return -1;
            }

            return IndexOf((BufferedCoordinate2D) value);
        }

        void IList.Insert(Int32 index, Object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            checkFrozen();
            checkIndex(index, "index");
            ICoordinate coord = value as ICoordinate;

            if (coord == null)
            {
                throw new ArgumentException("value must be an ICoordinate instance.");
            }

            Insert(index, _factory.CoordinateFactory.Create(coord));
        }

        void ICollection.CopyTo(Array array, Int32 index)
        {
            checkCopyToParameters(array, index, "index");

            for (int i = 0; i < Count; i++)
            {
                array.SetValue(this[i], i + index);
            }
        }

        Boolean ICollection.IsSynchronized
        {
            get { return false; }
        }

        Object ICollection.SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        ICoordinateSequence ICoordinateSequence.Clone()
        {
            return Clone();
        }

        Object ICloneable.Clone()
        {
            return Clone();
        }

        protected void OnSequenceChanged()
        {
            EventHandler e = SequenceChanged;

            if (e != null)
            {
                e(this, EventArgs.Empty);
            }

            _min = -1;
            _max = -1;
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

        private void checkIndex(Int32 index, String parameterName)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(parameterName, index,
                                                      "Index must be between 0 and "+
                                                      "Count - 1.");
            }
        }

        private void checkOrdinate(Ordinates ordinate)
        {
            if (ordinate == Ordinates.Z || ordinate == Ordinates.M)
            {
                throw new ArgumentOutOfRangeException("ordinate", ordinate,
                                                      "The ICoordinateSequence does " +
                                                      "not have this dimension");
            }
        }

        private void checkFrozen()
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException(
                    "Sequence is frozen and cannot be modified.");
            }
        }

        private void checkIndexes(Int32 endIndex, Int32 startIndex)
        {
            if (endIndex < startIndex)
            {
                throw new ArgumentException(
                    "startIndex must be less than or equal to endIndex.");
            }

            checkIndex(startIndex, "startIndex");
            checkIndex(endIndex, "endIndex");
        }

        private void checkCopyToParameters(Array array, Int32 arrayIndex, String arrayIndexName)
        {
            if (array == null) throw new ArgumentNullException("array");

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    arrayIndexName, arrayIndex, "Index cannot be less than 0");
            }

            if (arrayIndex >= array.Length)
            {
                throw new ArgumentException(
                    "Index is greater than or equal to length of array");
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException(String.Format(
                    "The number of elements to copy is greater than the " +
                    "remaining space of 'array' when starting at '{0}'.", arrayIndexName));
            }
        }
    }
}