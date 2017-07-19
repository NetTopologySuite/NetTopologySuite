using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for GeoJsonSerializerTest and is intended
    ///    to contain all GeoJsonSerializerTest Unit Tests
    ///</summary>
    [TestFixture]
    public class GeoJsonSerializerTest
    {        
        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerFeatureCollectionTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            IFeature feature = new Feature(new Point(23, 56), attributes);
            FeatureCollection featureCollection = new FeatureCollection(new Collection<IFeature> {feature})
                                        {CRS = new NamedCRS("name1")};
            JsonSerializer serializer = new GeoJsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            serializer.Serialize(writer, featureCollection);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}],\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}}", sb.ToString());
        }

        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerFeatureTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            IFeature feature = new Feature(new Point(23, 56), attributes);
            JsonSerializer serializer = new GeoJsonSerializer { NullValueHandling = NullValueHandling.Ignore };
            serializer.Serialize(writer, feature);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}", sb.ToString());
        }

        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerGeometryTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            JsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, new Point(23, 56));
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Point\",\"coordinates\":[23.0,56.0]}", sb.ToString());
        }

        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerAttributesTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            JsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, attributes);
            writer.Flush();
            Assert.AreEqual("{\"test1\":\"value1\"}", sb.ToString());
        }

        /// <summary>
        /// test for a real life failure
        /// </summary>
        [Test]
        public void GeoJsonDeserializePolygonTest()
        {
            StringBuilder sb = new StringBuilder();
            JsonSerializer serializer = new GeoJsonSerializer();

            var s = "{\"type\": \"Polygon\",\"coordinates\": [[[-180,-67.8710140098964],[-180,87.270282879440586],[180,87.270282879440586],[180,-67.8710140098964],[-180,-67.8710140098964]]],\"rect\": {\"min\": [-180,-67.8710140098964],\"max\": [180,87.270282879440586]}}";
            var poly = serializer.Deserialize<Polygon>(JObject.Parse(s).CreateReader());


        }

        /// <summary>
        /// A regression test for issue #176.
        /// </summary>
        [Test]
        public void GeoJsonDeserializeIssue176()
        {
            var json = "{\"type\": \"FeatureCollection\", \"features\": [{\"geometry\": {\"type\": \"MultiLineString\", \"coordinates\": [[[4.312534690660468, 50.86397630523681], [4.3123741298132, 50.86410662321104], [4.3117962346022605, 50.86460569278546], [4.311641246312468, 50.86473604656781], [4.311486257173598, 50.86486639115116], [4.310866830672937, 50.865400370885304], [4.310702590905416, 50.8655413803989], [4.310537857205873, 50.865685895327395], [4.309343127703767, 50.86671136111375], [4.30910140290358, 50.86685194298859], [4.309235158440695, 50.866978977313416], [4.309367524689472, 50.86724077182908], [4.30944269026719, 50.86741972668383], [4.309492859063413, 50.86759499231628], [4.30965266455282, 50.86823003358657], [4.3096753421952725, 50.86830594288734], [4.309564413957949, 50.86832443289371], [4.307724672582708, 50.86845359674803], [4.307607252849442, 50.86845970307919], [4.307670889594874, 50.868563654006316], [4.308043889573612, 50.86915743734844], [4.308079516442333, 50.86921471841474], [4.308125392554221, 50.86928916558026], [4.308127419849172, 50.86929244776792], [4.308196264641599, 50.86940260377017], [4.3082971349109895, 50.86954288173691], [4.308404474053069, 50.86951185980503], [4.308563868940113, 50.869466238334375], [4.308754385494002, 50.86941238904831], [4.308918886082128, 50.86964386070719], [4.308869060487539, 50.8699967526562], [4.308860045098102, 50.870071180803436], [4.308849996146171, 50.87015407649411], [4.30873968077953, 50.87090988109565], [4.308676588924472, 50.87108979107392], [4.308601466557999, 50.871312152099264], [4.308506809761185, 50.87136585203428], [4.308174210899854, 50.87152471516386], [4.308032254714888, 50.87158142000959], [4.307898241516563, 50.87162571432238], [4.307732820842351, 50.8716556721419], [4.307571276399429, 50.87166514476547], [4.307465003982949, 50.87165794332038], [4.307420475634054, 50.87172706718126], [4.307281367355878, 50.87204111361056], [4.30718822640899, 50.87220548238517], [4.3070513749237564, 50.8724313701361], [4.306924769988122, 50.87263197573104], [4.3066569135743125, 50.87296458033302], [4.306565146590265, 50.87305741485658], [4.306401963241673, 50.87319246400709], [4.306254813113519, 50.873368777543085], [4.3060662255995075, 50.873378395656964], [4.3059099274196, 50.873401793418026], [4.305687218544723, 50.873461113480644], [4.3056247303728625, 50.873496121116055], [4.305525589634249, 50.8735717594719], [4.304864687010617, 50.87411034970604], [4.304555579148699, 50.87436761358954], [4.304415184192414, 50.87441594569204], [4.304283800852846, 50.87446118116545], [4.304228826881132, 50.87450614344795], [4.303708026161975, 50.87493258793568], [4.3036635867756665, 50.87503416225161], [4.303660886296727, 50.87511778956311], [4.303689887157587, 50.87515745790934], [4.3035335949720865, 50.87521269310873], [4.303021285747978, 50.875478551740436], [4.30294820410489, 50.87551630283081], [4.302879046585187, 50.87555202444099], [4.302351139512337, 50.87584113613631], [4.302125301833399, 50.87601262689481], [4.301999936895161, 50.876107817500156], [4.301847834360379, 50.87624130588339], [4.301650282434175, 50.87653287462897], [4.301567441683079, 50.87672785361711], [4.3014905199246805, 50.876896460879664], [4.301434549765169, 50.87710895743045], [4.301309050855777, 50.877345380257076]]]}, \"properties\": {\"name\": \"Itin\\u00e9raires Cyclables R\\u00e9gionaux - Gewestelijke Fietsroutes\", \"type\": \"route\", \"route\": \"bicycle\", \"operator\": \"Brussels Mobility\", \"ref\": \"11b\", \"colour\": \"#77AAD2\", \"network\": \"lcn\"}, \"type\": \"Feature\", \"id\": \"icr.115\", \"geometry_name\": \"wkb_geometry\"}]}";

            var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
            var textReader = new StringReader(json);

            var features = serializer.Deserialize<FeatureCollection>(new JsonTextReader(textReader));

            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
        }
    }
}