using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    [TestFixture]
    public class IsValidTest : GeometryTestCase
    {

        [Test]
        public void TestInvalidCoordinate()
        {
            var badCoord = new Coordinate(1.0, double.NaN);
            Coordinate[]  pts = { new Coordinate(0.0, 0.0), badCoord };
            var line = GeometryFactory.CreateLineString(pts);
            var isValidOp = new IsValidOp(line);
            bool valid = isValidOp.IsValid;
            var err = isValidOp.ValidationError;
            var errCoord = err.Coordinate;

            Assert.AreEqual(TopologyValidationErrors.InvalidCoordinate, err.ErrorType);
            Assert.IsTrue(double.IsNaN(errCoord.Y));
            Assert.AreEqual(false, valid);
        }

        [Test]
        public void TestZeroAreaPolygon()
        {
            var g = Read("POLYGON((0 0, 0 0, 0 0, 0 0, 0 0))");
            Assert.That(() => g.IsValid, Throws.Nothing);
        }

        [Test]
        public void TestValidSimplePolygon()
        {
            CheckValid(
                "POLYGON ((10 89, 90 89, 90 10, 10 10, 10 89))");
        }

        [Test]
        public void TestInvalidSimplePolygonRingSelfIntersection()
        {
            CheckInvalid(TopologyValidationErrors.SelfIntersection,
                "POLYGON ((10 90, 90 10, 90 90, 10 10, 10 90))");
        }

        [Test]
        public void TestSimplePolygonHole()
        {
            CheckValid(
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (60 20, 20 70, 90 90, 60 20))");
        }

        [Test]
        public void TestPolygonTouchingHoleAtVertex()
        {
            CheckValid(
                "POLYGON ((240 260, 40 260, 40 80, 240 80, 240 260), (140 180, 40 260, 140 240, 140 180))");
        }

        [Test]
        public void TestPolygonMultipleHolesTouchAtSamePoint()
        {
            CheckValid(
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (40 80, 60 80, 50 50, 40 80), (20 60, 20 40, 50 50, 20 60), (40 20, 60 20, 50 50, 40 20))");
        }

        [Test]
        public void TestPolygonHoleOutsideShellAllTouch()
        {
            CheckInvalid(TopologyValidationErrors.HoleOutsideShell,
                "POLYGON ((10 10, 30 10, 30 50, 70 50, 70 10, 90 10, 90 90, 10 90, 10 10), (50 50, 30 10, 70 10, 50 50))");
        }

        [Test]
        public void TestPolygonHoleOutsideShellDoubleTouch()
        {
            CheckInvalid(TopologyValidationErrors.HoleOutsideShell,
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (20 80, 80 80, 80 20, 20 20, 20 80), (90 70, 150 50, 90 20, 110 40, 90 70))");
        }

        [Test]
        public void TestPolygonNestedHolesAllTouch()
        {
            CheckInvalid(TopologyValidationErrors.NestedHoles,
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (20 80, 80 80, 80 20, 20 20, 20 80), (50 80, 80 50, 50 20, 20 50, 50 80))");
        }

        [Test]
        public void TestInvalidPolygonHoleProperIntersection()
        {
            CheckInvalid(TopologyValidationErrors.SelfIntersection,
                "POLYGON ((10 90, 50 50, 10 10, 10 90), (20 50, 60 70, 60 30, 20 50))");
        }

        [Test]
        public void TestInvalidPolygonDisconnectedInterior()
        {
            CheckInvalid(TopologyValidationErrors.DisconnectedInteriors,
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (20 80, 30 80, 20 20, 20 80), (80 30, 20 20, 80 20, 80 30), (80 80, 30 80, 80 30, 80 80))");
        }

        [Test]
        public void TestValidMultiPolygonTouchAtVertices()
        {
            CheckValid(
                "MULTIPOLYGON (((10 10, 10 90, 90 90, 90 10, 80 80, 50 20, 20 80, 10 10)), ((90 10, 10 10, 50 20, 90 10)))");
        }

        [Test]
        public void TestInvalidMultiPolygonHoleOverlapCrossing()
        {
            CheckInvalid(TopologyValidationErrors.SelfIntersection,
                "MULTIPOLYGON (((20 380, 420 380, 420 20, 20 20, 20 380), (220 340, 180 240, 60 200, 140 100, 340 60, 300 240, 220 340)), ((60 200, 340 60, 220 340, 60 200)))");
        }


        [Test]
        public void TestValidMultiPolygonTouchAtVerticesSegments()
        {
            CheckValid(
                "MULTIPOLYGON (((60 40, 90 10, 90 90, 10 90, 10 10, 40 40, 60 40)), ((50 40, 20 20, 80 20, 50 40)))");
        }

        [Test]
        public void TestInvalidMultiPolygonNestedAllTouchAtVertices()
        {
            CheckInvalid(TopologyValidationErrors.NestedShells,
                "MULTIPOLYGON (((10 10, 20 30, 10 90, 90 90, 80 30, 90 10, 50 20, 10 10)), ((80 30, 20 30, 50 20, 80 30)))");
        }

        [Test]
        public void TestValidMultiPolygonHoleTouchVertices()
        {
            CheckValid(
                "MULTIPOLYGON (((20 380, 420 380, 420 20, 20 20, 20 380), (220 340, 80 320, 60 200, 140 100, 340 60, 300 240, 220 340)), ((60 200, 340 60, 220 340, 60 200)))");
        }

        [Test]
        public void TestLineString()
        {
            var g = Read("LINESTRING(0 0, 0 0)");
            Assert.That(() => g.IsValid, Throws.Nothing);
        }

        [Test]
        public void TestLinearRingTriangle()
        {
            var g = Read(
                "LINEARRING (100 100, 150 200, 200 100, 100 100)");
            Assert.That(g.IsValid);
        }

        [Test]
        public void TestLinearRingSelfCrossing()
        {
            var g = Read(
                "LINEARRING (150 100, 300 300, 100 300, 350 100, 150 100)");
            Assert.That(!g.IsValid);
        }

        [Test]
        public void TestLinearRingSelfCrossing2()
        {
            var g = Read(
                "LINEARRING (0 0, 100 100, 100 0, 0 100, 0 0)");
            Assert.That(!g.IsValid);
        }

        //=============================================

        private void CheckValid(string wkt)
        {
            CheckValid(true, wkt);
        }

        private void CheckValid(bool isExpectedValid, string wkt)
        {
            var geom = Read(wkt);
            bool isValid = geom.IsValid;
            Assert.That(isValid, Is.EqualTo(isExpectedValid));
        }


        private void CheckInvalid(TopologyValidationErrors expectedErrType, string wkt)
        {
            var geom = Read(wkt);
            var validOp = new IsValidOp(geom);
            var err = validOp.ValidationError;
            Assert.That(err, Is.Not.Null);
            Assert.That(err.ErrorType, Is.EqualTo(expectedErrType));
        }

    }
}
