#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance
{
    [TestFixture]
    public class IndexedFacetDistanceTest : AbstractDistanceTest
    {
        public IndexedFacetDistanceTest()
        {
            this.SkipTestsThatRelyOnCheckingPointInPolygon = true;
        }

        protected override Coordinate[] NearestPoints(Geometry g1, Geometry g2)
        {
            return IndexedFacetDistance.NearestPoints(g1, g2);
        }
    }
}
