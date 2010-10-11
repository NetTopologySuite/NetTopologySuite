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
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NPack.Interfaces;

#endregion

namespace GeoAPI.IO.WellKnownText
{
    public class WktReader<TCoordinate> : IWktGeometryReader<TCoordinate>, IWktCoordinateSystemReader<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static IGeometry<TCoordinate> ToGeometry(String wktData, IGeometryFactory<TCoordinate> geoFactory)
        {
            return GeometryFromWkt.Parse(wktData, geoFactory);
        }

        public static IGeometry<TCoordinate> ToGeometry(Stream wktData, IGeometryFactory<TCoordinate> geoFactory)
        {
            return GeometryFromWkt.Parse(wktData, geoFactory);
        }

        public static IGeometry<TCoordinate> ToGeometry(TextReader wktData, IGeometryFactory<TCoordinate> geoFactory)
        {
            return GeometryFromWkt.Parse(wktData, geoFactory);
        }

        public static IInfo ToCoordinateSystemInfo(String wktData, ICoordinateSystemFactory<TCoordinate> factory)
        {
            return null;
            //return CoordinateSystemWktReader.Parse(wktData, factory);
        }

        public static IInfo ToCoordinateSystemInfo(TextReader wktData, ICoordinateSystemFactory factory)
        {
            return null;
            //return CoordinateSystemWktReader.Parse(wktData, factory);
        }

        private IGeometryFactory<TCoordinate> _geoFactory;
        private ICoordinateSystemFactory<TCoordinate> _coordSysFactory;

        public WktReader(IGeometryFactory<TCoordinate> geoFactory,
                         ICoordinateSystemFactory<TCoordinate> coordSysFactory)
        {
            _geoFactory = geoFactory;
            _coordSysFactory = coordSysFactory;
        }

        #region IWktCoordinateSystemReader Members

        public ICoordinateSystemFactory<TCoordinate> CoordinateSystemFactory
        {
            set { _coordSysFactory = value; }
        }

        public ICoordinateSystemAuthorityFactory<TCoordinate> AuthorityFactory
        {
            set { throw new NotImplementedException(); }
        }

        public ICoordinateSystem<TCoordinate> ReadCoordinateSystem(String wkt)
        {
            return (this as IWktCoordinateSystemReader<TCoordinate>).Read(wkt);
        }

        public ICoordinateSystem<TCoordinate> ReadCoordinateSystem(TextReader wkt)
        {
            return (this as IWktCoordinateSystemReader<TCoordinate>).Read(wkt);
        }

        ICoordinateSystem<TCoordinate> IWktCoordinateSystemReader<TCoordinate>.Read(
                                                                        String wkt)
        {
            return ToCoordinateSystemInfo(wkt, _coordSysFactory) 
                                    as ICoordinateSystem<TCoordinate>;
        }

        ICoordinateSystem<TCoordinate> IWktCoordinateSystemReader<TCoordinate>.Read(
                                                                        TextReader wkt)
        {
            return ToCoordinateSystemInfo(wkt, _coordSysFactory) 
                                    as ICoordinateSystem<TCoordinate>;
        }

        #endregion

        #region IWktGeometryReader Members

        public IGeometry<TCoordinate> Read(String wkt)
        {
            return GeometryFromWkt.Parse(wkt, _geoFactory);
        }

        public IGeometry<TCoordinate> Read(TextReader wktData)
        {
            return GeometryFromWkt.Parse(wktData, _geoFactory);
        }

        public IEnumerable<IGeometry<TCoordinate>> ReadAll(TextReader wktData)
        {
            while (wktData.Peek() >= 0)
            {
                yield return GeometryFromWkt.Parse(wktData, _geoFactory);
            }
        }

        public IGeometryFactory<TCoordinate> GeometryFactory
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _geoFactory = value;
            }
        }

        #endregion

        #region IWktGeometryReader Members

        IGeometry IWktGeometryReader.Read(String wkt)
        {
            return Read(wkt);
        }

        IGeometry IWktGeometryReader.Read(TextReader wktData)
        {
            return Read(wktData);
        }

        IEnumerable<IGeometry> IWktGeometryReader.ReadAll(TextReader wktData)
        {
            while (wktData.Peek() >= 0)
            {
                yield return GeometryFromWkt.Parse(wktData, _geoFactory);
            }
        }

        IGeometryFactory IWktGeometryReader.GeometryFactory
        {
            set
            {
                if (value != null && !(value is IGeometryFactory<TCoordinate>))
                {
                    throw new ArgumentException("not type IGeometryFactory<TCoordinate>", "value");
                }
                GeometryFactory = value as IGeometryFactory<TCoordinate>;
            }
        }

        #endregion

        #region IWktCoordinateSystemReader<TCoordinate> Members

        ICoordinateSystemFactory IWktCoordinateSystemReader.CoordinateSystemFactory
        {
            set
            {
                if (value != null && !(value is ICoordinateSystemFactory<TCoordinate>))
                {
                    throw new ArgumentException("not type ICoordinateSystemFactory<TCoordinate>", "value");                    
                }
                CoordinateSystemFactory = value as ICoordinateSystemFactory<TCoordinate>;
            }
        }

        ICoordinateSystemAuthorityFactory IWktCoordinateSystemReader.AuthorityFactory
        {
            set { throw new NotImplementedException(); }
        }

        ICoordinateSystem IWktCoordinateSystemReader.Read(String wkt)
        {
            return ReadCoordinateSystem(wkt);
        }

        ICoordinateSystem IWktCoordinateSystemReader.Read(TextReader wkt)
        {
            return ReadCoordinateSystem(wkt);
        }

        #endregion
    }
}