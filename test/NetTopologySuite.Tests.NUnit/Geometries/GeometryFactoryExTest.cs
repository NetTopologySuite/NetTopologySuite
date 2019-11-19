using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryFactoryExTest : GeometryFactoryTest
    {
        [Test]
        public void TestShellRingOrientationSetting()
        {

            // check default
            var gf = new GeometryFactoryEx();
            Assert.AreEqual(LinearRingOrientation.CounterClockwise, gf.OrientationOfExteriorRing);

            // Check for valid ring orientation values
            gf = new GeometryFactoryEx();
            TestShellRingOrientationSetter(gf, LinearRingOrientation.DontCare, 0);
            gf = new GeometryFactoryEx();
            TestShellRingOrientationSetter(gf, LinearRingOrientation.Clockwise, 0);
            gf = new GeometryFactoryEx();
            TestShellRingOrientationSetter(gf, LinearRingOrientation.RightHandRule, 0);
            gf = new GeometryFactoryEx();
            TestShellRingOrientationSetter(gf, LinearRingOrientation.CounterClockwise, 0);
            gf = new GeometryFactoryEx();
            TestShellRingOrientationSetter(gf, LinearRingOrientation.LeftHandRule, 0);

            // check for invalid arguments
            gf = new GeometryFactoryEx();
            TestShellRingOrientationSetter(gf, (LinearRingOrientation)(-2), 1);
            TestShellRingOrientationSetter(gf, (LinearRingOrientation)2, 1);

            // check prevention of setting after first usage
            var ro = gf.OrientationOfExteriorRing;
            // don't fail if value does not change
            TestShellRingOrientationSetter(gf, ro, 0);
            TestShellRingOrientationSetter(gf, LinearRingOrientation.RightHandRule, 2);
            TestShellRingOrientationSetter(gf, LinearRingOrientation.DontCare, 2);
        }

        [Test]
        public void TestShellRingOrientationEnforcement()
        {
            TestShellRingOrientationEnforcement(LinearRingOrientation.DontCare);
            TestShellRingOrientationEnforcement(LinearRingOrientation.CounterClockwise);
            TestShellRingOrientationEnforcement(LinearRingOrientation.Clockwise);
        }

        [Test]
        public void TestEnvelopeToGeometry()
        {
            var env = new Envelope(-10, 10, -8, 8);

            var gf = new GeometryFactoryEx();
            Assert.IsTrue(((Polygon)gf.ToGeometry(env)).Shell.IsCCW );

            gf = new GeometryFactoryEx {OrientationOfExteriorRing = LinearRingOrientation.DontCare };
            Assert.IsFalse(((Polygon)gf.ToGeometry(env)).Shell.IsCCW);

            gf = new GeometryFactoryEx { OrientationOfExteriorRing = LinearRingOrientation.Clockwise };
            Assert.IsFalse(((Polygon)gf.ToGeometry(env)).Shell.IsCCW);

            gf = new GeometryFactoryEx { OrientationOfExteriorRing = LinearRingOrientation.CounterClockwise };
            Assert.IsTrue(((Polygon)gf.ToGeometry(env)).Shell.IsCCW);
        }

        private static void TestShellRingOrientationEnforcement(LinearRingOrientation ringOrientation)
        {
            var gf = new GeometryFactoryEx();
            gf.OrientationOfExteriorRing = ringOrientation;
            var cf = gf.CoordinateSequenceFactory;

            var origShellSequence = CreateRing(cf, 0, ringOrientation);
            var origHolesSequences = new[]
            {
                CreateRing(cf, 1, ringOrientation),
                CreateRing(cf, 2, ringOrientation)
            };

            var shell = gf.CreateLinearRing(origShellSequence.Copy());
            var holes = new LinearRing[]
            {
                gf.CreateLinearRing(origHolesSequences[0].Copy()),
                gf.CreateLinearRing(origHolesSequences[1].Copy())
            };

            var polygon = gf.CreatePolygon(shell, holes);
            switch (ringOrientation)
            {
                case LinearRingOrientation.CW:
                    if (polygon.Shell.IsCCW)
                        Assert.Fail($"Shell ring orientation requested {LinearRingOrientation.CW}, was CCW");
                    if (!polygon.Holes[0].IsCCW)
                        Assert.Fail($"Hole[0] ring orientation requested {LinearRingOrientation.CCW}, was CW"); ;
                    if (!polygon.Holes[1].IsCCW)
                        Assert.Fail($"Hole[1] ring orientation requested {LinearRingOrientation.CCW}, was CW"); ;
                    break;
                case LinearRingOrientation.CCW:
                    if (!polygon.Shell.IsCCW)
                        Assert.Fail($"Shell ring orientation requested {LinearRingOrientation.CCW}, was CW");
                    if (polygon.Holes[0].IsCCW)
                        Assert.Fail($"Hole[0] ring orientation requested {LinearRingOrientation.CW}, was CCW"); ;
                    if (polygon.Holes[1].IsCCW)
                        Assert.Fail($"Hole[1] ring orientation requested {LinearRingOrientation.CW}, was CCW"); ;
                    break;
                case LinearRingOrientation.DontCare:
                    if (polygon.Shell.IsCCW != Orientation.IsCCW(origShellSequence))
                        Assert.Fail($"Shell ring orientation flipped");
                    if (polygon.Holes[0].IsCCW != Orientation.IsCCW(origHolesSequences[0]))
                        Assert.Fail($"Hole[0] ring orientation flipped");
                    if (polygon.Holes[1].IsCCW != Orientation.IsCCW(origHolesSequences[1]))
                        Assert.Fail($"Hole[1] ring orientation flipped");
                    break;
            }

            // Set up a polygon with hole using buffer
            var pt = new Coordinate(0, 0);
            var c = gf.CreatePoint(pt);
            var b = c.Buffer(10d);
            var s = c.Buffer(6d);
            var test = (Polygon)b.Difference(s);

            if (ringOrientation == LinearRingOrientation.CW)
            {
                if (test.Shell.IsCCW)
                    Assert.Fail($"Buffer should return shell ring with CW orientation");
                if (!test.Holes[0].IsCCW)
                    Assert.Fail($"Buffer should return hole ring with CCW orientation");
            }
            else if (ringOrientation == LinearRingOrientation.CCW)
            {
                if (!test.Shell.IsCCW)
                    Assert.Fail($"Buffer should return shell ring with CCW orientation");
                if (test.Holes[0].IsCCW)
                    Assert.Fail($"Buffer should return hole ring with CW orientation");
            }
        }

        private static Coordinate[] CreateCcwRing(int number)
        {
            if (number == 0)
                return new Coordinate[]{
                    new Coordinate(0, 0), new Coordinate(10, 0),
                    new Coordinate(10, 10), new Coordinate(0, 10),
                    new Coordinate(0, 0)};
            if (number == 1)
                return new Coordinate[]{
                    new Coordinate(2, 1), new Coordinate(9, 1),
                    new Coordinate(9, 8), new Coordinate(2, 1)
                };

            return new Coordinate[]{
                new Coordinate(1, 2), new Coordinate(8, 9),
                new Coordinate(1, 9), new Coordinate(1, 2)
            };
        }

        private static bool reverseSequence = false;

        private static CoordinateSequence CreateRing(CoordinateSequenceFactory factory, int ring, LinearRingOrientation orientation)
        {
            var coordinates = CreateCcwRing(ring);
            if (orientation == LinearRingOrientation.Clockwise)
                CoordinateArrays.Reverse(coordinates);
            if (orientation == LinearRingOrientation.DontCare)
            {
                if (reverseSequence) CoordinateArrays.Reverse(coordinates);
                reverseSequence = !reverseSequence;
            }

            return factory.Create(coordinates);
        }

        private static void TestShellRingOrientationSetter(GeometryFactoryEx gf, LinearRingOrientation ringOrientation, int failKind)
        {
            try
            {
                gf.OrientationOfExteriorRing = ringOrientation;
                if (failKind != 0)
                    Assert.Fail("Setting exterior ring orientation should have failed!");
                Assert.AreEqual(ringOrientation, gf.OrientationOfExteriorRing);
            }
            catch (ArgumentOutOfRangeException iae)
            {
                if (failKind != 1)
                    Assert.Fail("Wrong argument");
            }
            catch (InvalidOperationException isa)
            {
                if (failKind != 2)
                    Assert.Fail("Setting exterior ring orientation twice!");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
