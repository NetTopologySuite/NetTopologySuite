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
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;

namespace NetTopologySuite.IO
{
	public class MsSql2008GeographyWriter
	{
		private readonly SqlGeographyBuilder _builder = new SqlGeographyBuilder();

		public SqlGeography Write(IGeometry geometry)
		{
			_builder.SetSrid(geometry.SRID);
			AddGeometry(geometry);
			return _builder.ConstructedGeography;
		}

		private void AddGeometry(IGeometry geometry)
		{
			if (geometry is IPoint)
			{
				AddPoint(geometry);
			}
			else if (geometry is ILineString)
			{
				AddLineString(geometry);
			}
			else if (geometry is IPolygon)
			{
				AddPolygon(geometry);
			}
			else if (geometry is IMultiPoint)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.MultiPoint);
			}
			else if (geometry is IMultiLineString)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.MultiLineString);
			}
			else if (geometry is IMultiPolygon)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.MultiPolygon);
			}
			else if (geometry is IGeometryCollection)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.GeometryCollection);
			}
		}

		private void AddGeometryCollection(IGeometry geometry, OpenGisGeographyType type)
		{
			_builder.BeginGeography(type);
			var coll = geometry as IGeometryCollection;
            Debug.Assert(coll != null, "coll != null");
		    Array.ForEach(coll.Geometries, AddGeometry);
		    _builder.EndGeography();
		}

		private void AddPolygon(IGeometry geometry)
		{
			_builder.BeginGeography(OpenGisGeographyType.Polygon);
			IPolygon polygon = geometry as IPolygon;
		    Debug.Assert(polygon != null, "polygon != null");
		    AddCoordinates(polygon.ExteriorRing.Coordinates);
			Array.ForEach<ILineString>(polygon.InteriorRings, delegate(ILineString ring)
			{
				AddCoordinates(ring.Coordinates);
			});
			_builder.EndGeography();
		}

		private void AddLineString(IGeometry geometry)
		{
			_builder.BeginGeography(OpenGisGeographyType.LineString);
			AddCoordinates(geometry.Coordinates);
			_builder.EndGeography();
		}

		private void AddPoint(IGeometry geometry)
		{
			_builder.BeginGeography(OpenGisGeographyType.Point);
			AddCoordinates(geometry.Coordinates);
			_builder.EndGeography();
		}

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

		public SqlGeography ConstructedGeography
		{
			get { return _builder.ConstructedGeography; }
		}
	}
}
