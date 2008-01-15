using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// A RelateNode is a Node that maintains a list of EdgeStubs
    /// for the edges that are incident on it.
    /// </summary>
    public class RelateNode<TCoordinate> : Node<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public RelateNode(TCoordinate coord, EdgeEndStar<TCoordinate> edges) :
            base(coord, edges) {}

        /// <summary>
        /// Update the <see cref="IntersectionMatrix"/> with the 
        /// contribution for this component. A component only 
        /// contributes if it has a labeling for both parent geometries.
        /// </summary>
        public override void ComputeIntersectionMatrix(IntersectionMatrix im)
        {
            Debug.Assert(Label != null);
            im.SetAtLeastIfValid(Label.Value[0].On, Label.Value[1].On, Dimensions.Point);
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