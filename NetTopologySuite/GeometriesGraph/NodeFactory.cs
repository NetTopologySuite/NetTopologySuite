using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class NodeFactory<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
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
