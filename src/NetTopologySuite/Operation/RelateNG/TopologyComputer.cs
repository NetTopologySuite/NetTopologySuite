using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.RelateNG
{
    internal class TopologyComputer
    {

        private const string MSG_GEOMETRY_DIMENSION_UNEXPECTED = "Unexpected combination of geometry dimensions";

        private readonly TopologyPredicate _predicate;
        private readonly RelateGeometry _geomA;
        private readonly RelateGeometry _geomB;
        private readonly Dictionary<Coordinate, NodeSections> _nodeMap = new Dictionary<Coordinate, NodeSections>();

        public TopologyComputer(TopologyPredicate predicate, RelateGeometry geomA, RelateGeometry geomB)
        {
            _predicate = predicate;
            _geomA = geomA;
            _geomB = geomB;

            InitExteriorDims();
        }

        /// <summary>
        /// Determine a priori partial EXTERIOR topology based on dimensions.
        /// </summary>
        private void InitExteriorDims()
        {
            var dimRealA = _geomA.DimensionReal;
            var dimRealB = _geomB.DimensionReal;

            /*
             * For P/L case, P exterior intersects L interior
             */
            if (dimRealA == Dimension.P && dimRealB == Dimension.L)
            {
                UpdateDim(Location.Exterior, Location.Interior, Dimension.L);
            }
            else if (dimRealA == Dimension.L && dimRealB == Dimension.P)
            {
                UpdateDim(Location.Interior, Location.Exterior, Dimension.L);
            }
            /*
             * For P/A case, the Area Int and Bdy intersect the Point exterior.
             */
            else if (dimRealA == Dimension.P && dimRealB == Dimension.A)
            {
                UpdateDim(Location.Exterior, Location.Interior, Dimension.A);
                UpdateDim(Location.Exterior, Location.Boundary, Dimension.L);
            }
            else if (dimRealA == Dimension.A && dimRealB == Dimension.P)
            {
                UpdateDim(Location.Interior, Location.Exterior, Dimension.A);
                UpdateDim(Location.Boundary, Location.Exterior, Dimension.L);
            }
            else if (dimRealA == Dimension.L && dimRealB == Dimension.A)
            {
                UpdateDim(Location.Exterior, Location.Interior, Dimension.A);
            }
            else if (dimRealA == Dimension.A && dimRealB == Dimension.L)
            {
                UpdateDim(Location.Interior, Location.Exterior, Dimension.A);
            }
            //-- cases where one geom is EMPTY
            else if (dimRealA == Dimension.False || dimRealB == Dimension.False)
            {
                if (dimRealA != Dimension.False)
                {
                    InitExteriorEmpty(RelateGeometry.GEOM_A);
                }
                if (dimRealB != Dimension.False)
                {
                    InitExteriorEmpty(RelateGeometry.GEOM_B);
                }
            }
        }

        private void InitExteriorEmpty(bool geomNonEmpty)
        {
            var dimNonEmpty = GetDimension(geomNonEmpty);
            switch (dimNonEmpty)
            {
                case Dimension.P:
                    UpdateDim(geomNonEmpty, Location.Interior, Location.Exterior, Dimension.P);
                    break;
                case Dimension.L:
                    if (GetGeometry(geomNonEmpty).HasBoundary)
                    {
                        UpdateDim(geomNonEmpty, Location.Boundary, Location.Exterior, Dimension.P);
                    }
                    UpdateDim(geomNonEmpty, Location.Interior, Location.Exterior, Dimension.L);
                    break;
                case Dimension.A:
                    UpdateDim(geomNonEmpty, Location.Boundary, Location.Exterior, Dimension.L);
                    UpdateDim(geomNonEmpty, Location.Interior, Location.Exterior, Dimension.A);
                    break;
            }
        }

        private RelateGeometry GetGeometry(bool isA)
        {
            return isA ? _geomA : _geomB;
        }

        public Dimension GetDimension(bool isA)
        {
            return GetGeometry(isA).Dimension;
        }

        public bool IsAreaArea()
        {
            return GetDimension(RelateGeometry.GEOM_A) == Dimension.A
                && GetDimension(RelateGeometry.GEOM_B) == Dimension.A;
        }

        /// <summary>
        /// Indicates whether the input geometries require self-noding
        /// for correct evaluation of specific spatial predicates.
        /// Self-noding is required for geometries which may self-cross
        /// - i.e.lines, and overlapping polygons in GeometryCollections.
        /// Self-noding is required for geometries which may
        /// have self-crossing linework.
        /// This causes the coordinates of nodes created by
        /// crossing segments to be computed explicitly.
        /// This ensures that node locations match in situations
        /// where a self-crossing and mutual crossing occur at the same logical location.
        /// The canonical example is a self-crossing line tested against a single segment
        /// identical to one of the crossed segments.
        /// </summary>
        public bool IsSelfNodingRequired
        {
            get
            {
                if (_predicate.RequireSelfNoding())
                {
                    if (_geomA.IsSelfNodingRequired ||
                        _geomB.IsSelfNodingRequired) return true;
                }
                return false;
            }
        }

        public bool IsExteriorCheckRequired(bool isA)
        {
            return _predicate.RequireExteriorCheck(isA);
        }

        private void UpdateDim(Location locA, Location locB, Dimension dimension)
        {
            //System.Diagnostics.Trace.WriteLine(LocationUtility.ToLocationSymbol(locA) + "/" + LocationUtility.ToLocationSymbol(locB) + ": " + dimension);
            _predicate.UpdateDimension(locA, locB, dimension);
        }

        private void UpdateDim(bool isAB, Location loc1, Location loc2, Dimension dimension)
        {
            if (isAB)
            {
                UpdateDim(loc1, loc2, dimension);
            }
            else
            {
                // is ordered BA
                UpdateDim(loc2, loc1, dimension);
            }
        }

        public bool IsResultKnown =>  _predicate.IsKnown;

        public bool Result => _predicate.Value;

        /// <summary>
        /// Finalize the evaluation
        /// </summary>
        public void Finish()
        {
            _predicate.Finish();
        }

        private NodeSections GetNodeSections(Coordinate nodePt)
        {
            if (!_nodeMap.TryGetValue(nodePt, out var node))
            {
                node = new NodeSections(nodePt);
                _nodeMap[nodePt] = node;
            }
            return node;
        }

        public void AddIntersection(NodeSection a, NodeSection b)
        {
            if (!a.IsSameGeometry(b))
            {
                UpdateIntersectionAB(a, b);
            }
            //-- add edges to node to allow full topology evaluation later
            AddNodeSections(a, b);
        }

        /// <summary>
        /// Update topology for an intersection between A and B.
        /// </summary>
        /// <param name="a">The section for geometry A</param>
        /// <param name="b">The section for geometry B</param>
        private void UpdateIntersectionAB(NodeSection a, NodeSection b)
        {
            if (NodeSection.IsAreaArea(a, b))
            {
                UpdateAreaAreaCross(a, b);
            }
            UpdateNodeLocation(a, b);
        }

        /// <summary>
        /// Updates topology for an AB Area-Area crossing node.
        /// Sections cross at a node if (a) the intersection is proper
        /// (i.e. in the interior of two segments)
        /// or(b) if non-proper then whether the linework crosses
        /// is determined by the geometry of the segments on either side of the node.
        /// In these situations the area geometry interiors intersect(in dimension 2).
        /// </summary>
        /// <param name="a">The section for geometry A</param>
        /// <param name="b">The section for geometry B</param>
        private void UpdateAreaAreaCross(NodeSection a, NodeSection b)
        {
            bool isProper = NodeSection.AreProper(a, b);
            if (isProper || PolygonNodeTopology.IsCrossing(a.NodePt,
                a.GetVertex(0), a.GetVertex(1),
                b.GetVertex(0), b.GetVertex(1)))
            {
                UpdateDim(Location.Interior, Location.Interior, Dimension.A);
            }
        }

        /// <summary>
        /// Updates topology for a node at an AB edge intersection.
        /// </summary>
        /// <param name="a">The section for geometry A</param>
        /// <param name="b">The section for geometry B</param>
        private void UpdateNodeLocation(NodeSection a, NodeSection b)
        {
            var pt = a.NodePt;
            var locA = _geomA.LocateNode(pt, a.Polygonal);
            var locB = _geomB.LocateNode(pt, b.Polygonal);
            UpdateDim(locA, locB, Dimension.P);
        }

        private void AddNodeSections(NodeSection ns0, NodeSection ns1)
        {
            var sections = GetNodeSections(ns0.NodePt);
            sections.AddNodeSection(ns0);
            sections.AddNodeSection(ns1);
        }

        public void AddPointOnPointInterior(Coordinate pt)
        {
            UpdateDim(Location.Interior, Location.Interior, Dimension.P);
        }

        public void AddPointOnPointExterior(bool isGeomA, Coordinate pt)
        {
            UpdateDim(isGeomA, Location.Interior, Location.Exterior, Dimension.P);
        }

        public void AddPointOnGeometry(bool isA, Location locTarget, Dimension dimTarget, Coordinate pt)
        {
            UpdateDim(isA, Location.Interior, locTarget, Dimension.P);
            switch (dimTarget)
            {
                case Dimension.P:
                    return;
                case Dimension.L:
                    /*
                     * Because zero-length lines are handled, 
                     * a point lying in the exterior of the line target 
                     * may imply either P or L for the Exterior interaction
                     */
                    //TODO: determine if effective dimension of linear target is L?
                    //updateDim(isGeomA, Location.Exterior, locTarget, Dimension.P); 
                    return;
                case Dimension.A:
                    /*
                     * If a point intersects an area target, then the area interior and boundary
                     * must extend beyond the point and thus interact with its exterior.
                     */
                    UpdateDim(isA, Location.Exterior, Location.Interior, Dimension.A);
                    UpdateDim(isA, Location.Exterior, Location.Boundary, Dimension.L);
                    return;
            }
            throw new InvalidOperationException("Unknown target dimension: " + dimTarget);
        }

        /// <summary>
        /// Add topology for a line end.
        /// The line end point must be "significant";
        /// i.e.not contained in an area if the source is a mixed-dimension GC.
        /// </summary>
        /// <param name="isLineA">the input containing the line end</param>
        /// <param name="locLineEnd">the location of the line end (Interior or Boundary)</param>
        /// <param name="locTarget">the location on the target geometry</param>
        /// <param name="dimTarget">the dimension of the interacting target geometry element,
        /// (if any), or the dimension of the target</param>
        /// <param name="pt">the line end coordinate</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddLineEndOnGeometry(bool isLineA, Location locLineEnd, Location locTarget, Dimension dimTarget, Coordinate pt)
        {
            //-- record topology at line end point
            UpdateDim(isLineA, locLineEnd, locTarget, Dimension.P);

            //-- Line and Area targets may have additional topology
            switch (dimTarget)
            {
                case Dimension.P:
                    return;
                case Dimension.L:
                    AddLineEndOnLine(isLineA, locLineEnd, locTarget, pt);
                    return;
                case Dimension.A:
                    AddLineEndOnArea(isLineA, locLineEnd, locTarget, pt);
                    return;
            }
            throw new InvalidOperationException("Unknown target dimension: " + dimTarget);
        }

        private void AddLineEndOnLine(bool isLineA, Location locLineEnd, Location locLine, Coordinate pt)
        {
            UpdateDim(isLineA, locLineEnd, locLine, Dimension.P);
            /*
             * When a line end is in the EXTERIOR of a Line, 
             * some length of the source Line INTERIOR
             * is also in the target Line EXTERIOR. 
             * This works for zero-length lines as well. 
             */

            if (locLine == Location.Exterior)
            {
                UpdateDim(isLineA, Location.Interior, Location.Exterior, Dimension.L);
            }
        }

        private void AddLineEndOnArea(bool isLineA, Location locLineEnd, Location locArea, Coordinate pt)
        {
            if (locArea != Location.Boundary)
            {
                /*
                 * When a line end is in an Area INTERIOR or EXTERIOR 
                 * some length of the source Line Interior  
                 * AND the Exterior of the line
                 * is also in that location of the target.
                 * NOTE: this assumes the line end is NOT also in an Area of a mixed-dim GC
                 */
                //TODO: handle zero-length lines?
                UpdateDim(isLineA, Location.Interior, locArea, Dimension.L);
                UpdateDim(isLineA, Location.Exterior, locArea, Dimension.A);
            }
        }

        /// <summary>
        /// Adds topology for an area vertex interaction with a target geometry element.
        /// Assumes the target geometry element has highest dimension
        /// (i.e. if the point lies on two elements of different dimension,
        /// the location on the higher dimension element is provided.
        /// This is the semantic provided by <see cref="RelatePointLocator"/>.
        /// <para/>
        /// Note that in a GeometryCollection containing overlapping or adjacent polygons,
        /// the area vertex location may be <see cref="Location.Interior"/> instead of <see cref="Location.Boundary"/>.
        /// </summary>
        /// <param name="isAreaA">The input that is the area</param>
        /// <param name="locArea">The location on the area</param>
        /// <param name="locTarget">The location on the target geometry element</param>
        /// <param name="dimTarget">The dimension of the target geometry element</param>
        /// <param name="pt">The point of interaction</param>
        public void AddAreaVertex(bool isAreaA, Location locArea, Location locTarget, Dimension dimTarget, Coordinate pt)
        {
            if (locTarget == Location.Exterior)
            {
                UpdateDim(isAreaA, Location.Interior, Location.Exterior, Dimension.A);
                /*
                 * If area vertex is on Boundary further topology can be deduced
                 * from the neighbourhood around the boundary vertex.
                 * This is always the case for polygonal geometries.
                 * For GCs, the vertex may be either on boundary or in interior
                 * (i.e. of overlapping or adjacent polygons) 
                 */
                if (locArea == Location.Boundary)
                {
                    UpdateDim(isAreaA, Location.Boundary, Location.Exterior, Dimension.L);
                    UpdateDim(isAreaA, Location.Exterior, Location.Exterior, Dimension.A);
                }
                return;
            }
            switch (dimTarget)
            {
                case Dimension.P:
                    AddAreaVertexOnPoint(isAreaA, locArea, pt);
                    return;
                case Dimension.L:
                    AddAreaVertexOnLine(isAreaA, locArea, locTarget, pt);
                    return;
                case Dimension.A:
                    AddAreaVertexOnArea(isAreaA, locArea, locTarget, pt);
                    return;
            }
            throw new InvalidOperationException("Unknown target dimension: " + dimTarget);
        }

        /// <summary>
        /// Updates topology for an area vertex (in Interior or on Boundary)
        /// intersecting a point.
        /// Note that because the largest dimension of intersecting target is determined,
        /// the intersecting point is not part of any other target geometry,
        /// and hence its neighbourhood is in the Exterior of the target.
        /// </summary>
        /// <param name="isAreaA">A flag indicating whether the area is the A input</param>
        /// <param name="locArea">The location of th evertex in the area</param>
        /// <param name="pt">The point at whicht topology is being updated</param>
        private void AddAreaVertexOnPoint(bool isAreaA, Location locArea, Coordinate pt)
        {
            //-- Assert: locArea != EXTERIOR
            //-- Assert: locTarget == INTERIOR
            /*
             * The vertex location intersects the Point.
             */
            UpdateDim(isAreaA, locArea, Location.Interior, Dimension.P);
            /*
             * The area interior intersects the point's exterior neighbourhood.
             */
            UpdateDim(isAreaA, Location.Interior, Location.Exterior, Dimension.A);
            /*
             * If the area vertex is on the boundary, 
             * the area boundary and exterior intersect the point's exterior neighbourhood
             */
            if (locArea == Location.Boundary)
            {
                UpdateDim(isAreaA, Location.Boundary, Location.Exterior, Dimension.L);
                UpdateDim(isAreaA, Location.Exterior, Location.Exterior, Dimension.A);
            }
        }

        private void AddAreaVertexOnLine(bool isAreaA, Location locArea, Location locTarget, Coordinate pt)
        {
            //-- Assert: locArea != EXTERIOR
            /*
             * If an area vertex intersects a line, all we know is the 
             * intersection at that point.  
             * e.g. the line may or may not be collinear with the area boundary,
             * and the line may or may not intersect the area interior.
             * Full topology is determined later by node analysis
             */
            UpdateDim(isAreaA, locArea, locTarget, Dimension.P);
            if (locArea == Location.Interior)
            {
                /*
                 * The area interior intersects the line's exterior neighbourhood.
                 */
                UpdateDim(isAreaA, Location.Interior, Location.Exterior, Dimension.A);
            }
        }

        public void AddAreaVertexOnArea(bool isAreaA, Location locArea, Location locTarget, Coordinate pt)
        {
            if (locTarget == Location.Boundary)
            {
                if (locArea == Location.Boundary)
                {
                    //-- B/B topology is fully computed later by node analysis
                    UpdateDim(isAreaA, Location.Boundary, Location.Boundary, Dimension.P);
                }
                else
                {
                    // locArea == INTERIOR
                    UpdateDim(isAreaA, Location.Interior, Location.Interior, Dimension.A);
                    UpdateDim(isAreaA, Location.Interior, Location.Boundary, Dimension.L);
                    UpdateDim(isAreaA, Location.Interior, Location.Exterior, Dimension.A);
                }
            }
            else
            {
                //-- locTarget is INTERIOR or EXTERIOR` 
                UpdateDim(isAreaA, Location.Interior, locTarget, Dimension.A);
                /*
                 * If area vertex is on Boundary further topology can be deduced
                 * from the neighbourhood around the boundary vertex.
                 * This is always the case for polygonal geometries.
                 * For GCs, the vertex may be either on boundary or in interior
                 * (i.e. of overlapping or adjacent polygons) 
                 */
                if (locArea == Location.Boundary)
                {
                    UpdateDim(isAreaA, Location.Boundary, locTarget, Dimension.L);
                    UpdateDim(isAreaA, Location.Exterior, locTarget, Dimension.A);
                }
            }
        }

        public void EvaluateNodes()
        {
            foreach (var nodeSections in _nodeMap.Values)
            {
                if (nodeSections.HasInteractionAB)
                {
                    EvaluateNode(nodeSections);
                    if (IsResultKnown)
                        return;
                }
            }
        }

        private void EvaluateNode(NodeSections nodeSections)
        {
            var p = nodeSections.Coordinate;
            var node = nodeSections.CreateNode();
            //-- Node must have edges for geom, but may also be in interior of a overlapping GC
            bool isAreaInteriorA = _geomA.IsNodeInArea(p, nodeSections.GetPolygonal(RelateGeometry.GEOM_A));
            bool isAreaInteriorB = _geomB.IsNodeInArea(p, nodeSections.GetPolygonal(RelateGeometry.GEOM_B));
            node.Finish(isAreaInteriorA, isAreaInteriorB);
            EvaluateNodeEdges(node);
        }

        private void EvaluateNodeEdges(RelateNode node)
        {
            //TODO: collect distinct dim settings by using temporary matrix?
            foreach (var e in node.Edges)
            {
                //-- An optimization to avoid updates for cases with a linear geometry
                if (IsAreaArea())
                {
                    UpdateDim(e.Location(RelateGeometry.GEOM_A, Position.Left),
                              e.Location(RelateGeometry.GEOM_B, Position.Left), Dimension.A);
                    UpdateDim(e.Location(RelateGeometry.GEOM_A, Position.Right),
                              e.Location(RelateGeometry.GEOM_B, Position.Right), Dimension.A);
                }
                UpdateDim(e.Location(RelateGeometry.GEOM_A, Position.On),
                          e.Location(RelateGeometry.GEOM_B, Position.On), Dimension.L);
            }
        }

    }
}
