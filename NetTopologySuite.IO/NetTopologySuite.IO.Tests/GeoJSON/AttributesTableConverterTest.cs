using System;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

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
            AttributesTable value = new AttributesTable();
            value.AddAttribute("test1", "value1");
            value.AddAttribute("test2", "value2");
            JsonSerializer serializer = new JsonSerializer();
            target.WriteJson(writer, value, serializer);
            writer.Flush();
            Assert.AreEqual("\"properties\":{\"test1\":\"value1\",\"test2\":\"value2\"}", sb.ToString());
        }

        ///<summary>
        ///    A test for ReadJson
        ///</summary>
        [Test]
        public void ReadJsonTest()
        {
            const string json = "{\"properties\":{\"test1\":\"value1\",\"test2\":\"value2\"}}";
            AttributesTableConverter target = new AttributesTableConverter();
            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
            {
                JsonSerializer serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                reader.Read();
                AttributesTable result =
                    (AttributesTable)
                    target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsFalse(reader.Read());  // read the end of object and ensure there are no more tokens available
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.AreEqual("value2", result["test2"]);
            }
        }

        [Test]
        public void ReadJsonWithInnerObjectTest()
        {
            const string json = "{\"properties\":{\"test1\":\"value1\",\"test2\": { \"innertest1\":\"innervalue1\" }}}";
            AttributesTableConverter target = new AttributesTableConverter();
            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
            {
                JsonSerializer serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                reader.Read();
                AttributesTable result =
                    (AttributesTable)
                    target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsFalse(reader.Read()); // read the end of object and ensure there are no more tokens available
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.IsNotNull(result["test2"]);
                Assert.IsInstanceOf<IAttributesTable>(result["test2"]);
                IAttributesTable inner = (IAttributesTable)result["test2"];
                Assert.AreEqual(1, inner.Count);
                Assert.AreEqual("innervalue1", inner["innertest1"]);
            }
        }
    }
}