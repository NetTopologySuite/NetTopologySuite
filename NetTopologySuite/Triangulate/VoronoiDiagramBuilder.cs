using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{

    /// <summary>
    /// A utility class which creates Voronoi Diagrams
    /// from collections of points.
    /// The diagram is returned as a <see cref="IGeometryCollection"/> of <see cref="Polygon"/>s,
    /// clipped to the larger of a supplied envelope or to an envelope determined
    /// by the input sites.
    /// </summary>
    /// <author>Martin Davis</author>
    public class VoronoiDiagramBuilder
    {
        private ICollection<ICoordinate> siteCoords;
        private double tolerance = 0.0;
        private QuadEdgeSubdivision subdiv = null;
        private Envelope clipEnv = null;
        private Envelope diagramEnv = null;

        /// <summary>
        /// Creates a new Voronoi diagram builder.
        ///
        ///
        public VoronoiDiagramBuilder()
        {
        }

        /// <summary>
        /// Sets the sites (point or vertices) which will be diagrammed.
        /// All vertices of the given geometry will be used as sites.
        /// </summary>
        /// <param name="geom">geom the geometry from which the sites will be extracted.</param>
        public void SetSites(IGeometry geom)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            siteCoords = DelaunayTriangulationBuilder.ExtractUniqueCoordinates(geom);
        }

        /// <summary>
        /// Sets the sites (point or vertices) which will be diagrammed
        /// from a collection of <see cref="ICoordinate"/>s.
        /// </summary>
        /// <param name="geom">a collection of Coordinates.</param>
        public void SetSites(ICollection<ICoordinate> geom)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            siteCoords = DelaunayTriangulationBuilder.Unique(CoordinateArrays.ToCoordinateArray(geom));
        }

        /// <summary>
        /// Sets the envelope to clip the diagram to.
        /// The diagram will be clipped to the larger
        /// of this envelope or an envelope surrounding the sites.
        /// </summary>
        /// <remarks>the clip envelope.</remarks>
        public Envelope ClipEnvelope
        {
            set
            {
                this.clipEnv = value;
            }
        }

        /// <summary>
        /// Sets the snapping tolerance which will be used
        /// to improved the robustness of the triangulation computation.
        /// A tolerance of 0.0 specifies that no snapping will take place.
        /// </summary>
        /// <remarks>tolerance the tolerance distance to use</remarks>
        public double Tolerance
        {
            set
            {
                this.tolerance = value;
            }
        }

        private void Create()
        {
            if (subdiv != null) return;

            Envelope siteEnv = DelaunayTriangulationBuilder.Envelope(siteCoords);
            diagramEnv = siteEnv;
            // add a buffer around the final envelope
            double expandBy = Math.Max(diagramEnv.Width, diagramEnv.Height);
            diagramEnv.ExpandBy(expandBy);
            if (clipEnv != null)
                diagramEnv.ExpandToInclude(clipEnv);

            var vertices = DelaunayTriangulationBuilder.ToVertices(siteCoords);
            subdiv = new QuadEdgeSubdivision(siteEnv, tolerance);
            IncrementalDelaunayTriangulator triangulator = new IncrementalDelaunayTriangulator(subdiv);
            triangulator.InsertSites(vertices);
        }

        /// <summary>
        /// Gets the <see cref="QuadEdgeSubdivision"/> which models the computed diagram.
        /// </summary>
        /// <returns>the subdivision containing the triangulation</returns>
        public QuadEdgeSubdivision GetSubdivision()
        {
            Create();
            return subdiv;
        }

        /// <summary>
        /// Gets the faces of the computed diagram as a <see cref="IGeometryCollection"/> 
        /// of <see cref="Polygon"/>s, clipped as specified.
        /// </summary>
        /// <param name="geomFact">the geometry factory to use to create the output</param>
        /// <returns>the faces of the diagram</returns>
        public IGeometryCollection GetDiagram(IGeometryFactory geomFact)
        {
            Create();
            IGeometryCollection polys = subdiv.GetVoronoiDiagram(geomFact);

            // clip polys to diagramEnv
            return ClipGeometryCollection(polys, diagramEnv);
        }

        private static IGeometryCollection ClipGeometryCollection(IGeometryCollection geom, Envelope clipEnv)
        {
            IGeometry clipPoly = geom.Factory.ToGeometry(clipEnv);
            var clipped = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                IGeometry g = geom.GetGeometryN(i);
                IGeometry result = null;
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