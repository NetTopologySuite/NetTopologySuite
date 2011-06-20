using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Triangulate.Quadedge;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
    ///<summary>
    /// A utility class which creates Voronoi Diagrams
    /// from collections of points.
    /// The diagram is returned as a {@link GeometryCollection} of {@link Polygon}s,
    /// clipped to the larger of a supplied envelope or to an envelope determined
    /// by the input sites.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class VoronoiDiagramBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private CoordinateList<TCoordinate> _siteCoords;
        private double _tolerance;
        private QuadEdgeSubdivision<TCoordinate> _subdiv;
        private IExtents<TCoordinate> _clipEnv;
        private IExtents<TCoordinate> _diagramEnv;

        ///<summary>
        /// Creates a new Voronoi diagram builder.
        ///</summary>
        public VoronoiDiagramBuilder(IGeometryFactory<TCoordinate> geomFactory)
        {
            _geomFactory = geomFactory;
        }

        ///<summary>
        /// Sets the sites (point or vertices) which will be diagrammed.
        /// All vertices of the given geometry will be used as sites.
        ///</summary>
        ///<param name="geom">the geometry from which the sites will be extracted.</param>
        public void SetSites(IGeometry<TCoordinate> geom)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = DelaunayTriangulationBuilder<TCoordinate>.ExtractUniqueCoordinates(geom);
        }

        ///<summary>Sets the sites (point or vertices) which will be diagrammed from a <see cref="ICoordinateSequence{TCoordinate}"/>.
        ///</summary>
        ///<param name="coords">the sequence</param>
        public void SetSites(ICoordinateSequence<TCoordinate> coords)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = DelaunayTriangulationBuilder<TCoordinate>.Unique(coords);
        }

        ///<summary>
        /// Gets/sets the envelope to clip the diagram to.
        /// The diagram will be clipped to the larger
        /// of this envelope or an envelope surrounding the sites.
        ///</summary>
        public IExtents<TCoordinate> ClipExtents
        {
            get { return _clipEnv; }
            set { _clipEnv = value; }
        }

        ///<summary>
        /// Gets/sets the snapping tolerance which will be used
        /// to improved the robustness of the triangulation computation.
        /// A tolerance of 0.0 specifies that no snapping will take place.
        ///</summary>
        public Double Tolerance
        {
            get { return _tolerance; }
            set { _tolerance = value;}
        }

        private void Create()
        {
            if (_subdiv != null) return;

            IExtents<TCoordinate> siteEnv = DelaunayTriangulationBuilder<TCoordinate>.Extents(_geomFactory, _siteCoords);
            _diagramEnv = siteEnv;
            // add a buffer around the final envelope
            Double expandBy = Math.Max(_diagramEnv.GetSize(Ordinates.X), _diagramEnv.GetSize(Ordinates.Y));
            _diagramEnv.ExpandBy(expandBy);
            if (_clipEnv != null)
                _diagramEnv.ExpandToInclude(_clipEnv);

            List<Vertex<TCoordinate>> vertices = DelaunayTriangulationBuilder<TCoordinate>.ToVertices(_siteCoords);
            _subdiv = new QuadEdgeSubdivision<TCoordinate>(_geomFactory, siteEnv, _tolerance);
            IncrementalDelaunayTriangulator<TCoordinate> triangulator = new IncrementalDelaunayTriangulator<TCoordinate>(_subdiv);
            triangulator.InsertSites(vertices);
        }

        ///<summary>
        /// Gets the <see cref="QuadEdgeSubdivision{TCoordinate}"/> which models the computed diagram.
        ///</summary>
        ///<returns>the subdivision containing the triangulation</returns>
        public QuadEdgeSubdivision<TCoordinate> GetSubdivision()
        {
            Create();
            return _subdiv;
        }

        ///<summary>
        /// Gets the faces of the computed diagram as a <see cref="IGeometryCollection{TCoordinate}"/> of <see cref="IPolygon{TCoordinate}"/>s, clipped as specified.
        ///</summary>
        ///<returns></returns>
        public IGeometry<TCoordinate> GetDiagram()
        {
            Create();
            IGeometry<TCoordinate> polys = _subdiv.GetVoronoiDiagram();

            List<IGeometry<TCoordinate>> clippedPolys = new List<IGeometry<TCoordinate>>();
            // clip polys to _diagramEnv
            ClipGeometry(polys, _diagramEnv.ToGeometry(), clippedPolys);
           return  _geomFactory.CreateGeometryCollection(clippedPolys);
            

        }

        private void ClipGeometry(IGeometry<TCoordinate> geom, IGeometry<TCoordinate> clipPoly, List<IGeometry<TCoordinate>> clipped)
        {
            IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;
            if (gc != null)
            {    foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    ClipGeometry(geometry, clipPoly, clipped);
                }
                return;
            }

            IGeometry<TCoordinate> result = null;
            if (_clipEnv.Contains(geom.Extents))
                result = geom;
            else if (_clipEnv.Intersects(geom.Extents))
            {
                result = clipPoly.Intersection(geom);
                // keep vertex key info
                result.UserData = geom.UserData;
            }

            if (result != null && !result.IsEmpty)
            {
                clipped.Add(result);
            }

        }
    }
}
