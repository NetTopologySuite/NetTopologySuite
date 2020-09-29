using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Quadrant values
    /// </summary>
    /// <remarks>
    /// The quadants are numbered as follows:
    /// <para>
    /// <code>
    /// 1 - NW | 0 - NE <br/>
    /// -------+------- <br/>
    /// 2 - SW | 3 - SE
    /// </code>
    /// </para>
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public readonly struct Quadrant
    {
        /// <summary>
        /// Undefined
        /// </summary>
        public static Quadrant Undefined => new Quadrant(-1);

        /// <summary>
        /// North-East
        /// </summary>
        public static Quadrant NE => new Quadrant(0);

        /// <summary>
        /// North-West
        /// </summary>
        public static Quadrant NW => new Quadrant(1);

        /// <summary>
        /// South-West
        /// </summary>
        public static Quadrant SW => new Quadrant(2);

        /// <summary>
        /// South-East
        /// </summary>
        public static Quadrant SE => new Quadrant(3);

        /// <summary>
        /// Creates a quadrant with t
        /// </summary>
        /// <param name="value"></param>
        public Quadrant(int value)
        {
            if (value < -1 || 4 < value)
                throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }

        /// <summary>
        /// Creates a quadrant of a directed line segment (specified as x and y
        /// displacements, which cannot both be 0).
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <exception cref="ArgumentException">If the displacements are both 0</exception>
        public Quadrant(double dx, double dy)
        {
            if (dx == 0.0 && dy == 0.0)
                throw new ArgumentException("Cannot compute the quadrant for point ( " + dx + ", " + dy + " )");

            if (dx >= 0.0)
                Value = dy >= 0.0 ? NE.Value : SE.Value;
            else
                Value = dy >= 0.0 ? NW.Value : SW.Value;
        }

        /// <summary>
        /// Returns the quadrant of a directed line segment from p0 to p1.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <exception cref="ArgumentException"> if the points are equal</exception>
        public Quadrant(Coordinate p0, Coordinate p1)
        {
            if (p1.X == p0.X && p1.Y == p0.Y)
                throw new ArgumentException("Cannot compute the quadrant for two identical points " + p0);

            if (p1.X >= p0.X)
                Value = p1.Y >= p0.Y ? NE.Value : SE.Value;
            else
                Value = p1.Y >= p0.Y ? NW.Value : SW.Value;
        }

        internal int Value { get; }

        #region object overrides

        /// <inheritdoc cref="object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return 13 ^ Value.GetHashCode();
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj is Quadrant q)
                return this == q;
            return false;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            switch (Value)
            {
                case 0:
                    return "NE(=0)";
                case 1:
                    return "NW(=1)";
                case 2:
                    return "SW(=2)";
                case 3:
                    return "SE(=3)";
                default:
                    return "Undefinded(=-1)";
            }
        }

        #endregion

        /// <summary>
        /// Returns true if the quadrants are 1 and 3, or 2 and 4.
        /// </summary>
        /// <param name="quad">A quadrant</param>
        public bool IsOpposite(Quadrant quad)
        {
            if (this == quad)
                return false;
            int diff = (Value - quad.Value + 4) % 4;
            // if quadrants are not adjacent, they are opposite
            if (diff == 2)
                return true;
            return false;
        }

        /// <summary>
        /// Returns the right-hand quadrant of the halfplane defined by the two quadrants,
        /// or -1 if the quadrants are opposite, or the quadrant if they are identical.
        /// </summary>
        /// <param name="quad1"></param>
        /// <param name="quad2"></param>
        public static Quadrant CommonHalfPlane(Quadrant quad1, Quadrant quad2)
        {
            // if quadrants are the same they do not determine a unique common halfplane.
            // Simply return one of the two possibilities
            if (quad1 == quad2)
                return quad1;
            int diff = (quad1.Value - quad2.Value + 4) % 4;
            // if quadrants are not adjacent, they do not share a common halfplane
            if (diff == 2)
                return Quadrant.Undefined;

            var min = (quad1 < quad2) ? quad1 : quad2;
            var max = (quad1 > quad2) ? quad1 : quad2;
            // for this one case, the righthand plane is NOT the minimum index;
            if (min == NW && max == Quadrant.SW)
                return Quadrant.SW;
            // in general, the halfplane index is the minimum of the two adjacent quadrants
            return min;
        }

        /// <summary>
        /// Returns whether this quadrant lies within the given halfplane (specified
        /// by its right-hand quadrant).
        /// </summary>
        /// <param name="halfPlane"></param>
        public bool IsInHalfPlane(Quadrant halfPlane)
        {
            if (halfPlane == SE)
                return this == SE || this == SW;
            return this == halfPlane || Value == halfPlane.Value + 1;
        }

        /// <summary>
        /// Returns <c>true</c> if the given quadrant is 0 or 1.
        /// </summary>
        public bool IsNorthern
        {
            get => Value == NE.Value || Value == NW.Value;
        }

        /// <summary>
        /// Equality operator for quadrants
        /// </summary>
        /// <param name="lhs">Quadrant value on the left-hand-side</param>
        /// <param name="rhs">Quadrant value on the right-hand-side</param>
        /// <returns><c>true</c> if quadrant value of <paramref name="lhs"/> and <paramref name="rhs"/> are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Quadrant lhs, Quadrant rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Inequality operator for quadrants
        /// </summary>
        /// <param name="lhs">Quadrant value on the left-hand-side</param>
        /// <param name="rhs">Quadrant value on the right-hand-side</param>
        /// <returns><c>true</c> if quadrant value of <paramref name="lhs"/> and <paramref name="rhs"/> are <b>not</b> equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Quadrant lhs, Quadrant rhs)
        {
            return lhs.Value != rhs.Value;
        }

        /// <summary>
        /// Greater than (&gt;) operator for quadrants
        /// </summary>
        /// <param name="lhs">Quadrant value on the left-hand-side</param>
        /// <param name="rhs">Quadrant value on the right-hand-side</param>
        /// <returns><c>true</c> if quadrant value of <paramref name="lhs"/> and <paramref name="rhs"/> are <b>not</b> equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Quadrant lhs, Quadrant rhs)
        {
            return lhs.Value > rhs.Value;
        }

        /// <summary>
        /// Less than (&lt;) operator for quadrants
        /// </summary>
        /// <param name="lhs">Quadrant value on the left-hand-side</param>
        /// <param name="rhs">Quadrant value on the right-hand-side</param>
        /// <returns><c>true</c> if quadrant value of <paramref name="lhs"/> and <paramref name="rhs"/> are <b>not</b> equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Quadrant lhs, Quadrant rhs)
        {
            return lhs.Value < rhs.Value;
        }
        ///// <summary>
        ///// Converts an integer value to a <see cref="Quadrant"/>
        ///// </summary>
        ///// <param name="value">The integer value</param>
        //public static implicit operator Quadrant(int value)
        //{
        //    return new Quadrant(value/* + 1*/);
        //}

        ///// <summary>
        ///// Converts an integer value to a <see cref="Quadrant"/>
        ///// </summary>
        ///// <param name="value">The integer value</param>
        //public static implicit operator int(Quadrant value)
        //{
        //    return value.Value/* - 1*/;
        //}
    }
}
