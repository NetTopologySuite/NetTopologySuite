using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// An edge of a polygonization graph.
    /// </summary>
    public class PolygonizeEdge<TCoordinate> : Edge<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ILineString<TCoordinate> _line;

        public PolygonizeEdge(ILineString<TCoordinate> line,
                              DirectedEdge<TCoordinate> directedEdge0, DirectedEdge<TCoordinate> directedEdge1)
            : base(directedEdge0, directedEdge1)
        {
            _line = line;
        }

        public ILineString<TCoordinate> Line
        {
            get { return _line; }
        }
    }
}