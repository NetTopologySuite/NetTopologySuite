using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixtureAttribute]
    public class SerializabilityTest
    {
        [TestAttribute]
        public void TestSerializable()
        {
            GeometryFactory fact = new GeometryFactory();
            NetTopologySuite.Utilities.GeometricShapeFactory gsf = 
                new NetTopologySuite.Utilities.GeometricShapeFactory(fact);
            IGeometry g = gsf.CreateCircle();
            // serialize the object
            byte[] bytes = SerializationUtility.Serialize(g);            
            // Assert that there was some serialized content produced
            Assert.IsTrue(bytes.Length > 0, "There was no serialized packet produced");
            // deserialize and check
            IGeometry gCopy = SerializationUtility.Deserialize<IGeometry>(bytes);
            Assert.IsTrue(gCopy.EqualsExact(g), "The deserialized object does not match the original");
        }
    }
}