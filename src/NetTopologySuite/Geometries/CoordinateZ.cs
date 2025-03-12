using System;
using System.Globalization;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A lightweight class used to store coordinates on the 2-dimensional Cartesian plane
    /// and an additional z-ordinate (<see cref="Z"/>) value.
    /// <para>
    /// This base data object is suitable for use with coordinate sequences with
    /// <c>dimension</c> = 3 and <c>measures</c> = 0.
    /// </para>
    /// </summary><remarks>
    /// It is distinct from <see cref="Point"/>, which is a subclass of <see cref="Geometry"/>.
    /// Unlike objects of type <see cref="Point"/> (which contain additional
    /// information such as an envelope, a precision model, and spatial reference
    /// system information), a <c>CoordinateZ</c> only contains ordinate values
    /// and properties.
    /// <para/>
    /// <c>CoordinateZ</c>s are two-dimensional points, with an additional Z-ordinate.    
    /// If an Z-ordinate value is not specified or not defined,
    /// constructed coordinates have a Z-ordinate of <c>NaN</c>
    /// (which is also the value of <see cref="Coordinate.NullOrdinate"/>).
    /// <para/>
    /// Apart from the basic accessor functions, NTS supports
    /// only specific operations involving the Z-ordinate.
    /// <para/>
    /// Implementations may optionally support Z-ordinate and M-measure values
    /// as appropriate for a <see cref="CoordinateSequence"/>. Use of <see cref="Z"/>
    /// and <see cref="Coordinate.M"/> setters or <see cref="P:NetTopologySuite.Geometries.CoordinateZ.this[int]" /> indexer are recommended.
    /// </remarks>
    [Serializable]
#pragma warning disable 612,618
    public class CoordinateZ : Coordinate
    {
        /// <summary>
        /// Gets or sets the Z-ordinate value.
        /// </summary>
        public sealed override double Z { get; set; }

        /// <summary>
        /// Constructs a <c>CoordinateZ</c> at (x,y,z).
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        public CoordinateZ(double x, double y, double z) : base(x, y)
        {
            Z = z;
        }

        /// <summary>
        ///  Constructs a <c>CoordinateZ</c> at (0,0,NaN).
        /// </summary>
        public CoordinateZ() : this(0.0, 0.0, NullOrdinate) { }

        /// <summary>
        /// Constructs a <c>CoordinateZ</c> having the same (x,y) values as
        /// <paramref name="c"/>.
        /// </summary>
        /// <param name="c"><c>Coordinate</c> to copy.</param>
        public CoordinateZ(Coordinate c) : this(c.X, c.Y, c.Z) { }

        /// <summary>
        /// Constructs a <c>CoordinateZ</c> at (x,y,NaN).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        public CoordinateZ(double x, double y) : this(x, y, NullOrdinate) { }

        /// <summary>
        /// Gets or sets the ordinate value for the given index.
        /// </summary>
        /// <remarks>
        /// The base implementation supports 0 (X), 1 (Y) and 2 (Z) as values for the index.
        /// </remarks>
        /// <param name="ordinateIndex">The ordinate index</param>
        /// <returns>The ordinate value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ordinateIndex"/> is not in the valid range.</exception>
        public override double this[int ordinateIndex]
        {
            get
            {
                switch (ordinateIndex)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                }
                //throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
                return double.NaN;
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
                    case 2:
                        Z = value;
                        return;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
        }

        /// <summary>
        /// Implicit conversion of a <c>Tuple</c> to a <c>CoordinateZ</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CoordinateZ((double x, double y) value)
            => new CoordinateZ(value.x, value.y);

        /// <summary>
        /// Implicit conversion of a <c>Tuple</c> to a <c>CoordinateZ</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CoordinateZ((double x, double y, double z) value)
            => new CoordinateZ(value.x, value.y, value.z);
        
        /// <summary>
        /// Deconstructs this <c>CoordinateZ</c> into its x, y and z values.
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordinate value</param>
        /// <param name="z">The z-ordinate value</param>
        public void Deconstruct(out double x, out double y, out double z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Gets/Sets <c>CoordinateZ</c>s (x,y,z) values.
        /// </summary>
        public override Coordinate CoordinateValue
        {
            get { return this; }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        /// <summary>
        /// Create a Coordinate of the same type as this Coordinate,
        /// using the provided values for <paramref name="x"/>, <paramref name="y"/> and <paramref name="z"/>.
        /// </summary>
        /// <remarks>A provided value for <paramref name="m"/> will be silently dropped.</remarks>
        /// <param name="x">The x-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="y">The y-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="z">The z-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <param name="m">The m-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <returns>A new <see cref="CoordinateZ"/></returns>
        public override Coordinate Create(double x = 0d, double y = 0d, double z = NullOrdinate, double m = NullOrdinate) => new CoordinateZ(x, y, z);

        /// <summary>
        /// Returns <c>true</c> if <paramref name="other"/> 
        /// has the same values for X, Y and Z.
        /// </summary>
        /// <param name="other">A <see cref="CoordinateZ"/> with which to do the 3D comparison.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is a <see cref="CoordinateZ"/> 
        /// with the same values for X, Y and Z.
        /// </returns>
        public bool Equals3D(CoordinateZ other)
        {
            return (X == other.X) && (Y == other.Y) &&
                ((Z == other.Z) || (double.IsNaN(Z) && double.IsNaN(other.Z)));
        }

        /// <summary>
        /// Tests if another CoordinateZ has the same value for Z, within a tolerance.
        /// </summary>
        /// <param name="c">A <see cref="CoordinateZ"/>.</param>
        /// <param name="tolerance">The tolerance value.</param>
        /// <returns><c>true</c> if the Z ordinates are within the given tolerance.</returns>
        public bool EqualInZ(CoordinateZ c, double tolerance)
        {
            return EqualsWithTolerance(this.Z, c.Z, tolerance);
        }

        /// <summary>
        /// Returns a <c>string</c> of the form <i>(x, y, z)</i> .
        /// </summary>
        /// <returns><c>string</c> of the form <i>(x, y, z)</i></returns>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "({0:R}, {1:R}, {2:R})", X, Y, Z);
        }

        ///// <summary>
        ///// Create a new object as copy of this instance.
        ///// </summary>
        //public override Coordinate Copy()
        //{
        //    return new CoordinateZ(X, Y, Z);
        //}

        /// <summary>
        /// Computes the 3-dimensional Euclidean distance to another location.
        /// </summary>
        /// <param name="c">A <see cref="CoordinateZ"/> with which to do the distance comparison.</param>
        /// <returns>the 3-dimensional Euclidean distance between the locations.</returns>
        public double Distance3D(CoordinateZ c)
        {
            double dx = X - c.X;
            double dy = Y - c.Y;
            double dz = Z - c.Z;
            if (double.IsNaN(dz)) dz = 0;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
