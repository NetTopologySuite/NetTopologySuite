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
            var triangulator = new PolygonTriangulator(geom);
            return triangulator.GetResult();
        }

        private readonly GeometryFactory _geomFact;
        private readonly Geometry _inputGeom;
        private List<Tri.Tri> _triList;

        /// <summary>
        /// Constructs a new triangulator.
        /// </summary>
        /// <param name="inputGeom">The input geometry</param>
        public PolygonTriangulator(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
            _geomFact = _inputGeom.Factory;
        }

        /// <summary>
        /// Gets the triangulation as a <see cref="GeometryCollection"/> of triangular <see cref="Geometries.Polygon"/>s.
        /// </summary>
        /// <returns>A collection of the result triangle polygons</returns>
        public Geometry GetResult()
        {
            Compute();
            return Tri.Tri.ToGeometry(_triList, _geomFact);
        }

        /// <summary>
        /// Gets the triangulation as a list of <see cref="Tri.Tri"/>s.
        /// </summary>
        /// <returns>The list of Tris in the triangulation</returns>
        public List<Tri.Tri> GetTriangles()
        {
            Compute();
            return _triList;
        }

        private void Compute()
        {
            var polys = PolygonExtracter.GetPolygons(_inputGeom);
            _triList = new List<Tri.Tri>();
            foreach (var poly in polys)
            {
                if (poly.IsEmpty) continue;
                var polyTriList = TriangulatePolygon((Geometries.Polygon)poly);
                _triList.AddRange(polyTriList);
            }
        }

        /// <summary>
        /// Computes the triangulation of a single polygon
        /// </summary>
        /// <returns>A list of triangular polygons</returns>
        private IList<Tri.Tri> TriangulatePolygon(Geometries.Polygon poly)
        {
            var polyShell = PolygonHoleJoiner.Join(poly);

            var triList = PolygonEarClipper.Triangulate(polyShell);
            //Tri.validate(triList);

            return triList;
        }

    }
}
