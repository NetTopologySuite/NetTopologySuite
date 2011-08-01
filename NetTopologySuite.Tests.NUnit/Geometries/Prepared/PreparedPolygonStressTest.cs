using System;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    /// <summary>
    /// Stress tests <see cref="PreparedPolygon"> for contains operation.
    /// </summary>
    /// <author>Owner</author>
    public class PreparedPolygonPredicateStressTest
    {
        bool testFailed = false;

        [Test]
        public void Test()
        {
            PredicateStressTester tester = new PredicateStressTester();
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
                bool expectedResult = target.Contains(test);

                PreparedGeometryFactory pgFact = new PreparedGeometryFactory();
                IPreparedGeometry prepGeom = pgFact.Create(target);

                bool prepResult = prepGeom.Contains(test);

                if (prepResult != expectedResult)
                {
                    return false;
                }
                return true;
            }

            private static Boolean CheckIntersects(IGeometry target, IGeometry test)
            {
                bool expectedResult = target.Intersects(test);

                PreparedGeometryFactory pgFact = new PreparedGeometryFactory();
                IPreparedGeometry prepGeom = pgFact.Create(target);

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