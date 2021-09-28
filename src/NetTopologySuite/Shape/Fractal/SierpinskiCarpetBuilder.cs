using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape.Fractal
{
    public class SierpinskiCarpetBuilder : GeometricShapeBuilder
    {
        public SierpinskiCarpetBuilder(GeometryFactory geomFactory)
            :base(geomFactory)
        {
        }

        private static int RecursionLevelForSize(int numPts)
        {
            double pow4 = numPts/3d;
            double exp = Math.Log(pow4)/Math.Log(4);
            return (int) exp;
        }

        public override Geometry GetGeometry()
        {
            int level = RecursionLevelForSize(NumPoints);
            var baseLine = GetSquareBaseLine();
            var origin = baseLine.GetCoordinate(0);
            var holes = GetHoles(level, origin.X, origin.Y, Diameter);
            var shell = (LinearRing) ((Polygon) GeomFactory.ToGeometry(GetSquareExtent())).ExteriorRing;
            return GeomFactory.CreatePolygon(shell, holes);
        }

        private LinearRing[] GetHoles(int n, double originX, double originY, double width)
        {
            var holeList = new List<Geometry>();

            AddHoles(n, originX, originY, width, holeList);

            return GeometryFactory.ToLinearRingArray(holeList);
        }

        private void AddHoles(int n, double originX, double originY, double width, ICollection<Geometry> holeList)
        {
            if (n < 0) return;
            int n2 = n - 1;
            double widthThird = width/3.0;

            AddHoles(n2, originX,                originY,                widthThird, holeList);
            AddHoles(n2, originX + widthThird,   originY,                widthThird, holeList);
            AddHoles(n2, originX + 2*widthThird, originY,                widthThird, holeList);

            AddHoles(n2, originX,                originY + widthThird,   widthThird, holeList);
            AddHoles(n2, originX + 2*widthThird, originY + widthThird,   widthThird, holeList);

            AddHoles(n2, originX,                originY + 2*widthThird, widthThird, holeList);
            AddHoles(n2, originX + widthThird,   originY + 2*widthThird, widthThird, holeList);
            AddHoles(n2, originX + 2*widthThird, originY + 2*widthThird, widthThird, holeList);

            // add the centre hole
            holeList.Add(CreateSquareHole(originX + widthThird, originY + widthThird, widthThird));
        }

        private LinearRing CreateSquareHole(double x, double y, double width)
        {
            var pts = new[]
                          {
                              new Coordinate(x, y),
                              new Coordinate(x + width, y),
                              new Coordinate(x + width, y + width),
                              new Coordinate(x, y + width),
                              new Coordinate(x, y)
                          };
            return GeomFactory.CreateLinearRing(pts);
        }
    }
}
