using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Samples.SimpleTests.Geometries
{
    /// <summary>
    ///
    /// </summary>
    public class SerializationSamples : BaseSamples
    {
        private readonly string filepath = string.Empty;
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        private IFormatter serializer = null;
#pragma warning restore SYSLIB0011 // Type or member is obsolete

        private Coordinate[] coordinates = null;
        private Point point = null;
        private LineString line = null;
        private Polygon polygon = null;
        private MultiPoint multiPoint = null;

        public SerializationSamples() : base()
        {
            filepath = Path.GetTempPath() + "\\testserialization.bin";

            // don't use BinaryFormatter in production. read this instead:
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            serializer = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            point = Factory.CreatePoint(new Coordinate(100, 100));

            coordinates = new Coordinate[]
            {
                 new Coordinate(10,10),
                 new Coordinate(20,20),
                 new Coordinate(20,10),
            };
            line = Factory.CreateLineString(coordinates);

            coordinates = new Coordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,100),
                new Coordinate(200,200),
                new Coordinate(100,200),
                new Coordinate(100,100),
            };
            var linearRing = Factory.CreateLinearRing(coordinates);
            polygon = Factory.CreatePolygon(linearRing, null);

            coordinates = new Coordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,200),
                new Coordinate(300,300),
                new Coordinate(400,400),
                new Coordinate(500,500),
            };
            multiPoint = Factory.CreateMultiPointFromCoords(coordinates);
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
        private void TestSerialization(Geometry geom)
        {
            using (Stream stream = File.OpenWrite(filepath))
                serializer.Serialize(stream, geom);

            using (Stream stream = File.OpenRead(filepath))
                Console.WriteLine(serializer.Deserialize(stream));
        }
    }
}
