using System;
using GeoAPI.Coordinates;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Creates nodes for use in the <see cref="PlanarGraph{TCoordinate}"/>s 
    /// constructed during overlay operations.
    /// </summary>
    public class OverlayNodeFactory<TCoordinate> : NodeFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public override Node<TCoordinate> CreateNode(TCoordinate coord)
        {
            return new Node<TCoordinate>(coord, new DirectedEdgeStar<TCoordinate>());
        }
    }
}