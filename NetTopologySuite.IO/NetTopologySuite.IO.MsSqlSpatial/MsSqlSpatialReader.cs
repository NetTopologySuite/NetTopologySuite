using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;

namespace NetTopologySuite.IO
{
    public class MsSqlSpatialReader : WKBReader
    {
        public override IGeometry Read(Stream stream)
        {
            var byteOrder = (ByteOrder)stream.ReadByte();
            using (var reader = new ConfigurableBinaryReader(stream, byteOrder))
            {
                IGeometry geometry = Read(reader);
                int srid = -1;
                try
                {
                    srid = reader.ReadInt32();
                }
                catch { }
                if (srid == 0)
                    srid = -1;
                geometry.SRID = srid;
                return geometry;
            }
        }
    }
}