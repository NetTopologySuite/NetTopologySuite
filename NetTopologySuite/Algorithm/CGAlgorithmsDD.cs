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
            var dx1 = DD.valueOf(p2.X).selfSubtract(p1.X);
            var dy1 = DD.valueOf(p2.Y).selfSubtract(p1.Y);
            var dx2 = DD.valueOf(q.X).selfSubtract(p2.X);
            var dy2 = DD.valueOf(q.Y).selfSubtract(p2.Y);

            return SignOfDet2x2(dx1, dy1, dx2, dy2);
        }

        public static int SignOfDet2x2(DD x1, DD y1, DD x2, DD y2)
        {
            DD det = x1.multiply(y2).subtract(y1.multiply(x2));
            if (det.isZero())
                return 0;
            if (det.isNegative())
                return -1;
            return 1;

        }

    }
}