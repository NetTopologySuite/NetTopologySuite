using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class TestShapeFactory
    {

        public static Polygon CreateSquare(Coordinate origin, double size)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = 4;
            var g = gsf.CreateRectangle();
            // Polygon gRect = gsf.createRectangle();
            // Geometry g = gRect.getExteriorRing();
            return g;
        }

        public static Geometry CreateSineStar(Coordinate origin, double size, int nPts)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            gsf.ArmLengthRatio = 2;
            gsf.NumArms = 20;
            var poly = gsf.CreateSineStar();
            return poly;
        }

        public static Polygon CreateCircle(Coordinate origin, double size, int nPts)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            var circle = gsf.CreateCircle();
            return circle;
        }

        private static double HOLE_SIZE_FACTOR = 0.8;

        public static Geometry CreateSquareWithCircleHoles(Coordinate origin, double size, int nHoles, int nPtsHole)
        {
            var square = CreateSquare(origin, size);

            int gridSide = (int)Math.Sqrt(nHoles);
            if (gridSide * gridSide < nHoles)
                gridSide++;

            double gridSideLen = size / gridSide;
            double holeSize = HOLE_SIZE_FACTOR * gridSideLen;

            var holes = new LinearRing[nHoles];

            double baseX = origin.X - (size / 2) + gridSideLen / 2;
            double baseY = origin.Y - (size / 2) + gridSideLen / 2;

            int index = 0;
            for (int i = 0; i < gridSide; i++)
            {
                for (int j = 0; j < gridSide; j++)
                {
                    double x = baseX + i * gridSideLen;
                    double y = baseY + j * gridSideLen;
                    var circle = CreateCircle(new Coordinate(x, y), holeSize, nPtsHole);
                    holes[index++] = (LinearRing)circle.ExteriorRing;
                }
            }
            return square.Factory.CreatePolygon((LinearRing)square.ExteriorRing, holes);
        }

        public static Geometry CreateSlantedEllipses(Coordinate origin, double size, double scaleFactor, int nGeom,
            int nPts)
        {
            var circles = CreateCircleRow(origin, size, nGeom, nPts);
            var centre = circles.EnvelopeInternal.Centre;

            var scaleTrans = AffineTransformation.ScaleInstance(1, scaleFactor, centre.X, centre.Y);
            circles.Apply(scaleTrans);

            var centreScaled = circles.EnvelopeInternal.Centre;

            var rotateTrans = AffineTransformation.RotationInstance(Math.PI / 4, centreScaled.X, centreScaled.Y);
            circles.Apply(rotateTrans);

            return circles;
        }

        private static Geometry CreateCircleRow(Coordinate origin, double size, int nGeom, int nPts)
        {
            var circles = new Polygon[nGeom];

            int nPtsGeom = nPts / nGeom;

            double baseX = origin.X;
            double y = origin.Y;
            for (int i = 0; i < nGeom; i++)
            {

                var originGeom = new Coordinate(baseX + i * 2 * size, y);
                circles[i] = CreateCircle(originGeom, size, nPtsGeom);
            }
            return circles[0].Factory.CreateMultiPolygon(circles);
        }

        public static Geometry CreateExtentWithHoles(Geometry polygons)
        {
            var env = polygons.EnvelopeInternal.Copy();
            env.ExpandBy(env.Diameter);
            var factory = polygons.Factory;
            var shell = (LinearRing)((Polygon)factory.ToGeometry(env)).ExteriorRing;
            var holes = ExtractShells(polygons);
            return factory.CreatePolygon(shell, holes);
        }

        private static LinearRing[] ExtractShells(Geometry polygons)
        {
            int n = polygons.NumGeometries;
            var shells = new LinearRing[n];
            for (int i = 0; i < n; i++)
            {
                shells[i] = (LinearRing)((Polygon)polygons.GetGeometryN(i)).ExteriorRing;
            }
            return shells;
        }
    }
}
