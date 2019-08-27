using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// A utility class which creates Delaunay Triangulations
    /// from collections of points and extract the resulting
    /// triangulation edges or triangles as geometries.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DelaunayTriangulationBuilder
    {
        /// <summary>
        /// Extracts the unique <see cref="Coordinate"/>s from the given <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geom">the geometry to extract from</param>
        /// <returns>a List of the unique Coordinates</returns>
        public static CoordinateList ExtractUniqueCoordinates(Geometry geom)
        {
            if (geom == null)
                return new CoordinateList();

            var coords = geom.Coordinates;
            return Unique(coords);
        }

        public static CoordinateList Unique(Coordinate[] coords)
        {
            var coordsCopy = CoordinateArrays.CopyDeep(coords);
            Array.Sort(coordsCopy);
            var coordList = new CoordinateList(coordsCopy, false);
            return coordList;
        }

        /// <summary>
        /// Converts all <see cref="Coordinate"/>s in a collection to <see cref="Vertex"/>es.
        /// </summary>
        /// <param name="coords">the coordinates to convert</param>
        /// <returns>a List of Vertex objects</returns>
        public static IList<Vertex> ToVertices(ICollection<Coordinate> coords)
        {
            var verts = new List<Vertex>();
            foreach (var coord in coords)
            {
                verts.Add(new Vertex(coord));
            }
            return verts;
        }

        /// <summary>
        /// Computes the <see cref="Envelope"/> of a collection of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="coords">a List of Coordinates</param>
        /// <returns>the envelope of the set of coordinates</returns>
        public static Envelope Envelope(ICollection<Coordinate> coords)
        {
            var env = new Envelope();
            foreach (var coord in coords)
            {
                env.ExpandToInclude(coord);
            }
            return env;
        }

        private ICollection<Coordinate> _siteCoords;
        private double _tolerance;
        private QuadEdgeSubdivision _subdiv;

        /// <summary>
        /// Sets the sites (vertices) which will be triangulated.
        /// All vertices of the given geometry will be used as sites.
        /// </summary>
        /// <param name="geom">the geometry from which the sites will be extracted.</param>
        public void SetSites(Geometry geom)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = ExtractUniqueCoordinates(geom);
        }

        /// <summary>
        /// Sets the sites (vertices) which will be triangulated
        /// from a collection of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="coords">a collection of Coordinates.</param>
        public void SetSites(ICollection<Coordinate> coords)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = Unique(CoordinateArrays.ToCoordinateArray(coords));
        }

        /// <summary>
        /// Sets the snapping tolerance which will be used
        /// to improved the robustness of the triangulation computation.
        /// A tolerance of 0.0 specifies that no snapping will take place.
        /// </summary>
        ///// <param name="tolerance">the tolerance distance to use</param>
        public double Tolerance
        {
            set => _tolerance = value;
        }

        private void Create()
        {
            if (_subdiv != null) return;

            var siteEnv = Envelope(_siteCoords);
            var vertices = ToVertices(_siteCoords);
            _subdiv = new QuadEdgeSubdivision(siteEnv, _tolerance);
            var triangulator = new IncrementalDelaunayTriangulator(_subdiv);
            triangulator.InsertSites(vertices);
        }

        /// <summary>
        /// Gets the <see cref="QuadEdgeSubdivision"/> which models the computed triangulation.
        /// </summary>
        /// <returns>the subdivision containing the triangulation</returns>
        public QuadEdgeSubdivision GetSubdivision()
        {
            Create();
            return _subdiv;
        }

        /// <summary>
        /// Gets the edges of the computed triangulation as a <see cref="MultiLineString"/>.
        /// </summary>
        /// <param name="geomFact">the geometry factory to use to create the output</param>
        /// <returns>the edges of the triangulation</returns>
        public MultiLineString GetEdges(GeometryFactory geomFact)
        {
            Create();
            return _subdiv.GetEdges(geomFact);
        }

        /// <summary>
        /// Gets the faces of the computed triangulation as a <see cref="GeometryCollection"/>
        /// of <see cref="Polygon"/>.
        /// </summary>
        /// <param name="geomFact">the geometry factory to use to create the output</param>
        /// <returns>the faces of the triangulation</returns>
        public GeometryCollection GetTriangles(GeometryFactory geomFact)
        {
            Create();
            return _subdiv.GetTriangles(geomFact);
        }
    }
}
