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

namespace NetTopologySuite.Tests.NUnit.Operation.Overlay
{
    [TestFixture]
    public class OverlayNodingStressTest
    {
        private static int ITER_LIMIT = 10000;
        private static int BATCH_SIZE = 20;

        private Random rand = new Random((int)(Math.PI * 10e5));
        private int failureCount = 0;

        private double getRand()
        {
            double r = rand.NextDouble();
            return r;
        }

        [Test]
        public void TestNoding()
        {
            int iterLimit = ITER_LIMIT;
            for (int i = 0; i < iterLimit; i++)
            {
                Console.WriteLine("Iter: " + i
                        + "  Noding failure count = " + failureCount);
                double ang1 = getRand() * Math.PI;
                double ang2 = getRand() * Math.PI;
                //			Geometry[] geom = generateGeometryStar(ang1, ang2);
                IGeometry[] geom = GenerateGeometryAccum(ang1, ang2);
                CheckIntersection(geom[0], geom[1]);
            }
            Console.WriteLine(
                    "Test count = " + iterLimit
                    + "  Noding failure count = " + failureCount
                );
        }

        public IGeometry[] GenerateGeometryStar(double angle1, double angle2)
        {
            RotatedRectangleFactory rrFact = new RotatedRectangleFactory();
            IPolygon rr1 = rrFact.CreateRectangle(100, 20, angle1);
            IPolygon rr2 = rrFact.CreateRectangle(100, 20, angle2);

            // this line can be used to test for the presence of noding failures for
            // non-tricky cases
            // Geometry star = rr2;
            IGeometry star = rr1.Union(rr2);
            return new IGeometry[] { star, rr1 };
        }

        private static double MAX_DISPLACEMENT = 60;

        private IGeometry baseAccum = null;
        private int geomCount = 0;

        public IGeometry[] GenerateGeometryAccum(double angle1, double angle2)
        {
            RotatedRectangleFactory rrFact = new RotatedRectangleFactory();
            double basex = angle2 * MAX_DISPLACEMENT - (MAX_DISPLACEMENT / 2);
            Coordinate baseCoord = new Coordinate(basex, basex);
            IPolygon rr1 = rrFact.CreateRectangle(100, 20, angle1, baseCoord);

            // limit size of accumulated star
            geomCount++;
            if (geomCount >= BATCH_SIZE)
                geomCount = 0;
            if (geomCount == 0)
                baseAccum = null;

            if (baseAccum == null)
                baseAccum = rr1;
            else
            {
                // this line can be used to test for the presence of noding failures for
                // non-tricky cases
                // Geometry star = rr2;
                baseAccum = rr1.Union(baseAccum);
            }
            return new IGeometry[] { baseAccum, rr1 };
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
                IGeometry intTrial = baseGeom.Intersection(testGeom);
            }
            catch (Exception ex)
            {
                failureCount++;
            }

            // this will throw an intersection if a robustness error occurs,
            // stopping the run
            IGeometry intersection = SnapIfNeededOverlayOp.Intersection(baseGeom, testGeom);
            Console.WriteLine("Intersection:");
            Console.WriteLine(intersection);
        }
    }

    internal class RotatedRectangleFactory
    {
        public RotatedRectangleFactory()
        {
        }

        private static double PI_OVER_2 = Math.PI / 2;
        private GeometryFactory fact = new GeometryFactory();

        public IPolygon CreateRectangle(double length, double width, double angle)
        {
            return CreateRectangle(length, width, angle, new Coordinate(0, 0));
        }

        public IPolygon CreateRectangle(double length, double width, double angle, Coordinate baseCoord)
        {
            double posx = length / 2 * Math.Cos(angle);
            double posy = length / 2 * Math.Sin(angle);
            double negx = -posx;
            double negy = -posy;
            double widthOffsetx = (width / 2) * Math.Cos(angle + PI_OVER_2);
            double widthOffsety = (width / 2) * Math.Sin(angle + PI_OVER_2);

            Coordinate[] pts = new Coordinate[] {
			        new Coordinate(baseCoord.X + posx + widthOffsetx, baseCoord.Y + posy + widthOffsety),
			        new Coordinate(baseCoord.X + posx - widthOffsetx, baseCoord.Y + posy - widthOffsety),
			        new Coordinate(baseCoord.X + negx - widthOffsetx, baseCoord.Y + negy - widthOffsety),
			        new Coordinate(baseCoord.X + negx + widthOffsetx, baseCoord.Y + negy + widthOffsety),
			        new Coordinate(0,0),
	        };
            // close polygon
            pts[4] = new Coordinate(pts[0]);
            IPolygon poly = fact.CreatePolygon(fact.CreateLinearRing(pts), null);
            return poly;
        }
    }
}