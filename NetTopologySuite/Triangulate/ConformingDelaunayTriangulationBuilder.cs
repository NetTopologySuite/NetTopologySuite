using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Triangulate.Quadedge;
using NPack.Interfaces;
using C5;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
    ///<summary>
    /// A utility class which creates Conforming Delaunay Trianglulations
    /// from collections of points and linear constraints, and extract the resulting 
    /// triangulation edges or triangles as geometries. 
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class ConformingDelaunayTriangulationBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private CoordinateList<TCoordinate> _siteCoords;
        private IGeometry<TCoordinate> _constraintLines;
        private Double _tolerance;
        private QuadEdgeSubdivision<TCoordinate> _subdiv;

        private readonly TreeDictionary<TCoordinate, Vertex<TCoordinate>> _vertexMap = new TreeDictionary<TCoordinate, Vertex<TCoordinate>>();

        ///<summary>
        /// Creates an instance of this class
        ///</summary>
        ///<param name="geomFactory"></param>
        public ConformingDelaunayTriangulationBuilder(IGeometryFactory<TCoordinate> geomFactory)
        {
            _geomFactory = geomFactory;
        }

        ///<summary>
        /// Sets the sites (point or vertices) which will be triangulated.
        /// All vertices of the given geometry will be used as sites.
        ///</summary>
        ///<param name="geom">the geometry from which the sites will be extracted.</param>
        public void SetSites(IGeometry<TCoordinate> geom)
        {
            _siteCoords = DelaunayTriangulationBuilder<TCoordinate>.ExtractUniqueCoordinates(geom);
        }

        ///<summary>
        /// Sets the linear constraints to be conformed to.
        /// All linear components in the input will be used as constraints.
        ///</summary>
        ///<param name="constraintLines">the lines to constraint to</param>
        public void SetConstraints(IGeometry<TCoordinate> constraintLines)
        {
            _constraintLines = constraintLines;
        }

        ///<summary>Sets the snapping tolerance which will be used to improved the robustness of the triangulation computation. A tolerance of 0.0 specifies that no snapping will take place.
        ///</summary>
        public Double Tolerance
        {
            get { return _tolerance; }
            set { _tolerance = value ; }
        }


        private void Create()
        {
            if (_subdiv != null) return;

            IExtents<TCoordinate> siteExt = DelaunayTriangulationBuilder<TCoordinate>.Extents(_geomFactory, _siteCoords);

            List<Vertex<TCoordinate>> sites = CreateConstraintVertices(_siteCoords);

            List<Segment<TCoordinate>> segments = new List<Segment<TCoordinate>>();
            if (_constraintLines != null)
            {
                siteExt.ExpandToInclude(_constraintLines.Extents);
                CreateVertices(_constraintLines);
                segments = CreateConstraintSegments(_constraintLines);
            }

            ConformingDelaunayTriangulator<TCoordinate> cdt = new ConformingDelaunayTriangulator<TCoordinate>(_geomFactory, (List<Vertex<TCoordinate>>)sites, _tolerance);

            cdt.SetConstraints(segments, _vertexMap.Values);

            cdt.FormInitialDelaunay();
            cdt.EnforceConstraints();
            _subdiv = cdt.GetSubdivision();
        }

        private static List<Vertex<TCoordinate>> CreateConstraintVertices(CoordinateList<TCoordinate> coords)
        {
            List<Vertex<TCoordinate>> verts = new List<Vertex<TCoordinate>>();
            foreach (TCoordinate coord in coords)
                verts.Add(new ConstraintVertex<TCoordinate>(coord));

            return verts;
        }

        private void CreateVertices(IGeometry<TCoordinate> geom)
        {

            foreach (TCoordinate coordinate in geom.Coordinates)
            {
                Vertex<TCoordinate> v = new ConstraintVertex<TCoordinate>(coordinate);
                _vertexMap.Add(coordinate, v);
            }
        }

        private static List<Segment<TCoordinate>> CreateConstraintSegments(IGeometry<TCoordinate> geom)
        {
            List<Segment<TCoordinate>> constraintSegs = new List<Segment<TCoordinate>>();
            foreach (ILineString<TCoordinate> line in LinearComponentExtracter<TCoordinate>.GetLines(geom))
                constraintSegs.AddRange(CreateConstraintSegments(line));
            return constraintSegs;
        }

        private static IEnumerable<Segment<TCoordinate>> CreateConstraintSegments(ILineString<TCoordinate> line)
        {
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(line.Coordinates)    )
                yield return new Segment<TCoordinate>(pair.First, pair.Second);
        }

        ///<summary>
        /// Gets the QuadEdgeSubdivision which models the computed triangulation.
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
