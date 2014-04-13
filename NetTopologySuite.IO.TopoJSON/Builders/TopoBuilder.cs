using System;
using NetTopologySuite.Features;
using NetTopologySuite.IO.TopoJSON.Geometries;

namespace NetTopologySuite.IO.TopoJSON.Builders
{
    internal class TopoBuilder : ITopoBuilder
    {
        private readonly string _type;
        private readonly IAttributesTable _properties;
        private readonly double[][] _coordinates;
        private readonly int[][][] _arcs;
        private readonly TopoObject[] _geometries;

        public TopoBuilder(string type, IAttributesTable properties, 
            double[][] coordinates, int[][][] arcs, TopoObject[] geometries)
        {
            if (String.IsNullOrEmpty(type))
                throw new ArgumentNullException("type", "type null");
            _type = type;
            _properties = properties;
            _coordinates = coordinates;
            _arcs = arcs;
            _geometries = geometries;
        }

        public TopoObject Build()
        {
            TopoObject obj;
            switch (_type)
            {
                case "Point":                    
                    obj = new TopoPoint(_type, _coordinates);
                    break;
                case "LineString":
                    obj = new TopoLineString(_type, _arcs);
                    break;                
                case "Polygon":
                    obj = new TopoPolygon(_type, _arcs);
                    break;
                case "MultiPoint":
                    obj = new TopoMultiPoint(_type, _coordinates);
                    break;
                case "MultiLineString":
                    obj = new TopoMultiLineString(_type, _arcs);
                    break;
                case "MultiPolygon":
                    obj = new TopoMultiPolygon(_type, _arcs);
                    break;
                case "GeometryCollection":
                    obj = new TopoGeometryCollection(_type, _geometries);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled type: " + _type);
            }
            obj.Properties = _properties;
            return obj;
        }
    }
}
