using System;
using System.Collections;
using System.IO;

using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{    
    /// <summary>
    /// Converts a Well-Known Binary byte data to a <c>Geometry</c>.
    /// </summary>
    public class WKBReader
    {
        private GeometryFactory factory = null;

        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        protected GeometryFactory Factory
        {
            get { return factory; }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>. 
        /// </summary>
        public WKBReader() : this(new GeometryFactory()) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        public WKBReader(GeometryFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Geometry Read(byte[] data)
        {
            using(Stream stream = new MemoryStream(data))            
                return Read(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Geometry Read(Stream stream)
        {
            BinaryReader reader = null;
            ByteOrder byteOrder = (ByteOrder) stream.ReadByte();
            try
            {
                if (byteOrder == ByteOrder.BigIndian)
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
        protected Geometry Read(BinaryReader reader)
        {     
            WKBGeometryTypes geometryType = (WKBGeometryTypes)reader.ReadInt32();
            switch (geometryType)
            {
                case WKBGeometryTypes.WKBPoint:
                    return ReadPoint(reader);
                case WKBGeometryTypes.WKBLineString:
                    return ReadLineString(reader);
                case WKBGeometryTypes.WKBPolygon:
                    return ReadPolygon(reader);
                case WKBGeometryTypes.WKBMultiPoint:
                    return ReadMultiPoint(reader);
                case WKBGeometryTypes.WKBMultiLineString:
                    return ReadMultiLineString(reader);
                case WKBGeometryTypes.WKBMultiPolygon:
                    return ReadMultiPolygon(reader);
                case WKBGeometryTypes.WKBGeometryCollection:
                    return ReadGeometryCollection(reader);
                default:
                    throw new ArgumentException("Geometry type not recognized. GeometryCode: " + geometryType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected ByteOrder ReadByteOrder(BinaryReader reader)
        {
            byte byteOrder = reader.ReadByte();
            return (ByteOrder)byteOrder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Coordinate ReadCoordinate(BinaryReader reader)
        {
            return new Coordinate(reader.ReadDouble(), reader.ReadDouble());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected LinearRing ReadRing(BinaryReader reader)
        {
            int numPoints = reader.ReadInt32();
            Coordinate[] coordinates = new Coordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
                coordinates[i] = ReadCoordinate(reader);
            return Factory.CreateLinearRing(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadPoint(BinaryReader reader)
        {
            return Factory.CreatePoint(ReadCoordinate(reader));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadLineString(BinaryReader reader)
        {
            int numPoints = reader.ReadInt32();
            Coordinate[] coordinates = new Coordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
                coordinates[i] = ReadCoordinate(reader);
            return Factory.CreateLineString(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadPolygon(BinaryReader reader)
        {
            int numRings = reader.ReadInt32();
            LinearRing exteriorRing = ReadRing(reader);
            LinearRing[] interiorRings = new LinearRing[numRings - 1];
            for (int i = 0; i < numRings - 1; i++)
                interiorRings[i] = ReadRing(reader);
            return Factory.CreatePolygon(exteriorRing, interiorRings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadMultiPoint(BinaryReader reader)
        {
            int numGeometries = reader.ReadInt32();
            Point[] points = new Point[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPoint)
                    throw new ArgumentException("Point feature expected");
                points[i] = ReadPoint(reader) as Point;
            }
            return Factory.CreateMultiPoint(points);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadMultiLineString(BinaryReader reader)
        {
            int numGeometries = reader.ReadInt32();
            LineString[] strings = new LineString[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBLineString)
                    throw new ArgumentException("LineString feature expected");
                strings[i] = ReadLineString(reader) as LineString ;
            }
            return Factory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadMultiPolygon(BinaryReader reader)
        {
            int numGeometries = reader.ReadInt32();
            Polygon[] polygons = new Polygon[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPolygon)
                    throw new ArgumentException("Polygon feature expected");
                polygons[i] = ReadPolygon(reader) as Polygon;
            }
            return Factory.CreateMultiPolygon(polygons);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry ReadGeometryCollection(BinaryReader reader)
        {
            int numGeometries = reader.ReadInt32();
            Geometry[] geometries = new Geometry[numGeometries];

            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes)reader.ReadInt32();
                switch (geometryType)
                {
                    case WKBGeometryTypes.WKBPoint:
                        geometries[i] = ReadPoint(reader);
                        break;
                    case WKBGeometryTypes.WKBLineString:
                        geometries[i] = ReadLineString(reader);
                        break;
                    case WKBGeometryTypes.WKBPolygon:
                        geometries[i] = ReadPolygon(reader);
                        break;
                    case WKBGeometryTypes.WKBMultiPoint:
                        geometries[i] = ReadMultiPoint(reader);
                        break;
                    case WKBGeometryTypes.WKBMultiLineString:
                        geometries[i] = ReadMultiLineString(reader);
                        break;
                    case WKBGeometryTypes.WKBMultiPolygon:
                        geometries[i] = ReadMultiPolygon(reader);
                        break;
                    case WKBGeometryTypes.WKBGeometryCollection:
                        geometries[i] = ReadGeometryCollection(reader);
                        break;
                    default:
                        throw new ArgumentException("Should never reach here!");
                }                
            }
            return Factory.CreateGeometryCollection(geometries);
        }
    }
}
