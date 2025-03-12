using System;
using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes an interior point of a <see cref="Geometry"/>.
    /// An interior point is guaranteed to lie in the interior of the Geometry,
    /// if it possible to calculate such a point exactly.
    /// For collections the interior point is computed for the collection of
    /// non-empty elements of highest dimension.
    /// Otherwise, the point may lie on the boundary of the geometry.
    /// <para/>
    /// The interior point of an empty geometry is <c>POINT EMPTY</c>.
    /// <h2>Algorithm</h2>
    /// The point is chosen to be "close to the center" of the geometry.
    /// The location depends on the dimension of the input:
    /// <list type="bullet">
    /// <item><term>Dimension 2</term><description>the interior point is constructed in the middle of the longest interior segment
    /// of a line bisecting the area.</description></item>
    /// <item><term>Dimension 1</term><description>the interior point is the interior or boundary vertex closest to the centroid.</description></item>
    /// <item><term>Dimension 0</term><description>the point is the point closest to the centroid.</description></item>
    /// </list>
    /// <see cref="Centroid"/>
    /// <see cref="MaximumInscribedCircle"/>
    /// <see cref="LargestEmptyCircle"/>
    /// </summary>
    public static class InteriorPoint
    {
        /// <summary>
        /// Computes a location of an interior point in a <see cref="Geometry"/>.
        /// <para/>
        /// Handles all geometry types.
        /// </summary>
        /// <param name="geom">A geometry in which to find an interior point</param>
        /// <returns>the location of an interior point, or <c>POINT EMPTY</c> if the input is empty
        /// </returns>
        [Obsolete("Use GetInteriorCoord")]
        public static Point GetInteriorPoint(Geometry geom)
        {
            if (geom == null)
                throw new ArgumentException();

            var interiorCoord = GetInteriorCoord(geom);
            return interiorCoord != null
                ? geom.Factory.CreatePoint(interiorCoord)
                : geom.Factory.CreatePoint();
        }

        /// <summary>
        /// Computes a location of an interior point in a <see cref="Geometry"/>.
        /// <para/>
        /// Handles all geometry types.
        /// </summary>
        /// <remarks>
        /// This function is called <c>GetInteriorPoint</c> in JTS.
        /// It has been renamed to <c>GetInteriorCoord</c> to prevent a breaking change.</remarks>
        /// <param name="geom">A geometry in which to find an interior point</param>
        /// <returns>the location of an interior point, or <c>null</c> if the input is empty
        /// </returns>
        public static Coordinate GetInteriorCoord(Geometry geom)
        {
            if (geom.IsEmpty)
                return null;

            Coordinate interiorPt;
            //var dim = geom.Dimension;
            var dim = DimensionNonEmpty(geom);
            switch (dim)
            {
                case Dimension.False:
                    // This should not happen, but just in case.
                    return null;

                case Dimension.Point:
                    interiorPt = InteriorPointPoint.GetInteriorPoint(geom);
                    break;

                case Dimension.Curve:
                    interiorPt = InteriorPointLine.GetInteriorPoint(geom);
                    break;

                default:
                    interiorPt = InteriorPointArea.GetInteriorPoint(geom);
                    break;
            }

            return interiorPt;
        }

        private static Dimension DimensionNonEmpty(Geometry geom)
        {
            var dimFilter = new DimensionNonEmptyFilter();
            geom.Apply(dimFilter);
            return dimFilter.Dimension;
        }

        private class DimensionNonEmptyFilter : IGeometryFilter
        {
            private Dimension _dim = Geometries.Dimension.False;

            public Dimension Dimension { get => _dim; }


            public void Filter(Geometry elem)
            {
                if (elem is GeometryCollection)
                    return;
                if (!elem.IsEmpty)
                {
                    var elemDim = elem.Dimension;
                    if (elemDim > _dim) _dim = elemDim;
                }
            }
        }

}
}
