using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{    
    /// <summary>
    /// Converts a Well-Known Binary byte data to a <c>Geometry</c>.
    /// </summary>
    public class WKBReader
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
        public WKBReader() : this(GeometryFactory.Default) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        public WKBReader(IGeometryFactory factory)
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
        public virtual IGeometry Read(Stream stream)
        {
            BinaryReader reader = null;
            ByteOrder byteOrder = (ByteOrder) stream.ReadByte();
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

        protected enum CoordinateSystem { XY=1, XYZ=2,  XYM=3, XYZM=4};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IGeometry Read(BinaryReader reader)
        {     
            WKBGeometryTypes geometryType = (WKBGeometryTypes) reader.ReadInt32();
            switch (geometryType)
            {
                //Point
                case WKBGeometryTypes.WKBPoint:
                    return ReadPoint(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBPointZ:
                    return ReadPoint(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBPointM:
                    return ReadPoint(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBPointZM:
                    return ReadPoint(reader, CoordinateSystem.XYZM);
                //Line String
                case WKBGeometryTypes.WKBLineString:
                    return ReadLineString(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBLineStringZ:
                    return ReadLineString(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBLineStringM:
                    return ReadLineString(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBLineStringZM:
                    return ReadLineString(reader, CoordinateSystem.XYZM);
                //Polygon
                case WKBGeometryTypes.WKBPolygon:
                    return ReadPolygon(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBPolygonZ:
                    return ReadPolygon(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBPolygonM:
                    return ReadPolygon(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBPolygonZM:
                    return ReadPolygon(reader, CoordinateSystem.XYZM);
                //Multi Point
                case WKBGeometryTypes.WKBMultiPoint:
                    return ReadMultiPoint(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBMultiPointZ:
                    return ReadMultiPoint(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBMultiPointM:
                    return ReadMultiPoint(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBMultiPointZM:
                    return ReadMultiPoint(reader, CoordinateSystem.XYZM);
                //Multi Line String
                case WKBGeometryTypes.WKBMultiLineString:
                    return ReadMultiLineString(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBMultiLineStringZ:
                    return ReadMultiLineString(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBMultiLineStringM:
                    return ReadMultiLineString(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBMultiLineStringZM:
                    return ReadMultiLineString(reader, CoordinateSystem.XYZM);
                //Multi Polygon
                case WKBGeometryTypes.WKBMultiPolygon:
                    return ReadMultiPolygon(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBMultiPolygonZ:
                    return ReadMultiPolygon(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBMultiPolygonM:
                    return ReadMultiPolygon(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBMultiPolygonZM:
                    return ReadMultiPolygon(reader, CoordinateSystem.XYZM);
                //Geometry Collection
                case WKBGeometryTypes.WKBGeometryCollection:
                    return ReadGeometryCollection(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBGeometryCollectionZ:
                    return ReadGeometryCollection(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBGeometryCollectionM:
                    return ReadGeometryCollection(reader, CoordinateSystem.XYM);
                case WKBGeometryTypes.WKBGeometryCollectionZM:
                    return ReadGeometryCollection(reader, CoordinateSystem.XYZM);
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
            return (ByteOrder) byteOrder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected ICoordinate ReadCoordinate(BinaryReader reader, CoordinateSystem cs)
        {
            Coordinate coordinate;
            switch (cs)
            {
                case CoordinateSystem.XY:
                    coordinate = new Coordinate(reader.ReadDouble(), reader.ReadDouble());
                    break;
                case CoordinateSystem.XYZ:
                    coordinate = new Coordinate(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                    break;
                case CoordinateSystem.XYM:
                    coordinate = new Coordinate(reader.ReadDouble(), reader.ReadDouble());
                    reader.ReadDouble();
                    break;
                case CoordinateSystem.XYZM:
                    coordinate = new Coordinate(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                    reader.ReadDouble();
                    break;
                default:
                    throw new ArgumentException(String.Format("Coordinate system not supported: {0}", cs));
            }
            return coordinate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected ILinearRing ReadRing(BinaryReader reader, CoordinateSystem cs)
        {
            int numPoints = reader.ReadInt32();
            ICoordinate[] coordinates = new ICoordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
                coordinates[i] = ReadCoordinate(reader, cs);
            return Factory.CreateLinearRing(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadPoint(BinaryReader reader, CoordinateSystem cs)
        {
            return Factory.CreatePoint(ReadCoordinate(reader, cs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadLineString(BinaryReader reader, CoordinateSystem cs)
        {
            int numPoints = reader.ReadInt32();
            ICoordinate[] coordinates = new ICoordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
                coordinates[i] = ReadCoordinate(reader, cs);
            return Factory.CreateLineString(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadPolygon(BinaryReader reader, CoordinateSystem cs)
        {
            int numRings = reader.ReadInt32();
            ILinearRing exteriorRing = ReadRing(reader, cs);
            ILinearRing[] interiorRings = new ILinearRing[numRings - 1];
            for (int i = 0; i < numRings - 1; i++)
                interiorRings[i] = ReadRing(reader, cs);
            return Factory.CreatePolygon(exteriorRing, interiorRings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadMultiPoint(BinaryReader reader, CoordinateSystem cs)
        {
            int numGeometries = reader.ReadInt32();
            IPoint[] points = new IPoint[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPoint)
                    throw new ArgumentException("IPoint feature expected");
                points[i] = ReadPoint(reader, cs) as IPoint;
            }
            return Factory.CreateMultiPoint(points);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadMultiLineString(BinaryReader reader, CoordinateSystem cs)
        {
            int numGeometries = reader.ReadInt32();
            ILineString[] strings = new ILineString[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes) reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBLineString)
                    throw new ArgumentException("ILineString feature expected");
                strings[i] = ReadLineString(reader, cs) as ILineString ;
            }
            return Factory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadMultiPolygon(BinaryReader reader, CoordinateSystem cs)
        {
            int numGeometries = reader.ReadInt32();
            IPolygon[] polygons = new IPolygon[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes) reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPolygon)
                    throw new ArgumentException("IPolygon feature expected");
                polygons[i] = ReadPolygon(reader, cs) as IPolygon;
            }
            return Factory.CreateMultiPolygon(polygons);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        protected IGeometry ReadGeometryCollection(BinaryReader reader, CoordinateSystem cs)
        {
            int numGeometries = reader.ReadInt32();
            IGeometry[] geometries = new IGeometry[numGeometries];

            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                WKBGeometryTypes geometryType = (WKBGeometryTypes) reader.ReadInt32();
                switch (geometryType)
                {
                    //Point
                    case WKBGeometryTypes.WKBPoint:
                        geometries[i] = ReadPoint(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBPointZ:
                        geometries[i] = ReadPoint(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBPointM:
                        geometries[i] = ReadPoint(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBPointZM:
                        geometries[i] = ReadPoint(reader, CoordinateSystem.XYZM);
                        break;
                    //Line String
                    case WKBGeometryTypes.WKBLineString:
                        geometries[i] = ReadLineString(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBLineStringZ:
                        geometries[i] = ReadLineString(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBLineStringM:
                        geometries[i] = ReadLineString(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBLineStringZM:
                        geometries[i] = ReadLineString(reader, CoordinateSystem.XYZM);
                        break;
                    //Polygon
                    case WKBGeometryTypes.WKBPolygon:
                        geometries[i] = ReadPolygon(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBPolygonZ:
                        geometries[i] = ReadPolygon(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBPolygonM:
                        geometries[i] = ReadPolygon(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBPolygonZM:
                        geometries[i] = ReadPolygon(reader, CoordinateSystem.XYZM);
                        break;
                    //Multi Point
                    case WKBGeometryTypes.WKBMultiPoint:
                        geometries[i] = ReadMultiPoint(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBMultiPointZ:
                        geometries[i] = ReadMultiPoint(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBMultiPointM:
                        geometries[i] = ReadMultiPoint(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBMultiPointZM:
                        geometries[i] = ReadMultiPoint(reader, CoordinateSystem.XYZM);
                        break;
                    //Multi Line String
                    case WKBGeometryTypes.WKBMultiLineString:
                        geometries[i] = ReadMultiLineString(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBMultiLineStringZ:
                        geometries[i] = ReadMultiLineString(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBMultiLineStringM:
                        geometries[i] = ReadMultiLineString(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBMultiLineStringZM:
                        geometries[i] = ReadMultiLineString(reader, CoordinateSystem.XYZM);
                        break;
                    //Multi Polygon
                    case WKBGeometryTypes.WKBMultiPolygon:
                        geometries[i] = ReadMultiPolygon(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBMultiPolygonZ:
                        geometries[i] = ReadMultiPolygon(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBMultiPolygonM:
                        geometries[i] = ReadMultiPolygon(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBMultiPolygonZM:
                        geometries[i] = ReadMultiPolygon(reader, CoordinateSystem.XYZM);
                        break;
                    //Geometry Collection
                    case WKBGeometryTypes.WKBGeometryCollection:
                        geometries[i] = ReadGeometryCollection(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBGeometryCollectionZ:
                        geometries[i] = ReadGeometryCollection(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBGeometryCollectionM:
                        geometries[i] = ReadGeometryCollection(reader, CoordinateSystem.XYM);
                        break;
                    case WKBGeometryTypes.WKBGeometryCollectionZM:
                        geometries[i] = ReadGeometryCollection(reader, CoordinateSystem.XYZM);
                        break;
                    default:
                        throw new ArgumentException("Should never reach here!");
                }                
            }
            return Factory.CreateGeometryCollection(geometries);
        }
    }
}
