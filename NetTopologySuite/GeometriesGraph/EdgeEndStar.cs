using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// An <see cref="EdgeEndStar{TCoordinate, TEdgeEnd}"/> is an ordered list of 
    /// <see cref="EdgeEnd{TCoordinate}"/> around a node.
    /// They are maintained in CCW order (starting with the positive x-axis) 
    /// around the node for efficient lookup and topology building.
    /// </summary>
    public abstract class EdgeEndStar<TCoordinate, TEdgeEnd> : IEnumerable<EdgeEnd<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
        where TEdgeEnd : EdgeEnd<TCoordinate>
    {
        // A map which maintains the edges in sorted order around the node.
        private readonly SortedList<EdgeEnd<TCoordinate>, TEdgeEnd> _edgeMap
            = new SortedList<EdgeEnd<TCoordinate>, TEdgeEnd>();

        // A list of all outgoing edges in the result, in CCW order.
        private readonly List<TEdgeEnd> _edgeList = new List<TEdgeEnd>();

        // The location of the point for this star in Geometry i Areas.
        private Locations[] _ptInAreaLocation = new Locations[] { Locations.None, Locations.None };

        /// <summary> 
        /// Insert a EdgeEnd into this EdgeEndStar.
        /// </summary>
        public abstract void Insert(EdgeEnd<TCoordinate> e);

        /// <summary> 
        /// Insert an EdgeEnd into the map, and clear the <see cref="Edges"/> cache,
        /// since the list of edges has now changed.
        /// </summary>
        protected void InsertEdgeEnd(EdgeEnd<TCoordinate> e, TEdgeEnd obj)
        {
            // Diego Guidi says: i have inserted this line because if 
            // i try to add an object already present
            // in the list, a System.ArgumentException was thrown.
            if (_edgeMap.ContainsKey(e))
            {
                return;
            }

            _edgeMap.Add(e, obj);
            _edgeList.Clear();   // edge list has changed - clear the cache
        }

        /// <returns>
        /// Gets the coordinate for the node this star is based at.
        /// </returns>
        public TCoordinate Coordinate
        {
            get
            {
                if (_edgeList.Count == 0)
                {
                    return default(TCoordinate);
                }
                else
                {
                    return _edgeList[0].Coordinate;
                }
            }
        }

        public Int32 Degree
        {
            get { return _edgeMap.Count; }
        }

        public IEnumerator<EdgeEnd<TCoordinate>> GetEnumerator()
        {
            ComputeEdgeList();

            foreach (TEdgeEnd edge in _edgeList)
            {
                yield return edge;
            }
        }

        public ReadOnlyCollection<TEdgeEnd> Edges
        {
            get
            {
                return _edgeList.AsReadOnly();
            }
        }

        public EdgeEnd<TCoordinate> GetNextCW(EdgeEnd<TCoordinate> ee)
        {
            Int32 i = _edgeList.IndexOf(ee);
            Int32 iNextCW = i - 1;

            if (i == 0)
            {
                iNextCW = _edgeList.Count - 1;
            }

            return _edgeList[iNextCW];
        }

        public virtual void ComputeLabeling(IEnumerable<GeometryGraph<TCoordinate>> geom)
        {
            computeEdgeEndLabels();

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
                Label label = e.Label;

                for (Int32 geometryIndex = 0; geometryIndex < 2; geometryIndex++)
                {
                    if (label.IsLine(geometryIndex)
                        && label.GetLocation(geometryIndex) == Locations.Boundary)
                    {
                        hasDimensionalCollapseEdge[geometryIndex] = true;
                    }
                }
            }

            foreach (EdgeEnd<TCoordinate> e in this)
            {
                Label label = e.Label;

                for (Int32 geometryIndex = 0; geometryIndex < 2; geometryIndex++)
                {
                    if (label.IsAnyNull(geometryIndex))
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

                        label.SetAllLocationsIfNull(geometryIndex, loc);
                    }
                }
            }
        }

        public Locations GetLocation(Int32 geomIndex, TCoordinate p, IEnumerable<GeometryGraph<TCoordinate>> geom)
        {
            // compute location only on demand
            if (_ptInAreaLocation[geomIndex] == Locations.None)
            {
                _ptInAreaLocation[geomIndex] = SimplePointInAreaLocator.Locate(p, geom[geomIndex].Geometry);
            }

            return _ptInAreaLocation[geomIndex];
        }

        public Boolean IsAreaLabelsConsistent
        {
            get
            {
                computeEdgeEndLabels();
                return checkAreaLabelsConsistent(0);
            }
        }

        public void PropagateSideLabels(Int32 geomIndex)
        {
            // Since edges are stored in CCW order around the node,
            // As we move around the ring we move from the right to the left side of the edge
            Locations startLoc = Locations.None;

            // initialize loc to location of last Curve side (if any)
            foreach (EdgeEnd<TCoordinate> e in this)
            {
                Label label = e.Label;

                if (label.IsArea(geomIndex) && label.GetLocation(geomIndex, Positions.Left) != Locations.None)
                {
                    startLoc = label.GetLocation(geomIndex, Positions.Left);
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
                Label label = e.Label;

                // set null On values to be in current location
                if (label.GetLocation(geomIndex, Positions.On) == Locations.None)
                {
                    label.SetLocation(geomIndex, Positions.On, currLoc);
                }

                // set side labels (if any)
                if (label.IsArea(geomIndex))
                {
                    Locations leftLoc = label.GetLocation(geomIndex, Positions.Left);
                    Locations rightLoc = label.GetLocation(geomIndex, Positions.Right);

                    // if there is a right location, that is the next location to propagate
                    if (rightLoc != Locations.None)
                    {
                        if (rightLoc != currLoc)
                        {
                            throw new TopologyException("side location conflict", e.Coordinate);
                        }

                        if (leftLoc == Locations.None)
                        {
                            Assert.ShouldNeverReachHere("found single null side (at " + e.Coordinate + ")");
                        }

                        currLoc = leftLoc;
                    }
                    else
                    {
                        /* RHS is null - LHS must be null too.
                        *  This must be an edge from the other point, which has no location
                        *  labeling for this point.  This edge must lie wholly inside or outside
                        *  the other point (which is determined by the current location).
                        *  Assign both sides to be the current location.
                        */
                        Assert.IsTrue(label.GetLocation(geomIndex, Positions.Left) == Locations.None,
                                      "found single null side");
                        label.SetLocation(geomIndex, Positions.Right, currLoc);
                        label.SetLocation(geomIndex, Positions.Left, currLoc);
                    }
                }
            }
        }

        public Int32 FindIndex(EdgeEnd<TCoordinate> search)
        {
            ComputeEdgeList();

            return _edgeList.FindIndex(delegate(TEdgeEnd match)
                                {
                                    return match == search;
                                });
        }

        public virtual void Write(StreamWriter outstream)
        {
            foreach (EdgeEnd<TCoordinate> edgeEnd in this)
            {
                edgeEnd.Write(outstream);
            }
        }

        protected List<TEdgeEnd> EdgesInternal
        {
            get
            {
                ComputeEdgeList();
                return _edgeList;
            }
        }

        protected void ComputeEdgeList()
        {
            if (_edgeList.Count == 0)
            {
                _edgeList.AddRange(_edgeMap.Values);
            }
        }

        private void computeEdgeEndLabels()
        {
            // Compute edge label for each EdgeEnd
            foreach (EdgeEnd<TCoordinate> edgeEnd in this)
            {
                edgeEnd.ComputeLabel();
            }
        }

        private Boolean checkAreaLabelsConsistent(Int32 geomIndex)
        {
            // Since edges are stored in CCW order around the node,
            // as we move around the ring we move from the right to the left side of the edge
            IList<TEdgeEnd> edges = _edgeList;

            // if no edges, trivially consistent
            if (edges.Count <= 0)
            {
                return true;
            }

            // initialize startLoc to location of last Curve side (if any)
            Int32 lastEdgeIndex = edges.Count - 1;
            Label startLabel = edges[lastEdgeIndex].Label;
            Locations startLoc = startLabel.GetLocation(geomIndex, Positions.Left);

            Assert.IsTrue(startLoc != Locations.None, "Found unlabelled area edge");

            Locations currLoc = startLoc;

            foreach (EdgeEnd<TCoordinate> e in this)
            {
                Label label = e.Label;

                // we assume that we are only checking a area
                Assert.IsTrue(label.IsArea(geomIndex), "Found non-area edge");
                Locations leftLoc = label.GetLocation(geomIndex, Positions.Left);
                Locations rightLoc = label.GetLocation(geomIndex, Positions.Right);

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

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}