// 	Ported from PostGIS:
// 	http://svn.refractions.net/postgis/trunk/java/jdbc/src/org/postgis/binary/BinaryParser.java

using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Converts a PostGIS binary data to a <c>Geometry</c>.
	/// </summary>
	public class PostGisReader
	{
		private IGeometryFactory factory = null;

        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        protected IGeometryFactory Factory
        {
            get { return factory; }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>. 
        /// </summary>
        public PostGisReader() : this(GeometryFactory.Default) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
		public PostGisReader(IGeometryFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IGeometry Read(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))            
                return Read(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public IGeometry Read(Stream stream)
        {
            BinaryReader reader = null;
            ByteOrder byteOrder = (ByteOrder) stream.ReadByte();
			// "Rewind" to let Read(BinaryReader) skip this byte
			// in collection and non-collection geometries.
			stream.Position = 0;
            try
            {
                if (byteOrder == ByteOrder.BigEndian)
                     reader = new BEBinaryReader(stream);
                else reader = new BinaryReader(stream);
                return Read(reader);
            }
            finally
            {
                if (reader != null) 
                    reader.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IGeometry Read(BinaryReader reader)
        {
			// Dummy read, just for bytes compatibility.
			// The byte order is determined only once.
			reader.ReadByte();

			int typeword = reader.ReadInt32();

			// cut off high flag bits
			PostGisGeometryType geometryType = (PostGisGeometryType)(typeword & 0x1FFFFFFF);

			bool hasZ = (typeword & 0x80000000) != 0;
			bool hasM = (typeword & 0x40000000) != 0;
			bool hasS = (typeword & 0x20000000) != 0;

			int srid = -1;

			if (hasS)
				srid = reader.ReadInt32();
			
			IGeometry result;
            switch (geometryType)
            {
                case PostGisGeometryType.Point:
					result = ReadPoint(reader, hasZ, hasM);
					break;
				case PostGisGeometryType.LineString:
					result = ReadLineString(reader, hasZ, hasM);
					break;
				case PostGisGeometryType.Polygon:
					result = ReadPolygon(reader, hasZ, hasM);
					break;
				case PostGisGeometryType.MultiPoint:
					result = ReadMultiPoint(reader);
					break;
				case PostGisGeometryType.MultiLineString:
					result = ReadMultiLineString(reader);
					break;
				case PostGisGeometryType.MultiPolygon:
					result = ReadMultiPolygon(reader);
					break;
				case PostGisGeometryType.GeometryCollection:
					result = ReadGeometryCollection(reader);
					break;
				default:
                    throw new ArgumentException("Geometry type not recognized. GeometryCode: " + geometryType);
            }

			result.SRID = hasS ? srid : -1;
			return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
		protected ICoordinate ReadCoordinate(BinaryReader reader, bool hasZ, bool hasM)
        {
			double X = reader.ReadDouble();
			double Y = reader.ReadDouble();
			ICoordinate result;
			if (hasZ)
			{
				double Z = reader.ReadDouble();
				result = new Coordinate(X, Y, Z);
			}
			else result = new Coordinate(X, Y);
			
			if (hasM)
			{
				double M = reader.ReadDouble();
				//result.setM(M);
			}

			return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
		protected IPoint ReadPoint(BinaryReader reader, bool hasZ, bool hasM)
        {
            return Factory.CreatePoint(ReadCoordinate(reader, hasZ, hasM));
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		protected ICoordinate[] ReadCoordinateArray(BinaryReader reader, bool hasZ, bool hasM)
		{
			int numPoints = reader.ReadInt32();
			ICoordinate[] coordinates = new ICoordinate[numPoints];
			for (int i = 0; i < numPoints; i++)
				 coordinates[i] = ReadCoordinate(reader, hasZ, hasM);
			return coordinates;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
		protected ILineString ReadLineString(BinaryReader reader, bool hasZ, bool hasM)
        {
			ICoordinate[] coordinates = ReadCoordinateArray(reader, hasZ, hasM);
            return Factory.CreateLineString(coordinates);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		protected ILinearRing ReadLinearRing(BinaryReader reader, bool hasZ, bool hasM)
		{
			ICoordinate[] coordinates = ReadCoordinateArray(reader, hasZ, hasM);
			return Factory.CreateLinearRing(coordinates);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
		protected IPolygon ReadPolygon(BinaryReader reader, bool hasZ, bool hasM)
        {
			int numRings = reader.ReadInt32();
            ILinearRing exteriorRing = ReadLinearRing(reader, hasZ, hasM);
            ILinearRing[] interiorRings = new ILinearRing[numRings - 1];
            for (int i = 0; i < numRings - 1; i++)
				interiorRings[i] = ReadLinearRing(reader, hasZ, hasM);
            return Factory.CreatePolygon(exteriorRing, interiorRings);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="container"></param>
		protected void ReadGeometryArray(BinaryReader reader, IGeometry[] container)
		{
			for (int i = 0; i < container.Length; i++)
				container[i] = Read(reader);			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IMultiPoint ReadMultiPoint(BinaryReader reader)
        {
			int numGeometries = reader.ReadInt32();
			IPoint[] points = new IPoint[numGeometries];
			ReadGeometryArray(reader, points);
            return Factory.CreateMultiPoint(points);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IMultiLineString ReadMultiLineString(BinaryReader reader)
        {
			int numGeometries = reader.ReadInt32();
            ILineString[] strings = new ILineString[numGeometries];
			ReadGeometryArray(reader, strings);
			return Factory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IMultiPolygon ReadMultiPolygon(BinaryReader reader)
        {
            int numGeometries = reader.ReadInt32();
            IPolygon[] polygons = new IPolygon[numGeometries];
			ReadGeometryArray(reader, polygons);
			return Factory.CreateMultiPolygon(polygons);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IGeometryCollection ReadGeometryCollection(BinaryReader reader)
        {
			int numGeometries = reader.ReadInt32();
			IGeometry[] geometries = new IGeometry[numGeometries];
			ReadGeometryArray(reader, geometries);
            return Factory.CreateGeometryCollection(geometries);
        }
	}
}
