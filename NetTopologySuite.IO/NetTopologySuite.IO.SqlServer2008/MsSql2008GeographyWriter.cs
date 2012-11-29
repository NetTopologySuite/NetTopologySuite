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

using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;
using Microsoft.SqlServer.Types;

namespace NetTopologySuite.IO
{
    public class MsSql2008GeographyWriter : IBinaryGeometryWriter, IGeometryWriter<SqlGeography>
	{
		//private readonly SqlGeographyBuilder _builder = new SqlGeographyBuilder();

	    public SqlGeography WriteGeography(IGeometry geometry)
	    {
	        var builder = new SqlGeographyBuilder();
            
            builder.SetSrid(geometry.SRID);
            AddGeometry(builder, geometry);
            return builder.ConstructedGeography;
        }
        
        SqlGeography IGeometryWriter<SqlGeography>.Write(IGeometry geometry)
        {
            return WriteGeography(geometry);
        }

        public byte[] Write(IGeometry geometry)
        {
            var sqlGeography = WriteGeography(geometry);
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    sqlGeography.Write(bw);
                }
                return ms.ToArray();
            }
        }

	    public void Write(IGeometry geometry, Stream stream)
	    {
            var sqlGeography = WriteGeography(geometry);
	        using(var bw = new BinaryWriter(stream))
	        {
	            sqlGeography.Write(bw);
	        }
	    }

	    private void AddGeometry(SqlGeographyBuilder builder, IGeometry geometry)
		{
			if (geometry is IPoint)
			{
                AddPoint(builder, geometry);
			}
			else if (geometry is ILineString)
			{
                AddLineString(builder, geometry);
			}
			else if (geometry is IPolygon)
			{
                AddPolygon(builder, geometry);
			}
			else if (geometry is IMultiPoint)
			{
                AddGeometryCollection(builder, geometry, OpenGisGeographyType.MultiPoint);
			}
			else if (geometry is IMultiLineString)
			{
                AddGeometryCollection(builder, geometry, OpenGisGeographyType.MultiLineString);
			}
			else if (geometry is IMultiPolygon)
			{
                AddGeometryCollection(builder, geometry, OpenGisGeographyType.MultiPolygon);
			}
			else if (geometry is IGeometryCollection)
			{
                AddGeometryCollection(builder, geometry, OpenGisGeographyType.GeometryCollection);
			}
		}

		private void AddGeometryCollection(SqlGeographyBuilder builder, IGeometry geometry, OpenGisGeographyType type)
		{
			builder.BeginGeography(type);
			var coll = geometry as IGeometryCollection;
            Debug.Assert(coll != null, "coll != null");
		    Array.ForEach(coll.Geometries, geometry1 => AddGeometry(builder, geometry1));
		    builder.EndGeography();
		}

		private void AddPolygon(SqlGeographyBuilder builder, IGeometry geometry)
		{
			builder.BeginGeography(OpenGisGeographyType.Polygon);
			var polygon = geometry as IPolygon;
		    Debug.Assert(polygon != null, "polygon != null");

            AddCoordinates(builder, TryReverseRing((ILinearRing)polygon.ExteriorRing, true).CoordinateSequence);

            Array.ForEach(polygon.InteriorRings, ring => AddCoordinates(builder, TryReverseRing((ILinearRing)polygon.ExteriorRing, false).CoordinateSequence));
			builder.EndGeography();
		}

        private static ILinearRing TryReverseRing(ILinearRing ring, bool ccw)
        {
            if (ring.IsCCW == ccw)
                return ring;
            return (ILinearRing) ring.Reverse();
        }

		private void AddLineString(SqlGeographyBuilder builder, IGeometry geometry)
		{
			builder.BeginGeography(OpenGisGeographyType.LineString);
            AddCoordinates(builder, ((ILineString)geometry).CoordinateSequence);
			builder.EndGeography();
		}

		private void AddPoint(SqlGeographyBuilder builder, IGeometry geometry)
		{
			builder.BeginGeography(OpenGisGeographyType.Point);
            AddCoordinates(builder, ((IPoint)geometry).CoordinateSequence);
			builder.EndGeography();
		}

		private void AddCoordinates(SqlGeographyBuilder builder, ICoordinateSequence coordinates)
		{
		    for (var i = 0; i < coordinates.Count; i++)
		    {
		        AddCoordinate(builder, coordinates, i, i);
		    }
		}

        private void AddCoordinate(SqlGeographyBuilder builder, ICoordinateSequence coordinates, int index, int geographyIndex)
        {
            var x = coordinates.GetOrdinate(index, Ordinate.Y);
            var y = coordinates.GetOrdinate(index, Ordinate.X);

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

            if (geographyIndex == 0)
            {
                builder.BeginFigure(x, y, z, m);
            }
            else
            {
                builder.AddLine(x, y, z, m);
            }

            if (geographyIndex == coordinates.Count - 1)
            {
                builder.EndFigure();
            }
        }

        /*
        private void AddCoordinates(Coordinate[] coordinates)
        {
            
            int points = 0;

			Array.ForEach(coordinates, delegate(Coordinate coordinate)
			{
				double? z = null;
				if (!double.IsNaN(coordinate.Z) && !double.IsInfinity(coordinate.Z))
				{
					z = coordinate.Z;
				}
				if (points == 0)
				{
					_builder.BeginFigure(coordinate.Y, coordinate.X, z, null);
				}
				else
				{
					_builder.AddLine(coordinate.Y, coordinate.X, z, null);
				}
				points++;
			});
			if (points != 0)
			{
				_builder.EndFigure();
			}
		}
         */

        /*
		public SqlGeography ConstructedGeography
		{
			get { return _builder.ConstructedGeography; }
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
                value = Ordinates.XY | (value & AllowedOrdinates);
                _handleOrdinates = value;
            }
        }

        #endregion

        #region Implementation of IBinaryGeometryWriter

        public ByteOrder ByteOrder
        {
            get { return ByteOrder.LittleEndian; }
            set { }
        }

        #endregion
	}
}
