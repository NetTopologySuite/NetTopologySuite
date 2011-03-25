using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Data;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO.GeoTools
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