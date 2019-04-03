using GeoAPI.Geometries;
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
        protected override Coordinate[] NearestPoints(IGeometry g1, IGeometry g2)
        {
            return new DistanceOp(g1, g2).NearestPoints();
        }
    }
}
