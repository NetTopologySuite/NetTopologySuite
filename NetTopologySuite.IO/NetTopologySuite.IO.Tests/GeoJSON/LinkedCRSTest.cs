using System;
using NUnit.Framework;
using NetTopologySuite.CoordinateSystems;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for LinkedCRSTest and is intended
    ///    to contain all LinkedCRSTest Unit Tests
    ///</summary>
    [TestFixture]
    public class LinkedCRSTest
    {        
        ///<summary>
        ///    A test for LinkedCRS Constructor
        ///</summary>
        [Test]
        public void LinkedCRSConstructorTest()
        {
            Uri href = new Uri("http://www.compass.ie/");
            const string type = "Testtype";
            LinkedCRS target = new LinkedCRS(href, type);
            Assert.AreEqual(href.AbsoluteUri, target.Properties["href"]);
            Assert.AreEqual(type, target.Properties["type"]);
            Assert.AreEqual(CRSTypes.Link, target.Type);
        }

        ///<summary>
        ///    A test for LinkedCRS Constructor
        ///</summary>
        [Test]
        public void LinkedCRSConstructorTest1()
        {
            const string href = "http://www.compass.ie/";
            const string type = "Testtype";
            LinkedCRS target = new LinkedCRS(href, type);
            Assert.AreEqual(href, target.Properties["href"]);
            Assert.AreEqual(type, target.Properties["type"]);
            Assert.AreEqual(CRSTypes.Link, target.Type);
        }
    }
}