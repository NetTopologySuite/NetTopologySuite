using GeoAPI.Geometries;
using NetTopologySuite.IO.GML2;
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
    }
}