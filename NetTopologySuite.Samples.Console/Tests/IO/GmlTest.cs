using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.SnapRound;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.Tests.IO
{
    [TestFixture]
    public class GmlTest
    {
        [Test]
        public void TestPointWithBlankAfterCoordinates()
        {
            const string gml = "<gml:Point srsName=\"SDO:8265\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:coordinates decimal=\".\" cs=\",\" ts=\" \">-89.5589359049658,44.535657997424 </gml:coordinates></gml:Point>";
            var reader = new GMLReader();
            var geom = reader.Read(gml);

            Assert.IsNotNull(geom);
            Assert.IsInstanceOf<IPoint>(geom);
        }

        /*
    protected
     IPoint ReadPoint(XmlReader reader)
            {
    while
     (reader.Read())
                {
    switch
     (reader.NodeType)
                    {
    case
     XmlNodeType.Element:

    if
     (IsStartElement(reader,
    "coord"
    ))

    return
     Factory.CreatePoint(ReadCoordinate(reader));

    else

    if
     (IsStartElement(reader,
    "coordinates"
    ))
                            {
                                reader.Read();
    // Jump to values

      reader.Value = reader.Value.Trim();

    string
    [] coords = reader.Value.Split(
    ' '
    );

    if
     (coords.Length != 1)

    throw

    new

    ApplicationException
    (
    "Should never reach here!"
    );
                                ICoordinate c = ReadCoordinates(coords[0]);
                                Factory.CreatePoint(c);
                            }

    break
    ;

    default
    :

    break
    ;
                    }
                }

    throw

    new

    ArgumentException
    (
    "ShouldNeverReachHere!"
    );
            }
            */
            [Test]
        public void Test()
        {
            var rdr = new WKTReader();
            var wkt1 = rdr.Read("LINESTRING (31.26822419712768 13.10560781249994, 33.12591323377792 15.99391676083394, 35.37062066594295 16.870724851333293)");
            var wkt2 = rdr.Read("LINESTRING (34.233240446942204 16.426451284228865, 34.98360215508764 15.581301171439307, 34.26116755535489 14.059781270564386)");

            var touches = wkt1.Touches(wkt2);
            var snr = SnapRoundFunctions.SnapRound(wkt1, wkt2, 1000000000);
            touches = snr.GetGeometryN(0).Touches(snr.GetGeometryN(1));

            rdr = new WKTReader(new GeometryFactory(new PrecisionModel(1000000000)));
            wkt1 = rdr.Read("LINESTRING (31.26822419712768 13.10560781249994, 33.12591323377792 15.99391676083394, 35.37062066594295 16.870724851333293)");
            wkt2 = rdr.Read("LINESTRING (34.233240446942204 16.426451284228865, 34.98360215508764 15.581301171439307, 34.26116755535489 14.059781270564386)");

            touches = wkt1.Touches(wkt2);

            var t = SnapRoundFunctions.SnapRound(wkt1, wkt2, 1000000000);
        }
    }
}