using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// A base elevation model class.
    /// </summary>
    public class ElevationModel
    {
        private static ElevationModel _default = new ElevationModel();

        private readonly double _z;
        private readonly Envelope _extent;
        private readonly int _srid;

        /// <summary>
        /// Creates an elevation model that always returns <see cref="Coordinate.NullOrdinate"/> as result.
        /// </summary>
        public ElevationModel() : this(Coordinate.NullOrdinate)
        { }

        /// <summary>
        /// Creates an elevation model that always returns <paramref name="z"/> as result.
        /// </summary>
        /// <param name="z">The result value for <see cref="GetZ(Coordinate)"/> or its overloads.</param>
        internal ElevationModel(double z)
        {
            _z = z;
        }

        /// <summary>
        /// Creates an elevation model that always returns <paramref name="z"/> as result.
        /// </summary>
        /// <param name="z">The result value for <see cref="GetZ(Coordinate)"/> or its overloads.</param>
        /// <param name="extent">The extent where this elevation model is valid</param>
        /// <param name="srid">The spatial reference id</param>
        public ElevationModel(double z, Envelope extent, int srid)
        {
            _z = z;
            _extent = extent;
            _srid = srid;
        }

        /// <summary>
        /// Gets or sets a value indicating the default <see cref="ElevationModel"/>
        /// </summary>
        /// <remarks>The value <c>null</c> cannot be assigned to this property, it will be converted to a no-op elevation model.</remarks>
        public static ElevationModel Default
        {
            get => _default;
            set
            {
                if (value == null)
                    value = new ElevationModel();
                _default = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the extent where this elevation model is valid.
        /// </summary>
        /// <remarks>
        /// If this value is <c>null</c>, no check for the validity of the
        /// input arguments for <see cref="GetZ(Coordinate)"/> and its overload is made.
        /// </remarks>
        public Envelope Extent { get => _extent; }

        /// <summary>
        /// Gets a value indicating the spatial reference system this elevation model belongs to.
        /// </summary>
        public int SRID { get => _srid; }

        /// <summary>
        /// Gets the z-ordinate value for a given <paramref name="coordinate"/>.
        /// </summary>
        /// <param name="coordinate">A coordinate to get the z-ordinate value.</param>
        /// <returns>The z-ordinate value</returns>
        public virtual double GetZ(Coordinate coordinate)
        {
            if (_extent != null && !_extent.Contains(coordinate))
                return Coordinate.NullOrdinate;

            return _z;
        }

        /// <summary>
        /// Gets the z-ordinate value for a given pair of <paramref name="x"/> and <paramref name="y"/> ordinates.
        /// </summary>
        /// <param name="x">A x-ordinate value.</param>
        /// <param name="y">A y-ordinate value.</param>
        /// <returns>The z-ordinate value</returns>
        public double GetZ(double x, double y) => GetZ(new Coordinate(x, y));


        /// <summary>
        /// Creates a copy of <paramref name="c"/> that has the z-ordinate value at <paramref name="c"/>.
        /// If the elevation model can't retrieve a 
        /// </summary>
        /// <param name="c">A coordinate</param>
        /// <returns>A copy of <paramref name="c"/> with the z-ordinate value.</returns>
        public Coordinate CopyWithZ(Coordinate c)
        {
            // If it already has a z-ordinate value, return a copy
            if (!double.IsNaN(c.Z)) return c.Copy();

            // Get the z-ordinate value for c. If no z-ordinate value was supplied, return a copy of c
            double z = GetZ(c);
            if (double.IsNaN(z)) return c.Copy();

            int dim = Coordinates.Dimension(c);
            int measures = Coordinates.Measures(c);
            int spatial = Math.Max(3, dim - measures);
            dim = spatial + measures;
            var copy = Coordinates.Create(dim, measures);
            copy.CoordinateValue = c;
            copy.Z = z;
            return copy;
        }
    }

}
