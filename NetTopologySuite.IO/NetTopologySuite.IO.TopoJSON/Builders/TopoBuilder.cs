using System;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Geometries;

namespace NetTopologySuite.IO.Builders
{
    internal class TopoBuilder : ITopoBuilder
    {
        private readonly string _type;
        private readonly long _id;
        private readonly IAttributesTable _properties;
        private readonly double[][] _coordinates;
        private readonly int[][][] _arcs;
        private readonly TopoObject[] _geometries;

        public TopoBuilder(string type, 
            long id, 
            IAttributesTable properties, 
            double[][] coordinates, 
            int[][][] arcs, 
            TopoObject[] geometries)
        {
            if (String.IsNullOrEmpty(type))
                throw new ArgumentNullException("type", "type null");
            _type = type;
            _id = id;
            _properties = properties ?? new AttributesTable();
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
                    obj = new TopoCollection(_type, _geometries);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled type: " + _type);
            }
            obj.Id = _id;
            obj.Properties = _properties;
            return obj;
        }
    }
}
