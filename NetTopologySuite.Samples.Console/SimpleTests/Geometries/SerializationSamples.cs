using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.SimpleTests.Geometries
{
    public class SerializationSamples : BaseSamples
    {
        private readonly ICoordinate[] coordinates;
        private readonly String filepath = String.Empty;
        private readonly ILineString line;
        private readonly IMultiPoint multiPoint;
        private readonly IPoint point;
        private readonly IPolygon polygon;
        private readonly IFormatter serializer;

        public SerializationSamples()
        {
            filepath = Path.GetTempPath() + "\\testserialization.bin";

            serializer = new BinaryFormatter();

            ICoordinateFactory<BufferedCoordinate> coordFactory
                = new BufferedCoordinateFactory();

            point = GeoFactory.CreatePoint(coordFactory.Create(100, 100));

            coordinates = new ICoordinate[]
                              {
                                  coordFactory.Create(10, 10),
                                  coordFactory.Create(20, 20),
                                  coordFactory.Create(20, 10),
                              };
            line = GeoFactory.CreateLineString(coordinates);

            coordinates = new ICoordinate[]
                              {
                                  coordFactory.Create(100, 100),
                                  coordFactory.Create(200, 100),
                                  coordFactory.Create(200, 200),
                                  coordFactory.Create(100, 200),
                                  coordFactory.Create(100, 100),
                              };
            ILinearRing linearRing = GeoFactory.CreateLinearRing(coordinates);
            polygon = GeoFactory.CreatePolygon(linearRing, null);

            coordinates = new ICoordinate[]
                              {
                                  coordFactory.Create(100, 100),
                                  coordFactory.Create(200, 200),
                                  coordFactory.Create(300, 300),
                                  coordFactory.Create(400, 400),
                                  coordFactory.Create(500, 500),
                              };
            multiPoint = GeoFactory.CreateMultiPoint(coordinates);
        }

        public override void Start()
        {
            TestSerialization(point);
            TestSerialization(line);
            TestSerialization(polygon);
            TestSerialization(multiPoint);
        }

        private void TestSerialization(IGeometry geom)
        {
            using (Stream stream = File.OpenWrite(filepath))
            {
                serializer.Serialize(stream, geom);
            }

            using (Stream stream = File.OpenRead(filepath))
            {
                Console.WriteLine(serializer.Deserialize(stream));
            }
        }
    }
}