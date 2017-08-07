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
            using (BinaryReader reader = byteOrder == ByteOrder.BigEndian ? new BEBinaryReader(stream) : new BinaryReader(stream))
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