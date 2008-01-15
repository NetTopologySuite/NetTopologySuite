using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// A collection of <see cref="EdgeEnd{TCoordinate}"/>s which obey the following invariant:
    /// They originate at the same node and have the same direction.
    /// Contains all <c>EdgeEnd</c>s which start at the same point and are parallel.
    /// </summary>
    public class EdgeEndBundle<TCoordinate> : EdgeEnd<TCoordinate>, IEnumerable<EdgeEnd<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly List<EdgeEnd<TCoordinate>> _edgeEnds = new List<EdgeEnd<TCoordinate>>();

        public EdgeEndBundle(EdgeEnd<TCoordinate> e)
            : base(e.Edge, e.Coordinate, e.DirectedCoordinate, e.Label)
        {
            Insert(e);
        }

        public IEnumerator<EdgeEnd<TCoordinate>> GetEnumerator()
        {
            foreach (EdgeEnd<TCoordinate> edgeEnd in _edgeEnds)
            {
                yield return edgeEnd;
            }
        }

        public ReadOnlyCollection<EdgeEnd<TCoordinate>> EdgeEnds
        {
            get { return _edgeEnds.AsReadOnly(); }
        }

        public void Insert(EdgeEnd<TCoordinate> e)
        {
            // Assert: start point is the same
            // Assert: direction is the same
            _edgeEnds.Add(e);
        }

        /// <summary>
        /// This computes the overall edge label for the set of
        /// edges in this EdgeStubBundle.  It essentially merges
        /// the ON and side labels for each edge. 
        /// These labels must be compatible.
        /// </summary>
        public override void ComputeLabel()
        {
            // create the label.  If any of the edges belong to areas,
            // the label must be an area label
            Boolean isArea = false;

            foreach (EdgeEnd<TCoordinate> e in _edgeEnds)
            {
                if (e.Label.Value.IsArea())
                {
                    isArea = true;
                    break;
                }
            }

            if (isArea)
            {
                Label = new Label(Locations.None, Locations.None, Locations.None);
            }

            else
            {
                Label = new Label(Locations.None);
            }

            // compute the On label, and the side labels if present
            for (Int32 i = 0; i < 2; i++)
            {
                computeLabelOn(i);
                if (isArea)
                {
                    computeLabelSides(i);
                }
            }
        }

        /// <summary>
        /// Update the intersection matrix with the contribution for 
        /// the computed label for the <see cref="EdgeEnd{TCoordinate}"/>s.
        /// </summary>
        public void UpdateIntersectionMatrix(IntersectionMatrix im)
        {
            Edge<TCoordinate>.UpdateIntersectionMatrix(Label.Value, im);
        }

        public override void Write(StreamWriter outstream)
        {
            outstream.WriteLine("EdgeEndBundle--> Label: " + Label);
            foreach (EdgeEnd<TCoordinate> edgeEnd in _edgeEnds)
            {
                edgeEnd.Write(outstream);
                outstream.WriteLine();
            }
        }

        // Compute the overall ON location for the list of EdgeStubs.
        // (This is essentially equivalent to computing the self-overlay of a single Geometry)
        // edgeStubs can be either on the boundary (eg Polygon edge)
        // OR in the interior (e.g. segment of a LineString)
        // of their parent Geometry.
        // In addition, GeometryCollections use the mod-2 rule to determine
        // whether a segment is on the boundary or not.
        // Finally, in GeometryCollections it can still occur that an edge is both
        // on the boundary and in the interior (e.g. a LineString segment lying on
        // top of a Polygon edge.) In this case as usual the Boundary is given precendence.
        // These observations result in the following rules for computing the ON location:
        //  if there are an odd number of Bdy edges, the attribute is Bdy
        //  if there are an even number >= 2 of Bdy edges, the attribute is Int
        //  if there are any Int edges, the attribute is Int
        //  otherwise, the attribute is Null.
        private void computeLabelOn(Int32 geomIndex)
        {
            // compute the On location value
            Int32 boundaryCount = 0;
            Boolean foundInterior = false;
            Locations loc;

            foreach (EdgeEnd<TCoordinate> e in _edgeEnds)
            {
                loc = e.Label.Value[geomIndex][Positions.On];

                if (loc == Locations.Boundary)
                {
                    boundaryCount++;
                }

                if (loc == Locations.Interior)
                {
                    foundInterior = true;
                }
            }

            loc = Locations.None;

            if (foundInterior)
            {
                loc = Locations.Interior;
            }

            if (boundaryCount > 0)
            {
                loc = GeometryGraph<TCoordinate>.DetermineBoundary(boundaryCount);
            }

            Label = new Label(Label.Value, geomIndex, loc);
        }

        // Compute the labeling for each side
        private void computeLabelSides(Int32 geomIndex)
        {
            computeLabelSide(geomIndex, Positions.Left);
            computeLabelSide(geomIndex, Positions.Right);
        }

        // To compute the summary label for a side, the algorithm is:
        // FOR all edges
        // IF any edge's location is Interior for the side, side location = Interior
        // ELSE IF there is at least one Exterior attribute, side location = Exterior
        // ELSE  side location = Null
        // Note that it is possible for two sides to have apparently contradictory information
        // i.e. one edge side may indicate that it is in the interior of a point, while
        // another edge side may indicate the exterior of the same point.  This is
        // not an incompatibility - GeometryCollections may contain two Polygons that touch
        // along an edge.  This is the reason for Interior-primacy rule above - it
        // results in the summary label having the Geometry interior on both sides.
        private void computeLabelSide(Int32 geomIndex, Positions side)
        {
            foreach (EdgeEnd<TCoordinate> e in _edgeEnds)
            {
                if (e.Label.Value.IsArea())
                {
                    Locations loc = e.Label.Value[geomIndex, side];

                    if (loc == Locations.Interior)
                    {
                        Label = new Label(Label.Value, geomIndex, side, loc);
                        return;
                    }
                    else if (loc == Locations.Exterior)
                    {
                        Label = new Label(Label.Value, geomIndex, side, loc);
                    }
                }
            }
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}