using System;
using System.Globalization;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A lightweight class used to store coordinates on the 2-dimensional Cartesian plane
    /// and additional z- and m-ordinate values (<see cref="CoordinateZ.Z"/>, <see cref="M"/>).
    /// <para>
    /// This data object is suitable for use with coordinate sequences with
    /// <c>dimension</c> = 4 and <c>measures</c> = 1.
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is distinct from <see cref="Point"/>, which is a subclass of <see cref="Geometry"/>.
    /// Unlike objects of type <see cref="Point"/> (which contain additional
    /// information such as an envelope, a precision model, and spatial reference
    /// system information), a <c>CoordinateZM</c> only contains ordinate values
    /// and properties.
    /// <para/>
    /// <c>CoordinateZM</c>s are two-dimensional points, with an additional Z-ordinate.    
    /// If an Z-ordinate value is not specified or not defined,
    /// constructed coordinates have a Z-ordinate of <c>NaN</c>
    /// (which is also the value of <see cref="Coordinate.NullOrdinate"/>).
    /// <para/>
    /// Apart from the basic accessor functions, NTS supports
    /// only specific operations involving the Z- and/or M-ordinate.
    /// <para/>
    /// Implementations may optionally support Z-ordinate and M-measure values
    /// as appropriate for a <see cref="CoordinateSequence"/>. Use of <see cref="CoordinateZ.Z"/>
    /// and <see cref="M"/> setters or <see cref="P:NetTopologySuite.Geometries.CoordinateZM.this[int]" /> indexer are recommended.
    /// </remarks>
    [Serializable]
#pragma warning disable 612,618
    public sealed class CoordinateZM : CoordinateZ
    {
        /// <summary>
        /// Gets or sets the measure-ordinate value.
        /// </summary>
        public override double M { get; set; }

        /// <summary>
        /// Constructs a <c>CoordinateZM</c> at (x,y,z).
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The Measure value</param>
        public CoordinateZM(double x, double y, double z, double m) : base(x, y, z)
        {
            M = m;
        }

        /// <summary>
        ///  Constructs a <c>CoordinateZM</c> at (0,0,NaN,NaN).
        /// </summary>
        public CoordinateZM() : this(0.0, 0.0, NullOrdinate, NullOrdinate) { }

        /// <summary>
        /// Constructs a <c>CoordinateZM</c> having the same (x,y) values as
        /// <paramref name="c"/>.
        /// </summary>
        /// <param name="c"><c>Coordinate</c> to copy.</param>
        public CoordinateZM(Coordinate c) : this(c.X, c.Y, c.Z, c.M) { }

        /// <summary>
        /// Constructs a <c>CoordinateZM</c> at (x,y,NaN).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        public CoordinateZM(double x, double y) : this(x, y, NullOrdinate, NullOrdinate) { }


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
                    case 3:
                        return M;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
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
                    case 3:
                        M = value;
                        return;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
        }

        /// <summary>
        /// Implicit conversion of a <c>Tuple</c> to a <c>CoordinateZM</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CoordinateZM((double x, double y) value)
            => new CoordinateZM(value.x, value.y);

        /// <summary>
        /// Implicit conversion of a <c>Tuple</c> to a <c>CoordinateZM</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CoordinateZM((double x, double y, double z, double m) value)
            => new CoordinateZM(value.x, value.y, value.z, value.m);

        /// <summary>
        /// Deconstructs this <c>CoordinateZM</c> into its x, y, z and values.
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordinate value</param>
        /// <param name="z">The z-ordinate value</param>
        /// <param name="m"></param>
        public void Deconstruct(out double x, out double y, out double z, out double m)
        {
            x = X;
            y = Y;
            z = Z;
            m = M;
        }

        /// <summary>
        /// Gets/Sets <c>CoordinateZM</c>s (x,y,z) values.
        /// </summary>
        public override Coordinate CoordinateValue
        {
            get { return this; }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                M = value.M;
            }
        }

        /// <summary>
        /// Create a Coordinate of the same type as this Coordinate,
        /// using the provided values for <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>  and <paramref name="m"/>.
        /// </summary>
        /// <param name="x">The x-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="y">The y-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="z">The z-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <param name="m">The m-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <returns>A new <see cref="CoordinateZM"/></returns>
        public override Coordinate Create(double x = 0d, double y = 0d, double z = NullOrdinate, double m = NullOrdinate) => new CoordinateZM(x, y, z, m);

        /// <summary>
        /// Returns a <c>string</c> of the form <i>(x, y, z, m=m)</i> .
        /// </summary>
        /// <returns><c>string</c> of the form <i>(x, y, z, m=m)</i></returns>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "({0:R}, {1:R}, {2:R}, m={3:R})", X, Y, Z, M);
        }

        ///// <summary>
        ///// Create a new object as copy of this instance.
        ///// </summary>
        ///// <returns></returns>
        //public override Coordinate Copy()
        //{
        //    return new CoordinateZM(X, Y, Z, M);
        //}
    }
}
