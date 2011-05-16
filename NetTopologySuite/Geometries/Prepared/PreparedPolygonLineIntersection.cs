using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    /**
     * Computes the <tt>intersection</tt> spatial overlay function
     * for a target {@link PreparedLineString} relative to other {@link Geometry} classes.
     * Uses indexing to improve performance. 
     * 
     * @author Martin Davis
     *
     */
    public class PreparedPolygonLineIntersection
    {
        /**
         * Computes the intersection between a {@link PreparedLineString}
         * and a {@link Geometry}.
         * 
         * @param prep the prepared linestring
         * @param geom a test geometry
         * @return the intersection geometry
         */
        public static IGeometry Intersection(PreparedPolygon prep, IGeometry geom)
        {
            PreparedPolygonLineIntersection op = new PreparedPolygonLineIntersection(prep);
            return op.Intersection(geom);
        }

        protected PreparedPolygon prepPoly;

        /**
         * Creates an instance of this operation.
         * 
         * @param prepPoly the target PreparedPolygon
         */
        public PreparedPolygonLineIntersection(PreparedPolygon prepPoly)
        {
            this.prepPoly = prepPoly;
        }

        /**
         * Computes the intersection of this geometry with the given geometry.
         * 
         * @param geom the test geometry
         * @return a geometry corresponding to the intersection point set
         */
        public IGeometry Intersection(IGeometry geom)
	{
		// only handle A/L case for now
		if (! (geom is ILineString))
				return prepPoly.Geometry.Intersection(geom);
		
		// TODO: handle multilinestrings
		ICoordinate[] pts = geom.Coordinates;
		LineTopology lineTopo = new LineTopology(pts, geom.Factory);
		computeIntersection(lineTopo);
		return lineTopo.Result;

	}

        LineIntersector li = new RobustLineIntersector();

        private void computeIntersection(LineTopology lineTopo)
        {
            SegmentIntersectionDetector intDetector = new SegmentIntersectionDetector(li);
            intDetector.FindAllIntersectionTypes = true;
            //		prepPoly.getIntersectionFinder().intersects(lineSegStr, intDetector);
        }


    }
}