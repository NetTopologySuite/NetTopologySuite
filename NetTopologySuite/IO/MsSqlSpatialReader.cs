using System.IO;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	public class MsSqlSpatialReader : WKBReader
	{
		public override IGeometry Read(Stream stream)
		{
			BinaryReader reader = null;
			ByteOrder byteOrder = (ByteOrder)stream.ReadByte();
			try
			{
				if (byteOrder == ByteOrder.BigEndian)
					reader = new BEBinaryReader(stream);
				else reader = new BinaryReader(stream);
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
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

	}
}