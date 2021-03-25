using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue508Fixture
    {
        [Test]
        public void TestGeometryCollectionEmptyAccess()
        {
            Geometry gcEmpty = null;
            Assert.That(() => gcEmpty = GeometryCollection.Empty, Throws.Nothing);
            Assert.That(gcEmpty, Is.Not.Null);
            Assert.That(gcEmpty.NumGeometries, Is.EqualTo(0));

            foreach (var geometry in (GeometryCollection)gcEmpty)
            {
                if (geometry == gcEmpty)
                    continue;

                TestContext.WriteLine(geometry);
            }
        }

        [Test]
        public void TestNUnitAssertEquals()
        {
            var rdr = new WKTReader();
            var g1 = rdr.Read("MULTIPOINT ((0 0))");
            var g2 = rdr.Read("MULTIPOINT ((0 0))");
            Assert.That(g1, Is.EqualTo(g2).Using<Geometry>(EqualityComparer<Geometry>.Default));
            //Assert.That(g1, Is.EqualTo(g2));
        }
    }
}
