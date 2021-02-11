using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixture]
    public class SerializabilityTest
    {
        [Test, Order(0)]
        public void TestSerializable1()
        {
            var fact = new GeometryFactory();
            var gsf = new NetTopologySuite.Utilities.GeometricShapeFactory(fact) ;
            gsf.Size = 250;
            var g = (Geometry)gsf.CreateCircle();

            // serialize the object
            byte[] bytes = null;
            Assert.DoesNotThrow(() => bytes = SerializationUtility.Serialize(g));

            // Assert that there was some serialized content produced
            Assert.IsNotNull(bytes, "There was no serialized packet produced");
            Assert.IsTrue(bytes.Length > 0, "There was no data in the serialized packet produced");

            // deserialize and check
            var gCopy = SerializationUtility.Deserialize<Geometry>(bytes);
            Assert.IsTrue(gCopy.EqualsExact(g), "The deserialized object does not match the original");
        }

        [TestCase("POINT(10 10)")]
        [TestCase("LINESTRING(10  10, 20 20)")]
        [TestCase("POLYGON((0 0, 0 10, 10 10, 10 0, 0 0), (1 1, 1 2, 2 2, 2 1, 1 1), (8 8, 8 9, 9 9, 9 8, 8 8))")]
        [TestCase("MULTIPOINT((10 10), (20 20))")]
        [TestCase("MULTILINESTRING((10  10, 20 20), (20 20, 30 20))")]
        [TestCase("MULTIPOLYGON(((0 0, 0 10, 10 10, 10 0, 0 0), (1 1, 1 2, 2 2, 2 1, 1 1)), ((8 8, 8 9, 9 9, 9 8, 8 8)))")]
        [TestCase("GEOMETRYCOLLECTION(POINT(10 10),LINESTRING(10  10, 20 20),POLYGON((0 0, 0 10, 10 10, 10 0, 0 0), (1 1, 1 2, 2 2, 2 1, 1 1), (8 8, 8 9, 9 9, 9 8, 8 8)))")]
        public void TestSerializable(string wkt)
        {
            var reader = new WKTReader();
            var gS = reader.Read(wkt);
            byte[] buffer = SerializationUtility.Serialize(gS);

            var gD = SerializationUtility.Deserialize<Geometry>(buffer);
            Assert.IsTrue(gD.EqualsExact(gS));
        }
    }
}
