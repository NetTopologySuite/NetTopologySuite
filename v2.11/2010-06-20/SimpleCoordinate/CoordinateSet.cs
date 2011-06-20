#region License

/*
 *  The attached / following is part of NetTopologySuite.Coordinates.Simple.
 *  
 *  NetTopologySuite.Coordinates.Simple is free software ? 2009 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  This work was derived from NetTopologySuite.Coordinates.ManagedBufferedCoordinate
 *  by codekaizen
 *  
 *  Author: Felix Obermaier 2009
 *  
 */

#endregion
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;

namespace NetTopologySuite.Coordinates.Simple
{
    public class CoordinateSet : CoordinateSequence, ISet<Coordinate>
    {
        public CoordinateSet(CoordinateSequenceFactory factory)
            : base((CoordinateFactory)factory.CoordinateFactory, 0) { }

        internal CoordinateSet(
            ICoordinateSequence<Coordinate> sequence,
            CoordinateSequenceFactory factory)
            : base((CoordinateFactory)factory.CoordinateFactory, sequence)
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
