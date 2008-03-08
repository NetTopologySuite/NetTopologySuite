using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using GisSharpBlog.NetTopologySuite.Operation.Relate;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary> 
    /// Checks that a {GeometryGraph} representing an area
    /// (am <see cref="IPolygon{TCoordinate}" /> or <see cref="IMultiPolygon{TCoordinate}"/> )
    /// is consistent with the SFS semantics for area geometries.
    /// Checks include:
    /// Testing for rings which self-intersect (both properly and at nodes).
    /// Testing for duplicate rings.
    /// If an inconsistency if found the location of the problem is recorded.
    /// </summary>
    public class ConsistentAreaTester<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li;
        private readonly GeometryGraph<TCoordinate> _geomGraph;
        private readonly RelateNodeGraph<TCoordinate> _nodeGraph = new RelateNodeGraph<TCoordinate>();

        // the intersection point found (if any)
        private TCoordinate _invalidPoint;
         
        public ConsistentAreaTester(GeometryGraph<TCoordinate> geomGraph)
        {
            if (geomGraph == null) throw new ArgumentNullException("geomGraph");
            _geomGraph = geomGraph;
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geomGraph.Geometry.Factory);
        }

        /// <summary>
        /// Returns the intersection point, or <see langword="null" /> if none was found.
        /// </summary>        
        public TCoordinate InvalidPoint
        {
            get { return _invalidPoint; }
        }

        public Boolean IsNodeConsistentArea
        {
            get
            {
                /*
                * To fully check validity, it is necessary to
                * compute ALL intersections, including self-intersections within a single edge.
                */
                SegmentIntersector<TCoordinate> intersector = _geomGraph.ComputeSelfNodes(_li, true);

                if (intersector.HasProperIntersection)
                {
                    _invalidPoint = intersector.ProperIntersectionPoint;
                    return false;
                }

                _nodeGraph.Build(_geomGraph);
                return IsNodeEdgeAreaLabelsConsistent;
            }
        }

        /// <summary>
        /// Check all nodes to see if their labels are consistent.
        /// If any are not, return false.
        /// </summary>
        private Boolean IsNodeEdgeAreaLabelsConsistent
        {
            get
            {
                foreach (RelateNode<TCoordinate> node in _nodeGraph.Nodes)
                {

                    if (!node.Edges.IsAreaLabelsConsistent)
                    {
                        _invalidPoint = (TCoordinate)node.Coordinate.Clone();
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Checks for two duplicate rings in an area.
        /// Duplicate rings are rings that are topologically equal
        /// (that is, which have the same sequence of points up to point order).
        /// If the area is topologically consistent (determined by calling the
        /// <c>isNodeConsistentArea</c>,
        /// duplicate rings can be found by checking for EdgeBundles which contain more than one EdgeEnd.
        /// (This is because topologically consistent areas cannot have two rings sharing
        /// the same line segment, unless the rings are equal).
        /// The start point of one of the equal rings will be placed in invalidPoint.
        /// Returns <see langword="true"/> if this area Geometry is topologically consistent but has two duplicate rings.
        /// </summary>
        public Boolean HasDuplicateRings
        {
            get
            {
                foreach (RelateNode<TCoordinate> node in _nodeGraph.Nodes)
                {
                    foreach (EdgeEndBundle<TCoordinate> eeb in node.Edges)
                    {
                        if (eeb.EdgeEnds.Count > 1)
                        {
                            _invalidPoint = eeb.Edge.Coordinates[0];
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}