using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Locate
{
    /// <summary>
    /// An interface for classes which determine the <see cref="Location"/> of
    /// points in areal geometries.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IPointOnGeometryLocator
    {
        /// <summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="Geometry"/>.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <returns>The location of the point in the geometry</returns>
        Location Locate(Coordinate p);
    }

    /// <summary>
    /// Static methods for <see cref="IPointOnGeometryLocator"/> classes
    /// </summary>
    public static class PointOnGeometryLocatorExtensions
    {
        /// <summary>
        /// Convenience method to test a point for intersection with a geometry
        /// <para/>
        /// The geometry is wrapped in a <see cref="IPointOnGeometryLocator"/> class.
        /// </summary>
        /// <param name="locator">The locator to use.</param>
        /// <param name="coordinate">The coordinate to test.</param>
        /// <returns><c>true</c> if the point is in the interior or boundary of the geometry.</returns>
        public static bool Intersects(IPointOnGeometryLocator locator, Coordinate coordinate)
        {
            if (locator == null)
                throw new ArgumentNullException("locator");
            if (coordinate == null)
                throw new ArgumentNullException("coordinate");

            switch (locator.Locate(coordinate))
            {
                case Location.Boundary:
                case Location.Interior:
                    return true;

                case Location.Exterior:
                    return false;

                default:
                    throw new InvalidOperationException("IPointOnGeometryLocator.Locate should never return anything other than Boundary, Interior, or Exterior.");
            }
        }
    }
}