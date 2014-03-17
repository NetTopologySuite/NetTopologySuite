using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;
using NUnit.Framework;

/*
 * Tests Noding checking during overlay.
 * Intended to show that noding check failures due to robustness
 * problems do not occur very often (i.e. that the heuristic is
 * not triggering so often that a large performance penalty would be incurred.)
 *
 * The class generates test geometries for input to overlay which contain almost parallel lines
 * - this should cause noding failures relatively frequently.
 *
 * Can also be used to check that the cross-snapping heuristic fix for robustness
 * failures works well.  If snapping ever fails to fix a case,
 * an exception is thrown.  It is expected (and has been observed)
 * that cross-snapping works extremely well on this dataset.
 */

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Overlay
{
    [TestFixtureAttribute]
    [CategoryAttribute("Stress")]
    public class OverlayNodingStressTest
    {
        private static int ITER_LIMIT = 10000;
        private static int BATCH_SIZE = 20;
        private static double MAX_DISPLACEMENT = 60;

        private IGeometry _baseAccum;
        private int _geomCount;

        private readonly Random _rnd = new Random((int)(Math.PI * 10e5));
        private int _failureCount;

        private double GetRandomDouble()
        {
            var r = _rnd.NextDouble();
            return r;
        }

        [TestAttribute, Explicit("takes ages to complete")]
        public void TestNoding()
        {
            var iterLimit = ITER_LIMIT;
            for (int i = 0; i < iterLimit; i++)
            {
                Console.WriteLine("Iter: " + i
                        + "  Noding failure count = " + _failureCount);
                double ang1 = GetRandomDouble() * Math.PI;
                double ang2 = GetRandomDouble() * Math.PI;
                //			Geometry[] geom = generateGeometryStar(ang1, ang2);
                IGeometry[] geom = GenerateGeometryAccum(ang1, ang2);
                CheckIntersection(geom[0], geom[1]);
            }
            Console.WriteLine(
                    "Test count = " + iterLimit
                    + "  Noding failure count = " + _failureCount
                );
        }

        public IGeometry[] GenerateGeometryStar(double angle1, double angle2)
        {
            var rrFact = new RotatedRectangleFactory();
            var rr1 = rrFact.CreateRectangle(100, 20, angle1);
            var rr2 = rrFact.CreateRectangle(100, 20, angle2);

            // this line can be used to test for the presence of noding failures for
            // non-tricky cases
            // Geometry star = rr2;
            var star = rr1.Union(rr2);
            return new[] { star, rr1 };
        }

        public IGeometry[] GenerateGeometryAccum(double angle1, double angle2)
        {
            var rrFact = new RotatedRectangleFactory();
            var basex = angle2 * MAX_DISPLACEMENT - (MAX_DISPLACEMENT / 2);
            var baseCoord = new Coordinate(basex, basex);
            var rr1 = rrFact.CreateRectangle(100, 20, angle1, baseCoord);

            // limit size of accumulated star
            _geomCount++;
            if (_geomCount >= BATCH_SIZE)
                _geomCount = 0;
            if (_geomCount == 0)
                _baseAccum = null;

            if (_baseAccum == null)
                _baseAccum = rr1;
            else
            {
                // this line can be used to test for the presence of noding failures for
                // non-tricky cases
                // Geometry star = rr2;
                _baseAccum = rr1.Union(_baseAccum);
            }
            return new[] { _baseAccum, rr1 };
        }

        public void CheckIntersection(IGeometry baseGeom, IGeometry testGeom)
        {
            // this line can be used to test for the presence of noding failures for
            // non-tricky cases
            // Geometry star = rr2;
            Console.WriteLine("Star:");
            Console.WriteLine(baseGeom);
            Console.WriteLine("Rectangle:");
            Console.WriteLine(testGeom);

            // test to see whether the basic overlay code fails
            try
            {
                var intTrial = baseGeom.Intersection(testGeom);
                NetTopologySuite.Utilities.Assert.IsTrue(intTrial != null);
            }
            catch (Exception)
            {
                _failureCount++;
            }

            // this will throw an intersection if a robustness error occurs,
            // stopping the run
            var intersection = SnapIfNeededOverlayOp.Intersection(baseGeom, testGeom);
            Console.WriteLine("Intersection:");
            Console.WriteLine(intersection);
        }
    }

    internal class RotatedRectangleFactory
    {
        private const double PiOver2 = Math.PI/2;
        private readonly IGeometryFactory _fact = new GeometryFactory();

        public IPolygon CreateRectangle(double length, double width, double angle)
        {
            return CreateRectangle(length, width, angle, new Coordinate(0, 0));
        }

        public IPolygon CreateRectangle(double length, double width, double angle, Coordinate baseCoord)
        {
            var posx = length / 2 * Math.Cos(angle);
            var posy = length / 2 * Math.Sin(angle);
            var negx = -posx;
            var negy = -posy;
            var widthOffsetx = (width / 2) * Math.Cos(angle + PiOver2);
            var widthOffsety = (width / 2) * Math.Sin(angle + PiOver2);

            var pts = new[] {
			        new Coordinate(baseCoord.X + posx + widthOffsetx, baseCoord.Y + posy + widthOffsety),
			        new Coordinate(baseCoord.X + posx - widthOffsetx, baseCoord.Y + posy - widthOffsety),
			        new Coordinate(baseCoord.X + negx - widthOffsetx, baseCoord.Y + negy - widthOffsety),
			        new Coordinate(baseCoord.X + negx + widthOffsetx, baseCoord.Y + negy + widthOffsety),
			        new Coordinate(0,0)
            };
            // close polygon
            pts[4] = new Coordinate(pts[0]);
            var poly = _fact.CreatePolygon(_fact.CreateLinearRing(pts), null);
            return poly;
        }
    }
}