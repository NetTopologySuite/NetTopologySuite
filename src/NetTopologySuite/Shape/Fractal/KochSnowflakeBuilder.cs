using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Shape.Fractal
{
    public class KochSnowflakeBuilder : GeometricShapeBuilder
    {
        private readonly CoordinateList _coordList = new CoordinateList();

        public KochSnowflakeBuilder(GeometryFactory geomFactory)
            : base(geomFactory)
        {
        }

        private static int RecursionLevelForSize(int numPts)
        {
            double pow4 = numPts / 3d;
            double exp = Math.Log(pow4) / Math.Log(4);
            return (int)exp;
        }

        public override Geometry GetGeometry()
        {
            int level = RecursionLevelForSize(NumPoints);
            var baseLine = GetSquareBaseLine();
            var pts = GetBoundary(level, baseLine.GetCoordinate(0), baseLine.Length);
            return GeomFactory.CreatePolygon(
                                GeomFactory.CreateLinearRing(pts), null);
        }

        /// <summary>
        /// The height of an equilateral triangle of side one
        /// </summary>
        private static readonly double HeightFactor = Math.Sin(Math.PI / 3.0);
        private const double OneThird = 1.0 / 3.0;
        private static readonly double ThirdHeight = HeightFactor / 3.0;
        private const double TwoThirds = 2.0 / 3.0;

        private Coordinate[] GetBoundary(int level, Coordinate origin, double width)
        {
            double y = origin.Y;
            // for all levels beyond 0 need to vertically shift shape by height of one "arm" to centre it
            if (level > 0)
            {
                y += ThirdHeight * width;
            }

            var p0 = new Coordinate(origin.X, y);
            var p1 = new Coordinate(origin.X + width / 2, y + width * HeightFactor);
            var p2 = new Coordinate(origin.X + width, y);
            AddSide(level, p0, p1);
            AddSide(level, p1, p2);
            AddSide(level, p2, p0);
            _coordList.CloseRing();
            return _coordList.ToCoordinateArray();
        }

        private void AddSide(int level, Coordinate p0, Coordinate p1)
        {
            if (level == 0)
                AddSegment(p0, p1);
            else
            {
                var baseV = Vector2D.Create(p0, p1);
                var midPt = baseV.Multiply(0.5).Translate(p0);

                var heightVec = baseV.Multiply(ThirdHeight);
                var offsetVec = heightVec.RotateByQuarterCircle(1);
                var offsetPt = offsetVec.Translate(midPt);

                int n2 = level - 1;
                var thirdPt = baseV.Multiply(OneThird).Translate(p0);
                var twoThirdPt = baseV.Multiply(TwoThirds).Translate(p0);

                // construct sides recursively
                AddSide(n2, p0, thirdPt);
                AddSide(n2, thirdPt, offsetPt);
                AddSide(n2, offsetPt, twoThirdPt);
                AddSide(n2, twoThirdPt, p1);
            }
        }

        private void AddSegment(Coordinate p0, Coordinate p1)
        {
            _coordList.Add(p1);
        }
    }
}