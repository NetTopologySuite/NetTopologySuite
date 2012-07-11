using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Implements basic computational geometry algorithms using <seealso cref="DD"/> arithmetic.
    /// </summary>
    /// <author>Martin Davis</author>
    public class CGAlgorithmsDD
    {
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
        {
            var dx1 = DD.ValueOf(p2.X).SelfSubtract(p1.X);
            var dy1 = DD.ValueOf(p2.Y).SelfSubtract(p1.Y);
            var dx2 = DD.ValueOf(q.X).SelfSubtract(p2.X);
            var dy2 = DD.ValueOf(q.Y).SelfSubtract(p2.Y);

            return SignOfDet2x2(dx1, dy1, dx2, dy2);
        }

        public static int SignOfDet2x2(DD x1, DD y1, DD x2, DD y2)
        {
            DD det = x1.Multiply(y2).Subtract(y1.Multiply(x2));
            if (det.IsZero)
                return 0;
            if (det.IsNegative)
                return -1;
            return 1;

        }

    }
}