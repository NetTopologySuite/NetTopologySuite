using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates.Simple
{
    public class CoordinateSet : CoordinateSequence, ISet<Coordinate>
    {
        public CoordinateSet(CoordinateSequenceFactory factory)
            : base(factory, 0) { }

        internal CoordinateSet(
            ICoordinateSequence<Coordinate> sequence,
            CoordinateSequenceFactory factory)
            : base(factory, sequence)
        {
            ICoordinateSequence<Coordinate> withoutDupes =
                sequence.WithoutDuplicatePoints();

            CoordinateSequence nativeSequence 
                = withoutDupes as CoordinateSequence;

            if (nativeSequence == null)
            {
                nativeSequence = factory.Create(withoutDupes) as CoordinateSequence;
            }

            SetSequenceInternal(nativeSequence);

        }

        #region ISet<Coordinate> Members

        public ISet<Coordinate> Union(ISet<Coordinate> a)
        {
            throw new NotImplementedException();
        }

        public ISet<Coordinate> Intersect(ISet<Coordinate> a)
        {
            throw new NotImplementedException();
        }

        public ISet<Coordinate> Minus(ISet<Coordinate> a)
        {
            throw new NotImplementedException();
        }

        public ISet<Coordinate> ExclusiveOr(ISet<Coordinate> a)
        {
            throw new NotImplementedException();
        }

        public Boolean ContainsAll(IEnumerable<Coordinate> c)
        {
            throw new NotImplementedException();
        }

        public Boolean IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public new Boolean Add(Coordinate o)
        {
            throw new NotImplementedException();
        }

        public new Boolean AddRange(IEnumerable<Coordinate> c)
        {
            throw new NotImplementedException();
        }

        public Boolean RemoveAll(IEnumerable<Coordinate> c)
        {
            throw new NotImplementedException();
        }

        public Boolean RetainAll(IEnumerable<Coordinate> c)
        {
            throw new NotImplementedException();
        }

        public new ISet<Coordinate> Clone()
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

        #region ISet<Coordinate> Members

        void ISet<Coordinate>.Clear()
        {
            Clear();
        }

        #endregion
    }
}
