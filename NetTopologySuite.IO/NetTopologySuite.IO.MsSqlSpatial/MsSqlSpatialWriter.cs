using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;

namespace NetTopologySuite.IO
{
	public class MsSqlSpatialWriter : WKBWriter
	{
		protected override int SetByteStream(IGeometry geometry)
		{
			return base.SetByteStream(geometry) + 4; // sizeof(int)
		}

		public override byte[] Write(IGeometry geometry)
		{
			byte[] bytes = new byte[SetByteStream(geometry)];
			Write(geometry, new MemoryStream(bytes));
			return bytes;
		}

		public override void Write(IGeometry geometry, Stream stream)
		{
			using (BinaryWriter writer = EncodingType == ByteOrder.LittleEndian ? new BinaryWriter(stream) : new BEBinaryWriter(stream))
			{
				Write(geometry, writer);
				writer.Write(geometry.SRID);
			}
		}
	}
}