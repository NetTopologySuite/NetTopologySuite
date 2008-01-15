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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ILineString<TCoordinate> _line;

        public PolygonizeEdge(ILineString<TCoordinate> line)
        {
            _line = line;
        }

        public ILineString<TCoordinate> Line
        {
            get { return _line; }
        }
    }
}