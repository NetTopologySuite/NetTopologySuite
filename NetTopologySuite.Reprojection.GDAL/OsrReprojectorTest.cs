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
                    Reprojector.Instance = new OsrReprojector(new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYZM));
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
            var srFrom = r.GetSpatialReference(4326);
            var srTo = r.GetSpatialReference(25832);

            var pt1 = srFrom.Factory.CreatePoint(new Coordinate(0, 0));
            Console.WriteLine(pt1);
            var pt2 = r.Reproject(pt1, srFrom, srTo);
            Console.WriteLine(pt2);
            var pt3 = r.Reproject(pt2, srTo, srFrom);
            Console.WriteLine(pt3);

        }

    }
}
