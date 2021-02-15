using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A class that can be used to test coordinates for equality.
    /// <para/>
    /// It uses the algorithm that was default for NTS prior to v2.2,
    /// i.e. checks if the 2d distance between coordinates <c>x</c>
    /// and <c>y</c> is less than or equal to a tolerance value.
    /// </summary>
    public class CoordinateEqualityComparer : EqualityComparer<Coordinate>
    {
        /// <inheritdoc cref="EqualityComparer{T}.Equals(T, T)"/>
        public sealed override bool Equals(Coordinate x, Coordinate y)
        {
            return AreEqual(x, y, 0d);
        }

        /// <summary>
        /// Compares <see cref="Coordinate"/>s <paramref name="x"/> and <paramref name="y"/> for equality allowing for a <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="x">A <c>Coordinate</c></param>
        /// <param name="y">A <c>Coordinate</c></param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> can be considered equal; otherwise <c>false</c>.</returns>
        public bool Equals(Coordinate x, Coordinate y, double tolerance)
        {
            return AreEqual(x, y, tolerance);
        }

        /// <inheritdoc cref="EqualityComparer{T}.GetHashCode(T)"/>
        public sealed override int GetHashCode(Coordinate c)
        {
            return c.GetHashCode();
        }

        /// <summary>
        /// Method to test 2 <see cref="Coordinate"/>s for equality, allowing a tolerance.
        /// </summary>
        /// <param name="a">The 1st Coordinate</param>
        /// <param name="b">The 2nd Coordinate</param>
        /// <param name="tolerance">A tolerance value</param>
        /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> can be considered equal.</returns>
        protected virtual bool AreEqual(Coordinate a, Coordinate b, double tolerance)
        {
            if (tolerance == 0)
                return a.Equals(b);

            return a.Distance(b) <= tolerance;
        }
    }

    /// <summary>
    /// A class that can be used to test coordinates for equality.
    /// <para/>
    /// This class test for each ordinate if the distance is less
    /// than a tolerance value.
    /// </summary>
    public sealed class PerOrdinateEqualityComparer : CoordinateEqualityComparer
    {
        /// <summary>
        /// Method to test 2 <see cref="Coordinate"/>s for equality, allowing a tolerance.
        /// </summary>
        /// <param name="a">The 1st Coordinate</param>
        /// <param name="b">The 2nd Coordinate</param>
        /// <param name="tolerance">A tolerance value</param>
        /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> can be considered equal.</returns>
        protected override bool AreEqual(Coordinate a, Coordinate b, double tolerance)
        {
            return Distance(a.X, b.X) <= tolerance &&
                   Distance(a.Y, b.Y) <= tolerance &&
                   Distance(a.Z, b.Z) <= tolerance &&
                   Distance(a.M, b.M) <= tolerance;
        }

        /// <summary>
        /// Computes the distance between two <see cref="double"/> values
        /// </summary>
        /// <param name="a">1st double</param>
        /// <param name="b">2nd double</param>
        /// <returns>The distance between <paramref name="a"/> and <paramref name="b"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Distance(double a, double b)
        {
            if (double.IsNaN(a) && double.IsNaN(b))
                return 0d;

            if (double.IsNaN(a) || double.IsNaN(b))
                return double.PositiveInfinity;

            return Math.Abs(a - b);
        }
    }
}
