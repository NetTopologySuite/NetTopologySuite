
using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Reprojection
{
    [TestFixture((string)null)]
    [TestFixture("esriwkt")]
    public class ProjNetReprojectorTest
    {
        //private readonly ConsoleTraceListener _ctr = new ConsoleTraceListener();

        public ProjNetReprojectorTest(string definitionKind)
        {
            Reprojector.Instance = new ProjNetReprojector(definitionKind);
        }

        [Test]
        public void Test()
        {
            var r = Reprojector.Instance;
            var srFrom = r.SpatialReferenceFactory.GetSpatialReference(4326);
            var srTo = r.SpatialReferenceFactory.GetSpatialReference(25832);

            var pt1 = srFrom.Factory.CreatePoint(new Coordinate(6, 0));
            Console.WriteLine(pt1);
            var pt2 = r.Reproject(pt1, srTo);
            Console.WriteLine(pt2);
            var pt3 = r.Reproject(pt2, srFrom);
            Console.WriteLine(pt3);

        }

        [Test]
        public void TestOrig()
        {
            var r = Reprojector.Instance;

            var srFrom = r.SpatialReferenceFactory.GetSpatialReference(4326);
            var srTo = r.SpatialReferenceFactory.GetSpatialReference(25832);

            var pt1 = srFrom.Factory.CreatePoint(new Coordinate(0, 0));
            Console.WriteLine(pt1);
            var pt2 = r.Reproject(pt1, srTo);
            Console.WriteLine(pt2);
            var pt3 = r.Reproject(pt2, srFrom);
            Console.WriteLine(pt3);

        }
    }
}
