using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;
using GeoAPI.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// An ordered list of <see cref="EdgeEndBundle{TCoordinate}"/>s around a 
    /// <see cref="RelateNode{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// They are maintained in CCW order (starting with the positive x-axis) around the node
    /// for efficient lookup and topology building.
    /// </remarks>
    public class EdgeEndBundleStar<TCoordinate> : EdgeEndStar<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Insert a EdgeEnd in order in the list.
        /// If there is an existing EdgeStubBundle which is parallel, the EdgeEnd is
        /// added to the bundle.  Otherwise, a new EdgeEndBundle is created
        /// to contain the EdgeEnd.
        /// </summary>
        public override void Insert(EdgeEnd<TCoordinate> e)
        {
            EdgeEndBundle<TCoordinate> eb = EdgeMap[e] as EdgeEndBundle<TCoordinate>;

            if (eb == null)
            {
                eb = new EdgeEndBundle<TCoordinate>(e);
                InsertEdgeEnd(e, eb);
            }
            else
            {
                eb.Insert(e);
            }
        }

        /// <summary>
        /// Update the <see cref="IntersectionMatrix"/> with the contribution for the 
        /// <see cref="EdgeEnd{TCoordinate}"/>s around the node.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIntersectionMatrix(IntersectionMatrix im)
        {
            foreach (EdgeEndBundle<TCoordinate> end in this)
            {
                end.UpdateIntersectionMatrix(im);
            }
        }
    }
}