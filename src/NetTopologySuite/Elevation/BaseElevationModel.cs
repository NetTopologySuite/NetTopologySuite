using NetTopologySuite.Geometries;
using System;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Elevation
{
    /// <summary>
    /// Base implementation of an elevation model
    /// </summary>
    public abstract class BaseElevationModel : IElevationModel
    {
        /// <summary>
        /// Creates an instance of this elevation model class
        /// </summary>
        /// <param name="srid">The id of the spatial reference system in which x- and y-ordinates have to query for z-ordinate values</param>
        /// <param name="extent">The extent this elevation model covers</param>
        public BaseElevationModel(int srid, Envelope extent)
        {
            SRID = srid;
            Extent = extent;
        }

        /// <summary>
        /// Gets a value indicating the spatial reference id for the elevation model
        /// </summary>
        public int SRID { get; }

        /// <inheritdoc>/>
        public Envelope Extent { get; }

        /// <inheritdoc/>
        public virtual double GetZ(double x, double y, out bool success)
        {
            if (!Extent.Intersects(x, y))
            {
                success = false;
                return Coordinate.NullOrdinate;
            }

            success = true;
            return GetZValue(x, y);
        }

        /// <summary>
        /// Function to get the z-ordinate value for the point at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordiante value</param>
        /// <returns></returns>
        protected abstract double GetZValue(double x, double y);

        /// <summary>
        /// Gets the z-ordinate value for the given coordinate <paramref name="p"/>
        /// </summary>
        /// <param name="p">The point for which to get the z-ordinate value</param>
        /// <param name="success">A flag indicating if the returned z-ordinate value is actually useful</param>
        public double GetZ(Coordinate p, out bool success)
        {
            if (p == null)
                throw new ArgumentNullException("p is null.", nameof(p));

            success = false;
            return HasValidZ(p) ? p.Z : GetZ(p.X, p.Y, out success);

        }

        /// <inheritdoc/>
        public abstract void PopulateZ(Geometry geom);

        /// <summary>
        /// Check if coordinate <paramref name="p"/> has a valid z-ordinate value
        /// </summary>
        /// <param name="p">The coordinate</param>
        /// <returns><c>true</c> if <c>p.Z != double.NaN</c></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool HasValidZ(Coordinate p) => IsValidZ(p.Z);

        /// <summary>
        /// Check if <paramref name="z"/> is a valid z-ordinate value
        /// </summary>
        /// <param name="z">A z-ordinate value</param>
        /// <returns><c>true</c> if <c>z != double.NaN</c></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsValidZ(double z) => !double.IsNaN(z);
    }
}
