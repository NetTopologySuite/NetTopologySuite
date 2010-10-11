using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// An edge of a <see cref="LineMergeGraph{TCoordinate}"/>. 
    /// The <c>marked</c> field indicates whether this 
    /// Edge has been logically deleted from the graph.
    /// </summary>
    public class LineMergeEdge<TCoordinate> : Edge<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ILineString<TCoordinate> _line;

        /// <summary>
        /// Constructs a LineMergeEdge with vertices given by the specified LineString.
        /// </summary>
        public LineMergeEdge(ILineString<TCoordinate> line, DirectedEdge<TCoordinate> directedEdge0,
                             DirectedEdge<TCoordinate> directedEdge1)
            : base(directedEdge0, directedEdge1)
        {
            _line = line;
        }

        /// <summary>
        /// Returns the LineString specifying the vertices of this edge.
        /// </summary>
        public ILineString<TCoordinate> Line
        {
            get { return _line; }
        }
    }
}