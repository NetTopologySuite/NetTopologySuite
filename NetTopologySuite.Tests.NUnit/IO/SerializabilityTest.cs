using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixtureAttribute]
    public class SerializabilityTest
    {
        private GeometryFactory fact = new GeometryFactory();

        [TestAttribute]
        public void TestSerializable()
        {
            var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var stream = new System.IO.MemoryStream();

            NetTopologySuite.Utilities.GeometricShapeFactory gsf = new NetTopologySuite.Utilities.GeometricShapeFactory(fact);
            IGeometry g = gsf.CreateCircle();

            // serialize the object
            serializer.Serialize(stream, g);

            // Assert that there was some serialized content produced
            Assert.IsTrue(stream.Length > 0, "There was no serialized packet produced");

            // Move the position of the memory stream to the start to enable reading/deserialization
            stream.Position = 0;

            IGeometry gCopy = (IGeometry)serializer.Deserialize(stream);
            Assert.IsTrue(gCopy.EqualsExact(g), "The deserialized object does not match the original");
        }
    }
}