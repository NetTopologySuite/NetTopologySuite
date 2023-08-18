using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Relate
{
    public class ContainsTest : GeometryTestCase
    {
        /**
         * From GEOS #572
         * A case where B is contained in A, but
         * the JTS relate algorithm fails to compute this correctly
         * when using an FP intersection algorithm.
         * This case works when using CGAlgorithmsDD#intersection(Coordinate, Coordinate, Coordinate, Coordinate).
         *
         * The cause is that the long segment in A nodes the single-segment line in B.
         * The node location cannot be computed precisely.
         * The node then tests as not lying precisely on the original long segment in A.
         *
         * The solution is to change the relate algorithm so that it never computes
         * new intersection points, only ones which occur at existing vertices.
         * (The topology of the implicit intersections can still be computed
         * to contribute to the intersection matrix result).
         * This will require a complete reworking of the relate algorithm.
         */
        [Test]
        public void TestContainsIncorrect()
        {
            string a = "LINESTRING (1 0, 0 2, 0 0, 2 2)";
            string b = "LINESTRING (0 0, 2 2)";
            CheckContains(a, b);
        }

        /**
         * From GEOS #933.
         * A case where B is contained in A, but 
         * the JTS relate algorithm fails to compute this correctly.
         * when using an FP intersection algorithm.
         * This case works when using CGAlgorithmsDD#intersection(Coordinate, Coordinate, Coordinate, Coordinate).
         */
        [Test]
        public void TestContainsGEOS933()
        {
            string a = "MULTILINESTRING ((0 0, 1 1), (0.5 0.5, 1 0.1, -1 0.1))";
            string b = "LINESTRING (0 0, 1 1)";
            CheckContains(a, b);
        }

        private void CheckContains(string wktA, string wktB)
        {
            var geomA = Read(wktA);
            var geomB = Read(wktB);
            bool actual = geomA.Contains(geomB);
            Assert.That(actual, Is.True);
        }

        private void CheckContainsError(string wktA, string wktB)
        {
            var geomA = Read(wktA);
            var geomB = Read(wktB);
            bool actual = geomA.Contains(geomB);
            Assert.That(actual, Is.False);
        }
    }
}
