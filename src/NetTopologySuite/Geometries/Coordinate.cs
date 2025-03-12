using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A lightweight class used to store coordinates on the 2-dimensional Cartesian plane.
    /// <para>
    /// The base data object is suitable for use with coordinate sequences with
    /// <c>dimension</c> = 2 and <c>measures</c> = 0.
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is distinct from <see cref="Point"/>, which is a subclass of <see cref="Geometry"/>.
    /// Unlike objects of type <see cref="Point"/> (which contain additional
    /// information such as an envelope, a precision model, and spatial reference
    /// system information), a <c>Coordinate</c> only contains ordinate values
    /// and properties.
    /// <para/>
    /// Implementations may optionally support Z-ordinate and M-measure values
    /// as appropriate for a <see cref="CoordinateSequence"/>. Use of <see cref="Z"/>
    /// and <see cref="M"/> setters or <see cref="P:NetTopologySuite.Geometries.Coordinate.this[int]" /> indexer are recommended.
    /// </remarks>
    [Serializable]
#pragma warning disable 612,618
    public class Coordinate : IComparable, IComparable<Coordinate>
    {
        ///<summary>
        /// The value used to indicate a null or missing ordinate value.
        /// In particular, used for the value of ordinates for dimensions
        /// greater than the defined dimension of a coordinate.
        ///</summary>
        public const double NullOrdinate = double.NaN;

        // Coordinate is auto-[Serializable], so replacing with auto properties could break compat.
#pragma warning disable IDE0032
        private double _x;

        private double _y;
#pragma warning restore IDE0032

        /// <summary>
        /// Gets or sets the X-ordinate value.
        /// </summary>
        public double X
        {
            get => _x;
            set => _x = value;
        }

        /// <summary>
        /// Gets or sets the Y-ordinate value.
        /// </summary>
        public double Y
        {
            get => _y;
            set => _y = value;
        }

        /// <summary>
        /// Gets or sets the Z-ordinate value, if supported.
        /// If no Z value is present, returns <see cref="NullOrdinate"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an attempt is made to <b>set</b> the Z-ordinate value on an instance where
        /// the Z-ordinate value is not supported.
        /// </exception>
        public virtual double Z
        {
            get => NullOrdinate; 
            set { throw new InvalidOperationException($"{GetType().Name} does not support setting Z-ordinate");}
        }

        /// <summary>
        /// Gets or sets the value of the measure, if supported.
        /// If no measure value is present, returns <see cref="NullOrdinate"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an attempt is made to <b>set</b> the measure value on an instance where
        /// measures are not supported.
        /// </exception>
        public virtual double M
        {
            get => NullOrdinate;
            set { throw new InvalidOperationException($"{GetType().Name} does not support setting M-measure"); }
        }

        /// <summary>
        /// Constructs a <c>Coordinate</c> at (x,y).
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///  Constructs a <c>Coordinate</c> at (0,0).
        /// </summary>
        public Coordinate() : this(0.0, 0.0) { }

        /// <summary>
        /// Constructs a <c>Coordinate</c> having the same (x,y,z) values as
        /// <paramref name="c"/>.
        /// </summary>
        /// <param name="c"><c>Coordinate</c> to copy.</param>
        public Coordinate(Coordinate c) : this(c.X, c.Y) { }

        /// <summary>
        /// Gets or sets the value for the given ordinate.
        /// </summary>
        /// <param name="ordinate">The ordinate.</param>
        /// <returns>The ordinate value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ordinate"/> is not one of <see cref="Ordinate.X"/>, <see cref="Ordinate.Y"/>, <see cref="Ordinate.Z"/>, or <see cref="Ordinate.M"/>.</exception>
        public virtual double this[Ordinate ordinate]
        {
            get
            {
                switch (ordinate)
                {
                    case Ordinate.X:
                        return X;

                    case Ordinate.Y:
                        return Y;

                    case Ordinate.Z:
                        return Z;

                    case Ordinate.M:
                        return M;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(ordinate), ordinate, "Coordinate instances only recognize X, Y, Z, and M ordinates.");
                }
            }

            set
            {
                switch (ordinate)
                {
                    case Ordinate.X:
                        X = value;
                        break;

                    case Ordinate.Y:
                        Y = value;
                        break;

                    case Ordinate.Z:
                        Z = value;
                        break;

                    case Ordinate.M:
                        M = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(ordinate), ordinate, "Coordinate instances only recognize X, Y, Z, and M ordinates.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the ordinate value for the given index.
        /// </summary>
        /// <remarks>
        /// The base implementation supports 0 (X) and 1 (Y) as values for the index.
        /// </remarks>
        /// <param name="ordinateIndex">The ordinate index</param>
        /// <returns>The ordinate value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ordinateIndex"/> is not in the valid range.</exception>
        public virtual double this[int ordinateIndex]
        {
            get
            {
                switch (ordinateIndex)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                }
                return double.NaN;
                // disable for now to avoid regression issues
                //throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
            set
            {
                switch (ordinateIndex)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
        }

        /// <summary>
        /// Implicitly cast a tuple to a new <see cref="Coordinate"/> as a copy of this instance.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Coordinate((double x, double y) value)
            => new Coordinate(value.x, value.y);
        
        /// <summary>
        /// Deconstructs this <see cref="Coordinate"/> into its components.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Deconstruct(out double x, out double y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Gets/Sets <c>Coordinate</c>s (x,y,z) values.
        /// </summary>
        public virtual Coordinate CoordinateValue
        {
            get
            {
                return this;
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>Gets a value indicating if the <c>Coordinate</c>
        /// has valid x- and y ordinate values
        /// <para/>
        /// An ordinate value is valid if it is finite.
        /// </summary>
        /// <returns><c>true</c> if the coordinate is valid</returns>
        /// <see cref="double.IsInfinity"/>
        /// <see cref="double.IsNaN"/>
        public bool IsValid 
        {
            get
            {
                if (!/*double.*/IsFinite(X)) return false;
                if (!/*double.*/IsFinite(Y)) return false;
                return true;
            }
        }

        /// <summary>
        /// Predicate to check if a <see cref="double"/> value is finite.
        /// <para/>
        /// It is finite if both <see cref="double.IsInfinity"/> and <see cref="double.IsNaN"/> return <c>false</c>
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns><c>value</c></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFinite(double value) => !double.IsInfinity(value) && !double.IsNaN(value);

        /// <summary>
        /// Returns whether the planar projections of the two <c>Coordinate</c>s are equal.
        ///</summary>
        /// <param name="other"><c>Coordinate</c> with which to do the 2D comparison.</param>
        /// <returns>
        /// <c>true</c> if the x- and y-coordinates are equal;
        /// the Z coordinates do not have to be equal.
        /// </returns>
        public bool Equals2D(Coordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Tests if another Coordinate has the same values for the X and Y ordinates,
        /// within a specified tolerance value.  The Z ordinate is ignored.
        /// </summary>
        /// <param name="c">A <see cref="Coordinate"/>.</param>
        /// <param name="tolerance">The tolerance value to use.</param>
        /// <returns><c>true</c> if the X and Y ordinates are within the given tolerance.</returns>
        /// <remarks>The Z ordinate is ignored.</remarks>
        public bool Equals2D(Coordinate c, double tolerance)
        {
            if (!EqualsWithTolerance(X, c.X, tolerance))
                return false;
            if (!EqualsWithTolerance(Y, c.Y, tolerance))
                return false;
            return true;
        }

        protected static bool EqualsWithTolerance(double v1, double v2, double tolerance)
        {
            return Math.Abs(v1 - v2) <= tolerance;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Coordinate other)
        {
            return Equals2D(other);
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x &lt; other.x || ((this.x == other.x) AND (this.y &lt; other.y))
        ///    0  : this.x == other.x AND this.y = other.y
        ///    1  : this.x &gt; other.x || ((this.x == other.x) AND (this.y &gt; other.y))
        /// </summary>
        /// <param name="o"><c>Coordinate</c> with which this <c>Coordinate</c> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>Coordinate</c>
        ///         is less than, equal to, or greater than the specified <c>Coordinate</c>.
        /// </returns>
        public int CompareTo(object o)
        {
            //Like in JTS
            //return CompareTo((Coordinate) o);

            if (ReferenceEquals(o, null))
                throw new ArgumentNullException(nameof(o));
            if (!(o is Coordinate oc))
                throw new ArgumentException($"Invalid type: '{o.GetType()}'", nameof(o));

            return CompareTo(oc);
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x &lt; other.x || ((this.x == other.x) AND (this.y &lt; other.y))
        ///    0  : this.x == other.x AND this.y = other.y
        ///    1  : this.x &gt; other.x || ((this.x == other.x) AND (this.y &gt; other.y))
        /// </summary>
        /// <param name="other"><c>Coordinate</c> with which this <c>Coordinate</c> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>Coordinate</c>
        ///         is less than, equal to, or greater than the specified <c>Coordinate</c>.
        /// </returns>
        public int CompareTo(Coordinate other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (X < other.X)
                return -1;
            if (X > other.X)
                return 1;
            if (Y < other.Y)
                return -1;
            return Y > other.Y ? 1 : 0;
        }

        /// <summary>
        /// Create a copy of this <see cref="Coordinate"/>.
        /// </summary>
        /// <returns>A copy of this coordinate.</returns>
        public virtual Coordinate Copy()
        {
            return (Coordinate)MemberwiseClone();
        }

        /// <summary>
        /// Create a Coordinate of the same type as this Coordinate, using the provided values.
        /// </summary>
        /// <remarks>Depending on the actual type the following limitations are in place:
        /// <list type="table">
        /// <listheader><term>Coordinate (Sub-)Class</term><description>Limitation</description></listheader>
        /// <item><term><c>Coordinate</c></term><description><paramref name="z"/>-parameter and <paramref name="m"/>-parameter are silently dropped.</description></item>
        /// <item><term><c>CoordinateZ</c></term><description><paramref name="m"/>-parameter is silently dropped.</description></item>
        /// <item><term><c>CoordinateM</c></term><description><paramref name="z"/>-parameter is silently dropped.</description></item>
        /// <item><term><c>CoordinateZM</c></term><description>No parameter is dropped.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="x">The x-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="y">The y-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="z">The z-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <param name="m">The m-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <returns>A new <see cref="Coordinate"/></returns>
        public virtual Coordinate Create(double x = 0, double y = 0, double z = NullOrdinate, double m = NullOrdinate) => new Coordinate(x, y);

        /// <summary>
        /// Computes the 2-dimensional Euclidean distance to another location.
        /// </summary>
        /// <param name="c">A <see cref="Coordinate"/> with which to do the distance comparison.</param>
        /// <returns>the 2-dimensional Euclidean distance between the locations.</returns>
        /// <remarks>The Z-ordinate is ignored.</remarks>
        public double Distance(Coordinate c)
        {
            double dx = X - c.X;
            double dy = Y - c.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

#region System.Object overrides
        /// <summary>
        /// Returns <c>true</c> if <c>other</c> has the same values for the x and y ordinates.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// </summary>
        /// <param name="o"><c>Coordinate</c> with which to do the comparison.</param>
        /// <returns><c>true</c> if <c>other</c> is a <c>Coordinate</c> with the same values for the x and y ordinates.</returns>
        public sealed override bool Equals(object o)
        {
            if (o is Coordinate other)
                return Equals(other);
            return false;
        }

        /// <summary>
        /// Gets a hashcode for this coordinate.
        /// </summary>
        /// <returns>A hashcode for this coordinate.</returns>
        public sealed override int GetHashCode()
        {
            int result = 17;
            // ReSharper disable NonReadonlyFieldInGetHashCode
            result = 37 * result + X.GetHashCode();
            result = 37 * result + Y.GetHashCode();
            // ReSharper restore NonReadonlyFieldInGetHashCode
            return result;
        }

        /// <summary>
        /// Returns a <c>string</c> of the form <I>(x,y,z)</I> .
        /// </summary>
        /// <returns><c>string</c> of the form <I>(x,y,z)</I></returns>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "({0:R}, {1:R})", X, Y);
        }
#endregion
    }
}
