using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue87Fixture
    {
        private string[] _wkts = new[]
        {
            "LINESTRING (564 564, 0 0)", "LINESTRING (596 2427, 0 3024)", "LINESTRING (522 605, 274 1245)",
            "LINESTRING (275 1475, 275 1716)", "LINESTRING (275 1716, 596 2427)", "LINESTRING (274 1245, 275 1475)",
            "LINESTRING (522 1242, 274 1245)", "LINESTRING (642 1716, 275 1716)", "LINESTRING (640 1476, 275 1475)",
            "LINESTRING (641 1272, 522 1242)", "LINESTRING (641 1272, 640 1476)", "LINESTRING (640 1476, 642 1716)",
            "LINESTRING (645 1718, 647 2423)", "LINESTRING (780 1244, 641 1272)", "LINESTRING (640 1476, 640 1476)",
            "LINESTRING (795 1476, 640 1476)", "LINESTRING (645 1718, 642 1716)", "LINESTRING (801 1713, 645 1718)",
            "LINESTRING (801 1713, 801 2428)", "LINESTRING (835 1249, 780 1244)", "LINESTRING (835 1249, 838 1354)",
            "LINESTRING (795 1476, 838 1354)", "LINESTRING (837 1617, 795 1476)", "LINESTRING (825 1706, 837 1617)",
            "LINESTRING (825 1706, 801 1713)", "LINESTRING (959 1757, 825 1706)", "LINESTRING (932 1189, 835 1249)",
            "LINESTRING (564 564, 522 605)", "LINESTRING (912 571, 932 1189)", "LINESTRING (959 1757, 959 2423)",
            "LINESTRING (1001 1354, 1044 1476)", "LINESTRING (1003 1238, 1001 1354)",
            "LINESTRING (1044 1476, 1003 1617)",
            "LINESTRING (1016 1713, 1003 1617)", "LINESTRING (1003 1238, 932 1189)", "LINESTRING (1016 1713, 959 1757)",
            "LINESTRING (780 564, 564 564)", "LINESTRING (1614 1233, 1003 1238)", "LINESTRING (1615 1723, 1016 1713)",
            "LINESTRING (1615 1469, 1044 1476)", "LINESTRING (647 2423, 596 2427)", "LINESTRING (801 2428, 647 2423)",
            "LINESTRING (912 571, 780 564)", "LINESTRING (959 2423, 801 2428)", "LINESTRING (1516 643, 912 571)",
            "LINESTRING (1494 2358, 959 2423)", "LINESTRING (1516 643, 2160 0)", "LINESTRING (1614 1233, 1516 643)",
            "LINESTRING (1615 1469, 1614 1233)", "LINESTRING (1615 1723, 1615 1469)",
            "LINESTRING (1494 2358, 1615 1723)",
            "LINESTRING (2160 3024, 1494 2358)"};

        [Test]
        public void TestIssue87()
        {
            var wktReader = new WKTReader();
            var lines = new List<IGeometry>(_wkts.Length);
            lines.AddRange(_wkts.Select(wkt => wktReader.Read(wkt))/*.Where(g => g.Length > 0 )*/);
            var polygonizer = new NetTopologySuite.Operation.Polygonize.Polygonizer();
            polygonizer.Add(lines);

            IGeometry res;
            Assert.DoesNotThrow( () =>  res = polygonizer.GetGeometry());
        }
    }
}