using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Computes a triangulation of each polygon in a {@link Geometry}.
    /// A polygon triangulation is a non-overlapping set of triangles which
    /// cover the polygon and have the same vertices as the polygon.
    /// The priority is on performance rather than triangulation quality,
    /// so that the output may contain many narrow triangles.
    /// <para/>
    /// Holes are handled by joining them to the shell to form a
    /// (self-touching) polygon shell with no holes.
    /// Although invalid, this can be triangulated effectively.
    /// <para/>
    /// For better-quality triangulation use <see cref="ConstrainedDelaunayTriangulator"/>.
    /// </summary>
    /// <seealso cref="ConstrainedDelaunayTriangulator"/>
    /// <author>Martin Davis</author>
    public class PolygonTriangulator
    {
        /// <summary>
        /// Computes a triangulation of each polygon in a geometry.
        /// </summary>
        /// <param name="geom">A geometry containing polygons</param>
        /// <returns>A <c>GeometryCollection</c> containing the polygons</returns>
        public static Geometry Triangulate(Geometry geom)
        {
            var clipper = new PolygonTriangulator(geom);
            return clipper.Compute();
        }

        private readonly GeometryFactory _geomFact;
        private readonly Geometry _inputGeom;

        /// <summary>
        /// Constructs a new triangulator.
        /// </summary>
        /// <param name="inputGeom">The input geometry</param>
        public PolygonTriangulator(Geometry inputGeom)
        {
            _geomFact = new GeometryFactory();
            _inputGeom = inputGeom;
        }

        private Geometry Compute()
        {
            var polys = PolygonExtracter.GetPolygons(_inputGeom);
            var triList = new List<Tri.Tri>();
            foreach (var poly in polys)
            {
                var polyTriList = TriangulatePolygon((Geometries.Polygon)poly);
                triList.AddRange(polyTriList);
            }
            return Tri.Tri.ToGeometry(triList, _geomFact);
        }

        /// <summary>
        /// Computes the triangulation of a single polygon
        /// </summary>
        /// <returns>A list of triangular polygons</returns>
        private IList<Tri.Tri> TriangulatePolygon(Geometries.Polygon poly)
        {
            /*
             * Normalize to ensure that shell and holes have canonical orientation.
             * 
             * TODO: perhaps better to just correct orientation of rings?
             */
            var polyNorm = (Geometries.Polygon)poly.Normalized();
            var polyShell = PolygonHoleJoiner.Join(polyNorm);

            var triList = PolygonEarClipper.Triangulate(polyShell);
            //Tri.validate(triList);

            return triList;
        }

    }
}
