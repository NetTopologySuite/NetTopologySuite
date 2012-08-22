using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for AttributesTableConverterTest and is intended
    ///    to contain all AttributesTableConverterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class AttributesTableConverterTest
    {       
        ///<summary>
        ///    A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            AttributesTableConverter target = new AttributesTableConverter();
            Type objectType = typeof(AttributesTable); 
            const bool expected = true; 
            bool actual = target.CanConvert(objectType);
            Assert.AreEqual(expected, actual);
        }

        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonTest()
        {
            AttributesTableConverter target = new AttributesTableConverter();
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            var value = new AttributesTable();
            value.AddAttribute("test1", "value1");
            value.AddAttribute("test2", "value2");
            JsonSerializer serializer = new JsonSerializer();
            target.WriteJson(writer, value, serializer);
            writer.Flush();
            Assert.AreEqual("\"properties\":{\"test2\":\"value2\",\"test1\":\"value1\"}", sb.ToString());
        }
    }
}