using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// A collection of EdgeStubs which obey the following invariant:
    /// They originate at the same node and have the same direction.
    /// Contains all <c>EdgeEnd</c>s which start at the same point and are parallel.
    /// </summary>
    public class EdgeEndBundle : EdgeEnd
    {
        private IList edgeEnds = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public EdgeEndBundle(EdgeEnd e) : base(e.Edge, e.Coordinate, e.DirectedCoordinate, new Label(e.Label))
        {
            Insert(e);
        }       

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() 
        { 
            return edgeEnds.GetEnumerator(); 
        }

        /// <summary>
        /// 
        /// </summary>
        public IList EdgeEnds
        {
            get
            {
                return edgeEnds; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void Insert(EdgeEnd e)
        {
            // Assert: start point is the same
            // Assert: direction is the same
            edgeEnds.Add(e);
        }

        /// <summary>
        /// This computes the overall edge label for the set of
        /// edges in this EdgeStubBundle.  It essentially merges
        /// the ON and side labels for each edge. 
        /// These labels must be compatible
        /// </summary>
        public override void ComputeLabel()
        {
            // create the label.  If any of the edges belong to areas,
            // the label must be an area label
            bool isArea = false;
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); )
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                if (e.Label.IsArea())
                    isArea = true;
            }
            if (isArea)
                 label = new Label(Locations.Null, Locations.Null, Locations.Null);
            else label = new Label(Locations.Null);

            // compute the On label, and the side labels if present
            for (int i = 0; i < 2; i++)
            {
                ComputeLabelOn(i);
                if (isArea)
                    ComputeLabelSides(i);
            }

        }

        /// <summary>
        /// Compute the overall ON location for the list of EdgeStubs.
        /// (This is essentially equivalent to computing the self-overlay of a single Geometry)
        /// edgeStubs can be either on the boundary (eg Polygon edge)
        /// OR in the interior (e.g. segment of a LineString)
        /// of their parent Geometry.
        /// In addition, GeometryCollections use the mod-2 rule to determine
        /// whether a segment is on the boundary or not.
        /// Finally, in GeometryCollections it can still occur that an edge is both
        /// on the boundary and in the interior (e.g. a LineString segment lying on
        /// top of a Polygon edge.) In this case as usual the Boundary is given precendence.
        /// These observations result in the following rules for computing the ON location:
        ///  if there are an odd number of Bdy edges, the attribute is Bdy
        ///  if there are an even number >= 2 of Bdy edges, the attribute is Int
        ///  if there are any Int edges, the attribute is Int
        ///  otherwise, the attribute is Null.
        /// </summary>
        /// <param name="geomIndex"></param>
        private void ComputeLabelOn(int geomIndex)
        {
            // compute the On location value
            int boundaryCount = 0;
            bool foundInterior = false;
            Locations loc = Locations.Null;

            for (IEnumerator it = GetEnumerator(); it.MoveNext(); )
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                loc = e.Label.GetLocation(geomIndex);
                if (loc == Locations.Boundary) 
                    boundaryCount++;
                if (loc == Locations.Interior) 
                    foundInterior = true;
            }

            loc = Locations.Null;
            if (foundInterior) 
                loc = Locations.Interior;
            if (boundaryCount > 0) 
                loc = GeometryGraph.DetermineBoundary(boundaryCount);            
            label.SetLocation(geomIndex, loc);
        }

        /// <summary>
        /// Compute the labelling for each side
        /// </summary>
        /// <param name="geomIndex"></param>
        private void ComputeLabelSides(int geomIndex)
        {
            ComputeLabelSide(geomIndex, Positions.Left);
            ComputeLabelSide(geomIndex, Positions.Right);
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
        private void ComputeLabelSide(int geomIndex, Positions side)
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); )
            {
                EdgeEnd e = (EdgeEnd) it.Current;
                if (e.Label.IsArea()) 
                {
                    Locations loc = e.Label.GetLocation(geomIndex, side);
                    if (loc == Locations.Interior)
                    {
                        label.SetLocation(geomIndex, side, Locations.Interior);
                        return;
                    }
                    else if (loc == Locations.Exterior)
                        label.SetLocation(geomIndex, side, Locations.Exterior);
                }
            }
        }

        /// <summary>
        /// Update the IM with the contribution for the computed label for the EdgeStubs.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            Edge.UpdateIM(label, im);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public override void Write(StreamWriter outstream)
        {
            outstream.WriteLine("EdgeEndBundle--> Label: " + label);
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); )
            {
                EdgeEnd ee = (EdgeEnd) it.Current;
                ee.Write(outstream);
                outstream.WriteLine();
            }
        }
    }
}
