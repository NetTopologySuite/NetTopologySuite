using GeoAPI.Geometries;
using NetTopologySuite.Data;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.GeoTools
{
    public class StringToGeometryConverter : CustomConverterBase<string, IGeometry>
    {
        private readonly WKTReader _reader;

        public StringToGeometryConverter() : this(new GeometryFactory())
        {
        }

        public StringToGeometryConverter(IGeometryFactory geometryFactory)
        {
            _reader = new WKTReader(geometryFactory);
        }

        public override IGeometry Convert(string source)
        {
            return _reader.Read(source);
        }
    }
}