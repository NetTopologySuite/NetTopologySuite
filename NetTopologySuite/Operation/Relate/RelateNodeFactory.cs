using System;
using GeoAPI.Coordinates;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Used by the <see cref="NodeMap{TCoordinate}"/> in a 
    /// <see cref="RelateNodeGraph{TCoordinate}"/> to create 
    /// <see cref="RelateNode{TCoordinate}"/>s.
    /// </summary>
    public class RelateNodeFactory<TCoordinate> : NodeFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public override Node<TCoordinate> CreateNode(TCoordinate coord)
        {
            return new RelateNode<TCoordinate>(coord, new EdgeEndBundleStar<TCoordinate>());
        }
    }
}