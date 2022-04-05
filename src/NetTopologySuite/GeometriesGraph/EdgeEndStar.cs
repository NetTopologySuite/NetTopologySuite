using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A EdgeEndStar is an ordered list of EdgeEnds around a node.
    /// They are maintained in CCW order (starting with the positive x-axis) around the node
    /// for efficient lookup and topology building.
    /// </summary>
    abstract public class EdgeEndStar
    {
        /// <summary>
        /// A map which maintains the edges in sorted order around the node.
        /// </summary>
        protected IDictionary<EdgeEnd, EdgeEnd> edgeMap = new SortedDictionary<EdgeEnd, EdgeEnd>();

        /// <summary>
        /// A list of all outgoing edges in the result, in CCW order.
        /// </summary>
        protected IList<EdgeEnd> edgeList;

        /// <summary>
        /// The location of the point for this star in Geometry i Areas.
        /// </summary>
        private readonly Location[] _ptInAreaLocation = new[] { Location.Null, Location.Null };

        /*
        /// <summary>
        ///
        /// </summary>
        protected EdgeEndStar() { }
         */
        /// <summary>
        /// Insert a EdgeEnd into this EdgeEndStar.
        /// </summary>
        /// <param name="e">An <c>EdgeEnd</c></param>
        abstract public void Insert(EdgeEnd e);

        /// <summary>
        /// Insert an EdgeEnd into the map, and clear the edgeList cache,
        /// since the list of edges has now changed.
        /// </summary>
        /// <param name="e">An EdgeEnd</param>
        /// <param name="obj">An EdgeEnd</param>
        protected void InsertEdgeEnd(EdgeEnd e, EdgeEnd obj)
        {
            edgeMap[e] = obj;
            edgeList = null;    // edge list has changed - clear the cache
        }

        /// <returns>
        /// The coordinate for the node this star is based at.
        /// </returns>
        public Coordinate Coordinate
        {
            get
            {
                var it = GetEnumerator();
                if (!it.MoveNext())
                    return null;
                var e = it.Current;
                return e.Coordinate;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int Degree => edgeMap.Count;

        /// <summary>
        /// Iterator access to the ordered list of edges is optimized by
        /// copying the map collection to a list.  (This assumes that
        /// once an iterator is requested, it is likely that insertion into
        /// the map is complete).
        /// </summary>
        /// <returns>Access to ordered list of edges.</returns>
        public IEnumerator<EdgeEnd> GetEnumerator()
        {
            return Edges.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        public IList<EdgeEnd> Edges
        {
            get
            {
                if (edgeList == null)
                    edgeList = new List<EdgeEnd>(edgeMap.Values);
                return edgeList;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ee"></param>
        /// <returns></returns>
        public EdgeEnd GetNextCW(EdgeEnd ee)
        {
            var temp = Edges;
            temp = null;    // Hack for calling property
            int i = edgeList.IndexOf(ee);
            int iNextCW = i - 1;
            if (i == 0)
                iNextCW = edgeList.Count - 1;
            return edgeList[iNextCW];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomGraph"></param>
        public virtual void ComputeLabelling(GeometryGraph[] geomGraph)
        {
            ComputeEdgeEndLabels(geomGraph[0].BoundaryNodeRule);
            // Propagate side labels  around the edges in the star
            // for each parent Geometry
            PropagateSideLabels(0);
            PropagateSideLabels(1);

            /*
            * If there are edges that still have null labels for a point
            * this must be because there are no area edges for that point incident on this node.
            * In this case, to label the edge for that point we must test whether the
            * edge is in the interior of the point.
            * To do this it suffices to determine whether the node for the edge is in the interior of an area.
            * If so, the edge has location Interior for the point.
            * In all other cases (e.g. the node is on a line, on a point, or not on the point at all) the edge
            * has the location Exterior for the point.
            *
            * Note that the edge cannot be on the Boundary of the point, since then
            * there would have been a parallel edge from the Geometry at this node also labelled Boundary
            * and this edge would have been labelled in the previous step.
            *
            * This code causes a problem when dimensional collapses are present, since it may try and
            * determine the location of a node where a dimensional collapse has occurred.
            * The point should be considered to be on the Exterior
            * of the polygon, but locate() will return Interior, since it is passed
            * the original Geometry, not the collapsed version.
            *
            * If there are incident edges which are Line edges labelled Boundary,
            * then they must be edges resulting from dimensional collapses.
            * In this case the other edges can be labelled Exterior for this Geometry.
            *
            * MD 8/11/01 - NOT True!  The collapsed edges may in fact be in the interior of the Geometry,
            * which means the other edges should be labelled Interior for this Geometry.
            * Not sure how solve this...  Possibly labelling needs to be split into several phases:
            * area label propagation, symLabel merging, then finally null label resolution.
            */
            bool[] hasDimensionalCollapseEdge = { false, false };
            foreach (var e in Edges)
            {
                var label = e.Label;
                for (int geomi = 0; geomi < 2; geomi++)
                    if (label.IsLine(geomi) && label.GetLocation(geomi) == Location.Boundary)
                        hasDimensionalCollapseEdge[geomi] = true;
            }
            foreach (var e in Edges)
            {
                var label = e.Label;
                for (int geomi = 0; geomi < 2; geomi++)
                {
                    if (label.IsAnyNull(geomi))
                    {
                        Location loc;
                        if (hasDimensionalCollapseEdge[geomi])
                            loc = Location.Exterior;
                        else
                        {
                            var p = e.Coordinate;
                            loc = GetLocation(geomi, p, geomGraph);
                        }
                        label.SetAllLocationsIfNull(geomi, loc);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void ComputeEdgeEndLabels(IBoundaryNodeRule boundaryNodeRule)
        {
            // Compute edge label for each EdgeEnd
            foreach (var ee in Edges)
            {
                ee.ComputeLabel(boundaryNodeRule);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="p"></param>
        /// <param name="geom"></param>
        /// <returns></returns>
        private Location GetLocation(int geomIndex, Coordinate p, GeometryGraph[] geom)
        {
            // compute location only on demand
            if (_ptInAreaLocation[geomIndex] == Location.Null)
                _ptInAreaLocation[geomIndex] = SimplePointInAreaLocator.Locate(p, geom[geomIndex].Geometry);
            return _ptInAreaLocation[geomIndex];
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsAreaLabelsConsistent(GeometryGraph geometryGraph)
        {
            ComputeEdgeEndLabels(geometryGraph.BoundaryNodeRule);
            return CheckAreaLabelsConsistent(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        private bool CheckAreaLabelsConsistent(int geomIndex)
        {
            // Since edges are stored in CCW order around the node,
            // As we move around the ring we move from the right to the left side of the edge
            var edges = Edges;
            // if no edges, trivially consistent
            if (edges.Count <= 0)
                return true;
            // initialize startLoc to location of last Curve side (if any)
            int lastEdgeIndex = edges.Count - 1;
            var startLabel = edges[lastEdgeIndex].Label;
            var startLoc = startLabel.GetLocation(geomIndex, Geometries.Position.Left);
            Assert.IsTrue(startLoc != Location.Null, "Found unlabelled area edge");

            var currLoc = startLoc;
            foreach (var e in Edges)
            {
                var label = e.Label;
                // we assume that we are only checking a area
                Assert.IsTrue(label.IsArea(geomIndex), "Found non-area edge");
                var leftLoc = label.GetLocation(geomIndex, Geometries.Position.Left);
                var rightLoc = label.GetLocation(geomIndex, Geometries.Position.Right);
                // check that edge is really a boundary between inside and outside!
                if (leftLoc == rightLoc)
                    return false;
                // check side location conflict
                if (rightLoc != currLoc)
                    return false;
                currLoc = leftLoc;
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        public void PropagateSideLabels(int geomIndex)
        {
            // Since edges are stored in CCW order around the node,
            // As we move around the ring we move from the right to the left side of the edge
            var startLoc = Location.Null;
            // initialize loc to location of last Curve side (if any)
            foreach (var e in Edges)
            {
                var label = e.Label;
                if (label.IsArea(geomIndex) && label.GetLocation(geomIndex, Geometries.Position.Left) != Location.Null)
                    startLoc = label.GetLocation(geomIndex, Geometries.Position.Left);
            }
            // no labelled sides found, so no labels to propagate
            if (startLoc == Location.Null)
                return;

            var currLoc = startLoc;
            foreach (var e in Edges)
            {
                var label = e.Label;
                // set null On values to be in current location
                if (label.GetLocation(geomIndex, Geometries.Position.On) == Location.Null)
                    label.SetLocation(geomIndex, Geometries.Position.On, currLoc);
                // set side labels (if any)
                if (label.IsArea(geomIndex))
                {
                    var leftLoc   = label.GetLocation(geomIndex, Geometries.Position.Left);
                    var rightLoc  = label.GetLocation(geomIndex, Geometries.Position.Right);
                    // if there is a right location, that is the next location to propagate
                    if (rightLoc != Location.Null)
                    {
                        if (rightLoc != currLoc)
                            throw new TopologyException("side location conflict", e.Coordinate);
                        if (leftLoc == Location.Null)
                            Assert.ShouldNeverReachHere("found single null side (at " + e.Coordinate + ")");
                        currLoc = leftLoc;
                    }
                    else
                    {
                        /* RHS is null - LHS must be null too.
                        *  This must be an edge from the other point, which has no location
                        *  labelling for this point.  This edge must lie wholly inside or outside
                        *  the other point (which is determined by the current location).
                        *  Assign both sides to be the current location.
                        */
                        Assert.IsTrue(label.GetLocation(geomIndex, Geometries.Position.Left) == Location.Null, "found single null side");
                        label.SetLocation(geomIndex, Geometries.Position.Right, currLoc);
                        label.SetLocation(geomIndex, Geometries.Position.Left, currLoc);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eSearch"></param>
        /// <returns></returns>
        public int FindIndex(EdgeEnd eSearch)
        {
            GetEnumerator();   // force edgelist to be computed
            for (int i = 0; i < edgeList.Count; i++ )
            {
                var e = edgeList[i];
                if (e == eSearch)
                    return i;
            }
            return -1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public virtual void Write(StreamWriter outstream)
        {
            outstream.WriteLine($"EdgeEndStar:   {Coordinate}");
            foreach (var e in Edges)
                e.Write(outstream);
        }

        /// <inheritdoc cref="object.ToString()"/>>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.AppendLine($"EdgeEndStar:   {Coordinate}");
            foreach (var e in this)
                buf.AppendLine(e.ToString());
            return buf.ToString();
        }
    }
}
