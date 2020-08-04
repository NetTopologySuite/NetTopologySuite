using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape.Fractal
{
    /// <summary>
    /// Encodes points as the index along finite planar Hilbert curves.
    /// <para>
    /// The planar Hilbert Curve is a continuous space-filling curve.
    /// In the limit the Hilbert curve has infinitely many vertices and fills
    /// the space of the unit square.
    /// A sequence of finite approximations to the infinite Hilbert curve
    /// is defined by the level number.
    /// The finite Hilbert curve at level n Hₙ contains 2ⁿ⁺¹ points.
    /// Each finite Hilbert curve defines an ordering of the
    /// points in the 2-dimensional range square containing the curve.
    /// Curves fills the range square of side 2ˡᵉᵛᵉˡ.
    /// Curve points have ordinates in the range [0, 2ˡᵉᵛᵉˡ - 1].
    /// The index of a point along a Hilbert curve is called the Hilbert code.
    /// The code for a given point is specific to the level chosen.
    /// </para>
    /// <para>
    /// This implementation represents codes using 32-bit integers.
    /// This allows levels 0 to 16 to be handled.
    /// The class supports encoding points in the range of a given level curve
    /// and decoding the point for a given code value.
    /// </para>
    /// <para>
    /// The Hilbert order has the property that it tends to preserve locality.
    /// This means that codes which are near in value will have spatially proximate
    /// points.  The converse is not always true - the delta between
    /// codes for nearby points is not always small.  But the average delta
    /// is small enough that the Hilbert order is an effective way of linearizing space
    /// to support range queries.
    /// </para>
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    /// <seealso cref="HilbertCurveBuilder"/>
    /// <seealso cref="MortonCode"/>
    public static class HilbertCode
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
        /// The level of the finite Hilbert curve which contains at least
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
        /// Encodes a point (x,y)
        /// in the range of the the Hilbert curve at a given level
        /// as the index of the point along the curve.
        /// The index will lie in the range [0, 2ˡᵉᵛᵉˡ⁺¹].
        /// </summary>
        /// <param name="level">The level of the Hilbert curve.</param>
        /// <param name="x">The x ordinate of the point.</param>
        /// <param name="y">The y ordinate of the point.</param>
        /// <returns>The index of the point along the Hilbert curve.</returns>
        public static int Encode(int level, int x, int y)
        {
            // Fast Hilbert curve algorithm by http://threadlocalmutex.com/
            // Ported from C++ https://github.com/rawrunprotected/hilbert_curves (public
            // domain)

            int lvl = LevelClamp(level);

            x = x << (16 - lvl);
            y = y << (16 - lvl);

            long a = x ^ y;
            long b = 0xFFFF ^ a;
            long c = 0xFFFF ^ (x | y);
            long d = x & (y ^ 0xFFFF);

            long A = a | (b >> 1);
            long B = (a >> 1) ^ a;
            long C = ((c >> 1) ^ (b & (d >> 1))) ^ c;
            long D = ((a & (c >> 1)) ^ (d >> 1)) ^ d;

            a = A;
            b = B;
            c = C;
            d = D;
            A = ((a & (a >> 2)) ^ (b & (b >> 2)));
            B = ((a & (b >> 2)) ^ (b & ((a ^ b) >> 2)));
            C ^= ((a & (c >> 2)) ^ (b & (d >> 2)));
            D ^= ((b & (c >> 2)) ^ ((a ^ b) & (d >> 2)));

            a = A;
            b = B;
            c = C;
            d = D;
            A = ((a & (a >> 4)) ^ (b & (b >> 4)));
            B = ((a & (b >> 4)) ^ (b & ((a ^ b) >> 4)));
            C ^= ((a & (c >> 4)) ^ (b & (d >> 4)));
            D ^= ((b & (c >> 4)) ^ ((a ^ b) & (d >> 4)));

            a = A;
            b = B;
            c = C;
            d = D;
            C ^= ((a & (c >> 8)) ^ (b & (d >> 8)));
            D ^= ((b & (c >> 8)) ^ ((a ^ b) & (d >> 8)));

            a = C ^ (C >> 1);
            b = D ^ (D >> 1);

            long i0 = x ^ y;
            long i1 = b | (0xFFFF ^ (i0 | a));

            i0 = (i0 | (i0 << 8)) & 0x00FF00FF;
            i0 = (i0 | (i0 << 4)) & 0x0F0F0F0F;
            i0 = (i0 | (i0 << 2)) & 0x33333333;
            i0 = (i0 | (i0 << 1)) & 0x55555555;

            i1 = (i1 | (i1 << 8)) & 0x00FF00FF;
            i1 = (i1 | (i1 << 4)) & 0x0F0F0F0F;
            i1 = (i1 | (i1 << 2)) & 0x33333333;
            i1 = (i1 | (i1 << 1)) & 0x55555555;

            long index = ((i1 << 1) | i0) >> (32 - 2 * lvl);
            return (int)index;
        }

        /// <summary>
        /// Clamps a level to the range valid for
        /// the index algorithm used.
        /// </summary>
        /// <param name="level">The level of a Hilbert curve.</param>
        /// <returns>A valid level.</returns>
        private static int LevelClamp(int level)
        {
            // clamp order to [1, 16]
            int lvl = level < 1 ? 1 : level;
            lvl = lvl > MaxLevel ? MaxLevel : lvl;
            return lvl;
        }

        /// <summary>
        /// Computes the point on a Hilbert curve
        /// of given level for a given code index.
        /// The point ordinates will lie in the range [0, 2ˡᵉᵛᵉˡ - 1].
        /// </summary>
        /// <param name="level">The Hilbert curve level.</param>
        /// <param name="index">The index of the point on the curve.</param>
        /// <returns>The point on the Hilbert curve.</returns>
        public static Coordinate Decode(int level, int index)
        {
            CheckLevel(level);
            int lvl = LevelClamp(level);

            index = index << (32 - 2 * lvl);

            long i0 = Deinterleave(index);
            long i1 = Deinterleave(index >> 1);

            long t0 = (i0 | i1) ^ 0xFFFF;
            long t1 = i0 & i1;

            long prefixT0 = PrefixScan(t0);
            long prefixT1 = PrefixScan(t1);

            long a = (((i0 ^ 0xFFFF) & prefixT1) | (i0 & prefixT0));

            long x = (a ^ i1) >> (16 - lvl);
            long y = (a ^ i0 ^ i1) >> (16 - lvl);

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
