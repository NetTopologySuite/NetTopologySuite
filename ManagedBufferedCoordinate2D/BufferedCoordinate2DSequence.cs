using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate2D>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate2D>;
    using GeoAPI.DataStructures;

    public class BufferedCoordinate2DSequence : IBufferedCoordSequence
    {
        private enum SequenceStorage
        {
            Unknown = 0,
            PrependList,
            MainList,
            AppendList
        }

        private readonly IVectorBuffer<BufferedCoordinate2D, DoubleComponent> _buffer;
        private readonly BufferedCoordinate2DSequenceFactory _factory;
        private List<Int32> _sequence;
        private Boolean _reversed;
        private Int32 _startIndex = -1;
        private Int32 _endIndex = -1;
        private List<Int32> _appendedIndexes;
        private List<Int32> _prependedIndexes;
        private SortedSet<Int32> _skipIndexes;
        private Boolean _isFrozen;
        private Int32 _max = -1;
        private Int32 _min = -1;

        internal BufferedCoordinate2DSequence(BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
            : this(0, factory, buffer) { }

        internal BufferedCoordinate2DSequence(Int32 size, BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
        {
            if (size < 0) throw new ArgumentOutOfRangeException("size", size,
                                                                 "Size should be greater " +
                                                                 "than 0");
            _factory = factory;
            _buffer = buffer;
            _sequence = new List<Int32>(Math.Max(size, 8));

            for (Int32 i = 0; i < size; i++)
            {
                _sequence.Add(-1);
            }
        }

        //internal BufferedCoordinate2DSequence(List<Int32> sequence,
        //                                      BufferedCoordinate2DSequenceFactory factory,
        //                                      IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
        //    : this(false, sequence, null, null, null, null, null, factory, buffer) { }

        internal BufferedCoordinate2DSequence(Boolean reverse,
                                              List<Int32> sequence,
                                              List<Int32> prepended,
                                              List<Int32> appended,
                                              SortedSet<Int32> skips,
                                              Int32? startIndex,
                                              Int32? endIndex,
                                              BufferedCoordinate2DSequenceFactory factory,
                                              IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
        {
            _reversed = reverse;
            _startIndex = startIndex ?? _startIndex;
            _endIndex = endIndex ?? _endIndex;
            _factory = factory;
            _buffer = buffer;
            _sequence = sequence;

            if (prepended != null)
            {
                _prependedIndexes = new List<Int32>(prepended);
            }

            if (appended != null)
            {
                _appendedIndexes = new List<Int32>(appended);
            }

            if (skips != null)
            {
                _skipIndexes = new SortedSet<Int32>(skips);
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            
            if (_reversed)
            {
                buffer.Append("Reversed ");
            }

            if (isSlice())
            {
                buffer.AppendFormat("Sliced ([{0} - {1}]", _startIndex, _endIndex);

                if (_prependedIndexes != null)
                {
                    buffer.AppendFormat(", {0} Prepended", _prependedIndexes.Count);
                }

                if (_appendedIndexes != null)
                {
                    buffer.AppendFormat(", {0} Appended", _appendedIndexes.Count);
                }

                buffer.Append(") ");
            }
            
            buffer.Append(IsFrozen ? "Frozen " : String.Empty);
            buffer.AppendFormat("Points: {0}", Count);

            return buffer.ToString();
        }

        #region IBufferedCoordSequence Members

        public IBufferedCoordSequence Append(BufferedCoordinate2D coordinate)
        {
            if (coordinate.IsEmpty)
            {
                throw new ArgumentException("Coordinate cannot be empty.");
            }

            Int32 coordIndex = coordinate.Index;
            appendCoordIndex(coordIndex);
            return this;
        }

        public IBufferedCoordSequence Append(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            BufferedCoordinate2DSequence seq = coordinates as BufferedCoordinate2DSequence;

            if (seq != null)
            {
                appendInternal(seq);
            }
            else
            {
                foreach (BufferedCoordinate2D coordinate in coordinates)
                {
                    addInternal(coordinate);
                }
            }

            OnSequenceChanged();

            return this;
        }

        public IBufferedCoordSequence Append(IBufferedCoordSequence coordinates)
        {
            BufferedCoordinate2DSequence seq = coordinates as BufferedCoordinate2DSequence;

            if (seq != null)
            {
                appendInternal(seq);
                return this;
            }

            return Append((IEnumerable<BufferedCoordinate2D>)coordinates);
        }

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

            if (item.IsEmpty)
            {
                throw new ArgumentException("Cannot add the empty " +
                                            "coordinate to a sequence.");
            }

            addInternal(item);
            OnSequenceChanged();
        }

        public IBufferedCoordSequence AddRange(IEnumerable<BufferedCoordinate2D> coordinates,
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
                if (!allowRepeated)
                {
                    if (lastIndex >= 0 && lastIndex == coordinate.Index)
                    {
                        lastIndex = coordinate.Index;
                        continue;
                    }

                    lastIndex = coordinate.Index;
                }

                addInternal(coordinate);
            }

            OnSequenceChanged();

            return this;
        }

        public IBufferedCoordSequence AddRange(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            checkFrozen();

            foreach (BufferedCoordinate2D coordinate in coordinates)
            {
                addInternal(coordinate);
            }

            OnSequenceChanged();

            return this;
        }

        public IBufferedCoordSequence AddSequence(IBufferedCoordSequence sequence)
        {
            if (sequence == null) throw new ArgumentNullException("sequence");

            checkFrozen();

            _sequence.Capacity = Math.Max(_sequence.Capacity,
                                          sequence.Count - (_sequence.Capacity - Count));

            BufferedCoordinate2DSequence buf2DSeq = sequence as BufferedCoordinate2DSequence;

            // if we share a buffer, we can just import the indexes
            if (buf2DSeq != null && buf2DSeq._buffer == _buffer)
            {
                _sequence.AddRange(buf2DSeq._sequence);
            }
            else
            {
                foreach (BufferedCoordinate2D coordinate in sequence)
                {
                    addInternal(coordinate);
                }
            }

            OnSequenceChanged();
            return this;
        }

        public ISet<BufferedCoordinate2D> AsSet()
        {
            return new BufferedCoordinate2DSet(this, _factory, _buffer);
        }

        public IBufferedCoordSequence Clear()
        {
            _sequence.Clear();
            OnSequenceChanged();
            return this;
        }

        public IBufferedCoordSequence Clone()
        {
            BufferedCoordinate2DSequence clone
                = new BufferedCoordinate2DSequence(_factory, _buffer);

            clone._sequence.AddRange(_sequence);

            return clone;
        }

        public IBufferedCoordSequence CloseRing()
        {
            checkFrozen();

            if (Count < 3)
            {
                throw new InvalidOperationException(
                    "The coordinate sequence has less than 3 points, " +
                    "and cannot be a ring.");
            }

            if (!First.Equals(Last))
            {
                Add(First);
                OnSequenceChanged();
            }

            return this;
        }

        public Boolean Contains(BufferedCoordinate2D item)
        {
            if (!ReferenceEquals(item.Factory, _factory.CoordinateFactory))
            {
                return false;
            }

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
            get
            {
                if (isSlice())
                {
                    return computeSliceCount();
                }

                return _sequence.Count +
                       (_appendedIndexes == null ? 0 : _appendedIndexes.Count) +
                       (_prependedIndexes == null ? 0 : _prependedIndexes.Count) -
                       (_skipIndexes == null ? 0 : _skipIndexes.Count);
            }
        }

        public CoordinateDimensions Dimension
        {
            get { return CoordinateDimensions.Two; }
        }

        public Boolean Equals(IBufferedCoordSequence other)
        {
            return Equals(other, Tolerance.Zero);
        }

        public Boolean Equals(IBufferedCoordSequence other, Tolerance tolerance)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (Count != other.Count)
            {
                return false;
            }

            BufferedCoordinate2DSequence buf2DSeq
                = other as BufferedCoordinate2DSequence;

            Int32 count = Count;

            if (ReferenceEquals(buf2DSeq, null))
            {
                for (Int32 index = 0; index < count; index++)
                {
                    if (this[index].Equals(other[index]))
                    {
                        continue;
                    }

                    if (!tolerance.Equal(0, this[index].Distance(other[index])))
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

        public BufferedCoordinate2D First
        {
            get { return Count > 0 ? this[0] : new BufferedCoordinate2D(); }
        }

        public IBufferedCoordSequence Freeze()
        {
            _isFrozen = true;
            return this;
        }

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            Int32 count = Count;

            for (Int32 i = 0; i < count; i++)
            {
                Int32 index = i;
                SequenceStorage storage = transformIndex(index, out index);
                Int32 bufferIndex = getStorageValue(storage, index);
                yield return _buffer[bufferIndex];
            }
        }

        public Boolean HasRepeatedCoordinates
        {
            get
            {
                Int32 lastValue = -1;

                for (Int32 index = 0; index < Count; index++)
                {
                    Int32 transformedIndex;
                    SequenceStorage storage = transformIndex(index, out transformedIndex);

                    Int32 currentValue = getStorageValue(storage, transformedIndex);

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
                    Int32 comp = this[j].CompareTo(this[index]);

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
            Int32 index;

            List<Int32> firstList;
            SequenceStorage storage;

            if (_reversed)
            {
                firstList = _appendedIndexes;
                storage = SequenceStorage.AppendList;
            }
            else
            {
                firstList = _prependedIndexes;
                storage = SequenceStorage.PrependList;
            }

            if (firstList != null)
            {
                index = firstList.IndexOf(coordIndex);

                if (index >= 0)
                {
                    return inverseTransformIndex(index, storage);
                }
            }

            Int32 start = computeSliceStartOnMainSequence();
            Int32 end = computeSliceEndOnMainSequence();
            Int32 count = end - start + 1;

            index = _sequence.IndexOf(coordIndex, start, count);

            if (index >= 0)
            {
                return inverseTransformIndex(index, SequenceStorage.MainList);
            }

            List<Int32> lastList;

            if (_reversed)
            {
                lastList = _prependedIndexes;
                storage = SequenceStorage.PrependList;
            }
            else
            {
                lastList = _appendedIndexes;
                storage = SequenceStorage.AppendList;
            }

            if (lastList != null)
            {
                index = lastList.IndexOf(coordIndex);

                if (index >= 0)
                {
                    return inverseTransformIndex(index, storage);
                }
            }

            return -1;
        }

        public IBufferedCoordSequence Insert(Int32 index, BufferedCoordinate2D item)
        {
            checkFrozen();

            Boolean isSlice = this.isSlice();

            if (isSlice && (index > 0 && index <= LastIndex))
            {
                throw new NotSupportedException(
                    "Inserting into a sliced coordinate sequence not supported. " +
                    "Use the coordinate sequence factory to create a new sequence " +
                    "from this slice in order to insert coordinates at this index.");
            }

            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException("index", index,
                                                      "Index must be between 0 and Count.");
            }

            if (index == 0)
            {
                Prepend(item);
            }
            else if (index > LastIndex)
            {
                Append(item);
            }
            else
            {
                SequenceStorage storage = transformIndex(index, out index);
                List<Int32> list = getStorage(storage);
                list.Insert(index, item.Index);
            }

            OnSequenceChanged();
            return this;
        }

        public BufferedCoordinate2D this[Int32 index]
        {
            get
            {
                SequenceStorage storage = checkAndTransformIndex(index,
                                                                 "index",
                                                                 out index);
                Int32 bufferIndex = getStorageValue(storage, index);

                return bufferIndex < 0
                    ? new BufferedCoordinate2D()
                    : _buffer[bufferIndex];
            }
            set
            {
                checkFrozen();

                SequenceStorage storage = checkAndTransformIndex(index,
                                                                 "index",
                                                                 out index);
                List<Int32> list = getStorage(storage);

                // TODO: I can't figure out a test to prove the defect in the 
                // following commented-out line...

                //_sequence[index] = _buffer.Add(value);

                Debug.Assert(list != null);
                list[index] = _factory.CoordinateFactory.Create(value).Index;

                OnSequenceChanged();
            }
        }

        public Double this[Int32 index, Ordinates ordinate]
        {
            get
            {
                checkOrdinate(ordinate);

                return this[index][ordinate];
            }
            set
            {
                checkFrozen();
                checkOrdinate(ordinate);

                throw new NotImplementedException();
                //onSequenceChanged();
            }
        }

        public Boolean IsFixedSize
        {
            get { return IsFrozen; }
        }

        public Boolean IsFrozen
        {
            get { return _isFrozen; }
        }

        public Boolean IsReadOnly
        {
            get { return IsFrozen; }
        }

        public BufferedCoordinate2D Last
        {
            get { return Count > 0 ? this[Count - 1] : new BufferedCoordinate2D(); }
        }

        public Int32 LastIndex
        {
            get { return Count - 1; }
        }

        public BufferedCoordinate2D Maximum
        {
            get
            {
                if (_max < 0)
                {
                    Int32 maxIndex = -1;
                    BufferedCoordinate2D maxCoord = new BufferedCoordinate2D();

                    if (Count < 1)
                    {
                        return maxCoord;
                    }


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
        }

        public IBufferedCoordSequence Merge(IBufferedCoordSequence other)
        {
            IBufferedCoordSequence seq = _factory.Create(this);
            return seq.Append(other);
        }

        public BufferedCoordinate2D Minimum
        {
            get
            {
                if (Count < 1)
                {
                    return new BufferedCoordinate2D();
                }

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
        }

        public IBufferedCoordSequence Prepend(BufferedCoordinate2D coordinate)
        {
            if (coordinate.IsEmpty)
            {
                throw new ArgumentException("Coordinate cannot be empty.");
            }

            Int32 coordIndex = coordinate.Index;
            prependCoordIndex(coordIndex);
            return this;
        }

        public IBufferedCoordSequence Prepend(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            BufferedCoordinate2DSequence seq = coordinates as BufferedCoordinate2DSequence;

            if (seq != null)
            {
                prependInternal(seq);
            }
            else
            {
                foreach (BufferedCoordinate2D coordinate in Enumerable.Reverse(coordinates))
                {
                    Prepend(coordinate);
                }
            }

            return this;
        }

        public IBufferedCoordSequence Prepend(IBufferedCoordSequence coordinates)
        {
            BufferedCoordinate2DSequence seq = coordinates as BufferedCoordinate2DSequence;

            if (seq != null)
            {
                prependInternal(seq);
                return this;
            }

            return Prepend((IEnumerable<BufferedCoordinate2D>)coordinates);
        }

        public Boolean Remove(BufferedCoordinate2D item)
        {
            checkFrozen();

            if (item.IsEmpty)
            {
                return false;
            }

            Boolean result = false;
            Int32 coordIndex = item.Index;

            if (_reversed)
            {
                if (_appendedIndexes != null && _appendedIndexes.Remove(coordIndex))
                {
                    result = true;
                }
            }
            else
            {
                if (_prependedIndexes != null)
                {
                    Int32 lastIndex = _prependedIndexes.LastIndexOf(coordIndex);

                    if (lastIndex > -1)
                    {
                        _prependedIndexes.RemoveAt(lastIndex);
                        result = true;
                    }
                }
            }

            if (isSlice())
            {
                Int32 mainIndex;
                if ((_skipIndexes == null || !_skipIndexes.Contains(coordIndex)) &&
                    (mainIndex = _sequence.IndexOf(coordIndex)) > -1)
                {
                    _skipIndexes = _skipIndexes ?? new SortedSet<Int32>();
                    _skipIndexes.Add(mainIndex);
                    result = true;
                }
            }
            else
            {
                result = _sequence.Remove(coordIndex);
            }

            if (!result)
            {
                if (_reversed)
                {
                    if (_prependedIndexes != null)
                    {
                        Int32 lastIndex = _prependedIndexes.LastIndexOf(coordIndex);

                        if (lastIndex > -1)
                        {
                            _prependedIndexes.RemoveAt(lastIndex);
                            result = true;
                        }
                    }
                }
                else
                {
                    if (_appendedIndexes != null && _appendedIndexes.Remove(coordIndex))
                    {
                        result = true;
                    }
                }
            }

            if (result)
            {
                OnSequenceChanged();
            }

            return result;
        }

        public IBufferedCoordSequence RemoveAt(Int32 index)
        {
            checkFrozen();
            SequenceStorage storage = checkAndTransformIndex(index, "index", out index);

            if (isSlice() || storage == SequenceStorage.MainList)
            {
                skipIndex(index);
            }
            else
            {
                List<Int32> list = getStorage(storage);
                list.RemoveAt(index);
            }

            OnSequenceChanged();
            return this;
        }

        /// <summary>
        /// Reverses the coordinates in a sequence in-place.
        /// </summary>
        public IBufferedCoordSequence Reverse()
        {
            checkFrozen();

            _reversed = !_reversed;

            OnSequenceChanged();

            return this;
        }

        public IBufferedCoordSequence Reversed
        {
            get
            {
                IBufferedCoordSequence reversed = Clone();
                reversed.Reverse();
                return reversed;
            }
        }

        public IBufferedCoordSequence Scroll(BufferedCoordinate2D coordinateToBecomeFirst)
        {
            Int32 index = IndexOf(coordinateToBecomeFirst);

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("coordinateToBecomeFirst",
                                                      coordinateToBecomeFirst,
                                                      "Coordinate not found.");
            }

            return Scroll(index);
        }

        public IBufferedCoordSequence Scroll(Int32 indexToBecomeFirst)
        {
            checkIndex(indexToBecomeFirst, "indexToBecomeFirst");

            // TODO: consider using an internal start offset instead of a copy
            // also, not using the indexer on this sequence would boost performance.
            // 
            // This is a rather naive implementation, which should be adequate, as 
            // this method is used infrequently.
            checkFrozen();
            Int32 count = Count;
            List<Int32> scrolled = new List<Int32>(count);

            for (Int32 i = indexToBecomeFirst; i < count; i++)
            {
                scrolled.Add(this[i].Index);
            }

            for (int i = 0; i < indexToBecomeFirst; i++)
            {
                scrolled.Add(this[i].Index);
            }

            _prependedIndexes = null;
            _appendedIndexes = null;
            _skipIndexes = null;
            _startIndex = -1;
            _endIndex = -1;
            _reversed = false;
            _sequence = scrolled;

            OnSequenceChanged();

            return this;
        }

        public Pair<BufferedCoordinate2D> SegmentAt(Int32 index)
        {
            if (index < 0 || index >= LastIndex)
            {
                throw new ArgumentOutOfRangeException("index", index,
                    "Index must be between 0 and LastIndex - 1");
            }

            return new Pair<BufferedCoordinate2D>(this[index], this[index + 1]);
        }

        public IBufferedCoordSequence Slice(Int32 startIndex, Int32 endIndex)
        {
            checkIndexes(endIndex, startIndex);
            Freeze();

            List<Int32> prepended = null;
            List<Int32> appended = null;

            Int32 transformedStart, transformedEnd;
            SequenceStorage startStorage, endStorage;

            startStorage = transformIndex(startIndex, out transformedStart);
            endStorage = transformIndex(endIndex, out transformedEnd);

            Int32 mainSequenceStart, mainSequenceEnd;

            switch (startStorage)
            {
                case SequenceStorage.PrependList:
                    {
                        Int32 start = transformedStart;
                        Int32 end = (endStorage == SequenceStorage.PrependList)
                                                        ? transformedEnd
                                                        : 0;
                        Int32 count = Math.Abs(end - start) + 1;
                        prepended = new List<Int32>(count);

                        for (Int32 i = start; i <= end; i++)
                        {
                            prepended.Add(_prependedIndexes[i]);
                        }

                        // if the entire slice is contained in the prepended
                        // coordinates, then just create a new main sequence on it.
                        if (endStorage == SequenceStorage.PrependList)
                        {
                            return new BufferedCoordinate2DSequence(_reversed,
                                                                    prepended,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    0,
                                                                    count - 1,
                                                                    _factory,
                                                                    _buffer);
                        }
                    }
                    break;
                case SequenceStorage.AppendList:
                    {
                        Int32 start = transformedStart;
                        Int32 end = (startStorage == SequenceStorage.AppendList)
                                                        ? transformedEnd
                                                        : _appendedIndexes.Count;
                        Int32 count = Math.Abs(end - start) + 1;
                        appended = new List<Int32>(count);

                        for (Int32 i = 0; i < count; i++)
                        {
                            appended.Add(_appendedIndexes[i]);
                        }

                        // if the entire slice is contained in the appended
                        // coordinates, then just create a new main sequence on it.
                        if (endStorage == SequenceStorage.AppendList)
                        {
                            return new BufferedCoordinate2DSequence(_reversed,
                                                                    appended,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    0,
                                                                    count - 1,
                                                                    _factory,
                                                                    _buffer);
                        }
                    }
                    break;
                default:
                    break;
            }

            SortedSet<Int32> sliceSkips = null;
            Func<Int32> generator = null;
            Predicate<Int32> condition = null;

            // handle the case slice starts and stops in main
            if (startStorage == SequenceStorage.MainList &&
               endStorage == SequenceStorage.MainList)
            {
                if (_skipIndexes != null)
                {
                    Int32 i = transformedStart;
                    generator = () => i++;
                    condition = v => (v <= transformedEnd);
                    sliceSkips = new SortedSet<Int32>();
                    sliceSkips.AddRange(Set<Int32>
                                            .Create(generator, condition)
                                            .Intersect(_skipIndexes));
                }

                return new BufferedCoordinate2DSequence(_reversed,
                                                        _sequence,
                                                        null,
                                                        null,
                                                        sliceSkips,
                                                        transformedStart,
                                                        transformedEnd,
                                                        _factory, _buffer);
            }

            // handle the 3 cases where the slice intersects the main
            // sequence: either it does so completely, 
            // partially including the start, or partially including the end.
            if (startStorage != SequenceStorage.MainList &&
                endStorage != SequenceStorage.MainList)
            {
                mainSequenceStart = _startIndex;
                mainSequenceEnd = _endIndex;

                if (_skipIndexes != null)
                {
                    sliceSkips = new SortedSet<Int32>();
                    sliceSkips.AddRange(_skipIndexes);
                }
            }
            else
            {
                if (startStorage == SequenceStorage.MainList)
                {
                    if (_reversed)
                    {
                        mainSequenceStart = computeSliceStartOnMainSequence();
                        mainSequenceEnd = transformedStart;
                    }
                    else
                    {
                        mainSequenceStart = transformedStart;
                        mainSequenceEnd = computeSliceEndOnMainSequence();
                    }

                    Int32 i = transformedStart;
                    generator = () => i++;
                    condition = v => (v <= _sequence.Count);
                }
                else if (endStorage == SequenceStorage.MainList)
                {
                    if (_reversed)
                    {
                        mainSequenceStart = transformedEnd;
                        mainSequenceEnd = computeSliceStartOnMainSequence();
                    }
                    else
                    {
                        mainSequenceStart = computeSliceStartOnMainSequence();
                        mainSequenceEnd = transformedEnd;
                    }

                    Int32 i = 0;
                    generator = () => i++;
                    condition = v => (v <= transformedEnd);
                }
                else
                {
                    mainSequenceStart = -1;
                    mainSequenceEnd = -1;
                    Debug.Fail("Should never reach here.");
                }

                if (_skipIndexes != null)
                {
                    sliceSkips = new SortedSet<Int32>();
                    sliceSkips.AddRange(Set<Int32>
                                            .Create(generator, condition)
                                            .Intersect(_skipIndexes));
                }
            }


            // The two cases where the slice starts and ends completely 
            // within the prepended or appended cooridnates are already handled
            switch (endStorage)
            {
                case SequenceStorage.PrependList:
                    {
                        prepended = new List<Int32>();

                        for (Int32 i = 0; i <= transformedEnd; i++)
                        {
                            prepended.Add(_prependedIndexes[i]);
                        }
                    }
                    break;
                case SequenceStorage.AppendList:
                    {
                        appended = new List<Int32>();

                        for (Int32 i = 0; i <= transformedEnd; i++)
                        {
                            appended.Add(_appendedIndexes[i]);
                        }
                    }
                    break;
                default:
                    break;
            }

            return new BufferedCoordinate2DSequence(_reversed,
                                                    _sequence,
                                                    prepended,
                                                    appended,
                                                    sliceSkips,
                                                    mainSequenceStart,
                                                    mainSequenceEnd,
                                                    _factory,
                                                    _buffer);
        }

        public IBufferedCoordSequence Sort()
        {
            Sort(0, LastIndex, _factory.DefaultComparer);
            return this;
        }

        public IBufferedCoordSequence Sort(Int32 startIndex, Int32 endIndex)
        {
            Sort(startIndex, endIndex, _factory.DefaultComparer);
            return this;
        }

        public IBufferedCoordSequence Sort(Int32 startIndex, Int32 endIndex, IComparer<BufferedCoordinate2D> comparer)
        {
            if (isSlice())
            {
                throw new NotSupportedException("Sorting a slice is not supported.");
            }

            checkFrozen();

            checkIndexes(endIndex, startIndex);

            if (startIndex == endIndex)
            {
                return this;
            }

            List<BufferedCoordinate2D> coords = new List<BufferedCoordinate2D>(endIndex - startIndex + 1);

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
            return this;
        }

        public IBufferedCoordSequence Splice(IEnumerable<BufferedCoordinate2D> coordinates,
                                             Int32 startIndex,
                                             Int32 endIndex)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            seq.Prepend(coordinates);

            return seq;
        }

        public IBufferedCoordSequence Splice(BufferedCoordinate2D coordinate,
                                             Int32 startIndex,
                                             Int32 endIndex)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            seq.Prepend(coordinate);

            return seq;
        }

        public IBufferedCoordSequence Splice(IEnumerable<BufferedCoordinate2D> startCoordinates,
                                             Int32 startIndex,
                                             Int32 endIndex,
                                             BufferedCoordinate2D endCoordinate)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            return seq.Prepend(startCoordinates).Append(endCoordinate);
        }

        public IBufferedCoordSequence Splice(BufferedCoordinate2D startCoordinate,
                                             Int32 startIndex,
                                             Int32 endIndex,
                                             BufferedCoordinate2D endCoordinate)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            return seq.Prepend(startCoordinate).Append(endCoordinate);
        }

        public IBufferedCoordSequence Splice(IEnumerable<BufferedCoordinate2D> startCoordinates,
                                             Int32 startIndex,
                                             Int32 endIndex,
                                             IEnumerable<BufferedCoordinate2D> endCoordinates)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            return seq.Prepend(startCoordinates).Append(endCoordinates);
        }

        public IBufferedCoordSequence Splice(BufferedCoordinate2D startCoordinate,
                                             Int32 startIndex,
                                             Int32 endIndex,
                                             IEnumerable<BufferedCoordinate2D> endCoordinates)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            return seq.Prepend(startCoordinate).Append(endCoordinates);
        }

        public IBufferedCoordSequence Splice(Int32 startIndex,
                                             Int32 endIndex,
                                             IEnumerable<BufferedCoordinate2D> coordinates)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            return seq.Append(coordinates);
        }

        public IBufferedCoordSequence Splice(Int32 startIndex,
                                             Int32 endIndex,
                                             BufferedCoordinate2D coordinate)
        {
            BufferedCoordinate2DSequence seq = createSliceInternal(endIndex, startIndex);

            return seq.Append(coordinate);
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

        #region Explicit ICoordinateSequence Members
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

        Pair<ICoordinate> ICoordinateSequence.SegmentAt(Int32 index)
        {
            if (index < 0 || index >= LastIndex)
            {
                throw new ArgumentOutOfRangeException("index", index,
                    "Index must be between 0 and LastIndex - 1");
            }

            return new Pair<ICoordinate>(this[index], this[index + 1]);
        }

        ICoordinate[] ICoordinateSequence.ToArray()
        {
            ICoordinate[] array = new ICoordinate[Count];

            for (Int32 i = 0; i < Count; i++)
            {
                Int32 coordIndex = _sequence[i];
                ICoordinate coord = _buffer[coordIndex];
                array[i] = coord;
            }

            return array;
        }

        ICoordinate ICoordinateSequence.First
        {
            get { return First; }
        }

        ICoordinate ICoordinateSequence.Last
        {
            get { return Last; }
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

        Boolean ICoordinateSequence.Equals(ICoordinateSequence other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        ICoordinateSequence ICoordinateSequence.Freeze()
        {
            return Freeze();
        }

        ICoordinate ICoordinateSequence.Maximum
        {
            get { return Maximum; }
        }

        ICoordinate ICoordinateSequence.Minimum
        {
            get { return Minimum; }
        }

        ICoordinateSequence ICoordinateSequence.Reverse()
        {
            return Reverse();
        }

        ICoordinateSequence ICoordinateSequence.Reversed
        {
            get { return Reversed; }
        }

        #endregion

        #region Explicit IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Explicit IList Members
        Object IList.this[Int32 index]
        {
            get { return this[index]; }
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

            return Contains((BufferedCoordinate2D)value);
        }

        Int32 IList.IndexOf(Object value)
        {
            if (!(value is BufferedCoordinate2D))
            {
                return -1;
            }

            return IndexOf((BufferedCoordinate2D)value);
        }

        void IList.Insert(Int32 index, Object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            //checkFrozen();
            //index = checkAndTransformIndex(index, "index");
            ICoordinate coord = value as ICoordinate;

            if (coord == null)
            {
                throw new ArgumentException("value must be an ICoordinate instance.");
            }

            Insert(index, _factory.CoordinateFactory.Create(coord));
        }

        void IList.Clear()
        {
            Clear();
        }

        void IList.RemoveAt(Int32 index)
        {
            RemoveAt(index);
        }
        #endregion

        #region Explicit ICollection Members
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
        #endregion

        #region Explicit IList<BufferedCoordinate2D> Members

        void IList<BufferedCoordinate2D>.Insert(Int32 index, BufferedCoordinate2D item)
        {
            Insert(index, item);
        }

        void IList<BufferedCoordinate2D>.RemoveAt(Int32 index)
        {
            RemoveAt(index);
        }

        #endregion

        #region Explicit ICollection<BufferedCoordinate2D> Members

        void ICollection<BufferedCoordinate2D>.Clear()
        {
            Clear();
        }

        #endregion

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

        protected void SetSequenceInternal(BufferedCoordinate2DSequence sequence)
        {
            if (sequence.IsFrozen)
            {
                _sequence = sequence._sequence;
            }
            else
            {
                _sequence.Clear();
                _sequence.AddRange(sequence._sequence);
            }

            _startIndex = sequence._startIndex;
            _endIndex = sequence._endIndex;
            _reversed = sequence._reversed;
            _isFrozen = sequence._isFrozen;
            _appendedIndexes = sequence._appendedIndexes == null
                                        ? null
                                        : new List<Int32>(sequence._appendedIndexes);
            _prependedIndexes = sequence._prependedIndexes == null
                                        ? null
                                        : new List<Int32>(sequence._prependedIndexes);
            _skipIndexes = sequence._skipIndexes == null
                                        ? null
                                        : new SortedSet<Int32>(sequence._skipIndexes);
            _min = sequence._min;
            _max = sequence._max;
        }

        #region Private helper members
        //private void swap(Int32 i, Int32 j)
        //{
        //    //checkIndex(i, "i");
        //    //checkIndex(j, "j");

        //    if (i == j)
        //    {
        //        return;
        //    }

        //    Int32 temp = _sequence[i];
        //    _sequence[i] = _sequence[j];
        //    _sequence[j] = temp;
        //}

        private SequenceStorage checkAndTransformIndex(Int32 index, String parameterName, out Int32 transformedIndex)
        {
            checkIndex(index, parameterName);
            return transformIndex(index, out transformedIndex);
        }

        private void checkIndex(Int32 index, String parameterName)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(parameterName, index,
                                                      "Index must be between 0 and " +
                                                      "Count - 1.");
            }
        }

        private Int32 inverseTransformIndex(Int32 index, SequenceStorage storage)
        {
            index = index - Math.Max(0, _startIndex);
            return _reversed ? (Count - 1) - index : index;
        }

        private SequenceStorage transformIndex(Int32 index, out Int32 transformedIndex)
        {
            // First, project index on reversed sequence, if needed.
            // Since the index is reversed, we don't need to reverse
            // the prepend and append lists.
            Int32 projectedIndex = _reversed ? (Count - 1) - index : index;

            // Get the count of the indexes before the main sequence
            Int32 prependCount = _prependedIndexes == null
                                 ? 0
                                 : _prependedIndexes.Count;

            SequenceStorage storage;

            // If the index is smaller than the prepended count, 
            // index into the prepended storage
            if (projectedIndex < prependCount)
            {
                storage = SequenceStorage.PrependList;
                transformedIndex = prependCount - projectedIndex - 1;
            }
            else
            {
                Int32 endIndex = computeSliceEndOnMainSequence();
                Int32 startIndex = computeSliceStartOnMainSequence();
                Int32 skipCount = _skipIndexes == null ? 0 : _skipIndexes.Count;
                Int32 mainSequenceCount = (endIndex - startIndex) + 1 - skipCount;
                Int32 firstAppendIndex = prependCount + mainSequenceCount;

                // If the index is greater or equal to the sum of both
                // prepended and main sequence slices, then it must index
                // into the appended list
                if (projectedIndex >= firstAppendIndex)
                {
                    storage = SequenceStorage.AppendList;
                    transformedIndex = projectedIndex - firstAppendIndex;
                }
                else
                {
                    storage = SequenceStorage.MainList;
                    Int32 mainIndex = projectedIndex - prependCount + startIndex;

                    if (_skipIndexes != null)
                    {
                        Int32 skips = 0;
                        Int32 lastSkips;

                        do
                        {
                            lastSkips = skips;
                            skips = _skipIndexes.CountAtAndBefore(mainIndex + skips);
                        } while (lastSkips != skips);

                        mainIndex += skips;
                    }

                    transformedIndex = mainIndex;
                }
            }

            return storage;
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

        private Boolean isSlice()
        {
            return _startIndex >= 0 || _endIndex >= 0;
        }

        private void addInternal(BufferedCoordinate2D item)
        {
            if (!ReferenceEquals(item.Factory, _factory.CoordinateFactory))
            {
                item = _factory.CoordinateFactory.Create(item);
            }

            Int32 index = item.Index;

            appendCoordIndex(index);
        }

        private void appendCoordIndex(Int32 coordIndex)
        {
            // if we are already appending indexes, put it in the 
            // appropriate appending list... 
            if (_reversed && _prependedIndexes != null)
            {
                _prependedIndexes.Add(coordIndex);
                return;
            }

            if (!_reversed && _appendedIndexes != null)
            {
                _appendedIndexes.Add(coordIndex);
                return;
            }

            // not a slice, treat the whole sequence
            if (!isSlice())
            {
                if (_sequence.Count == 0)
                {
                    _sequence.Add(coordIndex);
                    return;
                }

                if (_reversed)
                {
                    // if we are appending to a reversed sequence, we
                    // really want to prepend... however this would
                    // translate into an insert (read: copy), which we are currently
                    // avoiding. Thus, add to the prepend list. 
                    // TODO: consider a heuristic to judge when an insert
                    // to the head of a list would be better than a multiple list
                    // sequence.
                    Debug.Assert(_prependedIndexes == null);
                    _prependedIndexes = new List<Int32>();
                    _prependedIndexes.Add(coordIndex);
                }
                else
                {
                    Debug.Assert(_appendedIndexes == null);
                    _sequence.Add(coordIndex);
                }

                return;
            }

            // project index to slice
            Int32 transformedIndex;
            transformIndex(LastIndex, out transformedIndex);

            // if the next coord in the sequence is the same, just adjust
            // the end of the slice
            if (_reversed)
            {
                if (transformedIndex > 0 &&
                    _startIndex >= 0 &&
                    _sequence[transformedIndex - 1] == coordIndex)
                {
                    _startIndex--;
                }
                else
                {
                    _prependedIndexes = new List<Int32>();
                    _prependedIndexes.Add(coordIndex);
                }
            }
            else
            {
                if (transformedIndex < _sequence.Count - 1 &&
                    _endIndex >= 0 &&
                    _sequence[transformedIndex + 1] == coordIndex)
                {
                    _endIndex++;
                }
                else
                {
                    _appendedIndexes = new List<Int32>();
                    _appendedIndexes.Add(coordIndex);
                }
            }
        }

        private void prependCoordIndex(Int32 coordIndex)
        {
            // if we are already prepending indexes, put it in the 
            // appropriate prepending list...
            if (_reversed)
            {
                if (_appendedIndexes != null)
                {
                    _appendedIndexes.Add(coordIndex);
                    return;
                }
            }
            else
            {
                if (_prependedIndexes != null)
                {
                    _prependedIndexes.Add(coordIndex);
                    return;
                }
            }

            // not a slice, treat the whole sequence
            if (!isSlice())
            {
                if (_sequence.Count == 0)
                {
                    _sequence.Add(coordIndex);
                    return;
                }

                if (_reversed)
                {
                    // if we are prepending to a reversed sequence, we
                    // really want to append, so just add it.
                    Debug.Assert(_appendedIndexes == null);
                    _sequence.Add(coordIndex);
                }
                else
                {
                    // We want to avoid copying the entire sequence in memory just to 
                    // insert a coordinate.
                    // Thus, add to the prepend list. 
                    // TODO: consider a heuristic to judge when an insert
                    // to the head of a list would be better than a multiple list
                    // sequence.
                    Debug.Assert(_prependedIndexes == null);
                    _prependedIndexes = new List<Int32>();
                    _prependedIndexes.Add(coordIndex);
                }

                return;
            }

            // This is a slice, which allows a few different ways to prepend

            // project index to slice
            Int32 transformedIndex;
            transformIndex(0, out transformedIndex);

            // if the next coord in the sequence is the same, just adjust
            // the end of the slice
            if (_reversed)
            {
                if (transformedIndex < _sequence.Count - 1 &&
                    _endIndex >= 0 &&
                    _sequence[transformedIndex + 1] == coordIndex)
                {
                    _endIndex++;
                }
                else
                {
                    _appendedIndexes = new List<Int32>();
                    _appendedIndexes.Add(coordIndex);
                }
            }
            else
            {
                if (transformedIndex > 0 &&
                    _startIndex >= 0 &&
                    _sequence[transformedIndex - 1] == coordIndex)
                {
                    _startIndex--;
                }
                else
                {
                    _prependedIndexes = new List<Int32>();
                    _prependedIndexes.Add(coordIndex);
                }
            }
        }

        private int computeSliceCount()
        {
            Int32 end = computeSliceEndOnMainSequence();
            Int32 start = computeSliceStartOnMainSequence();

            return end - start + 1 +
                   (_appendedIndexes == null ? 0 : _appendedIndexes.Count) +
                   (_prependedIndexes == null ? 0 : _prependedIndexes.Count) -
                   (_skipIndexes == null ? 0 : _skipIndexes.Count);
        }

        private void skipIndex(Int32 index)
        {
            if (_skipIndexes == null)
            {
                _skipIndexes = new SortedSet<Int32>();
            }

            _skipIndexes.Add(index);
        }

        private void appendInternal(BufferedCoordinate2DSequence sequence)
        {
            // check to see if the sequences have different buffers, 
            // if so, just do a normal range append
            if (!ReferenceEquals(sequence._buffer, _buffer))
            {
                Append((IEnumerable<BufferedCoordinate2D>)sequence);
                return;
            }

            if (_reversed)
            {
                appendInternalReverse(sequence);
            }
            else
            {
                appendInternalForward(sequence);
            }
        }

        private void prependInternal(BufferedCoordinate2DSequence sequence)
        {
            // check to see if the sequences have different buffers, 
            // if so, just do a normal range prepend
            if (!ReferenceEquals(sequence._buffer, _buffer))
            {
                Prepend((IEnumerable<BufferedCoordinate2D>)sequence);
                return;
            }

            if (_reversed)
            {
                prependInternalReverse(sequence);
            }
            else
            {
                prependInternalForward(sequence);
            }
        }

        private void appendInternalForward(BufferedCoordinate2DSequence sequence)
        {
            Int32 appendCount = sequence.Count;
            Int32 appendIndex = 0;

            // push the end index forward if the conditions are right:
            //  * no appended indexes
            //  * _endIndex is less than _sequence.Count
            //  * the index of the appending coordinate is the same 
            //    as the underlying sequence at _endIndex + 1
            if (_appendedIndexes == null)
            {
                for (; appendIndex < appendCount; appendIndex++)
                {
                    if (_endIndex >= _sequence.Count - 1 ||
                        sequence[appendIndex].Index != _sequence[_endIndex + 1])
                    {
                        break;
                    }

                    _endIndex--;
                }

                // added all coordinates by pushing the _startIndex back
                if (appendIndex >= appendCount)
                {
                    return;
                }

                // otherwise, we put them into a new list
                _appendedIndexes = new List<Int32>(Math.Max(4, appendCount - appendIndex));
            }

            for (Int32 i = appendIndex; i < appendCount; i++)
            {
                _appendedIndexes.Add(sequence[i].Index);
            }
        }

        private void prependInternalForward(BufferedCoordinate2DSequence sequence)
        {
            Int32 prependCount = sequence.Count;
            Int32 prependIndex = prependCount - 1;

            // push the start index back if the conditions are right:
            //  * no prepended indexes
            //  * _startIndex is greater than 0
            //  * the index of the prepending coordinate is the same 
            //    as the underlying sequence at _startIndex - 1
            if (_prependedIndexes == null)
            {
                for (; prependIndex <= 0; prependIndex--)
                {
                    if (_startIndex <= 0 ||
                        sequence[prependIndex].Index != _sequence[_startIndex - 1])
                    {
                        break;
                    }

                    _startIndex--;
                }

                // added all coordinates by pushing the _startIndex back
                if (prependIndex < 0)
                {
                    return;
                }

                // otherwise, we put them into a new list
                _prependedIndexes = new List<Int32>(Math.Max(4, prependIndex));
            }

            for (Int32 i = prependIndex; i >= 0; i--)
            {
                _prependedIndexes.Add(sequence[i].Index);
            }
        }

        private void appendInternalReverse(BufferedCoordinate2DSequence sequence)
        {
            Int32 appendCount = sequence.Count;
            Int32 appendIndex = 0;

            // push the start index forward if the conditions are right:
            //  * no prepended indexes
            //  * _startIndex is greater than 0
            //  * the index of the appending coordinate is the same 
            //    as the underlying sequence at _startIndex - 1
            if (_prependedIndexes == null)
            {
                for (; appendIndex > appendCount; appendIndex++)
                {
                    Int32 startIndex = computeSliceStartOnMainSequence();

                    if (startIndex <= 0 ||
                        sequence[appendIndex].Index != _sequence[_startIndex - 1])
                    {
                        break;
                    }

                    _startIndex--;
                }

                // added all coordinates by pushing the _endIndex forward
                if (appendIndex >= appendCount)
                {
                    return;
                }

                // otherwise, we put them into a new list
                _prependedIndexes = new List<Int32>(Math.Max(4, appendCount - appendIndex));
            }

            for (Int32 i = appendIndex; i < appendCount; i++)
            {
                _prependedIndexes.Add(sequence[i].Index);
            }
        }

        private void prependInternalReverse(BufferedCoordinate2DSequence sequence)
        {
            Int32 prependIndex = sequence.Count - 1;

            // push the end index forward if the conditions are right:
            //  * no appended indexes
            //  * _endIndex is less than _sequence.Count - 1
            //  * the index of the prepending coordinate is the same 
            //    as the underlying sequence at _endIndex + 1
            if (_appendedIndexes == null)
            {
                for (; prependIndex <= 0; prependIndex--)
                {
                    Int32 endIndex = computeSliceEndOnMainSequence();

                    if (endIndex >= _sequence.Count - 1 ||
                        sequence[prependIndex].Index != _sequence[_endIndex + 1])
                    {
                        break;
                    }

                    _endIndex++;
                }

                // added all coordinates by pushing the _endIndex forward
                if (prependIndex < 0)
                {
                    return;
                }

                // otherwise, we put them into a new list
                _appendedIndexes = new List<Int32>(Math.Max(4, prependIndex));
            }

            for (Int32 i = prependIndex; i >= 0; i--)
            {
                _appendedIndexes.Add(sequence[i].Index);
            }
        }

        private BufferedCoordinate2DSequence createSliceInternal(Int32 endIndex, Int32 startIndex)
        {
            checkIndexes(endIndex, startIndex);

            Freeze();

            return new BufferedCoordinate2DSequence(_reversed,
                                                    _sequence,
                                                    _prependedIndexes,
                                                    _appendedIndexes,
                                                    _skipIndexes,
                                                    startIndex, endIndex,
                                                    _factory, _buffer);
        }

        private Int32 computeSliceStartOnMainSequence()
        {
            return Math.Max(0, _startIndex);
        }

        private Int32 computeSliceEndOnMainSequence()
        {
            return (Int32)Math.Min(_sequence.Count - 1, (UInt32)_endIndex);
        }

        private List<Int32> getStorage(SequenceStorage storage)
        {
            switch (storage)
            {
                case SequenceStorage.AppendList:
                    return _appendedIndexes;
                case SequenceStorage.MainList:
                    return _sequence;
                case SequenceStorage.PrependList:
                    return _prependedIndexes;
                default:
                    Debug.Fail("Should never reach here");
                    throw new InvalidOperationException("Unknown storage");
            }
        }

        private Int32 getStorageValue(SequenceStorage storage, Int32 index)
        {
            List<Int32> list = getStorage(storage);
            return list[index];
        }
        #endregion
    }
}