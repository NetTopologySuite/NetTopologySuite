using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.DataStructures.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    public class BufferedCoordinateSequence : ICoordinateSequence<BufferedCoordinate2D>
    {
        #region ICoordinateSequence<BufferedCoordinate2D> Members

        public ICoordinateSequenceFactory<BufferedCoordinate2D> CoordinateSequenceFactory
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BufferedCoordinate2D[] ToArray()
        {
            throw new NotImplementedException();
        }

        public IExtents<BufferedCoordinate2D> ExpandEnvelope(IExtents<BufferedCoordinate2D> extents)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<BufferedCoordinate2D> coordinates, Boolean allowRepeated, Boolean reverse)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<BufferedCoordinate2D> coordinates)
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
                Swap(i, last - i);
            }
        }

        public ICoordinateSequence<BufferedCoordinate2D> Reversed
        {
            get { throw new NotImplementedException(); }
        }

        public Int32 Count
        {
            get { throw new NotImplementedException(); }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D this[Int32 index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ICoordinateSequence<BufferedCoordinate2D> Merge(ICoordinateSequence<BufferedCoordinate2D> other)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> AddSequence(ICoordinateSequence<BufferedCoordinate2D> sequence)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence<BufferedCoordinate2D> WithoutRepeatedPoints()
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate2D> AsSet()
        {
            throw new NotImplementedException();
        }

        public void CloseRing()
        {
            throw new NotImplementedException();
        }

        public void Scroll(BufferedCoordinate2D coordinateToBecomeFirst)
        {
            throw new NotImplementedException();
        }

        public void Scroll(Int32 indexToBecomeFirst)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D Minimum()
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate2D Maximum()
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

        public ICoordinateSequence<BufferedCoordinate2D> Slice(Int32 startIndex, Int32 endIndex)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Swaps two coordinates in a sequence.
        /// </summary>
        public void Swap(Int32 i, Int32 j)
        {
            if (i == j)
            {
                return;
            }

            throw new NotImplementedException();
        }

        #region IList<BufferedCoordinate2D> Members

        public Int32 IndexOf(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public void Insert(Int32 index, BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(Int32 index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection<BufferedCoordinate2D> Members

        public void Add(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(BufferedCoordinate2D[] array, Int32 arrayIndex)
        {
            throw new NotImplementedException();
        }

        public Boolean IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean Remove(BufferedCoordinate2D item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<BufferedCoordinate2D> Members

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinateSequence Members

        public Int32 Dimension
        {
            get { throw new NotImplementedException(); }
        }

        public Double this[Int32 index, Ordinates ordinate]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        ICoordinate[] ICoordinateSequence.ToArray()
        {
            throw new NotImplementedException();
        }

        public IExtents ExpandEnvelope(IExtents env)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence Merge(ICoordinateSequence other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList Members

        public Int32 Add(Object value)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(Object value)
        {
            throw new NotImplementedException();
        }

        public Int32 IndexOf(Object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(Int32 index, Object value)
        {
            throw new NotImplementedException();
        }

        public Boolean IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(Object value)
        {
            throw new NotImplementedException();
        }

        Object System.Collections.IList.this[Int32 index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, Int32 index)
        {
            throw new NotImplementedException();
        }

        public Boolean IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public Object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region ICloneable Members

        public Object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
