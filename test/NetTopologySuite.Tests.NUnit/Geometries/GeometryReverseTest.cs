using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryReverseTest : GeometryTestCase
    {
        [Test]
        public void TestReverse()
        {
            foreach (string wkt in GeometryTestData.WKT_ALL)
                CheckReverse(Read(wkt));
        }

        private void CheckReverse(Geometry g)
        {
            int SRID = 123;
            g.SRID = SRID;

            //User data left out for now
            //Object DATA = new Integer(999);
            //g.setUserData(DATA);

            var reverse = g.Reverse();

            Assert.That(g.GeometryType, Is.EqualTo(reverse.GeometryType), $"{g.GeometryType}: Geometry.GeometryType values are not the same");
            Assert.That(g.SRID, Is.EqualTo(reverse.SRID), $"{g.GeometryType}: Geometry.SRID values are not the same");

            Assert.That(CheckSequences(g, reverse), $"{g.GeometryType}: Sequences are not opposite");
        }

        private bool CheckSequences(Geometry g1, Geometry g2)
        {
            int numGeometries = g1.NumGeometries;
            if (numGeometries != g2.NumGeometries)
                return false;
            for (int i = 0; i < numGeometries; i++)
            {
                var gt1 = g1.GetGeometryN(i);
                int j = i;
                var gt2 = g2.GetGeometryN(j);

                if (gt1.GeometryType != gt2.GeometryType)
                    return false;

                if (gt1 is Point pt1) {
                    if (!CheckSequences(pt1.CoordinateSequence, ((Point)gt2).CoordinateSequence))
                        return false;
                }
                else if (gt1 is LineString ls1) {
                    if (!CheckSequences(ls1.CoordinateSequence, ((LineString)gt2).CoordinateSequence))
                        return false;
                }
                else if (gt1 is Polygon pl1) {
                    var pl2 = (Polygon)gt2;
                    if (!CheckSequences(pl1.ExteriorRing.CoordinateSequence,
                                        pl2.ExteriorRing.CoordinateSequence))
                    return false;
                    for (int k = 0; k < pl1.NumInteriorRings; k++)
                    {
                        if (!CheckSequences(pl1.GetInteriorRingN(k).CoordinateSequence,
                                            pl2.GetInteriorRingN(k).CoordinateSequence))
                        return false;
                    }
                }
                else if (gt1 is GeometryCollection) {
                    CheckSequences(gt1, gt2);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckSequences(CoordinateSequence c1, CoordinateSequence c2)
        {

            if (c1.Count != c2.Count)
                return false;
            if (c1.Dimension != c2.Dimension)
                return false;
            if (c1.Measures != c2.Measures)
                return false;

            for (int i = 0; i < c1.Count; i++)
            {
                int j = c1.Count - i - 1;
                for (int k = 0; k < c1.Dimension; k++)
                    if (c1.GetOrdinate(i, k) != c2.GetOrdinate(j, k))
                        if (!(double.IsNaN(c1.GetOrdinate(i, k)) && double.IsNaN(c2.GetOrdinate(j, k))))
                            return false;
            }
            return true;
        }
    }
}
