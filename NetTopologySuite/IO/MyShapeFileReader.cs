using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    ///  A class for reading shapefiles data.
    /// </summary>
    [Obsolete("Use ShapefileReader instead")]
    public class MyShapeFileReader
    {
        private int length = 0;

        /// <summary>
        /// Shape features reader.
        /// </summary>
        protected ShapeReader shapeReader = null;
        
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public MyShapeFileReader()
        {
            shapeReader = new ShapeReader();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public MyShapeFileReader(GeometryFactory factory)
        {
            shapeReader = new ShapeReader(factory);
        }

        /// <summary>
        /// Reads a shapefile containing geographic data, 
        /// and returns a collection of all the features contained.
        /// Since NTS Geometry Model not support Z and M data, those informations are ignored if presents in shapefile.
        /// </summary>
        /// <param name="filepath">Shapefile path.</param>
        /// <returns><c>GeometryCollection</c> containing all geometries in shapefile.</returns>
        public IGeometryCollection Read(string filepath)
        {
            using (Stream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(stream);
            }
        }        

        /// <summary>
        /// Reads a generic stream containing geographic data saved as shapefile structure, 
        /// and returns a collection of all the features contained.
        /// Since NTS Geometry Model not support Z and M data, those informations are ignored if presents in shapefile.
        /// </summary>
        /// <param name="stream">Shapefile data stream.</param>
        /// <returns><c>GeometryCollection</c> containing all geometries in shapefile.</returns>
        protected IGeometryCollection Read(Stream stream)
        {
            // Read big endian values
            using (BigEndianBinaryReader beReader = new BigEndianBinaryReader(stream))
            {
                // Verify File Code
                int fileCode = beReader.ReadInt32BE();
                Debug.Assert(fileCode == 9994);

                stream.Seek(20, SeekOrigin.Current);
                length = beReader.ReadInt32BE();                
                
                // Read little endian values
                using (BinaryReader leReader = new BinaryReader(stream))
                {
                    ArrayList list = null;

                    // Verify Version
                    int version = leReader.ReadInt32();
                    Debug.Assert(version == 1000);

                    // ShapeTypes
                    int shapeType = leReader.ReadInt32();         

                    switch ((ShapeGeometryType) shapeType)
                    {
                        case ShapeGeometryType.Point:
                        case ShapeGeometryType.PointZ:
                        case ShapeGeometryType.PointM:
                        case ShapeGeometryType.PointZM:
                            list = new ArrayList(ReadPointData(stream));
                            break;

                        case ShapeGeometryType.LineString:
                        case ShapeGeometryType.LineStringZ:
                        case ShapeGeometryType.LineStringM:
                        case ShapeGeometryType.LineStringZM:
                            list = new ArrayList(ReadLineStringData(stream));
                            break;

                        case ShapeGeometryType.Polygon:
                        case ShapeGeometryType.PolygonZ:
                        case ShapeGeometryType.PolygonM:
                        case ShapeGeometryType.PolygonZM:
                            list = new ArrayList(ReadPolygonData(stream));
                            break;

                        case ShapeGeometryType.MultiPoint:
                        case ShapeGeometryType.MultiPointZ:
                        case ShapeGeometryType.MultiPointM:
                        case ShapeGeometryType.MultiPointZM:
                            list = new ArrayList(ReadMultiPointData(stream));
                            break;

                        case ShapeGeometryType.MultiPatch:
                            throw new NotImplementedException("FeatureType " + shapeType + " not supported.");

                        default:
                            throw new ArgumentOutOfRangeException("FeatureType " + shapeType + " not recognized by the system");
                    }
                    IGeometryCollection collection = shapeReader.CreateGeometryCollection(list);
                    return collection;
                }                               
            }
        }             

        /// <summary>
        /// Reads Point shapefile
        /// </summary>
        /// <param name="stream"></param>
        protected IList ReadPointData(Stream stream)
        {
            IList list = new ArrayList();

            // Jump to first header                 
            stream.Seek(100, SeekOrigin.Begin);

            // Read big endian informations
            using (BigEndianBinaryReader beReader = new BigEndianBinaryReader(stream))
            {
                // Read little endian informations
                using (BinaryReader leReader = new BinaryReader(stream))
                {                                                                
                    // For each header                
                    while (stream.Position < stream.Length)
                    {
                        ReadFeatureHeader(beReader);                  
                                            
                        ICoordinate coordinate = shapeReader.ReadCoordinate(leReader);
                        IGeometry point = shapeReader.CreatePoint(coordinate);
                        list.Add(point);
                    }
                }              
            }
            return list;  
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beReader"></param>
        private void ReadFeatureHeader(BigEndianBinaryReader beReader)
        {
            int recordNumber = beReader.ReadInt32BE();
            int contentLength = beReader.ReadInt32BE();
            int shapeType = beReader.ReadInt32();
        }        

        /// <summary>
        /// Reads LineString shapefile
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected IList ReadLineStringData(Stream stream)
        {
            IList list = new ArrayList();

            // Jump to first header 
            stream.Seek(100, SeekOrigin.Begin);              

            // Read big endian informations
            using (BigEndianBinaryReader beReader = new BigEndianBinaryReader(stream))
            {
                // Read little endian informations
                using (BinaryReader leReader = new BinaryReader(stream))
                {
                    // For each header                
                    while (stream.Position < stream.Length)
                    {
                        ReadFeatureHeader(beReader);
                        shapeReader.ReadBoundingBox(leReader);

                        int[] indexParts = null;
                        int numParts = shapeReader.ReadNumParts(leReader);
                        int numPoints = shapeReader.ReadNumPoints(leReader);

                        indexParts = shapeReader.ReadIndexParts(leReader, numParts);
                        ICoordinate[] coordinates = shapeReader.ReadCoordinates(leReader, numPoints);

                        if (numParts == 1)
                            list.Add(shapeReader.CreateLineString(coordinates));
                        else list.Add(shapeReader.CreateMultiLineString(numPoints, indexParts, coordinates));
                    }                    
                }
            }
            return list;
        }

        /// <summary>
        /// Reads Polygon shapefile
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected IList ReadPolygonData(Stream stream)
        {
            IList list = new ArrayList();

            // Jump to first header                
            stream.Seek(100, SeekOrigin.Begin); 

            // Read big endian informations
            using (BigEndianBinaryReader beReader = new BigEndianBinaryReader(stream))
            {
                // Read little endian informations
                using (BinaryReader reader = new BinaryReader(stream))
                {                         
                    // For each header                
                    while (stream.Position < stream.Length)
                    {
                        ReadFeatureHeader(beReader);
                        shapeReader.ReadBoundingBox(reader);

                        int[] indexParts = null;
                        int numParts = shapeReader.ReadNumParts(reader);
                        int numPoints = shapeReader.ReadNumPoints(reader);

                        indexParts = shapeReader.ReadIndexParts(reader, numParts);
                        ICoordinate[] coordinates = shapeReader.ReadCoordinates(reader, numPoints);

                        if (numParts == 1)
                             list.Add(shapeReader.CreateSimpleSinglePolygon(coordinates));
                        else list.Add(shapeReader.CreateSingleOrMultiPolygon(numPoints, indexParts, coordinates));
                    }                    
                }
            }
            return list;
        }

        /// <summary>
        /// Reads MultiPoint shapefile
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected IList ReadMultiPointData(Stream stream)
        {
            IList list = new ArrayList();

            // Jump to first header                
            stream.Seek(100, SeekOrigin.Begin); 

            // Read big endian informations
            using (BigEndianBinaryReader beReader = new BigEndianBinaryReader(stream))
            {
                // Read little endian informations
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // For each header                
                    while (stream.Position < stream.Length)
                    {
                        ReadFeatureHeader(beReader);
                        shapeReader.ReadBoundingBox(reader);

                        int numPoints = shapeReader.ReadNumPoints(reader);
                        ICoordinate[] coords = new ICoordinate[numPoints];
                        for (int i = 0; i < numPoints; i++)
                            coords[i] = shapeReader.ReadCoordinate(reader);
                        list.Add(shapeReader.CreateMultiPoint(coords));
                    }
             
                }
            }
            return list;
        }        
    }
}
