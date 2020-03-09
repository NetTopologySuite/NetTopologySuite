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

        protected override double Distance(Geometry g1, Geometry g2)
        {
            return IndexedFacetDistance.Distance(g1, g2);
        }

        protected override bool IsWithinDistance(Geometry g1, Geometry g2, double distance)
        {
            return IndexedFacetDistance.IsWithinDistance(g1, g2, distance);
        }

        protected override Coordinate[] NearestPoints(Geometry g1, Geometry g2)
        {
            return IndexedFacetDistance.NearestPoints(g1, g2);
        }
    }
}
