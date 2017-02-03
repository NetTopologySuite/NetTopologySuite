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
			BinaryWriter writer = null;
			try
			{
				if (EncodingType == ByteOrder.LittleEndian)
					writer = new BinaryWriter(stream);
				else writer = new BEBinaryWriter(stream);
				Write(geometry, writer);
				writer.Write(geometry.SRID);
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
		}
	}
}