using System.Collections;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue336
    {
        [TestCaseSource(nameof(CsFactories))]
        public void Test(CoordinateSequenceFactory factory)
        {
            const string InputWkt = "LINESTRING (1 1, 2 2, 1 2, 2 1)";
            var i = new NtsGeometryServices(factory);
            var wktReader = new WKTReader(i);
            var g = wktReader.Read(InputWkt);
            g.Union();
        }

        [TestCaseSource(nameof(CsFactories))]
        public void TestJts(CoordinateSequenceFactory factory)
        {
            var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(factory);
            var geomA = gf.CreatePolygon(new [] {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0),
                    new Coordinate(10, 10),
                    new Coordinate(0, 10),
                    new Coordinate(0, 0)
                });
            var geomB = gf.CreatePolygon(new[] {
                new Coordinate(5, 5),
                new Coordinate(15, 5),
                new Coordinate(15, 15),
                new Coordinate(5, 15),
                new Coordinate(5, 5)
            });

            Assert.DoesNotThrow(() => geomA.Intersection(geomB));
            Assert.DoesNotThrow(() => geomB.Intersection(geomA));
        }

        public static IEnumerable CsFactories
        {
            get
            {
                yield return PackedCoordinateSequenceFactory.DoubleFactory;
                yield return PackedCoordinateSequenceFactory.FloatFactory;
            }
        }
    }
}
