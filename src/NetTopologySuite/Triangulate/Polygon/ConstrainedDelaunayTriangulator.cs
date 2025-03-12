using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Triangulate.Tri;
using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Computes the Constrained Delaunay Triangulation of polygons.
    /// The Constrained Delaunay Triangulation of a polygon is a set of triangles
    /// covering the polygon, with the maximum total interior angle over all
    /// possible triangulations.  It provides the "best quality" triangulation
    /// of the polygon.
    /// <para/>
    /// Holes are supported.
    /// </summary>
    /// <author>Martin Davis</author>
    public class ConstrainedDelaunayTriangulator
    {
        /// <summary>
        /// Computes the Constrained Delaunay Triangulation of each polygon element in a geometry.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <returns>A GeometryCollection of the computed triangle polygons</returns>
        public static Geometry Triangulate(Geometry geom)
        {
            var cdt = new ConstrainedDelaunayTriangulator(geom);
            return cdt.GetResult();
        }

        private readonly GeometryFactory _geomFact;
        private readonly Geometry _inputGeom;
        private List<Tri.Tri> _triList;

        /// <summary>
        /// Constructs a new Constrained Delaunay triangulator.
        /// </summary>
        /// <param name="inputGeom">The input geometry</param>
        public ConstrainedDelaunayTriangulator(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
            _geomFact = inputGeom.Factory;
        }

        /// <summary>
        /// Gets the triangulation as a <see cref="GeometryCollection"/> of triangular <see cref="Polygon"/>s.
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
        /// <returns>The list of <c>Tri</c>s in the triangulation</returns>
        public IList<Tri.Tri> GetTriangles()
        {
            Compute();
            return _triList;
        }

        private void Compute()
        {
            if (_triList != null) return;

            var polys = PolygonExtracter.GetPolygons(_inputGeom);
            _triList = new List<Tri.Tri>();
            foreach (var poly in polys)
            {
                var polyTriList = TriangulatePolygon((Geometries.Polygon)poly);
                _triList.AddRange(polyTriList);
            }
        }

        /// <summary>
        /// Computes the triangulation of a single polygon
        /// and returns it as a list of <see cref="Tri.Tri"/>s.
        /// </summary>
        /// <param name="poly">The input polygon</param>
        /// <returns>A list of <c>Tri</c>s forming the triangulation</returns>
        private IList<Tri.Tri> TriangulatePolygon(Geometries.Polygon poly)
        {
            var polyShell = PolygonHoleJoiner.Join(poly);
            var triList = PolygonEarClipper.Triangulate(polyShell);

            //long start = System.currentTimeMillis();
            TriangulationBuilder.Build(triList);
            TriDelaunayImprover.Improve(triList);
            //System.out.println("swap used: " + (System.currentTimeMillis() - start) + " milliseconds");

            return triList;
        }

    }

}
