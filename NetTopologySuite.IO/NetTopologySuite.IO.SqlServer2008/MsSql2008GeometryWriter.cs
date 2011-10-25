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
using Microsoft.SqlServer.Types;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    public class MsSql2008GeometryWriter
    {
        private readonly SqlGeometryBuilder builder = new SqlGeometryBuilder();

        public SqlGeometry Write(IGeometry geometry)
        {
            builder.SetSrid(geometry.SRID);
            AddGeometry(geometry);
            return builder.ConstructedGeometry;
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
                AddGeometryCollection(geometry, OpenGisGeometryType.MultiPoint);
            }
            else if (geometry is IMultiLineString)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.MultiLineString);
            }
            else if (geometry is IMultiPolygon)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.MultiPolygon);
            }
            else if (geometry is IGeometryCollection)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.GeometryCollection);
            }
        }

        private void AddGeometryCollection(IGeometry geometry, OpenGisGeometryType type)
        {
            builder.BeginGeometry(type);
            IGeometryCollection coll = geometry as IGeometryCollection;
            Array.ForEach<IGeometry>(coll.Geometries, delegate(IGeometry g)
            {
                AddGeometry(g);
            });
            builder.EndGeometry();
        }

        private void AddPolygon(IGeometry geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.Polygon);
            IPolygon polygon = geometry as IPolygon;
            AddCoordinates(polygon.ExteriorRing.Coordinates);
            Array.ForEach<ILineString>(polygon.InteriorRings, delegate(ILineString ring)
            {
                AddCoordinates(ring.Coordinates);
            });
            builder.EndGeometry();
        }

        private void AddLineString(IGeometry geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.LineString);
            AddCoordinates(geometry.Coordinates);
            builder.EndGeometry();
        }

        private void AddPoint(IGeometry geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.Point);
            AddCoordinates(geometry.Coordinates);
            builder.EndGeometry();
        }

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
                    builder.BeginFigure(coordinate.X, coordinate.Y, z, null);
                }
                else
                {
                    builder.AddLine(coordinate.X, coordinate.Y, z, null);
                }
                points++;
            });
            if (points != 0)
            {
                builder.EndFigure();
            }
        }

        public SqlGeometry ConstructedGeometry
        {
            get { return builder.ConstructedGeometry; }
        }
    }
}
