using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// An <see cref="EdgeEndStar{TCoordinate}"/> is an ordered list of 
    /// <see cref="EdgeEnd{TCoordinate}"/> around a node.
    /// They are maintained in CCW order (starting with the positive x-axis) 
    /// around the node for efficient lookup and topology building.
    /// </summary>
    public abstract class EdgeEndStar<TCoordinate> : IEnumerable<EdgeEnd<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        // A map which maintains the edges in sorted order around the node.
        private readonly SortedList<EdgeEnd<TCoordinate>, EdgeEnd<TCoordinate>> _edgeMap
            = new SortedList<EdgeEnd<TCoordinate>, EdgeEnd<TCoordinate>>();

        // A list of all outgoing edges in the result, in CCW order.
        private readonly List<EdgeEnd<TCoordinate>> _edgeList 
            = new List<EdgeEnd<TCoordinate>>();

        // The location of the point for this star in Geometry 0's area.
        private Locations _ptInAreaLocation0;

        // The location of the point for this star in Geometry 1's area.
        private Locations _ptInAreaLocation1;

        public override String ToString()
        {
            return "Edge ends at: " + Coordinate +
                   "Degree: " + Degree;
        }

        /// <summary> 
        /// Insert a EdgeEnd into this EdgeEndStar.
        /// </summary>
        public abstract void Insert(EdgeEnd<TCoordinate> e);

        /// <summary> 
        /// Insert an EdgeEnd into the map, and clear the <see cref="Edges"/> cache,
        /// since the list of edges has now changed.
        /// </summary>
        protected void InsertEdgeEnd(EdgeEnd<TCoordinate> e, EdgeEnd<TCoordinate> edgeEnd)
        {
            // Diego Guidi says: i have inserted this line because if 
            // i try to add an object already present
            // in the list, a System.ArgumentException was thrown.
            if (_edgeMap.ContainsKey(e))
            {
                return;
            }

            _edgeMap.Add(e, edgeEnd);
            _edgeList.Clear();   // edge list has changed - clear the cache
        }

        /// <returns>
        /// Gets the coordinate for the node this star is based at.
        /// </returns>
        public TCoordinate Coordinate
        {
            get
            {
                return _edgeList.Count == 0 
                    ? default(TCoordinate) 
                    : _edgeList[0].Coordinate;
            }
        }

        public Int32 Degree
        {
            get { return _edgeMap.Count; }
        }

        public IEnumerator<EdgeEnd<TCoordinate>> GetEnumerator()
        {
            ComputeEdgeList();

            foreach (EdgeEnd<TCoordinate> edge in _edgeList)
            {
                yield return edge;
            }
        }

        public IEnumerable<EdgeEnd<TCoordinate>> Edges
        {
            get
            {
                foreach (EdgeEnd<TCoordinate> edgeEnd in _edgeList)
                {
                    yield return edgeEnd;
                }
            }
        }

        public EdgeEnd<TCoordinate> GetNextCW(EdgeEnd<TCoordinate> ee)
        {
            Int32 i = _edgeList.IndexOf(ee);
            Int32 nextCWIndex = i - 1;

            if (i == 0)
            {
                nextCWIndex = _edgeList.Count - 1;
            }

            return _edgeList[nextCWIndex];
        }

        public void ComputeLabeling(params GeometryGraph<TCoordinate>[] geom)
        {
            ComputeLabeling(geom as IEnumerable<GeometryGraph<TCoordinate>>);
        }

        public virtual void ComputeLabeling(IEnumerable<GeometryGraph<TCoordinate>> geom)
        {
            IBoundaryNodeRule boundaryNodeRule = Slice.GetFirst(geom).BoundaryNodeRule;

            computeEdgeEndLabels(boundaryNodeRule);

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
            * there would have been a parallel edge from the Geometry at this node also labeled Boundary
            * and this edge would have been labeled in the previous step.
            * 
            * This code causes a problem when dimensional collapses are present, since it may try and
            * determine the location of a node where a dimensional collapse has occurred.
            * The point should be considered to be on the Exterior
            * of the polygon, but locate() will return Interior, since it is passed
            * the original Geometry, not the collapsed version.
            *
            * If there are incident edges which are Line edges labeled Boundary,
            * then they must be edges resulting from dimensional collapses.
            * In this case the other edges can be labeled Exterior for this Geometry.
            *
            * MD 8/11/01 - NOT True!  The collapsed edges may in fact be in the interior of the Geometry,
            * which means the other edges should be labeled Interior for this Geometry.
            * Not sure how solve this...  Possibly labeling needs to be split into several phases:
            * area label propagation, symLabel merging, then finally null label resolution.
            */
            Boolean[] hasDimensionalCollapseEdge = { false, false };

            foreach (EdgeEnd<TCoordinate> e in this)
            {
                if(e.Label == null)
                {
                    continue;
                }

                Label label = e.Label.Value;

                for (Int32 geometryIndex = 0; geometryIndex < 2; geometryIndex++)
                {
                    if (label.IsLine(geometryIndex)
                        && label[geometryIndex].On == Locations.Boundary)
                    {
                        hasDimensionalCollapseEdge[geometryIndex] = true;
                    }
                }
            }

            foreach (EdgeEnd<TCoordinate> e in this)
            {
                if (e.Label == null)
                {
                    continue;
                }

                Label label = e.Label.Value;

                for (Int32 geometryIndex = 0; geometryIndex < 2; geometryIndex++)
                {
                    if (label.AreAnyNone(geometryIndex))
                    {
                        Locations loc;

                        if (hasDimensionalCollapseEdge[geometryIndex])
                        {
                            loc = Locations.Exterior;
                        }
                        else
                        {
                            TCoordinate p = e.Coordinate;
                            loc = GetLocation(geometryIndex, p, geom);
                        }

                        label = Label.SetAllPositionsIfNone(label, geometryIndex, loc);
                    }
                }

                e.Label = label;
            }
        }

        public Locations GetLocation(Int32 geometryIndex, 
                                     TCoordinate p, 
                                     IEnumerable<GeometryGraph<TCoordinate>> geometries)
        {
            Locations location = geometryIndex == 0 
                ? _ptInAreaLocation0 
                : _ptInAreaLocation1;

            // compute location only on demand
            if (location == Locations.None)
            {
                IGeometry<TCoordinate> g = Slice.GetAt(geometries, geometryIndex).Geometry;
                location = SimplePointInAreaLocator.Locate(p, g);

                if (geometryIndex == 0)
                {
                    _ptInAreaLocation0 = location;
                }
                else
                {
                    _ptInAreaLocation1 = location;
                }
            }

            return location;
        }

        public Boolean IsAreaLabelsConsistent(IBoundaryNodeRule boundaryNodeRule)
        {
            computeEdgeEndLabels(boundaryNodeRule);
            return checkAreaLabelsConsistent(0);
        }

        public void PropagateSideLabels(Int32 geomIndex)
        {
            // Since edges are stored in CCW order around the node,
            // As we move around the ring we move from the right to the left side of the edge
            Locations startLoc = Locations.None;

            // initialize loc to location of last Curve side (if any)
            foreach (EdgeEnd<TCoordinate> e in this)
            {
                if (e.Label == null)
                {
                    continue;
                }

                Label label = e.Label.Value;

                if (label.IsArea(geomIndex) 
                    && label[geomIndex, Positions.Left] != Locations.None)
                {
                    startLoc = label[geomIndex, Positions.Left];
                }
            }

            // no labeled sides found, so no labels to propagate
            if (startLoc == Locations.None)
            {
                return;
            }

            Locations currLoc = startLoc;

            foreach (EdgeEnd<TCoordinate> e in this)
            {
                if (e.Label == null)
                {
                    continue;
                }

                Label label = e.Label.Value;

                // set null On values to be in current location
                if (label[geomIndex, Positions.On] == Locations.None)
                {
                    Locations left = label[geomIndex, Positions.Left];
                    Locations right = label[geomIndex, Positions.Right];
                    label = new Label(label, geomIndex, currLoc, left, right);
                }

                // set side labels (if any)
                if (label.IsArea(geomIndex))
                {
                    Locations left = label[geomIndex, Positions.Left];
                    Locations right = label[geomIndex, Positions.Right];

                    // if there is a right location, that is the next location to propagate
                    if (right != Locations.None)
                    {
                        if (right != currLoc)
                        {
                            throw new TopologyException("Side location conflict", e.Coordinate);
                        }

                        if (left == Locations.None)
                        {
                            Assert.ShouldNeverReachHere("Found single null side (at " + e.Coordinate + ").");
                        }

                        currLoc = left;
                    }
                    else
                    {
                        /* RHS is null - LHS must be null too.
                        *  This must be an edge from the other point, which has no location
                        *  labeling for this point.  This edge must lie wholly inside or outside
                        *  the other point (which is determined by the current location).
                        *  Assign both sides to be the current location.
                        */
                        Assert.IsTrue(label[geomIndex, Positions.Left] == Locations.None,
                                      "found single null side");

                        Locations on = label[geomIndex, Positions.On];
                        label = new Label(label, geomIndex, on, currLoc, currLoc);
                    }
                }

                e.Label = label;
            }
        }

        public Int32 FindIndex(EdgeEnd<TCoordinate> search)
        {
            ComputeEdgeList();

            return _edgeList.FindIndex(delegate(EdgeEnd<TCoordinate> match)
                                {
                                    return match == search;
                                });
        }

        protected List<EdgeEnd<TCoordinate>> EdgesInternal
        {
            get
            {
                ComputeEdgeList();
                return _edgeList;
            }
        }

        protected IDictionary<EdgeEnd<TCoordinate>, EdgeEnd<TCoordinate>> EdgeMap
        {
            get { return _edgeMap; }
        }

        protected void ComputeEdgeList()
        {
            if (_edgeList.Count == 0)
            {
                _edgeList.AddRange(_edgeMap.Values);
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private helper members
        private void computeEdgeEndLabels(IBoundaryNodeRule boundaryNodeRule)
        {
            // Compute edge label for each EdgeEnd
            foreach (EdgeEnd<TCoordinate> edgeEnd in this)
            {
                edgeEnd.ComputeLabel(boundaryNodeRule);
            }
        }

        private Boolean checkAreaLabelsConsistent(Int32 geomIndex)
        {
            // Since edges are stored in CCW order around the node,
            // as we move around the ring we move from the right
            // to the left side of the edge
            IList<EdgeEnd<TCoordinate>> edges = _edgeList;

            // if no edges, trivially consistent
            if (edges.Count <= 0)
            {
                return true;
            }

            // initialize startLoc to location of last Curve side (if any)
            Int32 lastEdgeIndex = edges.Count - 1;
            Debug.Assert(edges[lastEdgeIndex].Label != null);
            Label startLabel = edges[lastEdgeIndex].Label.Value;
            Locations startLoc = startLabel[geomIndex, Positions.Left];

            Assert.IsTrue(startLoc != Locations.None, "Found unlabelled area edge");

            Locations currLoc = startLoc;

            foreach (EdgeEnd<TCoordinate> e in this)
            {
                if (e.Label == null)
                {
                    continue;
                }

                Label label = e.Label.Value;

                // we assume that we are only checking a area
                Assert.IsTrue(label.IsArea(geomIndex), "Found non-area edge");
                Locations leftLoc = label[geomIndex, Positions.Left];
                Locations rightLoc = label[geomIndex, Positions.Right];

                // check that edge is really a boundary between inside and outside!
                if (leftLoc == rightLoc)
                {
                    return false;
                }

                // check side location conflict                 
                if (rightLoc != currLoc)
                {
                    return false;
                }

                currLoc = leftLoc;
            }

            return true;
        }
        #endregion
    }
}