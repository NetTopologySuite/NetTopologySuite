using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// A <see cref="RelateNode{TCoordinate}"/> is a <see cref="Node{TCoordinate}"/> 
    /// that maintains an ordered list of <see cref="EdgeEndBundle{TCoordinate}"/>s 
    /// for the edges that are incident on it.
    /// </summary>
    public class RelateNode<TCoordinate> : Node<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        public RelateNode(TCoordinate coord, EdgeEndStar<TCoordinate> edges) :
            base(coord, edges)
        {
        }

        /// <summary>
        /// Update the <see cref="IntersectionMatrix"/> with the 
        /// contribution for this component. A component only 
        /// contributes if it has a labeling for both parent geometries.
        /// </summary>
        public override void ComputeIntersectionMatrix(IntersectionMatrix im)
        {
            Debug.Assert(Label != null);
            Label label = Label.Value;
            im.SetAtLeastIfValid(label[0].On, label[1].On, Dimensions.Point);
        }

        /// <summary>
        /// Update the IM with the contribution for the 
        /// <see cref="EdgeEnd{TCoordinate}"/>s incident on this node.
        /// </summary>
        public void UpdateIntersectionMatrixFromEdges(IntersectionMatrix im)
        {
            EdgeEndBundleStar<TCoordinate> star = Edges as EdgeEndBundleStar<TCoordinate>;
            Debug.Assert(star != null);
            star.UpdateIntersectionMatrix(im);
        }
    }
}