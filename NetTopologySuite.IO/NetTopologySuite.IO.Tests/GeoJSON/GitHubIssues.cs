using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    [Category("GitHub")]
    [TestFixture]
    public class GitHubIssues
    {
        [Test(Description = "Testcase for Issue 83")]
        public void TestIssue83()
        {
            var geoJson = @"{ ""type"": ""Feature"", 
                              ""geometry"": { ""type"": ""Point"", ""coordinates"": [10.0, 60.0] }, 
                              ""id"": 1, 
                             ""properties"": { ""Name"": ""test"" } }";

            var s = GeoJsonSerializer.Create(GeometryFactory.Default);
            Feature f = null;
            Assert.DoesNotThrow(() =>
                f = s.Deserialize<Feature>(new JsonTextReader(new StringReader(geoJson)))
                );

            Assert.IsNotNull(f, "f != null");
            Assert.IsTrue(FeatureExtensions.HasID(f), "f.HasID()");
            Assert.AreEqual(1, FeatureExtensions.ID(f), "f.ID != 1");

            var sb = new StringBuilder();
            var tw = new JsonTextWriter(new StringWriter(sb));
            s.Serialize(tw, f);
            var geoJsonRes = sb.ToString();

            CompareJson(geoJson, geoJsonRes);
        }

        public void CompareJson(string expectedJson, string json)
        {
            JToken e = JToken.Parse(expectedJson);
            JToken j = JToken.Parse(json);
            if (!JToken.DeepEquals(e, j))
                Assert.Fail("The json's do not match:\n1: {0}\n2:{1}", expectedJson, json);
        }


        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88WithoutAdditionalProperties()
        {
            string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	}
}
";

            GeoJsonSerializer serializer = new GeoJsonSerializer();
            Feature feat = null;
            Assert.DoesNotThrow(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                    feat = serializer.Deserialize<Feature>(reader);
            });

            Assert.IsNotNull(feat);
            Assert.IsTrue(FeatureExtensions.HasID(feat));
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", FeatureExtensions.ID(feat));
            IGeometry geometry = feat.Geometry;
            Assert.IsNull(geometry);
            IAttributesTable attributes = feat.Attributes;
            Assert.IsNotNull(attributes);
            Assert.AreEqual(3, attributes.Count);
            Assert.AreEqual(FeatureExtensions.ID(feat), attributes["id"]);
            Assert.AreEqual(false, attributes["yesNo 1"]);
            Assert.AreEqual(DateTime.Parse("2016-02-16T00:00:00", CultureInfo.InvariantCulture), attributes["date 1"]);
        }

        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88WithFlatProperties()
        {
            string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	},	
	""collection_userinfo"" : ""()"",
	""collection_timestamp"" : ""2016-02-25T14:38:01.9087672"",
	""collection_todoid"" : """",
	""collection_templateid"" : ""nj7Glv-AqV0"",
	""collection_layerid"" : ""nj7Glv-AqV0""	
}
";

            GeoJsonSerializer serializer = new GeoJsonSerializer();
            Feature feat = null;
            Assert.DoesNotThrow(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                    feat = serializer.Deserialize<Feature>(reader);
            });

            Assert.IsNotNull(feat);
            Assert.IsNull(feat.Geometry);
        }

        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88()
        {
            string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	},
	""metadata"" : {
		""collection_userinfo"" : ""()"",
		""collection_timestamp"" : ""2016-02-25T14:38:01.9087672"",
		""collection_todoid"" : """",
		""collection_templateid"" : ""nj7Glv-AqV0"",
		""collection_layerid"" : ""nj7Glv-AqV0""
	}
}
";

            GeoJsonSerializer serializer = new GeoJsonSerializer();
            Feature feat = null;
            Assert.DoesNotThrow(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                    feat = serializer.Deserialize<Feature>(reader);
            });

            Assert.IsNotNull(feat);
            Assert.IsNull(feat.Geometry);
        }

        private Feature _feature;
        private GeoJsonReader _reader;
        private GeoJsonWriter _writer;

        [TestFixtureSetUp]
        public void GivenAGeoJsonReaderAndWriter()
        {
            _reader = new GeoJsonReader();
            _writer = new GeoJsonWriter();
            var geometry = new Point(1, 2);
            _feature = new Feature(geometry, new AttributesTable());
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfJsonObjectArraysPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[
          {
            ""xyz"":[
                {""zee"":""xyz""},
                {""hay"":""zus""}
            ]
          }
        ]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfJsonObjectArraysPropertyInGeoJsonThenWriteable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[
          {
            ""xyz"":[
                {""zee"":""xyz""},
                {""hay"":""zus""}
            ]
          }
        ]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            var written = _writer.Write(featureCollection);
            Assert.IsTrue(written.Contains("FeatureCollection"));
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfIntArraysPropertyInFeatureThenWritable()
        {
            var arrayOfInts = new[] { 1, 2, 3 };
            var anotherArrayOfInts = new[] { 4, 5, 6 };
            var arrayOfArrays = new[] { arrayOfInts, anotherArrayOfInts };
            _feature.Attributes.AddAttribute("foo", arrayOfArrays);
            var written = _writer.Write(_feature);
            Assert.IsTrue(written.Contains("Feature"));
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfIntArraysPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
    ""type"":""Feature"",
    ""geometry"":{
        ""type"":""Point"",""coordinates"":[1.0,2.0]},
        ""properties"":{
            ""foo"":[[1,2,3],[4,5,6]]
        }
}
";
            var featureCollection = _reader.Read<Feature>(geojsonString);
            CompareJson(geojsonString, _writer.Write(featureCollection));
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenPopulatedIntArrayPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [1.0,2.0]
      },
      ""properties"": {
        ""foo"": [1, 2]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 93")]
        public void WhenEmptyArrayPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[]
      }
    }
  ]
}
";
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(geojsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 95, FeatureCollection having \"bbox\" property")]
        public void TestWhenFeatureCollectionHasBBox()
        {
            const string geoJsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[ 
  { 
      ""geometry"":{ 
          ""type"":""Point"", 
          ""coordinates"":[ -6.09, 4.99 ]
      }, 
      ""type"":""Feature"", 
      ""properties"":{
          ""prop1"":[ ""a"", ""b"" ] 
      }, 
      ""id"":1 
  } ], 
  ""bbox"":[ -8.59, 4.35, -2.49, 10.73 ] 
}";
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(geoJsonString);
            Assert.AreEqual(1, featureCollection.Count);
            var res = new GeoJsonWriter().Write(featureCollection);
            CompareJson(geoJsonString, res);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 120, Feature having null properties")]
        public void TestRoundtripSerializingDeserializingFeature()
        {
            var gf = new GeometryFactory();
            var f1 = new Feature(gf.CreatePoint(new Coordinate(-104.50348159865847, 40.891762392617345)), null);
            var f2 = SandD(f1);
            DoCheck(f1, f2);

            var t1 = new TestFeature { Geometry = f1.Geometry };
            var t2 = SandD(t1);
            DoCheck(t1, t2);

            string jsonWithoutProps = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[-104.50348159865847,40.891762392617345]}}";            
            DoCheck(jsonWithoutProps);
            string jsonWithValidProps = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[-104.50348159865847,40.891762392617345]},\"properties\":{\"aaa\":1,\"bbb\":2}}";
            DoCheck(jsonWithValidProps, false);
            string jsonWithNullProps = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[-104.50348159865847,40.891762392617345]},\"properties\":null}";
            DoCheck(jsonWithNullProps);
        }

        private static void DoCheck(string json, bool nullProps = true)
        {
            GeoJsonReader reader = new GeoJsonReader();
            Feature feature = reader.Read<Feature>(json);
            Assert.IsNotNull(feature);
            IGeometry geometry = feature.Geometry;
            Assert.IsNotNull(geometry);
            Assert.IsInstanceOf<IPoint>(geometry);
            Assert.AreEqual(nullProps, feature.Attributes == null);
            Assert.IsNull(feature.BoundingBox);
        }

        private static IFeature SandD(IFeature input)
        {
            var s = new GeoJsonSerializer {NullValueHandling = NullValueHandling.Ignore, FloatFormatHandling = FloatFormatHandling.DefaultValue, FloatParseHandling = FloatParseHandling.Double};
            var sb = new StringBuilder();
            s.Serialize(new JsonTextWriter(new StringWriter(sb)), input, typeof(IFeature));
            return (IFeature)s.Deserialize<IFeature>(new JsonTextReader(new StringReader(sb.ToString())));

        }

        private static void DoCheck(IFeature f1, IFeature f2)
        {
            if (f1 == null)
            {
                Assert.That(f2, Is.Null);
                return;
            }
            if (f1.Geometry != null) Assert.That(f2.Geometry, Is.Not.Null, "f2.Geometry is not null");
            if (f1.Geometry != null) Assert.That(f1.Geometry.EqualsExact(f2.Geometry), Is.True, "f1.Geometry.EqualsExact(f2.Geometry)");
            if (f1.BoundingBox != null)
            {
                if (f1.BoundingBox.Equals(f2.BoundingBox))
                    Assert.That(true, Is.True);
                else
                {
                    Assert.That(Math.Abs(f1.BoundingBox.MinX - f2.BoundingBox.MinX), Is.LessThanOrEqualTo(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                    Assert.That(Math.Abs(f1.BoundingBox.MaxX - f2.BoundingBox.MaxX), Is.LessThanOrEqualTo(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                    Assert.That(Math.Abs(f1.BoundingBox.MinY - f2.BoundingBox.MinY), Is.LessThanOrEqualTo(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                    Assert.That(Math.Abs(f1.BoundingBox.MaxY - f2.BoundingBox.MaxY), Is.LessThanOrEqualTo(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                }

            }
            if (f1.Attributes != null)
            {
                Assert.That(f2.Attributes, Is.Not.Null);
                var names1 = f1.Attributes.GetNames();
                var names2 = f2.Attributes.GetNames();
                Assert.That(names1.Length, Is.EqualTo(names2.Length));
                foreach (var name in names1)
                {
                    var v1 = f1.Attributes[name];
                    object v2 = null;
                    Assert.DoesNotThrow(() => v2 = f2.Attributes[name]);
                    if (v1 == null)
                    {
                        Assert.That(v2, Is.Null);
                    }
                    else
                    {
                        Assert.That(v1, Is.EqualTo(v2));
                    }
                }
            }
            else
                Assert.That(f2.Attributes, Is.Null);
        }


        private class TestFeature : IFeature
        {
            private readonly AttributeMapper _mapper;

            public TestFeature()
            {
                _mapper = new AttributeMapper(this);
                Item1 = true;
                Item2 = "The quick brown fox jumped over the fence.";
                Item3 = double.Epsilon;
                Item4 = new[] {1, 2, 3, 4};
            }

            public IAttributesTable Attributes
            {
                get { return _mapper; }
                set { throw new NotSupportedException(); }
            }

            public IGeometry Geometry { get; set; }

            public Envelope BoundingBox
            {
                get { return Geometry != null ? Geometry.EnvelopeInternal : null; }
                set { throw new NotSupportedException();}
            }

            public bool Item1 { get; set; }
            public string Item2 { get; set; }

            public double Item3 { get; set; }

            public int[] Item4 { get; set; }

            private class AttributeMapper : IAttributesTable
            {
                private readonly TestFeature _feature;
                public AttributeMapper(TestFeature feature)
                {
                    _feature = feature;
                }
                public void AddAttribute(string attributeName, object value)
                {
                    switch (attributeName.ToLowerInvariant())
                    {
                        case "item1":
                            _feature.Item1 = (bool) value;
                            break;
                        case "item2":
                            _feature.Item2 = (string)value;
                            break;
                        case "item3":
                            _feature.Item3 = (double)value;
                            break;
                        case "item4":
                            _feature.Item4 = (int[])value;
                            break;
                        default:
                            throw new ArgumentException("attributeName");
                    }
                }

                public void DeleteAttribute(string attributeName)
                {
                    throw new NotSupportedException();
                }

                public Type GetType(string attributeName)
                {
                    switch (attributeName.ToLowerInvariant())
                    {
                        case "item1":
                            return _feature.Item1.GetType();
                        case "item2":
                            return _feature.Item2.GetType();
                        case "item3":
                            return _feature.Item3.GetType();
                        case "item4":
                            return _feature.Item4.GetType();
                    }
                    throw new ArgumentException("attributeName");
                }

                public object this[string attributeName]
                {
                    get
                    {
                        switch (attributeName.ToLowerInvariant())
                        {
                            case "item1":
                                return _feature.Item1;
                            case "item2":
                                return _feature.Item2;
                            case "item3":
                                return _feature.Item3;
                            case "item4":
                                return _feature.Item4;
                            default:
                                throw new ArgumentException("attributeName");
                        }
                    }
                    set { AddAttribute(attributeName, value); }
                }

                public bool Exists(string attributeName)
                {
                    switch (attributeName.ToLowerInvariant())
                    {
                        case "item1":
                        case "item2":
                        case "item3":
                        case "item4":
                            return true;
                    }
                    return false;
                }

                public int Count { get { return 4; } }
                public string[] GetNames()
                {
                    return new[] {"Item1", "Item2", "Item3", "Item4"};
                }

                public object[] GetValues()
                {
                    return new object[]
                        {_feature.Item1, _feature.Item2, _feature.Item3, _feature.Item4};
                }
            }
        }

        
    }

   
}