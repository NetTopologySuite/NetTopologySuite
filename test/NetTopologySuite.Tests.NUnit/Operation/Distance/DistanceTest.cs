using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance
{
    // DEVIATION: at the time of this commit, JTS' version is a copy-paste of AbstractDistanceTest
    // (well, technically, AbstractDistanceTest is a copy-paste of JTS' DistanceTest), which looks
    // like an oversight more than anything else.
    [TestFixture]
    public class DistanceTest : AbstractDistanceTest
    {
        protected override double Distance(Geometry g1, Geometry g2)
        {
            return g1.Distance(g2);
        }

        protected override bool IsWithinDistance(Geometry g1, Geometry g2, double distance)
        {
            return Distance(g1, g2) <= distance;
        }

        protected override Coordinate[] NearestPoints(Geometry g1, Geometry g2)
        {
            return new DistanceOp(g1, g2).NearestPoints();
        }

    }
}
