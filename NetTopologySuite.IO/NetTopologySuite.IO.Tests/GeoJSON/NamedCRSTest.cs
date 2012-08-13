using NUnit.Framework;
using NetTopologySuite.CoordinateSystems;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for NamedCRSTest and is intended
    ///    to contain all NamedCRSTest Unit Tests
    ///</summary>
    [TestFixture]
    public class NamedCRSTest
    {      
        ///<summary>
        ///    A test for NamedCRS Constructor
        ///</summary>
        [Test]
        public void NamedCRSConstructorTest()
        {
            const string name = "testName";
            NamedCRS target = new NamedCRS(name);
            Assert.AreEqual(name, target.Properties["name"]);
            Assert.AreEqual(CRSTypes.Name, target.Type);
        }
    }
}