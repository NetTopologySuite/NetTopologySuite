using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Simplify;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue35Tests
    {
        private readonly IGeometryFactory factory = GeometryFactory.Default;

        private WKTReader reader;        

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            reader = new WKTReader(factory);
        }

        [Test] 
        public void TestIsValid()
        {
            var geom1 = reader.Read(
                    @"POLYGON((719522.38754834363 6176994.3322154824
24.194645633515528,719522.38754834374 6176994.3322154824
24.194645633477126,719522.93551468418 6176993.6599836433
23.741832765874967,719521.12955784285 6176985.9724558685
21.2264974621797,719521.72333959443 6176985.3237007372
20.768071704926587,719507.01810000045 6176973.3368000006
20.768071704926587,719502.88183502527 6176978.4322331771
24.194645633542077,719498.62799999863 6176983.6725
20.670674058571322,719512.84729999851 6176995.0215000017
20.768524674823045,719513.44049503584 6176994.3733859034
21.226497462846655,719521.884040508 6176994.949906704
23.77857112312677,719522.38754834363 6176994.3322154824 24.194645633515528))");
            Assert.IsNotNull(geom1);
            Assert.IsTrue(geom1.IsValid);

            var geom2 = reader.Read(
                    @"POLYGON((719496.72750000039 6177012.6337
21.226497462484563,719501.41240093729 6177017.249279663
23.760978631209998,719526.8258614993 6176989.4829953332
23.760978631060315,719521.72333959443 6176985.3237007372
21.226497462484563,719512.84729999851 6176995.0215000017
21.226497462560005,719496.72750000039 6177012.6337 21.226497462484563))");
            Assert.IsNotNull(geom2);
            Assert.IsTrue(geom2.IsValid);

            var expected = reader.Read(
                    @"POLYGON ((719522.3875483436 6176994.332215482 24.194645633515528,
719522.3875483437 6176994.332215482 24.194645633477126, 719526.8258614993
6176989.482995333 23.760978631060315, 719521.7233395944 6176985.323700737
21.226497462484563, 719507.0181000005 6176973.336800001 20.768071704926587,
719502.8818350253 6176978.432233177 24.194645633542077, 719498.6279999986
6176983.6725 20.670674058571322, 719512.8472999985 6176995.021500002
20.768524674823045, 719496.7275000004 6177012.6337 21.226497462484563,
719501.4124009373 6177017.249279663 23.760978631209998, 719521.8258356823
6176994.945932509, 719521.884040508 6176994.949906704 23.77857112312677,
719522.3875483436 6176994.332215482 24.194645633515528))");
            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.IsValid);

            var actual = geom1.Union(geom2);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.IsValid);

            Assert.IsTrue(expected.EqualsExact(actual));
        }

        [Test]
        public void TestIsValid2()
        {
            var geom1 = reader.Read(
                @"POLYGON ((34.6084247111331 31.2600368705728, 34.6032199980889
31.0998473691012, 34.4841253356165 31.1049514260643, 34.4725915455589
31.2524625304851, 34.6084247111331 31.2600368705728))");
            geom1.SRID = 4326;            
            Assert.IsNotNull(geom1);
            Assert.IsTrue(geom1.IsValid);

            var geom2 = reader.Read(
                @"POLYGON ((34.6501882399183 31.4064219592108, 34.5539826799553
31.4701726314754, 34.3859127258032 31.4180129905316, 34.3963446539919
31.3600578339274, 34.3650488694256 31.2464657269832, 34.6768476119563
31.2638522739645, 34.6965523652017 31.3704897621162, 34.6501882399183
31.4064219592108))");
            geom2.SRID = 4326;
            Assert.IsNotNull(geom2);
            Assert.IsTrue(geom2.IsValid);

            var result = geom1.Difference(geom2);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(result.SRID, 4326);
        }

        [Test(Description="Simplification always returns a geometry of the same type as the input geometry, and by default it attempts to ensure valid topology (by applying  a buffer(0) - which is a bit of a hack, I admit). This is why it returns an empty polygon.")]
        public void TestSimplifyBadPoly()
        {
            var geom = new Polygon(new LinearRing(new ICoordinate[] 
            {
                new Coordinate(1, 1), 
                new Coordinate(1, 1),
                new Coordinate(1, 1), 
                new Coordinate(1, 1),
                new Coordinate(1, 1)
            }));
            Debug.WriteLine("Bad polygon: " + geom);
            var simple = DouglasPeuckerSimplifier.Simplify(geom, 0.1);
            Debug.WriteLine("Simple bad polygon: " + simple);
            Assert.AreEqual(geom.GetType(), simple.GetType());
            Assert.AreNotEqual(geom, simple, "Simplify didn't do anything to this invalid polygon.");
            // This happens with JTS 1.9.0, 1.8.0 still returns GeometryCollection.Empty
            Assert.AreEqual(geom.GetType(), Polygon.Empty); 
        }
    }
}
