/*
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    public class BufferedCoordinateSet : BufferedCoordinateSequence, ISet<BufferedCoordinate>
    {
        public BufferedCoordinateSet(BufferedCoordinateSequenceFactory factory, 
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer)
            : base(factory, buffer) { }

        internal BufferedCoordinateSet(
            ICoordinateSequence<BufferedCoordinate> sequence,
            BufferedCoordinateSequenceFactory factory,
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer)
            : base(factory, buffer)
        {
            ICoordinateSequence<BufferedCoordinate> withoutDupes =
                sequence.WithoutDuplicatePoints();

            BufferedCoordinateSequence nativeSequence 
                = withoutDupes as BufferedCoordinateSequence;

            if (nativeSequence == null)
            {
                nativeSequence = factory.Create(withoutDupes) as BufferedCoordinateSequence;
            }

            SetSequenceInternal(nativeSequence);
        }

        #region ISet<BufferedCoordinate> Members

        public ISet<BufferedCoordinate> Union(ISet<BufferedCoordinate> a)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate> Intersect(ISet<BufferedCoordinate> a)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate> Minus(ISet<BufferedCoordinate> a)
        {
            throw new NotImplementedException();
        }

        public ISet<BufferedCoordinate> ExclusiveOr(ISet<BufferedCoordinate> a)
        {
            throw new NotImplementedException();
        }

        public Boolean ContainsAll(IEnumerable<BufferedCoordinate> c)
        {
            throw new NotImplementedException();
        }

        public Boolean IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public new Boolean Add(BufferedCoordinate o)
        {
            throw new NotImplementedException();
        }

        public new Boolean AddRange(IEnumerable<BufferedCoordinate> c)
        {
            throw new NotImplementedException();
        }

        public Boolean RemoveAll(IEnumerable<BufferedCoordinate> c)
        {
            throw new NotImplementedException();
        }

        public Boolean RetainAll(IEnumerable<BufferedCoordinate> c)
        {
            throw new NotImplementedException();
        }

        public new ISet<BufferedCoordinate> Clone()
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

        #region ISet<BufferedCoordinate> Members

        void ISet<BufferedCoordinate>.Clear()
        {
            Clear();
        }

        #endregion
    }
}
*/