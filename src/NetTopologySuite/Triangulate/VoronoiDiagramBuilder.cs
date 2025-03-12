using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{

    /// <summary>
    /// A utility class which creates Voronoi Diagrams
    /// from collections of points.
    /// The diagram is returned as a <see cref="GeometryCollection"/> of <see cref="Polygon"/>s,
    /// representing the faces of the Voronoi diagram.
    /// /// The faces are clipped to the larger of:
    /// <list type="bullet">
    /// <item>
    ///     <description>an envelope supplied by <see cref="set_ClipEnvelope"/>.</description>
    /// </item>
    /// <item>
    ///     <description>an envelope determined by the input sites.</description>
    /// </item>
    /// </list>
    /// The <tt>userData</tt> attribute of each face <tt>Polygon</tt> is set to
    /// the <tt>Coordinate</tt>  of the corresponding input site.
    /// This allows using a <tt>Map</tt> to link faces to data associated with sites.
    /// </summary>
    /// <author>Martin Davis</author>
    public class VoronoiDiagramBuilder
    {
        private ICollection<Coordinate> _siteCoords;
        private double _tolerance;
        private QuadEdgeSubdivision _subdiv;
        private Envelope _clipEnv;
        private Envelope _diagramEnv;

        /// <summary>
        /// Sets the sites (point or vertices) which will be diagrammed.
        /// All vertices of the given geometry will be used as sites.
        /// </summary>
        /// <param name="geom">geom the geometry from which the sites will be extracted.</param>
        public void SetSites(Geometry geom)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = DelaunayTriangulationBuilder.ExtractUniqueCoordinates(geom);
        }

        /// <summary>
        /// Sets the sites (point or vertices) which will be diagrammed
        /// from a collection of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="coords">a collection of Coordinates.</param>
        public void SetSites(ICollection<Coordinate> coords)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = DelaunayTriangulationBuilder.Unique(CoordinateArrays.ToCoordinateArray(coords));
        }

        /// <summary>
        /// Sets the envelope to clip the diagram to.
        /// The diagram will be clipped to the larger
        /// of this envelope or an envelope surrounding the sites.
        /// </summary>
        /// <remarks>the clip envelope.</remarks>
        public Envelope ClipEnvelope
        {
            set => _clipEnv = value;
        }

        /// <summary>
        /// Sets the snapping tolerance which will be used
        /// to improved the robustness of the triangulation computation.
        /// A tolerance of 0.0 specifies that no snapping will take place.
        /// </summary>
        /// <remarks>tolerance the tolerance distance to use</remarks>
        public double Tolerance
        {
            set => _tolerance = value;
        }

        private void Create()
        {
            if (_subdiv != null) return;

            _diagramEnv = _clipEnv;
            if (_diagramEnv == null)
            {
                /* 
                 * If no user-provided clip envelope, 
                 * create one which encloses all the sites,
                 * with a 50% buffer around the edges.
                 */
                _diagramEnv = DelaunayTriangulationBuilder.Envelope(_siteCoords);
                // add a 50% buffer around the sites envelope
                double expandBy = _diagramEnv.Diameter;
                _diagramEnv.ExpandBy(expandBy);
            }

            var vertices = DelaunayTriangulationBuilder.ToVertices(_siteCoords);
            _subdiv = new QuadEdgeSubdivision(_diagramEnv, _tolerance);
            var triangulator = new IncrementalDelaunayTriangulator(_subdiv);
            /*
             * Avoid creating very narrow triangles along triangulation boundary.
             * These otherwise can cause malformed Voronoi cells.
             */
            triangulator.ForceConvex = false;
            triangulator.InsertSites(vertices);
        }

        /// <summary>
        /// Gets the <see cref="QuadEdgeSubdivision"/> which models the computed diagram.
        /// </summary>
        /// <returns>the subdivision containing the triangulation</returns>
        public QuadEdgeSubdivision GetSubdivision()
        {
            Create();
            return _subdiv;
        }

        /// <summary>
        /// Gets the faces of the computed diagram as a <see cref="GeometryCollection"/>
        /// of <see cref="Polygon"/>s, clipped as specified.
        /// <para/>
        /// The <see cref="Geometry.UserData"/> attribute of each face <see cref="Polygon"/> is set to
        /// the <c>Coordinate</c> of the corresponding input site.
        /// This allows using a <see cref="IDictionary{TKey,TValue}"/> to link faces to data associated with sites.
        /// </summary>
        /// <param name="geomFact">the geometry factory to use to create the output</param>
        /// <returns>a <see cref="GeometryCollection"/> containing the face <see cref="Polygon"/>s of the diagram</returns>
        public GeometryCollection GetDiagram(GeometryFactory geomFact)
        {
            Create();
            var polys = _subdiv.GetVoronoiDiagram(geomFact);

            // clip polys to diagramEnv
            return ClipGeometryCollection(polys, _diagramEnv);
        }

        private static GeometryCollection ClipGeometryCollection(GeometryCollection geom, Envelope clipEnv)
        {
            var clipPoly = geom.Factory.ToGeometry(clipEnv);
            var clipped = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = geom.GetGeometryN(i);
                Geometry result = null;
                // don't clip unless necessary
                if (clipEnv.Contains(g.EnvelopeInternal))
                    result = g;
                else if (clipEnv.Intersects(g.EnvelopeInternal))
                {
                    result = clipPoly.Intersection(g);
                    // keep vertex key info
                    result.UserData = g.UserData;
                }

                if (result != null && !result.IsEmpty)
                {
                    clipped.Add(result);
                }
            }
            return geom.Factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(clipped));
        }
    }
}
