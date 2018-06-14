using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Overlay
{
    [TestFixtureAttribute]
    public class FixedPrecisionSnappingTest
    {
        PrecisionModel pm;
        GeometryFactory fact;
        WKTReader rdr;

        public FixedPrecisionSnappingTest()
        {
            pm = new PrecisionModel(1.0);
            fact = new GeometryFactory(pm);
            rdr = new WKTReader(fact);
        }

        [TestAttribute]
        public void TestTriangles()
        {
            var a = rdr.Read("POLYGON ((545 317, 617 379, 581 321, 545 317))");
            var b = rdr.Read("POLYGON ((484 290, 558 359, 543 309, 484 290))");
            a.Intersection(b);
        }
    }
}
