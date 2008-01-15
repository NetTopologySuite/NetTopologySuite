using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Creates nodes for use in the <see cref="PlanarGraph{TCoordinate}"/>s 
    /// constructed during overlay operations.
    /// </summary>
    public class OverlayNodeFactory<TCoordinate> : NodeFactory<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public override Node<TCoordinate> CreateNode(TCoordinate coord)
        {
            return new Node<TCoordinate>(coord, new DirectedEdgeStar<TCoordinate>());
        }
    }
}