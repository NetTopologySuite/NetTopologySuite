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
    /// constructed coordinates have a M-ordinate of <code>NaN</code>
    /// (which is also the value of <see cref="Coordinate.NullOrdinate"/>).
    /// Apart from the basic accessor functions, NTS supports
    /// only specific operations involving the M-ordinate.
    /// <para/>
    /// Implementations may optionally support Z-ordinate and M-measure values
    /// as appropriate for a <see cref="ICoordinateSequence"/>. Use of <see cref="CoordinateZ.Z"/>
    /// and <see cref="M"/> setters or <see cref="P:NetTopologySuite.Geometries.CoordinateM.this[Ordinate]" /> indexer are recommended.
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
        /// <para>
        /// Do not use <see cref="Ordinate.M"/> to try to get or set the value of <see cref="M"/>.
        /// Use <see cref="Ordinate.Ordinate2"/> instead.
        /// </para>
        /// <para>
        /// <see cref="Ordinate.M"/> is the "standard" name of <see cref="Ordinate.Ordinate3"/> in
        /// the XYZM space.  Since this does not have Z, the "standard" names of everything after it
        /// (i.e., M) shift down by 1.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The base implementation supports <see cref="Ordinate.X"/>, <see cref="Ordinate.Y"/> and <see cref="Ordinate.Ordinate2"/> as values for the index.
        /// </remarks>
        /// <param name="ordinateIndex">The ordinate index</param>
        /// <returns>The ordinate value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ordinateIndex"/> is not in the valid range.</exception>
        public override double this[Ordinate ordinateIndex]
        {
            get
            {
                switch (ordinateIndex)
                {
                    case Ordinate.X:
                        return X;
                    case Ordinate.Y:
                        return Y;
                    case Ordinate.Ordinate2:
                        return M;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
            set
            {
                switch (ordinateIndex)
                {
                    case Ordinate.X:
                        X = value;
                        return;
                    case Ordinate.Y:
                        Y = value;
                        return;
                    case Ordinate.Ordinate2:
                        M = value;
                        return;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
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
        /// Returns a <c>string</c> of the form <i>(x, y, m=m)</i>.
        /// </summary>
        /// <returns><c>string</c> of the form <i>(x, y, m=m)</i></returns>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "({0:R}, {1:R}, m={2:R})", X, Y, M);
        }

        ///// <summary>
        ///// Create a new object as copy of this instance.
        ///// </summary>
        ///// <returns></returns>
        //public override Coordinate Copy()
        //{
        //    return new CoordinateM(X, Y, M);
        //}
    }
}