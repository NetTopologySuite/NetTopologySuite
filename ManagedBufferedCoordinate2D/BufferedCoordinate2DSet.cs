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
            IVectorBuffer<DoubleComponent, BufferedCoordinate2D> buffer)
            : base(factory, buffer) { }

        internal BufferedCoordinate2DSet(
            ICoordinateSequence<BufferedCoordinate2D> sequence,
            BufferedCoordinate2DSequenceFactory factory,
            IVectorBuffer<DoubleComponent, BufferedCoordinate2D> buffer)
            : base(factory, buffer)
        {
            ICoordinateSequence<BufferedCoordinate2D> withoutDupes =
                sequence.WithoutDuplicatePoints();

            BufferedCoordinate2DSequence nativeSequence 
                = withoutDupes as BufferedCoordinate2DSequence;

            if (nativeSequence == null)
            {
                nativeSequence = factory.Create(withoutDupes) as BufferedCoordinate2DSequence;
            }

            SetSequenceInternal(nativeSequence);
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

        public Boolean ContainsAll(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public Boolean IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public new Boolean Add(BufferedCoordinate2D o)
        {
            throw new NotImplementedException();
        }

        public new Boolean AddRange(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public Boolean RemoveAll(IEnumerable<BufferedCoordinate2D> c)
        {
            throw new NotImplementedException();
        }

        public Boolean RetainAll(IEnumerable<BufferedCoordinate2D> c)
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

        Object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISet<BufferedCoordinate2D> Members

        void ISet<BufferedCoordinate2D>.Clear()
        {
            Clear();
        }

        #endregion
    }
}
