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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GeoAPI.IO.WellKnownText
{
    public class WktWriter<TCoordinate> : IWktCoordinateSystemWriter<TCoordinate>, 
                                          IWktGeometryWriter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static String ToWkt(IGeometry<TCoordinate> geometry)
        {
            return GeometryToWkt.Write(geometry);
        }

        public static void ToWkt(IGeometry<TCoordinate> geometry, TextWriter writer)
        {
            GeometryToWkt.Write(geometry, writer);
        }

        #region IWktCoordinateSystemWriter Members

        public String Write(IInfo csInfo)
        {
            throw new NotImplementedException();
        }

        public void Write(IInfo csInfo, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IWktCoordinateSystemWriter<TCoordinate> Members

        public String Write(ICoordinateSystem<TCoordinate> csInfo)
        {
            throw new NotImplementedException();
        }

        public void Write(ICoordinateSystem<TCoordinate> csInfo, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IWktGeometryWriter Members

        public String Write(IGeometry<TCoordinate> geometry)
        {
            return ToWkt(geometry);
        }

        public String WriteAll(IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                buffer.AppendLine(ToWkt(geometry));
                buffer.AppendLine();
            }

            return buffer.ToString();
        }

        public void Write(IGeometry<TCoordinate> geometry, TextWriter writer)
        {
            ToWkt(geometry, writer);
        }

        public void WriteAll(IEnumerable<IGeometry<TCoordinate>> geometries, 
                             TextWriter writer)
        {
            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                ToWkt(geometry, writer);
            }
        }

        #endregion

        #region IWktGeometryWriter Members

        String IWktGeometryWriter.Write(IGeometry geometry)
        {
            throw new NotImplementedException();
        }

        String IWktGeometryWriter.WriteAll(IEnumerable<IGeometry> geometries)
        {
            throw new NotImplementedException();
        }

        void IWktGeometryWriter.Write(IGeometry geometry, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        void IWktGeometryWriter.WriteAll(IEnumerable<IGeometry> geometries, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
