using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    public class BufferedCoordinate2DSet : BufferedCoordinate2DSequence, ISet<BufferedCoordinate2D>
    {
        public BufferedCoordinate2DSet(BufferedCoordinate2DSequenceFactory factory, 
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
            : base(factory, buffer) { }

        internal BufferedCoordinate2DSet(
            ICoordinateSequence<BufferedCoordinate2D> sequence,
            BufferedCoordinate2DSequenceFactory factory,
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer)
            : base(factory, buffer)
        {
            ICoordinateSequence<BufferedCoordinate2D> withoutDupes =
                sequence.WithoutDuplicatePoints();

            throw new NotImplementedException();
        }

        #region ISet<BufferedCoordinate2D> Members

        public ISet<BufferedCoordinate2D> Union(ISet<BufferedCoordinate2D> a)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate2D> Intersect(ISet<BufferedCoordinate2D> a)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate2D> Minus(ISet<BufferedCoordinate2D> a)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate2D> ExclusiveOr(ISet<BufferedCoordinate2D> a)
        {
            throw new NotImplementedException();
        }

        public bool ContainsAll(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public new bool Add(BufferedCoordinate2D o)
        {
            throw new NotImplementedException();
        }

        public new bool AddRange(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public bool RemoveAll(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public bool RetainAll(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public new ISet<BufferedCoordinate2D> Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        public new System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
