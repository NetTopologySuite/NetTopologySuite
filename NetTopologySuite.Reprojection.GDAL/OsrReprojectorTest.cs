using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Reprojection
{
    [TestFixture("CoordinateArraySequence")]
    [TestFixture("PackedDoubleCoordinateSequence")]
    [TestFixture("DotSpatialAffineCoordinateSequence")]
    public class OsrReprojectorTest
    {
        public OsrReprojectorTest(string sequence)
        {
            switch (sequence)
            {
                case "PackedDoubleCoordinateSequence":
                    Reprojector.Instance = new OsrReprojector(PackedCoordinateSequenceFactory.DoubleFactory);
                    break;

                case "DotSpatialAffineCoordinateSequence":
                    Reprojector.Instance = new OsrReprojector(new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYZ));
                    break;

                default:
                    Reprojector.Instance = new OsrReprojector(CoordinateArraySequenceFactory.Instance);
                    break;

            }
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

            var reproj = r.ReprojectionFactory.Create(srFrom, srTo);
            Console.WriteLine(reproj.Apply(pt1));
        }

        [Test]
        public void TestOrig()
        {
            var r = Reprojector.Instance;
            var srFrom = r.SpatialReferenceFactory.GetSpatialReference(4326);
            var srTo = r.SpatialReferenceFactory.GetSpatialReference(31465);

            var pt1 = srFrom.Factory.CreatePoint(new Coordinate(0, 0));
            Console.WriteLine(pt1);
            var pt2 = r.Reproject(pt1, srTo);
            Console.WriteLine(pt2);
            var pt3 = r.Reproject(pt2, srFrom);
            Console.WriteLine(pt3);

            var reproj = r.ReprojectionFactory.Create(srFrom, srTo);
            Console.WriteLine(reproj.Apply(pt1));
        }

    }
}
