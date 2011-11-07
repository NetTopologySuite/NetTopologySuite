// Copyright 2008 - Ricardo Stuven (rstuven@gmail.com)
//
// This file is part of NetTopologySuite.IO.SqlServer2008
// The original source is part of of NHibernate.Spatial.
// NetTopologySuite.IO.SqlServer2008 is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// NetTopologySuite.IO.SqlServer2008 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NetTopologySuite.IO.SqlServer2008 if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.IO;
using GeoAPI.IO;
using Microsoft.SqlServer.Types;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    public class MsSql2008GeometryWriter : IBinaryGeometryWriter, IGeometryWriter<SqlGeometry>
    {
        //private readonly SqlGeometryBuilder _builder = new SqlGeometryBuilder();

        public byte[] Write(IGeometry geometry)
        {
            using (var ms = new MemoryStream())
            {
                Write(geometry, ms);
                return ms.ToArray();
            }
        }

        public void Write(IGeometry geometry, Stream stream)
        {
            var sqlGeometry = WriteGeometry(geometry);
            using (var writer = new BinaryWriter(stream))
                sqlGeometry.Write(writer);
        }

        SqlGeometry IGeometryWriter<SqlGeometry>.Write(IGeometry geometry)
        {
            return WriteGeometry(geometry);
        }

        public SqlGeometry WriteGeometry(IGeometry geometry)
        {
            var builder = new SqlGeometryBuilder();
            builder.SetSrid(geometry.SRID);
            AddGeometry(builder, geometry);
            return builder.ConstructedGeometry;
        }




        private void AddGeometry(SqlGeometryBuilder builder, IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                AddPoint(builder, (IPoint)geometry);
            }
            else if (geometry is ILineString)
            {
                AddLineString(builder, (ILineString)geometry);
            }
            else if (geometry is IPolygon)
            {
                AddPolygon(builder, (IPolygon)geometry);
            }
            else if (geometry is IMultiPoint)
            {
                AddGeometryCollection(builder, (IMultiPoint)geometry, OpenGisGeometryType.MultiPoint);
            }
            else if (geometry is IMultiLineString)
            {
                AddGeometryCollection(builder, (IMultiLineString)geometry, OpenGisGeometryType.MultiLineString);
            }
            else if (geometry is IMultiPolygon)
            {
                AddGeometryCollection(builder, (IMultiPolygon)geometry, OpenGisGeometryType.MultiPolygon);
            }
            else if (geometry is IGeometryCollection)
            {
                AddGeometryCollection(builder, (IGeometryCollection)geometry, OpenGisGeometryType.GeometryCollection);
            }
        }

        private void AddGeometryCollection(SqlGeometryBuilder builder, IGeometryCollection geometry, OpenGisGeometryType type)
        {
            builder.BeginGeometry(type);
            Array.ForEach(geometry.Geometries, geometry1 => AddGeometry(builder, geometry1));
            builder.EndGeometry();
        }

        private void AddPolygon(SqlGeometryBuilder builder, IPolygon geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.Polygon);
            AddCoordinates(builder, geometry.ExteriorRing.CoordinateSequence);
            Array.ForEach(geometry.InteriorRings, ring => AddCoordinates(builder, ring.CoordinateSequence));
            builder.EndGeometry();
        }

        private void AddLineString(SqlGeometryBuilder builder, ILineString geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.LineString);
            AddCoordinates(builder, geometry.CoordinateSequence);
            builder.EndGeometry();
        }

        private void AddPoint(SqlGeometryBuilder builder, IPoint geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.Point);
            AddCoordinates(builder, geometry.CoordinateSequence);
            builder.EndGeometry();
        }

        private void AddCoordinates(SqlGeometryBuilder builder, ICoordinateSequence coordinates)
        {
            for (var i = 0; i < coordinates.Count; i++)
            {
                AddCoordinate(builder, coordinates, i);
            }

        }

        private void AddCoordinate(SqlGeometryBuilder builder, ICoordinateSequence coordinates, int index)
        {
            var x = coordinates.GetOrdinate(index, Ordinate.X);
            var y = coordinates.GetOrdinate(index, Ordinate.Y);

            Double? z = null, m = null;
            if ((HandleOrdinates & Ordinates.Z) > 0)
            {
                z = coordinates.GetOrdinate(index, Ordinate.Z);
                if (Double.IsNaN(z.Value)) z = 0d;
            }

            if ((HandleOrdinates & Ordinates.M) > 0)
            {
                m = coordinates.GetOrdinate(index, Ordinate.M);
                if (Double.IsNaN(m.Value)) m = 0d;
            }

            if (index == 0)
            {
                builder.BeginFigure(x, y, z, m);
            }
            else
            {
                builder.AddLine(x, y, z, m);
            }

            if (index == coordinates.Count-1)
            {
                builder.EndFigure();
            }
        }

        /*
        private void AddCoordinates(Coordinate[] coordinates)
        {
            int points = 0;
            Array.ForEach<Coordinate>(coordinates, delegate(Coordinate coordinate)
            {
                double? z = null;
                if (!double.IsNaN(coordinate.Z) && !double.IsInfinity(coordinate.Z))
                {
                    z = coordinate.Z;
                }
                if (points == 0)
                {
                    _builder.BeginFigure(coordinate.X, coordinate.Y, z, null);
                }
                else
                {
                    _builder.AddLine(coordinate.X, coordinate.Y, z, null);
                }
                points++;
            });
            if (points != 0)
            {
                _builder.EndFigure();
            }
        }

        public SqlGeometry ConstructedGeometry
        {
            get { return _builder.ConstructedGeometry; }
        }
         */

        #region Implementation of IGeometryIOBase

        public bool HandleSRID
        {
            get { return true; }
            set { }
        }

        public Ordinates AllowedOrdinates
        {
            get { return Ordinates.XYZM; }
        }

        private Ordinates _handleOrdinates;
        public Ordinates HandleOrdinates
        {
            get { return _handleOrdinates; }
            set
            {
                value = Ordinates.XY | (AllowedOrdinates & value);
                _handleOrdinates = value;
            }
        }

        #endregion

        #region Implementation of IBinaryGeometryWriter

        public ByteOrder ByteOrder
        {
            get { return ByteOrder.LittleEndian; }
            set {  }
        }

        #endregion
    }
}
