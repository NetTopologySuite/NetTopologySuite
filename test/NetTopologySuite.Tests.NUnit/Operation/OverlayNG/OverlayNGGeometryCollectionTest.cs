using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    /// <summary>
    /// Tests supported OverlayNG semantics for GeometryCollection inputs.
    /// <para/>
    /// <b>Note:</b> currently only "simple" GCs are supported.
    /// Simple GCs are ones which can be flattened to a valid Multi-geometry.
    /// </summary>
    public class OverlayNGGeometryCollectionTest : OverlayNGTestCase
    {
        [Test]
        public void TestSimpleA_mP()
        {
            const string a = "POLYGON ((0 0, 0 1, 1 1, 0 0))";
            const string b = "GEOMETRYCOLLECTION ( MULTIPOINT ((0 0), (99 99)) )";
            CheckIntersection(a, b,
                "POINT (0 0)");
            CheckUnion(a, b,
                "GEOMETRYCOLLECTION (POINT (99 99), POLYGON ((0 0, 0 1, 1 1, 0 0)))");
        }

        [Test]
        public void TestSimpleP_mP()
        {
            const string a = "POINT(0 0)";
            const string b = "GEOMETRYCOLLECTION ( MULTIPOINT ((0 0), (99 99)) )";
            CheckIntersection(a, b,
                "POINT (0 0)");
            CheckUnion(a, b,
                "MULTIPOINT ((0 0), (99 99))");
        }

        [Test]
        public void TestSimpleP_mL()
        {
            const string a = "POINT(5 5)";
            const string b = "GEOMETRYCOLLECTION ( MULTILINESTRING ((1 9, 9 1), (1 1, 9 9)) )";
            CheckIntersection(a, b,
                "POINT (5 5)");
            CheckUnion(a, b,
                "MULTILINESTRING ((1 1, 5 5), (1 9, 5 5), (5 5, 9 1), (5 5, 9 9))");
        }

        [Test]
        public void TestSimpleP_mA()
        {
            const string a = "POINT(5 5)";
            const string b = "GEOMETRYCOLLECTION ( MULTIPOLYGON (((1 1, 1 5, 5 5, 5 1, 1 1)), ((9 9, 9 5, 5 5, 5 9, 9 9))) )";
            CheckIntersection(a, b,
                "POINT (5 5)");
            CheckUnion(a, b,
                "MULTIPOLYGON (((1 1, 1 5, 5 5, 5 1, 1 1)), ((9 9, 9 5, 5 5, 5 9, 9 9)))");
        }

        [Test]
        public void TestSimpleP_AA()
        {
            const string a = "POINT(5 5)";
            const string b = "GEOMETRYCOLLECTION ( POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1)), POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9)) )";
            CheckIntersection(a, b,
                "POINT (5 5)");
            CheckUnion(a, b,
                "MULTIPOLYGON (((1 1, 1 5, 5 5, 5 1, 1 1)), ((9 9, 9 5, 5 5, 5 9, 9 9)))");
        }

        [Test]
        public void TestSimpleL_AA()
        {
            const string a = "LINESTRING (0 0, 10 10)";
            const string b = "GEOMETRYCOLLECTION ( POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1)), POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9)) )";
            CheckIntersection(a, b,
                "MULTILINESTRING ((1 1, 5 5), (5 5, 9 9))");
            CheckUnion(a, b,
                "GEOMETRYCOLLECTION (LINESTRING (0 0, 1 1), LINESTRING (9 9, 10 10), POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1)), POLYGON ((5 5, 5 9, 9 9, 9 5, 5 5)))");
        }

        [Test]
        public void TestSimpleA_AA()
        {
            const string a = "POLYGON ((2 8, 8 8, 8 2, 2 2, 2 8))";
            const string b = "GEOMETRYCOLLECTION ( POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1)), POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9)) )";
            CheckIntersection(a, b,
                "MULTIPOLYGON (((2 2, 2 5, 5 5, 5 2, 2 2)), ((5 5, 5 8, 8 8, 8 5, 5 5)))");
            CheckUnion(a, b,
                "POLYGON ((1 1, 1 5, 2 5, 2 8, 5 8, 5 9, 9 9, 9 5, 8 5, 8 2, 5 2, 5 1, 1 1))");
        }
    }
}
