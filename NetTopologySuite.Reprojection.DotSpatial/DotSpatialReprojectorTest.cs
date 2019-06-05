
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NUnit.Framework;

    namespace NetTopologySuite.Reprojection
    {
        [TestFixture("DotSpatialAffineCoordinateSequence")]
        public class OsrReprojectorTest
        {
            public OsrReprojectorTest(string sequence)
            {
                switch (sequence)
                {
                    case "DotSpatialAffineCoordinateSequence":
                        Reprojector.Instance = new DotSpatialReprojector();
                        break;

                    default:
                        throw new ArgumentException(nameof(sequence));

                }
            }


            [Test]
            public void Test()
            {
                var r = DotSpatialReprojector.Instance;
                var srFrom = r.GetSpatialReference(4326);
                var srTo = r.GetSpatialReference(31466);

                var pt1 = srFrom.Factory.CreatePoint(new Coordinate(0, 0));
                Console.WriteLine(pt1);
                var pt2 = r.Reproject(pt1, srFrom, srTo);
                Console.WriteLine(pt2);
                var pt3 = r.Reproject(pt2, srTo, srFrom);
                Console.WriteLine(pt3);

            }

        }
    }

