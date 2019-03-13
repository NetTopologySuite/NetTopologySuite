using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes an interior point of a <see cref="IGeometry"/>.
    /// An interior point is guaranteed to lie in the interior of the Geometry,
    /// if it possible to calculate such a point exactly.
    /// Otherwise, the point may lie on the boundary of the geometry.
    /// <para>
    /// The interior point of an empty geometry is <code>POINT EMPTY</code>.
    /// </para>
    /// </summary>
    public static class InteriorPoint
    {
        public static IPoint GetInteriorPoint(IGeometry geom)
        {
            var factory = geom.Factory;

            if (geom.IsEmpty)
            {
                return CreatePointEmpty(factory);
            }

            Coordinate interiorPt = null;
            switch (geom.Dimension)
            {
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

            return CreatePointPrecise(factory, interiorPt);
        }

        private static IPoint CreatePointEmpty(IGeometryFactory factory)
        {
            return factory.CreatePoint();
        }

        private static IPoint CreatePointPrecise(IGeometryFactory factory, Coordinate coord)
        {
            factory.PrecisionModel.MakePrecise(coord);
            return factory.CreatePoint(coord);
        }
    }
}
