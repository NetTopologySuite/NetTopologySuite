using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A factory for producing nodes in a <see cref="GeometryGraph{TCoordinate}"/>.
    /// </summary>
    /// <typeparam name="TCoordinate">
    /// The type of the coordinate in the geometry.
    /// </typeparam>
    public class NodeFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /// <summary> 
        /// The basic node constructor does not allow for incident edges.
        /// </summary>
        public virtual Node<TCoordinate> CreateNode(TCoordinate coord)
        {
            return new Node<TCoordinate>(coord, null);
        }
    }
}