// 	Ported from PostGIS:
// 	http://svn.refractions.net/postgis/trunk/java/jdbc/src/org/postgis/binary/BinaryWriter.java

using System;
using System.IO;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Writes a PostGIS binary representation of a <c>Geometry</c>.
    /// </summary>
    public class PostGisWriter
    {
        protected ByteOrder encodingType;

        /// <summary>
        /// Initializes writer with LittleIndian byte order.
        /// </summary>
        public PostGisWriter() : 
            this(ByteOrder.LittleEndian) { }

        /// <summary>
        /// Initializes writer with the specified byte order.
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
		public PostGisWriter(ByteOrder encodingType)
        {
            this.encodingType = encodingType;
        }

        /// <summary>
        /// Writes a binary encoded PostGIS of a given geometry.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public byte[] Write(IGeometry geometry)
        {
            byte[] bytes = GetBytes(geometry);
            Write(geometry, new MemoryStream(bytes));
            return bytes;
        }

        /// <summary>
		/// Writes a binary encoded PostGIS of a given geometry.
		/// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public void Write(IGeometry geometry, Stream stream)
        {
            BinaryWriter writer = null;
            try
            {
				if (encodingType == ByteOrder.LittleEndian)
					 writer = new BinaryWriter(stream);
				else writer = new BEBinaryWriter(stream);
				Write(geometry, writer);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometry geometry, BinaryWriter writer)
        {
			if (geometry is IPoint)
                Write(geometry as IPoint, writer);
			else if (geometry is ILinearRing)
				Write(geometry as ILinearRing, writer);
			else if (geometry is ILineString)
                Write(geometry as ILineString, writer);
            else if (geometry is IPolygon)
                Write(geometry as IPolygon, writer);
            else if (geometry is IMultiPoint)
                Write(geometry as IMultiPoint, writer);
            else if (geometry is IMultiLineString)
                Write(geometry as IMultiLineString, writer);
            else if (geometry is IMultiPolygon)
                Write(geometry as IMultiPolygon, writer);
            else if (geometry is IGeometryCollection)
                Write(geometry as IGeometryCollection, writer);
            else throw new ArgumentException("Geometry not recognized: " + geometry.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="type"></param>
        /// <param name="writer"></param>
		private void WriteHeader(IGeometry geometry, PostGisGeometryType type, BinaryWriter writer)
		{
			writer.Write((byte) encodingType);

			// write typeword
			uint typeword = (uint) type;

            if (hasZ(geometry))
                typeword |= 0x80000000;			
			
            //if (geometry.HasMeasure)
			//    typeword |= 0x40000000;
			
			if (geometry.SRID != -1)
				typeword |= 0x20000000;
			writer.Write(typeword);

			if (geometry.SRID != -1)
				writer.Write(geometry.SRID);			
		}

		/// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
		/// <param name="baseGeometry"></param>
        /// <param name="writer"></param>
		protected void Write(ICoordinate coordinate, IGeometry baseGeometry, BinaryWriter writer)
		{
			if (coordinate != null)
			{
				writer.Write((double) coordinate.X);
				writer.Write((double) coordinate.Y);

                if (hasZ(baseGeometry))
				    writer.Write((double) coordinate.Z);
				
				//if (baseGeometry.HasMeasure)
				//    writer.Write((double)coordinate.M);
				
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(IPoint point, BinaryWriter writer)
        {
            WriteHeader(point, PostGisGeometryType.Point, writer);
            Write(point.Coordinate, point, writer);
        }

		/// <summary>
		/// Write an Array of "full" Geometries
		/// </summary>
		/// <param name="geometries"></param>
		/// <param name="writer"></param>
		private void Write(IGeometry[] geometries, BinaryWriter writer)
		{
			for (int i = 0; i < geometries.Length; i++)
				Write(geometries[i], writer);
		}

		/// <summary>
		/// Write an Array of "slim" Points (without endianness, srid and type, 
		/// part of LinearRing and Linestring, but not MultiPoint!
		/// </summary>
		/// <param name="coordinates"></param>
		/// <param name="baseGeometry"></param>
		/// <param name="writer"></param>
		private void Write(ICoordinate[] coordinates, IGeometry baseGeometry, BinaryWriter writer)
		{
			writer.Write((int) coordinates.Length);
			for (int i = 0; i < coordinates.Length; i++)
				Write(coordinates[i], baseGeometry, writer);			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(ILineString lineString, BinaryWriter writer)
        {
			WriteHeader(lineString, PostGisGeometryType.LineString, writer);
			Write(lineString.Coordinates, lineString, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="writer"></param>
        protected void Write(ILinearRing linearRing, BinaryWriter writer)
        {
			Write(linearRing.Coordinates, linearRing, writer);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(IPolygon polygon, BinaryWriter writer)
        {
			WriteHeader(polygon, PostGisGeometryType.Polygon, writer);
            writer.Write((int) polygon.NumInteriorRings + 1);
            Write(polygon.ExteriorRing as ILinearRing, writer);
			Write(polygon.InteriorRings, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPoint multiPoint, BinaryWriter writer)
        {
			WriteHeader(multiPoint, PostGisGeometryType.MultiPoint, writer);
			writer.Write((int)multiPoint.NumGeometries);
			Write(multiPoint.Geometries, writer);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiLineString multiLineString, BinaryWriter writer)
        {
			WriteHeader(multiLineString, PostGisGeometryType.MultiLineString, writer);
			writer.Write((int) multiLineString.NumGeometries);
			Write(multiLineString.Geometries, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPolygon multiPolygon, BinaryWriter writer)
        {
			WriteHeader(multiPolygon, PostGisGeometryType.MultiPolygon, writer);
			writer.Write((int) multiPolygon.NumGeometries);
			Write(multiPolygon.Geometries, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomCollection"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometryCollection geomCollection, BinaryWriter writer)
        {
			WriteHeader(geomCollection, PostGisGeometryType.GeometryCollection, writer);
			writer.Write((int) geomCollection.NumGeometries);
			Write(geomCollection.Geometries, writer);
        }

		/// <summary>
		/// Sets corrent length for Byte Stream.
		/// </summary>
		/// <param name="geometry"></param>
		/// <returns></returns>
		protected byte[] GetBytes(IGeometry geometry)
		{
			return new byte[SetByteStream(geometry)];
		}

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IGeometry geometry)
        {
			int result = 0;

			// write endian flag
			result += 1;

			// write typeword
			result += 4;

			if (geometry.SRID != -1)
				result += 4;
			
			if (geometry is IPoint)
				result += SetByteStream(geometry as IPoint);
            else if (geometry is ILineString)
                result +=  SetByteStream(geometry as ILineString);
            else if (geometry is IPolygon)
                result +=  SetByteStream(geometry as IPolygon);
            else if (geometry is IMultiPoint)
                result +=  SetByteStream(geometry as IMultiPoint);
            else if (geometry is IMultiLineString)
                result +=  SetByteStream(geometry as IMultiLineString);
            else if (geometry is IMultiPolygon)
                result +=  SetByteStream(geometry as IMultiPolygon);
            else if (geometry is IGeometryCollection)
                result +=  SetByteStream(geometry as IGeometryCollection);
            else throw new ArgumentException("ShouldNeverReachHere");

			return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IGeometryCollection geometry)
        {
			// 4-byte count + subgeometries
			return 4 + SetByteStream(geometry.Geometries);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiPolygon geometry)
        {
			// 4-byte count + subgeometries
			return 4 + SetByteStream(geometry.Geometries);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiLineString geometry)
        {
			// 4-byte count + subgeometries
			return 4 + SetByteStream(geometry.Geometries);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiPoint geometry)
        {
			// int size
			int result = 4;
			if (geometry.NumPoints > 0)
			{
				// We can shortcut here, as all subgeoms have the same fixed size
				result += geometry.NumPoints * SetByteStream(geometry.Geometries[0] as IGeometry);
			}
			return result;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IPolygon geometry)
        {
			// int length
			int result = 4;
			result += SetByteStream(geometry.ExteriorRing);
			for (int i = 0; i < geometry.NumInteriorRings; i++)
				result += SetByteStream(geometry.InteriorRings[i]);
			return result;
        }

		/// <summary>
        /// Write an Array of "full" Geometries
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		private int SetByteStream(IGeometry[] container)
		{
			int result = 0;
			for (int i = 0; i < container.Length; i++)
				result += SetByteStream(container[i]);
			return result;
		}

		/// <summary>
		/// Write an Array of "slim" Points (without endianness and type, part of
		/// LinearRing and Linestring, but not MultiPoint!
		/// </summary>
		/// <param name="coordinates"></param>
		/// <param name="geometry"></param>
		/// <returns></returns>
		private int SetByteStream(ICoordinate[] coordinates, IGeometry geometry)
		{
			// number of points
			int result = 4;
			// And the amount of the points itsself, in consistent geometries
			// all points have equal size.
			if (coordinates.Length > 0)
				result += coordinates.Length * SetByteStream(coordinates[0], geometry);
			return result;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(ILineString geometry)
        {
			return SetByteStream(geometry.Coordinates, geometry);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="geometry"></param>
		/// <returns></returns>
		protected int SetByteStream(ILinearRing geometry)
		{
			return SetByteStream(geometry.Coordinates, geometry);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IPoint geometry)
        {
			return SetByteStream(geometry.Coordinate, geometry);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="geometry"></param>
		/// <returns></returns>
		protected int SetByteStream(ICoordinate coordinate, IGeometry geometry)
		{
			if (coordinate == null)
				return 0;

			// x, y both have 8 bytes
			int result = 16;

            if (hasZ(geometry))
			    result += 8;
			
			//if (geometry.HasMeasure)
			//    result += 8;
			
			return result;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private static bool hasZ(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return false;
        
            if (geometry is IPoint) 
                 return hasZ((geometry as IPoint).CoordinateSequence);
            if (geometry is ILineString) 
                 return hasZ((geometry as ILineString).CoordinateSequence);
            else if (geometry is IPolygon) 
                 return hasZ((geometry as IPolygon).ExteriorRing.CoordinateSequence);
            else return hasZ(geometry.GetGeometryN(0));            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCoordinateSequence"></param>
        /// <returns></returns>
        private static bool hasZ(ICoordinateSequence coords)
        {
            if (coords == null || coords.Count == 0)
                return false;

            int dimensions = coords.Dimension;
            if (coords.Dimension == 3)
            {
                // CoordinateArraySequence will always return 3, so we have to
                // check, if the third ordinate contains NaN, then the geom is actually 2-dimensional
                return Double.IsNaN(coords.GetOrdinate(0, Ordinates.Z)) ? false : true;
            }
            else return false;
        }
	}
}
