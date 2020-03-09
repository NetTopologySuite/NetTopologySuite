#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    /// <summary>
    /// Stress tests <see cref="PreparedPolygon"/> for correctness of <see cref="PreparedPolygon.Contains(Geometry)"/> and
    /// <see cref="PreparedPolygon.Intersects(Geometry)"/> operation.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PreparedPolygonPredicateStressTest
    {
        bool testFailed = false;

        [Test]
        [Category("Stress")]
        public void Test()
        {
            var tester = new PredicateStressTester();
            tester.Run(1000);
        }

        class PredicateStressTester : StressTestHarness
        {
            public override bool CheckResult(Geometry target, Geometry test)
            {
                if (!CheckIntersects(target, test)) return false;
                if (!CheckContains(target, test)) return false;
                return true;
            }

            private static bool CheckContains(Geometry target, Geometry test)
            {
                bool expectedResult = target.Contains(test);

                var pgFact = new PreparedGeometryFactory();
                var prepGeom = pgFact.Create(target);

                bool prepResult = prepGeom.Contains(test);

                if (prepResult != expectedResult)
                {
                    return false;
                }
                return true;
            }

            private static bool CheckIntersects(Geometry target, Geometry test)
            {
                bool expectedResult = target.Intersects(test);

                var pgFact = new PreparedGeometryFactory();
                var prepGeom = pgFact.Create(target);

                bool prepResult = prepGeom.Intersects(test);

                if (prepResult != expectedResult)
                {
                    return false;
                }
                return true;
            }

        }
    }
}