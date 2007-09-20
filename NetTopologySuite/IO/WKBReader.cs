using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{    
    /// <summary>
    /// Converts a Well-Known Binary byte data to a <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// WKBReader reads <see cref="ICoordinate" /> XYZ values from the stream if Z is not <see cref="Double.NaN"/>, 
    /// otherwise <see cref="ICoordinate.Z" /> value is set to <see cref="Double.NaN"/>.
    /// </remarks>
    // Thanks to Roberto Acioli for ICoordinate.Z patch
    // TODO: support for XYZM streams
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
        public WKBReader() : this(new GeometryFactory()) { }

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

        /// <summary>
        /// 
        /// </summary>
        protected enum CoordinateSystem
        {
            /// <summary>
            /// 
            /// </summary>
            XY=1, 

            /// <summary>
            /// 
            /// </summary>
            XYZ=2,  

            /// <summary>
            /// 
            /// </summary>
            XYM=3, 

            /// <summary>
            /// 
            /// </summary>
            XYZM=4,
        };

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
                case WKBGeometryTypes.WKBPoint:
                    return ReadPoint(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBPointZ:
                    return ReadPoint(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBLineString:
                    return ReadLineString(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBLineStringZ:
                    return ReadLineString(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBPolygon:
                    return ReadPolygon(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBPolygonZ:
                    return ReadPolygon(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBMultiPoint:
                    return ReadMultiPoint(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBMultiPointZ:
                    return ReadMultiPoint(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBMultiLineString:
                    return ReadMultiLineString(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBMultiLineStringZ:
                    return ReadMultiLineString(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBMultiPolygon:
                    return ReadMultiPolygon(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBMultiPolygonZ:
                    return ReadMultiPolygon(reader, CoordinateSystem.XYZ);
                case WKBGeometryTypes.WKBGeometryCollection:
                    return ReadGeometryCollection(reader, CoordinateSystem.XY);
                case WKBGeometryTypes.WKBGeometryCollectionZ:
                    return ReadGeometryCollection(reader, CoordinateSystem.XYZ);
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
            switch (cs)
            {
                case CoordinateSystem.XY:
                    return new Coordinate(reader.ReadDouble(), reader.ReadDouble());
                case CoordinateSystem.XYZ:
                    return new Coordinate(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                default:
                    throw new ArgumentException(String.Format("Coordinate system not supported: {0}", cs));
            }            
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
                    case WKBGeometryTypes.WKBPoint:
                        geometries[i] = ReadPoint(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBPointZ:
                        geometries[i] = ReadPoint(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBLineString:
                        geometries[i] = ReadLineString(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBLineStringZ:
                        geometries[i] = ReadLineString(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBPolygon:
                        geometries[i] = ReadPolygon(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBPolygonZ:
                        geometries[i] = ReadPolygon(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBMultiPoint:
                        geometries[i] = ReadMultiPoint(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBMultiPointZ:
                        geometries[i] = ReadMultiPoint(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBMultiLineString:
                        geometries[i] = ReadMultiLineString(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBMultiLineStringZ:
                        geometries[i] = ReadMultiLineString(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBMultiPolygon:
                        geometries[i] = ReadMultiPolygon(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBMultiPolygonZ:
                        geometries[i] = ReadMultiPolygon(reader, CoordinateSystem.XYZ);
                        break;
                    case WKBGeometryTypes.WKBGeometryCollection:
                        geometries[i] = ReadGeometryCollection(reader, CoordinateSystem.XY);
                        break;
                    case WKBGeometryTypes.WKBGeometryCollectionZ:
                        geometries[i] = ReadGeometryCollection(reader, CoordinateSystem.XYZ);
                        break;
                    default:
                        throw new ArgumentException("Should never reach here!");
                }                
            }
            return Factory.CreateGeometryCollection(geometries);
        }
    }
}
