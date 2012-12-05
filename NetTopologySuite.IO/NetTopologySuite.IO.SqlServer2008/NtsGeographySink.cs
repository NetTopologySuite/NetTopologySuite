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
using System.Collections.Generic;
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
	internal class NtsGeographySink : IGeographySink
	{
		private IGeometry _geometry;
		private int _srid;
		
        private readonly Stack<OpenGisGeographyType> _types = new Stack<OpenGisGeographyType>();
		private CoordinateBuffer _coordinateBuffer = new CoordinateBuffer();
        //private List<Coordinate> _coordinates = new List<Coordinate>();
		private readonly List<ICoordinateSequence> _rings = new List<ICoordinateSequence>();
        //private readonly List<Coordinate[]> _rings = new List<Coordinate[]>();
		//private readonly List<IGeometry> _geometries = new List<IGeometry>();
		private bool _inFigure;

        private readonly GeoAPI.IGeometryServices _geometryServices;
	    private IGeometryFactory _factory;
	    private List<IGeometry> _ccGeometries;
	    private readonly Stack<List<IGeometry>> _ccGeometriesStack = new Stack<List<IGeometry>>();

        //public NtsGeographySink() 
        //    :this(GeometryFactory.Default)
        //{}

	    public NtsGeographySink(GeoAPI.IGeometryServices geometryServices)
	    {
            _geometryServices = geometryServices;
	    }

        public IGeometry ConstructedGeometry
        {
            get { return _geometry; }
        }

		private void AddCoordinate(double x, double y, double? z, double? m)
		{
			_coordinateBuffer.AddCoordinate(y, x, z, m);
            /*
            Coordinate coordinate;
			if (z.HasValue)
			{
				coordinate = new Coordinate(y, x, z.Value);
			}
			else
			{
				coordinate = new Coordinate(y, x);
			}
			_coordinates.Add(coordinate);
             */
		}

		#region IGeometrySink Members

		public void AddLine(double x, double y, double? z, double? m)
		{
			if (!_inFigure)
			{
				throw new ApplicationException();
			}
			AddCoordinate(x, y, z, m);
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (_inFigure)
			{
				throw new ApplicationException();
			}
            _coordinateBuffer = new CoordinateBuffer();
			//_coordinates = new List<Coordinate>();
			AddCoordinate(x, y, z, m);
			_inFigure = true;
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			_types.Push(type);

            switch (type)
            {
                case OpenGisGeographyType.GeometryCollection:
                case OpenGisGeographyType.MultiPoint:
                case OpenGisGeographyType.MultiLineString:
                case OpenGisGeographyType.MultiPolygon:
                    _ccGeometries = new List<IGeometry>();
                    _ccGeometriesStack.Push(_ccGeometries);
                    break;
            }
        }

		public void EndFigure()
		{
			var type = _types.Peek();
			if (type == OpenGisGeographyType.Polygon)
			{
				_rings.Add(_coordinateBuffer.ToSequence(_factory.CoordinateSequenceFactory));
                //_rings.Add(_coordinates.ToArray());
			}
			_inFigure = false;
		}

		public void EndGeography()
		{
			IGeometry geometry = null;

			OpenGisGeographyType type = _types.Pop();

			switch (type)
			{
				case OpenGisGeographyType.Point:
					geometry = BuildPoint();
					break;
				case OpenGisGeographyType.LineString:
					geometry = BuildLineString();
					break;
				case OpenGisGeographyType.Polygon:
					geometry = BuildPolygon();
					break;
				case OpenGisGeographyType.MultiPoint:
					geometry = BuildMultiPoint();
					break;
				case OpenGisGeographyType.MultiLineString:
					geometry = BuildMultiLineString();
					break;
				case OpenGisGeographyType.MultiPolygon:
					geometry = BuildMultiPolygon();
					break;
				case OpenGisGeographyType.GeometryCollection:
					geometry = BuildGeometryCollection();
					break;
			}

			if (_types.Count == 0)
			{
			    _geometry = geometry;
			    if (_geometry != null) _geometry.SRID = _srid;
			}
			else
			{
                switch (type)
                {
                    case OpenGisGeographyType.GeometryCollection:
                    case OpenGisGeographyType.MultiPoint:
                    case OpenGisGeographyType.MultiLineString:
                    case OpenGisGeographyType.MultiPolygon:
                        _ccGeometriesStack.Pop();
                        _ccGeometries = _ccGeometriesStack.Peek();
                        break;
                }
                _ccGeometries.Add(geometry);
            }
		}

		private IGeometry BuildPoint()
		{
            var seq = _coordinateBuffer.ToSequence(_factory.CoordinateSequenceFactory);
		    return _factory.CreatePoint(seq);
		    //return _factory.CreatePoint(_coordinates[0]);
		}

		private ILineString BuildLineString()
		{
		    var seq = _coordinateBuffer.ToSequence(_factory.CoordinateSequenceFactory);
		    return _factory.CreateLineString(seq);
            //return _factory.CreateLineString(_coordinates.ToArray());
		}

		private IGeometry BuildPolygon()
		{
			if (_rings.Count == 0)
			{
                return _factory.CreatePolygon(null, null);
			}
            var shell = _factory.CreateLinearRing(_rings[0]);
			var holes = _rings.GetRange(1, _rings.Count - 1)
					.ConvertAll(coordinates => _factory.CreateLinearRing(coordinates)).ToArray();
			_rings.Clear();
            return _factory.CreatePolygon(shell, holes);
		}

		private IGeometry BuildMultiPoint()
		{
			var points = _ccGeometries
                .ConvertAll(g => g as IPoint).ToArray();
            return _factory.CreateMultiPoint(points);
		}

		private IGeometry BuildMultiLineString()
		{
            var lineStrings = _ccGeometries
                .ConvertAll(g => g as ILineString).ToArray();
            return _factory.CreateMultiLineString(lineStrings);
		}

		private IGeometry BuildMultiPolygon()
		{
			var polygons =
                _ccGeometries.ConvertAll(g => g as IPolygon).ToArray();
            return _factory.CreateMultiPolygon(polygons);
		}

		private IGeometryCollection BuildGeometryCollection()
		{
            return _factory.CreateGeometryCollection(_ccGeometries.ToArray());
		}

		public void SetSrid(int srid)
		{
			_srid = srid;
		    _factory = _geometryServices.CreateGeometryFactory(_srid);
		}

		#endregion
	}
}
