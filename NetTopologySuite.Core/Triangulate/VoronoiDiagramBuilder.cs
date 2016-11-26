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
        public void SetSites(IGeometry geom)
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
            set
            {
                _clipEnv = value;
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
                _tolerance = value;
            }
        }

        private void Create()
        {
            if (_subdiv != null) return;

            Envelope siteEnv = DelaunayTriangulationBuilder.Envelope(_siteCoords);
            _diagramEnv = siteEnv;
            // add a buffer around the final envelope
            double expandBy = Math.Max(_diagramEnv.Width, _diagramEnv.Height);
            _diagramEnv.ExpandBy(expandBy);
            if (_clipEnv != null)
                _diagramEnv.ExpandToInclude(_clipEnv);

            var vertices = DelaunayTriangulationBuilder.ToVertices(_siteCoords);
            _subdiv = new QuadEdgeSubdivision(siteEnv, _tolerance);
            IncrementalDelaunayTriangulator triangulator = new IncrementalDelaunayTriangulator(_subdiv);
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
        /// Gets the faces of the computed diagram as a <see cref="IGeometryCollection"/> 
        /// of <see cref="Polygon"/>s, clipped as specified.
        /// <para/>
        /// The <see cref="IGeometry.UserData"/> attribute of each face <see cref="IPolygon"/> is set to 
        /// the <c>Coordinate</c> of the corresponding input site.
        /// This allows using a <see cref="IDictionary{TKey,TValue}"/> to link faces to data associated with sites.
        /// </summary>
        /// <param name="geomFact">the geometry factory to use to create the output</param>
        /// <returns>a <see cref="IGeometryCollection"/> containg the face <see cref="IPolygon"/>s of the diagram</returns>
        public IGeometryCollection GetDiagram(IGeometryFactory geomFact)
        {
            Create();
            IGeometryCollection polys = _subdiv.GetVoronoiDiagram(geomFact);

            // clip polys to diagramEnv
            return ClipGeometryCollection(polys, _diagramEnv);
        }

        private static IGeometryCollection ClipGeometryCollection(IGeometryCollection geom, Envelope clipEnv)
        {
            var clipPoly = geom.Factory.ToGeometry(clipEnv);
            var clipped = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = geom.GetGeometryN(i);
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