// Portions copyright 2005 - 2007: Diego Guidi
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

#region Namespace Imports

using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using NPack.Interfaces;

#endregion

namespace NetTopologySuite.IO.WellKnownBinary
{
    public class WkbWriter<TCoordinate> : IWkbWriter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static Byte[] ToWkb(IGeometry<TCoordinate> geometry)
        {
            return GeometryToWkb.Write(geometry);
        }

        public static Byte[] ToWkb(IGeometry<TCoordinate> geometry, WkbByteOrder byteOrder)
        {
            return GeometryToWkb.Write(geometry, byteOrder);
        }

        private WkbByteOrder _byteOrder;

        public WkbWriter() : this(WkbByteOrder.LittleEndian) { }

        public WkbWriter(WkbByteOrder byteOrder)
        {
            _byteOrder = byteOrder;
        }

        #region IWkbEncoder Members

        public IEnumerable<Byte> LazyWrite(IGeometry<TCoordinate> geometry)
        {
            throw new NotImplementedException();
        }

        public Byte[] Write(IGeometry<TCoordinate> geometry)
        {
            return GeometryToWkb.Write(geometry, _byteOrder);
        }

        public WkbByteOrder ByteOrder
        {
            get { return _byteOrder; }
            set { _byteOrder = value; }
        }
        #endregion

        #region IWkbWriter Members

        IEnumerable<Byte> IWkbWriter.LazyWrite(IGeometry geometry)
        {
            throw new NotImplementedException();
        }

        Byte[] IWkbWriter.Write(IGeometry geometry)
        {
			return GeometryToWkb.Write(geometry, _byteOrder);
        }

        #endregion
    }
}