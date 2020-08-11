using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// A collection of EdgeStubs which obey the following invariant:
    /// They originate at the same node and have the same direction.
    /// Contains all <c>EdgeEnd</c>s which start at the same point and are parallel.
    /// </summary>
    public class EdgeEndBundle : EdgeEnd
    {
        //private readonly IBoundaryNodeRule _boundaryNodeRule;
        private readonly IList<EdgeEnd> _edgeEnds = new List<EdgeEnd>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="boundaryNodeRule"></param>
        /// <param name="e"></param>
        public EdgeEndBundle(IBoundaryNodeRule boundaryNodeRule, EdgeEnd e)
            : base(e.Edge, e.Coordinate, e.DirectedCoordinate, new Label(e.Label))
        {
            /*
            if (boundaryNodeRule != null)
              this.boundaryNodeRule = boundaryNodeRule;
            else
              boundaryNodeRule = BoundaryNodeRules.OgcSfsBoundaryRule;
            */
            Insert(e);
        }

        public EdgeEndBundle(EdgeEnd e) : this (null, e){}

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator<EdgeEnd> GetEnumerator()
        {
            return _edgeEnds.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        public IList<EdgeEnd> EdgeEnds => _edgeEnds;

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        public void Insert(EdgeEnd e)
        {
            // Assert: start point is the same
            // Assert: direction is the same
            _edgeEnds.Add(e);
        }

        /// <summary>
        /// This computes the overall edge label for the set of
        /// edges in this EdgeStubBundle.  It essentially merges
        /// the ON and side labels for each edge.
        /// These labels must be compatible
        /// </summary>
        /// <param name="boundaryNodeRule"></param>
        public override void ComputeLabel(IBoundaryNodeRule boundaryNodeRule)
        {
            // create the label.  If any of the edges belong to areas,
            // the label must be an area label
            bool isArea = false;
            foreach (var e in _edgeEnds)
            {
                if (e.Label.IsArea())
                    isArea = true;
            }
            if (isArea)
                 Label = new Label(Location.Null, Location.Null, Location.Null);
            else Label = new Label(Location.Null);

            // compute the On label, and the side labels if present
            for (int i = 0; i < 2; i++)
            {
                ComputeLabelOn(i, boundaryNodeRule);
                if (isArea)
                    ComputeLabelSides(i);
            }

        }

        /// <summary>
        /// Compute the overall ON location for the list of EdgeStubs.
        /// (This is essentially equivalent to computing the self-overlay of a single Geometry)
        /// edgeStubs can be either on the boundary (e.g. Polygon edge)
        /// OR in the interior (e.g. segment of a LineString)
        /// of their parent Geometry.
        /// In addition, GeometryCollections use the <see cref="IBoundaryNodeRule"/> to determine
        /// whether a segment is on the boundary or not.
        /// Finally, in GeometryCollections it can occur that an edge is both
        /// on the boundary and in the interior (e.g. a LineString segment lying on
        /// top of a Polygon edge.) In this case the Boundary is given precedence.
        /// These observations result in the following rules for computing the ON location:
        ///  if there are an odd number of Bdy edges, the attribute is Bdy
        ///  if there are an even number >= 2 of Bdy edges, the attribute is Int
        ///  if there are any Int edges, the attribute is Int
        ///  otherwise, the attribute is Null.
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="boundaryNodeRule"></param>
        private void ComputeLabelOn(int geomIndex, IBoundaryNodeRule boundaryNodeRule)
        {
            // compute the On location value
            int boundaryCount = 0;
            bool foundInterior = false;
            Location loc;

            foreach (var e in _edgeEnds)
            {
                loc = e.Label.GetLocation(geomIndex);
                if (loc == Location.Boundary)
                    boundaryCount++;
                if (loc == Location.Interior)
                    foundInterior = true;
            }

            loc = Location.Null;
            if (foundInterior)
                loc = Location.Interior;
            if (boundaryCount > 0)
                loc = GeometryGraph.DetermineBoundary(boundaryNodeRule, boundaryCount);
            Label.SetLocation(geomIndex, loc);
        }

        /// <summary>
        /// Compute the labelling for each side
        /// </summary>
        /// <param name="geomIndex"></param>
        private void ComputeLabelSides(int geomIndex)
        {
            ComputeLabelSide(geomIndex, Position.Left);
            ComputeLabelSide(geomIndex, Position.Right);
        }

        /// <summary>
        /// To compute the summary label for a side, the algorithm is:
        /// FOR all edges
        /// IF any edge's location is Interior for the side, side location = Interior
        /// ELSE IF there is at least one Exterior attribute, side location = Exterior
        /// ELSE  side location = Null
        /// Note that it is possible for two sides to have apparently contradictory information
        /// i.e. one edge side may indicate that it is in the interior of a point, while
        /// another edge side may indicate the exterior of the same point.  This is
        /// not an incompatibility - GeometryCollections may contain two Polygons that touch
        /// along an edge.  This is the reason for Interior-primacy rule above - it
        /// results in the summary label having the Geometry interior on both sides.
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="side"></param>
        private void ComputeLabelSide(int geomIndex, Position side)
        {
            foreach (var e in _edgeEnds)
            {
                if (e.Label.IsArea())
                {
                    var loc = e.Label.GetLocation(geomIndex, side);
                    if (loc == Location.Interior)
                    {
                        Label.SetLocation(geomIndex, side, Location.Interior);
                        return;
                    }
                    if (loc == Location.Exterior)
                        Label.SetLocation(geomIndex, side, Location.Exterior);
                }
            }
        }

        /// <summary>
        /// Update the IM with the contribution for the computed label for the EdgeStubs.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            Edge.UpdateIM(Label, im);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public override void Write(StreamWriter outstream)
        {
            outstream.WriteLine("EdgeEndBundle--> Label: " + Label);
            foreach (var ee in _edgeEnds)
            {
                ee.Write(outstream);
                outstream.WriteLine();
            }
        }
    }
}
