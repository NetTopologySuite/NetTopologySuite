// 	Ported from PostGIS:
// 	http://svn.refractions.net/postgis/trunk/java/jdbc/src/org/postgis/binary/BinaryParser.java

using System;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
	/// <summary>
	/// Converts a PostGIS binary data to a <c>Geometry</c>.
	/// </summary>
	public class PostGisReader : IBinaryGeometryReader
	{
		private IGeometryFactory _factory;

        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        public IGeometryFactory Factory
        {
            get { return _factory; }
            set
            {
                if (value != null)
                    _factory = value;
            }
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
            _factory = factory;
            HandleOrdinates = AllowedOrdinates;
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
            var byteOrder = (ByteOrder) stream.ReadByte();
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
			var geometryType = (PostGisGeometryType)(typeword & 0x1FFFFFFF);

			var hasZ = (typeword & 0x80000000) != 0;
			var hasM = (typeword & 0x40000000) != 0;
			var hasS = (typeword & 0x20000000) != 0;

            var ordinates = Ordinates.XY;
            if (hasZ) ordinates |= Ordinates.Z;
            if (hasM) ordinates |= Ordinates.M;

			var srid = -1;

			if (hasS)
				srid = reader.ReadInt32();
			
			IGeometry result;
            switch (geometryType)
            {
                case PostGisGeometryType.Point:
                    result = ReadPoint(reader, ordinates);
					break;
				case PostGisGeometryType.LineString:
                    result = ReadLineString(reader, ordinates);
					break;
				case PostGisGeometryType.Polygon:
                    result = ReadPolygon(reader, ordinates);
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
	    /// Reads a point from the stream
	    /// </summary>
	    /// <param name="reader">The binary reader.</param>
        /// <param name="ordinates">The ordinates to read. <see cref="Ordinates.XY"/> are always read.</param>
        /// <returns>The Point.</returns>
	    protected IPoint ReadPoint(BinaryReader reader, Ordinates ordinates)
        {
            return Factory.CreatePoint(ReadCoordinateSequence(reader, 1, ordinates));
        }

	    /// <summary>
	    /// Reads a coordinate sequence from the stream, which length is not yet known.
	    /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <param name="ordinates">The ordinates to read. <see cref="Ordinates.XY"/> are always read.</param>
	    /// <returns>The coordinate sequence</returns>
	    protected ICoordinateSequence ReadCoordinateSequence(BinaryReader reader, Ordinates ordinates)
		{
			var numPoints = reader.ReadInt32();
	        return ReadCoordinateSequence(reader, numPoints, ordinates);
		}

	    /// <summary>
	    /// Reads a <see cref="ICoordinateSequence"/> from the stream
	    /// </summary>
	    /// <param name="reader">The binary reader</param>
	    /// <param name="numPoints">The number of points in the coordinate sequence.</param>
	    /// <param name="ordinates">The ordinates to read. <see cref="Ordinates.XY"/> are always read.</param>
	    /// <returns>The coordinate sequence</returns>
	    protected ICoordinateSequence ReadCoordinateSequence(BinaryReader reader, int numPoints, Ordinates ordinates)
        {
            var sequence = _factory.CoordinateSequenceFactory.Create(numPoints, ordinates);
            for (var i = 0; i < numPoints; i++)
            {
                sequence.SetOrdinate(i, Ordinate.X, reader.ReadDouble());
                sequence.SetOrdinate(i, Ordinate.Y, reader.ReadDouble());
                if ((ordinates & Ordinates.Z) != 0)
                    sequence.SetOrdinate(i, Ordinate.Z, reader.ReadDouble());
                if ((ordinates & Ordinates.M) != 0)
                    sequence.SetOrdinate(i, Ordinate.M, reader.ReadDouble());
            }
            return sequence;
        }

        /// <summary>
        /// Reads a <see cref="ILineString"/> from the input stream.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <param name="ordinates">The ordinates to read. <see cref="Ordinates.XY"/> are always read.</param>
        /// <returns>The LineString.</returns>
		protected ILineString ReadLineString(BinaryReader reader, Ordinates ordinates)
        {
            var coordinates = ReadCoordinateSequence(reader, ordinates);
            return Factory.CreateLineString(coordinates);
        }

		/// <summary>
        /// Reads a <see cref="ILinearRing"/> line string from the input stream.
        /// </summary>
		/// <param name="reader">The binary reader.</param>
        /// <param name="ordinates">The ordinates to read. <see cref="Ordinates.XY"/> are always read.</param>
        /// <returns>The LinearRing.</returns>
		protected ILinearRing ReadLinearRing(BinaryReader reader, Ordinates ordinates)
		{
			var coordinates = ReadCoordinateSequence(reader, ordinates);
			return Factory.CreateLinearRing(coordinates);
		}

        /// <summary>
        /// Reads a <see cref="IPolygon"/> from the input stream.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <param name="ordinates">The ordinates to read. <see cref="Ordinates.XY"/> are always read.</param>
        /// <returns>The LineString.</returns>
        protected IPolygon ReadPolygon(BinaryReader reader, Ordinates ordinates)
        {
			var numRings = reader.ReadInt32();
            var exteriorRing = ReadLinearRing(reader, ordinates);
            var interiorRings = new ILinearRing[numRings - 1];
            for (var i = 0; i < numRings - 1; i++)
				interiorRings[i] = ReadLinearRing(reader, ordinates);
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

	    #region Implementation of IGeometryIOSettings

	    public bool HandleSRID
	    {
	        get { return true; }
	        set { }
	    }

	    public Ordinates AllowedOrdinates
	    {
	        get { return _factory.CoordinateSequenceFactory.Ordinates | Ordinates.XYZM; }
	    }

	    private Ordinates _handleOrdinates;
	    public Ordinates HandleOrdinates
	    {
	        get { return _handleOrdinates; }
	        set 
            { 
                value |= AllowedOrdinates;
	            _handleOrdinates = value;
	        }
	    }

	    #endregion
	}
}
