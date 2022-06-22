using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture, Explicit]
    internal class Discussion615Fixture
    {
        private const string Yellow = @"MULTILINESTRING ((581.48338 22.03762 0, 710.47333 -54.56984 0, 787.49341 43.71192 0, 869.74788 -69.89133 0),
(621.42191 -126.44761 0, 697.84376 -171.36026 0, 704.37263 -127.16545 0),
(545.5879 -255.706 0, 581.8564 -211.918 0, 663.72983 -232.01741 0, 746.68054 -250.32223 0, 701.4347 -317.08099 0),
(488.27357 -82.01335 0, 462.6326 -122.15795 0, 423.55226 -118.09964 0, 399.9518 -94.2571 0, 389.54729 -64.07345 0),
(331.64959 -33.70801 0, 306.00861 -73.85261 0, 266.92828 -69.7943 0, 243.32781 -45.95176 0, 232.92331 -15.76811 0),
(268.85759 66.27012 4, 291.18921 11.22935 4, 341.43536 2.85909 4, 390.66643 2.85909 4, 422.13371 30.25265 4, 426.95531 58.66079 4),
(81.7704 46.11608 4, 104.10203 -8.92469 4, 154.34817 -17.29495 4, 203.57925 -17.29495 4, 235.04653 10.09862 4, 239.86813 38.50676 4),
(275.29323 128.6603 4, 249.65225 88.51571 4, 210.57191 92.57401 4, 186.97145 116.41656 4, 176.56695 146.6002 4),
(143.06951 132.39613 4, 165.40113 77.35537 4, 215.64728 68.98511 4, 264.87836 68.98511 4, 296.34564 96.37867 4, 301.16724 124.78681 4))";
        private const string Purple = @"MULTILINESTRING ((608.02914 -59.80156 0, 691.77913 47.44887 0, 787.49341 43.71192 0),
(513.98763 -196.48452 0, 581.8564 -211.918 0, 616.68852 -164.89973 0, 663.72983 -232.01741 0, 697.84376 -171.36026 0, 746.68054 -250.32223 0, 764.99434 -180.33321 0, 764.99434 -117.16363 0, 687.07094 -107.83176 0),
(331.57132 -192.21874 0, 414.51258 -157.80217 0, 462.6326 -122.15795 0), (223.06735 -108.26917 -4, 286.46845 -71.82346 -4, 322.76979 -95.83436 -4),
(434.41291 -8.06246 0, 407.42842 33.97069 0), (105.72662 -52.92049 0, 126.8438 -12.71313 0, 150.60545 -38.19321 0, 186.80724 -17.29495 0, 205.42906 -40.48568 0),
(256.93377 140.20334 0, 289.52543 119.71218 0), (319.83917 134.24864 0, 301.16724 124.78681 0))";


        [Test]
        public void Test()
        {
            var rdr = new IO.WKTReader();
            var ys = (NetTopologySuite.Geometries.MultiLineString)rdr.Read(Yellow);
            var ps = (NetTopologySuite.Geometries.MultiLineString)rdr.Read(Purple);

            foreach (var (y, p) in CrossingLineStrings(ys, ps))
                TestContext.WriteLine($"{y.Intersection(p)}:\n\t- {y}\n\t- {p}");
        }

        public IEnumerable<(NetTopologySuite.Geometries.LineString y, NetTopologySuite.Geometries.LineString p)>
            CrossingLineStrings(NetTopologySuite.Geometries.MultiLineString y, NetTopologySuite.Geometries.MultiLineString p)
        {
            var ys = ToArray(y);
            var ps = ToArray(p);

            return CrossingLineStrings(ys, ps);

            NetTopologySuite.Geometries.LineString[] ToArray(NetTopologySuite.Geometries.MultiLineString mls)
            {
                var res = new NetTopologySuite.Geometries.LineString[mls.NumGeometries];
                for (int i = 0; i < mls.NumGeometries; i++)
                    res[i] = (NetTopologySuite.Geometries.LineString)mls.GetGeometryN(i);
                return res;
            }


        }

        public IEnumerable<(NetTopologySuite.Geometries.LineString y, NetTopologySuite.Geometries.LineString p)>
            CrossingLineStrings(IReadOnlyList<NetTopologySuite.Geometries.LineString> ys, IReadOnlyList<NetTopologySuite.Geometries.LineString> ps)
        {
            for (int i = 0; i < ys.Count; i++)
            {
                var y = ys[i];
                if (y == null || y.IsEmpty) continue;

                var yp = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(y);

                for (int j = 0; j < ps.Count; j++)
                {
                    var p = ps[j];
                    if (p == null || p.IsEmpty) continue;

                    if (yp.Crosses(p) && !yp.Touches(p))
                        yield return (y, p);
                }
            }
        }
    }
}
