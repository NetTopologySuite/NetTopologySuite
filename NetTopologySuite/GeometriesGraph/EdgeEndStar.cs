using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
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
        protected IDictionary edgeMap = new SortedList();

        /// <summary> 
        /// A list of all outgoing edges in the result, in CCW order.
        /// </summary>
        protected IList edgeList;

        /// <summary>
        /// The location of the point for this star in Geometry i Areas.
        /// </summary>
        private Locations[] ptInAreaLocation = new Locations[] { Locations.Null, Locations.Null };

        /// <summary>
        /// 
        /// </summary>
        public EdgeEndStar() { }

        /// <summary> 
        /// Insert a EdgeEnd into this EdgeEndStar.
        /// </summary>
        /// <param name="e"></param>
        abstract public void Insert(EdgeEnd e);

        /// <summary> 
        /// Insert an EdgeEnd into the map, and clear the edgeList cache,
        /// since the list of edges has now changed.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="obj"></param>
        protected void InsertEdgeEnd(EdgeEnd e, object obj)
        {
            // Diego Guidi says: i have inserted this line because if i try to add an object already present
            // in the list, a System.ArgumentException was thrown.
            if (edgeMap.Contains(e))                            
                return;            
            edgeMap.Add(e, obj);
            edgeList = null;    // edge list has changed - clear the cache
        }

        /// <returns>
        /// The coordinate for the node this star is based at.
        /// </returns>
        public ICoordinate Coordinate
        {
            get
            {
                IEnumerator it = GetEnumerator();
                if (!it.MoveNext())
                    return null;
                EdgeEnd e = (EdgeEnd) it.Current;
                return e.Coordinate;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get
            {
                return edgeMap.Count;
            }
        }

        /// <summary>
        /// Iterator access to the ordered list of edges is optimized by
        /// copying the map collection to a list.  (This assumes that
        /// once an iterator is requested, it is likely that insertion into
        /// the map is complete).
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return Edges.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        public IList Edges
        {
            get
            {
                if (edgeList == null) 
                    edgeList = new ArrayList(edgeMap.Values);            
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
            IList temp = Edges;
            temp = null;    // Hack for calling property
            int i = edgeList.IndexOf(ee);
            int iNextCW = i - 1;
            if (i == 0)
                iNextCW = edgeList.Count - 1;
            return (EdgeEnd) edgeList[iNextCW];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public virtual void ComputeLabelling(GeometryGraph[] geom)
        {
            ComputeEdgeEndLabels();
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
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                Label label = e.Label;
                for (int geomi = 0; geomi < 2; geomi++) 
                    if (label.IsLine(geomi) && label.GetLocation(geomi) == Locations.Boundary)
                        hasDimensionalCollapseEdge[geomi] = true;                
            }        
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                Label label = e.Label;        
                for (int geomi = 0; geomi < 2; geomi++) 
                {
                    if (label.IsAnyNull(geomi)) 
                    {
                        Locations loc = Locations.Null;
                        if (hasDimensionalCollapseEdge[geomi])
                            loc = Locations.Exterior;                
                        else 
                        {
                            ICoordinate p = e.Coordinate;
                            loc = GetLocation(geomi, p, geom);
                        }
                        label.SetAllLocationsIfNull(geomi, loc);
                    }
                }        
            }        
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeEdgeEndLabels()
        {
            // Compute edge label for each EdgeEnd
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEnd ee = (EdgeEnd) it.Current;
                ee.ComputeLabel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="p"></param>
        /// <param name="geom"></param>
        /// <returns></returns>
        public Locations GetLocation(int geomIndex, ICoordinate p, GeometryGraph[] geom)
        {
            // compute location only on demand
            if (ptInAreaLocation[geomIndex] == Locations.Null) 
                ptInAreaLocation[geomIndex] = SimplePointInAreaLocator.Locate(p, geom[geomIndex].Geometry);            
            return ptInAreaLocation[geomIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAreaLabelsConsistent
        {
            get
            {
                ComputeEdgeEndLabels();
                return CheckAreaLabelsConsistent(0);
            }
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
            IList edges = Edges;
            // if no edges, trivially consistent
            if (edges.Count <= 0)
                return true;
            // initialize startLoc to location of last Curve side (if any)
            int lastEdgeIndex = edges.Count - 1;
            Label startLabel = ((EdgeEnd) edges[lastEdgeIndex]).Label;
            Locations startLoc = startLabel.GetLocation(geomIndex, Positions.Left);
            Assert.IsTrue(startLoc != Locations.Null, "Found unlabelled area edge");

            Locations currLoc = startLoc;
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                Label label = e.Label;
                // we assume that we are only checking a area
                Assert.IsTrue(label.IsArea(geomIndex), "Found non-area edge");
                Locations leftLoc = label.GetLocation(geomIndex, Positions.Left);
                Locations rightLoc = label.GetLocation(geomIndex, Positions.Right);        
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
            Locations startLoc = Locations.Null;
            // initialize loc to location of last Curve side (if any)
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                Label label = e.Label;
                if (label.IsArea(geomIndex) && label.GetLocation(geomIndex, Positions.Left) != Locations.Null)
                    startLoc = label.GetLocation(geomIndex, Positions.Left);
            }
            // no labelled sides found, so no labels to propagate
            if (startLoc == Locations.Null) 
                return;

            Locations currLoc = startLoc;
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); )
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                Label label = e.Label;
                // set null On values to be in current location
                if (label.GetLocation(geomIndex, Positions.On) == Locations.Null)
                    label.SetLocation(geomIndex, Positions.On, currLoc);
                // set side labels (if any)
                if (label.IsArea(geomIndex)) 
                {
                    Locations leftLoc   = label.GetLocation(geomIndex, Positions.Left);
                    Locations rightLoc  = label.GetLocation(geomIndex, Positions.Right);
                    // if there is a right location, that is the next location to propagate
                    if (rightLoc != Locations.Null) 
                    {            
                        if (rightLoc != currLoc)
                            throw new TopologyException("side location conflict", e.Coordinate);
                        if (leftLoc == Locations.Null) 
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
                        Assert.IsTrue(label.GetLocation(geomIndex, Positions.Left) == Locations.Null, "found single null side");
                        label.SetLocation(geomIndex, Positions.Right, currLoc);
                        label.SetLocation(geomIndex, Positions.Left, currLoc);
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
                EdgeEnd e = (EdgeEnd) edgeList[i];
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
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                e.Write(outstream);
            }
        }
    }
}
