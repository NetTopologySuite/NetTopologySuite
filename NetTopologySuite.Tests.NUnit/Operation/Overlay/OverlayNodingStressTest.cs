using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Overlay
{
    [TestFixture]
    public class OverlayNodingStressTest
    {
        private const int IterLimit = 10000;
        private const int BatchSize = 20;

        private readonly Random _rand = new Random((int)(Math.PI * 10e7));
        private int _failureCount;

        private double getRand()
        {
            double r = _rand.NextDouble();
            return r;
        }

        [Test][Ignore]
        public void TestNoding()
        {
            const int iterLimit = IterLimit;
            for (int i = 0; i < iterLimit; i++)
            {
                Console.WriteLine("Iter: " + i
                        + "  Noding failure count = " + _failureCount);
                double ang1 = getRand() * Math.PI;
                double ang2 = getRand() * Math.PI;
                //			Geometry[] geom = generateGeometryStar(ang1, ang2);
                IGeometry<Coordinate>[] geom = generateGeometryAccum(ang1, ang2);
                CheckIntersection(geom[0], geom[1]);
            }
            Console.WriteLine(
                    "Test count = " + iterLimit
                    + "  Noding failure count = " + _failureCount
                );
        }

        public IGeometry<Coordinate>[] generateGeometryStar(double angle1, double angle2)
        {
            RotatedRectangleFactory rrFact = new RotatedRectangleFactory();
            IPolygon<Coordinate> rr1 = rrFact.CreateRectangle(100, 20, angle1);
            IPolygon<Coordinate> rr2 = rrFact.CreateRectangle(100, 20, angle2);

            // this line can be used to Test for the presence of noding failures for
            // non-tricky cases
            // Geometry star = rr2;
            IGeometry<Coordinate> star = rr1.Union(rr2);
            return new IGeometry<Coordinate>[] { star, rr1 };
        }

        private const double MaxDisplacement = 60;

        private IGeometry<Coordinate> _baseAccum;
        private int _geomCount;

        public IGeometry<Coordinate>[] generateGeometryAccum(double angle1, double angle2)
        {
            RotatedRectangleFactory rrFact = new RotatedRectangleFactory();
            double basex = angle2 * MaxDisplacement - (MaxDisplacement / 2);
            Coordinate basis = GeometryUtils.CoordFac.Create(basex, basex);
            IPolygon<Coordinate> rr1 = rrFact.CreateRectangle(100, 20, angle1, basis);

            // limit size of accumulated star
            _geomCount++;
            if (_geomCount >= BatchSize)
                _geomCount = 0;
            if (_geomCount == 0)
                _baseAccum = null;

            if (_baseAccum == null)
                _baseAccum = rr1;
            else
            {
                // this line can be used to Test for the presence of noding failures for
                // non-tricky cases
                // Geometry star = rr2;
                _baseAccum = rr1.Union(_baseAccum);
            }
            return new IGeometry<Coordinate>[] { _baseAccum, rr1 };
        }

        public void CheckIntersection(IGeometry<Coordinate> basis, IGeometry<Coordinate> testGeom)
        {

            // this line can be used to Test for the presence of noding failures for
            // non-tricky cases
            // Geometry star = rr2;
            Console.WriteLine("Star:");
            Console.WriteLine(basis);
            Console.WriteLine("Rectangle:");
            Console.WriteLine(testGeom);

            // Test to see whether the basic overlay code fails
            try
            {
                IGeometry<Coordinate> intTrial = basis.Intersection(testGeom);
            }
            catch (Exception ex)
            {
                _failureCount++;
            }

            // this will throw an intersection if a robustness error occurs,
            // stopping the run
            IGeometry<Coordinate> intersection = SnapIfNeededOverlayOp<Coordinate>.Intersection(basis, testGeom);
            Console.WriteLine("Intersection:");
            Console.WriteLine(intersection);
        }
    }

    class RotatedRectangleFactory
    {

        private const double PiOver2 = Math.PI / 2;
        private readonly IGeometryFactory<Coordinate> _fact = GeometryUtils.GeometryFactory;

        public IPolygon<Coordinate> CreateRectangle(double length, double width, double angle)
        {
            return CreateRectangle(length, width, angle, _fact.CoordinateFactory.Create(0, 0));
        }

        public IPolygon<Coordinate> CreateRectangle(double length, double width, double angle, Coordinate basis)
        {
            double posx = length / 2 * Math.Cos(angle);
            double posy = length / 2 * Math.Sin(angle);
            double negx = -posx;
            double negy = -posy;
            double widthOffsetx = (width / 2) * Math.Cos(angle + PiOver2);
            double widthOffsety = (width / 2) * Math.Sin(angle + PiOver2);

            Coordinate[] pts = new Coordinate[] {
				_fact.CoordinateFactory.Create(basis[Ordinates.X] + posx + widthOffsetx, basis[Ordinates.Y] + posy + widthOffsety),
				_fact.CoordinateFactory.Create(basis[Ordinates.X] + posx - widthOffsetx, basis[Ordinates.Y] + posy - widthOffsety),
				_fact.CoordinateFactory.Create(basis[Ordinates.X] + negx - widthOffsetx, basis[Ordinates.Y] + negy - widthOffsety),
				_fact.CoordinateFactory.Create(basis[Ordinates.X] + negx + widthOffsetx, basis[Ordinates.Y] + negy + widthOffsety),
				_fact.CoordinateFactory.Create(0,0),
		};
            // close polygon
            pts[4] = _fact.CoordinateFactory.Create(pts[0]);
            IPolygon<Coordinate> poly = _fact.CreatePolygon(_fact.CreateLinearRing(pts), null);
            return poly;
        }


    }

}