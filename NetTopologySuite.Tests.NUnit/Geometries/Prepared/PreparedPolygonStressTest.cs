using System;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    /// <summary>
    /// Stress tests <see cref="PreparedPolygon"/> for correctness of <see cref="PreparedPolygon.Contains(IGeometry)"/> and
    /// <see cref="PreparedPolygon.Intersects(IGeometry)"/> operation.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PreparedPolygonPredicateStressTest
    {
        bool testFailed = false;
        [TestAttribute]
        [CategoryAttribute("Stress")]
        public void Test()
        {
            var tester = new PredicateStressTester();
            tester.Run(1000);
        }
        class PredicateStressTester : StressTestHarness
        {
            public override bool CheckResult(IGeometry target, IGeometry test)
            {
                if (!CheckIntersects(target, test)) return false;
                if (!CheckContains(target, test)) return false;
                return true;
            }
            private static bool CheckContains(IGeometry target, IGeometry test)
            {
                var expectedResult = target.Contains(test);
                var pgFact = new PreparedGeometryFactory();
                var prepGeom = pgFact.Create(target);
                var prepResult = prepGeom.Contains(test);
                if (prepResult != expectedResult)
                {
                    return false;
                }
                return true;
            }
            private static bool CheckIntersects(IGeometry target, IGeometry test)
            {
                var expectedResult = target.Intersects(test);
                var pgFact = new PreparedGeometryFactory();
                var prepGeom = pgFact.Create(target);
                var prepResult = prepGeom.Intersects(test);
                if (prepResult != expectedResult)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
