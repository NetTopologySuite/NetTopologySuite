using System;
using System.Globalization;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A lightweight class used to store coordinates on the 2-dimensional Cartesian plane
    /// and an additional measure (<see cref="M"/>) value.
    /// <para>
    /// This data object is suitable for use with coordinate sequences with
    /// <c>dimension</c> = 3 and <c>measures</c> = 1.
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is distinct from <see cref="Point"/>, which is a subclass of <see cref="Geometry"/>.
    /// Unlike objects of type <see cref="Point"/> (which contain additional
    /// information such as an envelope, a precision model, and spatial reference
    /// system information), a <c>CoordinateM</c> only contains ordinate values
    /// and properties.
    /// <para/>
    /// <c>CoordinateM</c>s are two-dimensional points, with an additional M-ordinate.    
    /// If an M-ordinate value is not specified or not defined,
    /// constructed coordinates have a M-ordinate of <c>NaN</c>
    /// (which is also the value of <see cref="Coordinate.NullOrdinate"/>).
    /// Apart from the basic accessor functions, NTS supports
    /// only specific operations involving the M-ordinate.
    /// <para/>
    /// Implementations may optionally support Z-ordinate and M-measure values
    /// as appropriate for a <see cref="CoordinateSequence"/>. Use of <see cref="CoordinateZ.Z"/>
    /// and <see cref="M"/> setters or <see cref="P:NetTopologySuite.Geometries.CoordinateM.this[int]" /> indexer are recommended.
    /// </remarks>
    [Serializable]
#pragma warning disable 612,618
    public sealed class CoordinateM : Coordinate
    {
        /// <summary>
        /// Gets or sets the M-ordinate value.
        /// </summary>
        public override double M { get; set; }

        /// <summary>
        /// Constructs a <c>CoordinateM</c> at (x,y,z).
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="m">The measure value</param>
        public CoordinateM(double x, double y, double m) : base(x, y)
        {
            M = m;
        }

        /// <summary>
        ///  Constructs a <c>CoordinateM</c> at (0,0,NaN).
        /// </summary>
        public CoordinateM() : this(0.0, 0.0, NullOrdinate) { }

        /// <summary>
        /// Constructs a <c>CoordinateM</c> having the same (x,y) values as
        /// <paramref name="c"/>.
        /// </summary>
        /// <param name="c"><c>Coordinate</c> to copy.</param>
        public CoordinateM(Coordinate c) : this(c.X, c.Y, c.M) { }

        /// <summary>
        /// Constructs a <c>CoordinateM</c> at (x,y,NaN).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        public CoordinateM(double x, double y) : this(x, y, NullOrdinate) { }

        /// <summary>
        /// Gets or sets the ordinate value for the given index.
        /// </summary>
        /// <remarks>
        /// The base implementation supports 0 (X), 1 (Y) and 2 (M) as values for the index.
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
                        return M;
                }
                return double.NaN;
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
                    case 2:
                        M = value;
                        return;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
        }

        /// <summary>
        /// Implicit conversion of a <c>(double x, double y)</c> value to a <c>CoordinateM</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CoordinateM((double x, double y) value)
            => new CoordinateM(value.x, value.y);
        
        /// <summary>
        /// Implicit conversion of a <c>(double x, double y, double m)</c> value to a <c>CoordinateM</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CoordinateM((double x, double y, double m) value)
            => new CoordinateM(value.x, value.y, value.m);

        /// <summary>
        /// Deconstructs this <c>CoordinateM</c> into its x, y and m values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="m"></param>
        public void Deconstruct(out double x, out double y, out double m)
        {
            x = X;
            y = Y;
            m = M;
        }

        /// <summary>
        /// Gets/Sets <c>CoordinateM</c>s (x,y,z) values.
        /// </summary>
        public override Coordinate CoordinateValue
        {
            get { return this; }
            set
            {
                X = value.X;
                Y = value.Y;
                M = value.M;
            }
        }

        /// <summary>
        /// Create a Coordinate of the same type as this Coordinate,
        /// using the provided values for <paramref name="x"/>, <paramref name="y"/> and <paramref name="m"/>.
        /// </summary>
        /// <remarks>A provided value for <paramref name="z"/> will be silently dropped.</remarks>
        /// <param name="x">The x-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="y">The y-ordinate value, if not provided, it is <c>0d</c>.</param>
        /// <param name="z">The z-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <param name="m">The m-ordinate value, if not provided, it is <see cref="Coordinate.NullOrdinate"/>.</param>
        /// <returns>A new <see cref="CoordinateM"/></returns>
        public override Coordinate Create(double x = 0d, double y = 0d, double z = NullOrdinate, double m = NullOrdinate) => new CoordinateM(x, y, m);

        /// <summary>
        /// Returns a <c>string</c> of the form <i>(x, y, m=m)</i>.
        /// </summary>
        /// <returns><c>string</c> of the form <i>(x, y, m=m)</i></returns>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "({0:R}, {1:R}, m={2:R})", X, Y, M);
        }
    }
}
