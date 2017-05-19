using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    ///     Computes the <see cref="SpatialFunction.Intersection" /> spatial overlay function for a
    ///     target <see cref="PreparedLineString" /> relative to other <see cref="IGeometry" /> classes.
    /// </summary>
    /// <remarks>Uses indexing to improve performance.</remarks>
    /// <author>Martin Davis</author>
    public class PreparedPolygonLineIntersection
    {
        private readonly PreparedPolygon _prepPoly;

        private readonly LineIntersector li = new RobustLineIntersector();

        /// <summary>
        ///     Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">The target PreparedPolygon</param>
        public PreparedPolygonLineIntersection(PreparedPolygon prepPoly)
        {
            _prepPoly = prepPoly;
        }

        /// <summary>
        ///     Computes the intersection between a <see cref="PreparedLineString" /> and a <see cref="IGeometry" />.
        /// </summary>
        /// <param name="prep">The prepared linestring</param>
        /// <param name="geom">A geometry</param>
        /// <returns>The intersection geometry</returns>
        public static IGeometry Intersection(PreparedPolygon prep, IGeometry geom)
        {
            var op = new PreparedPolygonLineIntersection(prep);
            return op.Intersection(geom);
        }

        /// <summary>
        ///     Computes the intersection of this geometry with the given geometry.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>A geometry corresponding to the intersection point set</returns>
        public IGeometry Intersection(IGeometry geom)
        {
            // only handle A/L case for now
            if (!(geom is ILineString))
                return _prepPoly.Geometry.Intersection(geom);

            // TODO: handle multilinestrings
            var pts = geom.Coordinates;
            var lineTopo = new LineTopology(pts, geom.Factory);
            ComputeIntersection(lineTopo);
            return lineTopo.Result;
        }

        private void ComputeIntersection(LineTopology lineTopo)
        {
            var intDetector = new SegmentIntersectionDetector(li);
            intDetector.FindAllIntersectionTypes = true;
            //		prepPoly.getIntersectionFinder().intersects(lineSegStr, intDetector);
        }
    }
}