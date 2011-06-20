using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// An ordered list of <see cref="EdgeEndBundle{TCoordinate}"/>s around a 
    /// <see cref="RelateNode{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// They are maintained in CCW order (starting with the positive x-axis) 
    /// around the node for efficient lookup and topology building.
    /// </remarks>
    public class EdgeEndBundleStar<TCoordinate> : EdgeEndStar<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Insert a <see cref="EdgeEnd{TCoordinate}"/> in order in the list.
        /// If there is an existing <see cref="EdgeEndBundle{TCoordinate}"/> 
        /// which is parallel, <paramref name="e"/> is added to the bundle.
        /// Otherwise, a new <see cref="EdgeEndBundle{TCoordinate}"/>
        /// is created to contain it.
        /// </summary>
        /// <param name="e">
        /// The <see cref="EdgeEnd{TCoordinate}"/> to add to the list.
        /// </param>
        public override void Insert(EdgeEnd<TCoordinate> e)
        {
            EdgeEnd<TCoordinate> ee;

            if (!EdgeMap.TryGetValue(e, out ee))
            {
                ee = new EdgeEndBundle<TCoordinate>(e);
                InsertEdgeEnd(e, ee);
            }
            else
            {
                EdgeEndBundle<TCoordinate> eb = ee as EdgeEndBundle<TCoordinate>;
                Debug.Assert(eb != null);
                eb.Insert(e);
            }
        }

        /// <summary>
        /// Update the <see cref="IntersectionMatrix"/> with the 
        /// contribution for the <see cref="EdgeEnd{TCoordinate}"/>s 
        /// around the node.
        /// </summary>
        /// <param name="im">
        /// The <see cref="IntersectionMatrix"/> to update.
        /// </param>
        public void UpdateIntersectionMatrix(IntersectionMatrix im)
        {
            foreach (EdgeEndBundle<TCoordinate> end in this)
            {
                end.UpdateIntersectionMatrix(im);
            }
        }
    }
}