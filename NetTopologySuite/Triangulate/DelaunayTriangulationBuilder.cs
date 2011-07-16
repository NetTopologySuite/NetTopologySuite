using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.Quadedge;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate
{
    ///<summary>
    /// A utility class which creates Delaunay Trianglulations
    /// from collections of points and extract the resulting 
    /// triangulation edges or triangles as geometries.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class DelaunayTriangulationBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Extracts the unique <see cref="TCoordinate"/>s from the given <see cref="IGeometry{TCoordinate}"/>
        ///</summary>
        ///<param name="geom">the geometry to extract from</param>
        ///<returns>a List of the unique Coordinates</returns>
        public static CoordinateList<TCoordinate> ExtractUniqueCoordinates(IGeometry<TCoordinate> geom)
        {
            if (geom == null)
                return new CoordinateList<TCoordinate>();

            return Unique(geom.Coordinates);
        }

        ///<summary>
        ///</summary>
        ///<param name="coords"></param>
        ///<returns></returns>
        public static CoordinateList<TCoordinate> Unique(ICoordinateSequence<TCoordinate> coords)
        {
            List<TCoordinate> tmp = new List<TCoordinate>(coords);
            tmp.Sort();
            CoordinateList<TCoordinate> coordList = new CoordinateList<TCoordinate>(tmp, false);
            return coordList;
        }

        ///<summary>
        /// Converts all <see cref="TCoordinate"/>s in a collection to <see cref="Vertex{TCoordinate}"/>es.
        ///</summary>
        ///<param name="coords">the coordinates to convert</param>
        ///<returns>a List of Vertex objects</returns>
        public static List<Vertex<TCoordinate>> ToVertices(IEnumerable<TCoordinate> coords)
        {
            List<Vertex<TCoordinate>> verts = new List<Vertex<TCoordinate>>();
            foreach (TCoordinate coord in coords)
                verts.Add(new Vertex<TCoordinate>(coord));
            return verts;
        }


        ///<summary>
        /// Computes the <see cref="IExtents{TCoordinate}"/> of a collection of <see cref="TCoordinate"/>s.
        ///</summary>
        ///<param name="geomFactory">Factory to create empty extents</param>
        ///<param name="coords">a set of Coordinates</param>
        ///<returns>the envelope of the set of coordinates</returns>
        public static IExtents<TCoordinate> Extents(IGeometryFactory<TCoordinate> geomFactory, IEnumerable<TCoordinate> coords)
        {
            IExtents<TCoordinate> extents = geomFactory.CreateExtents();
            foreach (TCoordinate coord in coords)
                extents.ExpandToInclude(coord);

            return extents;
        }

        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private List<TCoordinate> _siteCoords;
        private Double _tolerance;
        private QuadEdgeSubdivision<TCoordinate> _subdiv;

        ///<summary>
        /// Creates an instance of this class
        ///</summary>
        ///<param name="geometryFactory">factory to create geometries and extent</param>
        public DelaunayTriangulationBuilder(IGeometryFactory<TCoordinate> geometryFactory)
        {
            _geomFactory = geometryFactory;
        }

        ///<summary>
        /// Sets the sites (point or vertices) which will be triangulated.
        /// All vertices of the given geometry will be used as sites.
        ///</summary>
        ///<param name="geom">the geometry from which the sites will be extracted.</param>
        public void SetSites(IGeometry<TCoordinate> geom)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = ExtractUniqueCoordinates(geom);
        }

        ///<summary>
        /// Sets the sites (point or vertices) which will be triangulated
        /// from a collection of <see cref="TCoordinate"/>s
        ///</summary>
        ///<param name="coords">a set of coordinates</param>
        public void SetSites(IEnumerable<TCoordinate> coords)
        {
            // remove any duplicate points (they will cause the triangulation to fail)
            _siteCoords = Unique(_geomFactory.CoordinateSequenceFactory.Create(coords));
        }

        ///<summary>
        /// Sets the snapping tolerance which will be used
        /// to improved the robustness of the triangulation computation.
        /// A tolerance of 0.0 specifies that no snapping will take place.
        ///</summary>
        public Double Tolerance
        {
            get { return _tolerance; }
            set { _tolerance = value; }
        }

        private void Create()
        {
            if (_subdiv != null) return;

            IExtents<TCoordinate> siteExt = Extents(_geomFactory, _siteCoords);
            List<Vertex<TCoordinate>> vertices = ToVertices(_siteCoords);
            _subdiv = new QuadEdgeSubdivision<TCoordinate>(_geomFactory, siteExt, _tolerance);
            IncrementalDelaunayTriangulator<TCoordinate> triangulator = new IncrementalDelaunayTriangulator<TCoordinate>(_subdiv);
            triangulator.InsertSites(vertices);
        }

        ///<summary>
        /// Gets the <see cref="QuadEdgeSubdivision{TCoordinate}"/> which models the computed triangulation.
        ///</summary>
        ///<returns>the subdivision containing the triangulation</returns>
        public QuadEdgeSubdivision<TCoordinate> GetSubdivision()
        {
            Create();
            return _subdiv;
        }

        ///<summary>
        /// Gets the edges of the computed triangulation as a <see cref="IMultiLineString{TCoordinate}"/>.
        ///</summary>
        ///<returns>the edges of the triangulation</returns>
        public IGeometry<TCoordinate> GetEdges()
        {
            Create();
            return _subdiv.GetEdges();
        }

        ///<summary>
        /// Gets the faces of the computed triangulation as a <see cref="IMultiPolygon{TCoordinate}"/>.
        ///</summary>
        ///<returns>the faces of the triangulation</returns>
        public IGeometry<TCoordinate> GetTriangles()
        {
            Create();
            return _subdiv.GetTriangles();
        }
    }
}
