using NetTopologySuite.Geometries;
using System;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// A base elevation model class.
    /// </summary>
    public class ElevationModel
    {
        private static readonly ElevationModel _noZ = new ElevationModel();

        private readonly double _z;
        private readonly Envelope _extent;

        /// <summary>
        /// Creates an elevation model that always returns <see cref="Coordinate.NullOrdinate"/> as result.
        /// </summary>
        public ElevationModel() : this(Coordinate.NullOrdinate)
        { }

        /// <summary>
        /// Creates an elevation model that always returns <paramref name="z"/> as result.
        /// </summary>
        /// <param name="z">The result value for <see cref="GetZ(Coordinate)"/> or its overloads.</param>
        public ElevationModel(double z)
        {
            _z = z;
        }

        /// <summary>
        /// Creates an elevation model that always returns <paramref name="z"/> as result.
        /// </summary>
        /// <param name="z">The result value for <see cref="GetZ(Coordinate)"/> or its overloads.</param>
        /// <param name="extent">The extent where this elevation model is valid</param>
        public ElevationModel(double z, Envelope extent)
        {
            _z = z;
            _extent = extent;
        }

        /// <summary>
        /// Gets or sets a value indicating the default <see cref="ElevationModel"/>
        /// </summary>
        /// <remarks>The value <c>null</c> cannot be assigned to this property, it will be converted to a no-op elevation model.</remarks>
        public static ElevationModel NoZ
        {
            get => _noZ;
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
        /// Gets the z-ordinate value for a given <paramref name="coordinate"/>.
        /// <para/>
        /// For locations outside of <see cref="Extent"/>, <see cref="Coordinate.NullOrdinate"/> is returned.
        /// </summary>
        /// <param name="coordinate">A coordinate to get the z-ordinate value.</param>
        /// <returns>The z-ordinate value</returns>
        public virtual double GetZ(Coordinate coordinate)
        {
            if (!double.IsNaN(coordinate.Z))
                return coordinate.Z;
            return GetZ(coordinate.X, coordinate.Y);
        }

        /// <summary>
        /// Gets the z-ordinate value for a given pair of <paramref name="x"/> and <paramref name="y"/> ordinates.
        /// <para/>
        /// For locations outside of <see cref="Extent"/>, <see cref="Coordinate.NullOrdinate"/> is returned.
        /// </summary>
        /// <param name="x">A x-ordinate value.</param>
        /// <param name="y">A y-ordinate value.</param>
        /// <returns>The z-ordinate value</returns>
        public virtual double GetZ(double x, double y)
        {
            Span<double> xy = stackalloc double[2];
            Span<double> z = stackalloc double[1];
            xy[0] = x;
            xy[1] = y;
            z[0] = double.NaN;
            GetZ(xy, z);

            return z[0];
        }

        /// <summary>
        /// Gets missing <paramref name="z"/>-ordinate values for <paramref name="xy"/>-ordinate pairs.
        /// <para/>
        /// For locations outside of <see cref="Extent"/>, <see cref="Coordinate.NullOrdinate"/> is set.
        /// <para/>
        /// In order to update z-ordinate at index idx <c>double.IsNaN(z[idx])</c> has to be true.
        /// </summary>
        /// <param name="xy">An array of x- and y- ordinates</param>
        /// <param name="z">An array for the missing z-ordinate values</param>
        /// <exception cref="ArgumentException">Thrown if xy span isn't twice the size of z-span</exception>
        public virtual void GetZ(ReadOnlySpan<double> xy, Span<double> z)
        {
            if (xy.Length == 0) return;

            if (xy.Length != 2 * z.Length)
                throw new ArgumentException($"xy-span not twice the size of z-span.");

            for (int i = 0, j = 0; i < z.Length; i+=2, j++)
            {
                if (double.IsNaN(z[j]))
                {
                    z[j] = _extent != null && !_extent.Contains(xy[i], xy[i+1])
                        ? Coordinate.NullOrdinate
                        : _z;
                }
            }
        }
    }
}
