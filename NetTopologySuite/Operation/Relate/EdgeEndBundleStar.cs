using System;
using System.Collections;
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
        /// <param name="e"></param>
        public override void Insert(EdgeEnd<TCoordinate> e)
        {
            EdgeEndBundle<TCoordinate> eb = EdgeMap[e];

            if (eb == null)
            {
                eb = new EdgeEndBundle(e);
                InsertEdgeEnd(e, eb);
            }
            else
            {
                eb.Insert(e);
            }
        }

        /// <summary>
        /// Update the IM with the contribution for the EdgeStubs around the node.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                EdgeEndBundle esb = (EdgeEndBundle) it.Current;
                esb.UpdateIM(im);
            }
        }
    }
}