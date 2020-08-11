using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.SnapRound
{
    internal class PolygonCleaner
    {
        private static readonly PolygonCleanerTransformer Transformer = new PolygonCleanerTransformer();

        public static Geometry Clean(Geometry geom)
        {
            return Transformer.Transform(geom);
        }

        public class PolygonCleanerTransformer : GeometryTransformer
        {
            /// <inheritdoc cref="GeometryTransformer.TransformPolygon"/>
            protected override Geometry TransformPolygon(Polygon geom, Geometry parent)
            {
                // if parent is a MultiPolygon, let it do the cleaning
                if (parent is MultiPolygon) {
                    return geom;
                }
                return CreateValidArea(geom);
            }

            /// <inheritdoc cref="GeometryTransformer.TransformMultiPolygon"/>
            protected override Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent)
            {
                var roughGeom = base.TransformMultiPolygon(geom, parent);
                return CreateValidArea(roughGeom);
            }

            /// <summary>
            /// Creates a valid area geometry from one that possibly has bad topology (i.e.
            /// self-intersections).
            /// </summary>
            /// <param name="area">An area geometry possibly containing self-intersections</param>
            /// <returns>
            /// A valid area geometry
            /// </returns>
            private Geometry CreateValidArea(Geometry area)
            {
                if (area.IsValid) return area;
                // TODO: this is slow and has potential errors (due to buffer robustness failure)
                // TODO: replace with a proper polygon cleaner
                /*
                 * Creates a valid area geometry from one that possibly has bad topology (i.e.
                 * self-intersections). Since buffer can handle invalid topology, but always
                 * returns valid geometry, constructing a 0-width buffer "corrects" the
                 * topology. Note this only works for area geometries, since buffer always
                 * returns areas. This also may return empty geometries, if the input has no
                 * actual area.
                 */
                //return area;
                return area.Buffer(0.0);
            }
        }
    }
}
