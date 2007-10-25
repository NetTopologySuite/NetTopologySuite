using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public class SerializationSamples : BaseSamples
    {
        private readonly string filepath = String.Empty;
        private IFormatter serializer = null;

        private ICoordinate[] coordinates = null;
        private IPoint point = null;
        private ILineString line = null;
        private IPolygon polygon = null;
        private IMultiPoint multiPoint = null;

        public SerializationSamples() : base()
        {
            filepath = Path.GetTempPath() + "\\testserialization.bin";

            serializer = new BinaryFormatter(); 

            point = Factory.CreatePoint(new Coordinate(100, 100));

            coordinates = new ICoordinate[]
            {
                 new Coordinate(10,10),
                 new Coordinate(20,20),
                 new Coordinate(20,10),                 
            };
            line = Factory.CreateLineString(coordinates);

            coordinates = new ICoordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,100),
                new Coordinate(200,200),                
                new Coordinate(100,200),
                new Coordinate(100,100),
            };
            ILinearRing linearRing = Factory.CreateLinearRing(coordinates);
            polygon = Factory.CreatePolygon(linearRing, null);

            coordinates = new ICoordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,200),
                new Coordinate(300,300),                
                new Coordinate(400,400),
                new Coordinate(500,500),
            };
            multiPoint = Factory.CreateMultiPoint(coordinates);                                    
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            TestSerialization(point);
            TestSerialization(line);
            TestSerialization(polygon);
            TestSerialization(multiPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        private void TestSerialization(IGeometry geom)
        {            
            using (Stream stream = File.OpenWrite(filepath))
                serializer.Serialize(stream, geom);

            using (Stream stream = File.OpenRead(filepath))
                Console.WriteLine(serializer.Deserialize(stream));
        }
    }
}
