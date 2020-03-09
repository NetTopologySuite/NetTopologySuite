using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape.Fractal
{
    /// <summary>
    /// Encodes points as the index along the planar Morton (Z-order) curve.
    /// <para>
    /// The planar Morton (Z-order) curve is a continuous space-filling curve.
    /// The Morton curve defines an ordering of the
    /// points in the positive quadrant of the plane.
    /// The index of a point along the Morton curve is called the Morton code.
    /// </para>
    /// <para>
    /// A sequence of subsets of the Morton curve can be defined by a level number.
    /// Each level subset occupies a square range.
    /// The curve at level n Mₙ contains 2ⁿ⁺¹ points.
    /// It fills the range square of side 2ˡᵉᵛᵉˡ.
    /// Curve points have ordinates in the range [0, 2ˡᵉᵛᵉˡ - 1].
    /// The code for a given point is identical at all levels.
    /// The level simply determines the number of points in the curve subset
    /// and the size of the range square.
    /// </para>
    /// <para>
    /// This implementation represents codes using 32-bit integers.
    /// This allows levels 0 to 16 to be handled.
    /// The class supports encoding points
    /// and decoding the point for a given code value.
    /// </para>
    /// <para>
    /// The Morton order has the property that it tends to preserve locality.
    /// This means that codes which are near in value will have spatially proximate
    /// points.  The converse is not always true - the delta between
    /// codes for nearby points is not always small.  But the average delta
    /// is small enough that the Morton order is an effective way of linearizing space
    /// to support range queries.
    /// </para>
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    /// <seealso cref="MortonCurveBuilder"/>
    /// <seealso cref="HilbertCode"/>
    public static class MortonCode
    {
        /// <summary>
        /// The maximum curve level that can be represented.
        /// </summary>
        public static readonly int MaxLevel = 16;

        /// <summary>
        /// The number of points in the curve for the given level.
        /// The number of points is 2²ˡᵉᵛᵉˡ.
        /// </summary>
        /// <param name="level">The level of the curve</param>
        /// <returns>The number of points.</returns>
        public static int Size(int level)
        {
            CheckLevel(level);
            return (int)Math.Pow(2, 2 * level);
        }

        /// <summary>
        /// The maximum ordinate value for points
        /// in the curve for the given level.
        /// The maximum ordinate is 2ˡᵉᵛᵉˡ - 1.
        /// </summary>
        /// <param name="level">The level of the curve.</param>
        /// <returns>The maximum ordinate value.</returns>
        public static int MaxOrdinate(int level)
        {
            CheckLevel(level);
            return (int)Math.Pow(2, level) - 1;
        }

        /// <summary>
        /// The level of the finite Morton curve which contains at least
        /// the given number of points.
        /// </summary>
        /// <param name="numPoints">The number of points required.</param>
        /// <returns>The level of the curve.</returns>
        public static int Level(int numPoints)
        {
            int pow2 = (int)(Math.Log(numPoints) / Math.Log(2));
            int level = pow2 / 2;
            int size = Size(level);
            if (size < numPoints)
            {
                level++;
            }

            return level;
        }

        private static void CheckLevel(int level)
        {
            if (level > MaxLevel)
            {
                throw new ArgumentOutOfRangeException("level", level, $"Level must be in range 0 to {MaxLevel}.");
            }
        }

        /// <summary>
        /// Computes the index of the point (x,y)
        /// in the Morton curve ordering.
        /// </summary>
        /// <param name="x">The x ordinate of the point.</param>
        /// <param name="y">The y ordinate of the point.</param>
        /// <returns>The index of the point along the Morton curve.</returns>
        public static int Encode(int x, int y)
        {
            return (Interleave(y) << 1) + Interleave(x);
        }

        private static int Interleave(int x)
        {
            x &= 0x0000ffff;                 // x = ---- ---- ---- ---- fedc ba98 7654 3210
            x = (x ^ (x << 8)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
            x = (x ^ (x << 4)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
            x = (x ^ (x << 2)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
            x = (x ^ (x << 1)) & 0x55555555; // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
            return x;
        }

        /// <summary>
        /// Computes the point on the Morton curve
        /// for a given index.
        /// </summary>
        /// <param name="index">The index of the point on the curve.</param>
        /// <returns>The point on the curve.</returns>
        public static Coordinate Decode(int index)
        {
            long x = Deinterleave(index);
            long y = Deinterleave(index >> 1);
            return new Coordinate(x, y);
        }

        private static long PrefixScan(long x)
        {
            x = (x >> 8) ^ x;
            x = (x >> 4) ^ x;
            x = (x >> 2) ^ x;
            x = (x >> 1) ^ x;
            return x;
        }

        private static long Deinterleave(int x)
        {
            x = x & 0x55555555;
            x = (x | (x >> 1)) & 0x33333333;
            x = (x | (x >> 2)) & 0x0F0F0F0F;
            x = (x | (x >> 4)) & 0x00FF00FF;
            x = (x | (x >> 8)) & 0x0000FFFF;
            return x;
        }
    }
}
