using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// A ring of edges with the property that no node
    /// has degree greater than 2.  These are the form of rings required
    /// to represent polygons under the OGC SFS spatial data model.
    /// </summary>
    public class MinimalEdgeRing<TCoordinate> : EdgeRing<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        public MinimalEdgeRing(DirectedEdge<TCoordinate> start, IGeometryFactory<TCoordinate> geometryFactory)
            : base(start, geometryFactory) {}

        public override DirectedEdge<TCoordinate> GetNext(DirectedEdge<TCoordinate> de)
        {
            return de.NextMin;
        }

        public override void SetEdgeRing(DirectedEdge<TCoordinate> de, EdgeRing<TCoordinate> er)
        {
            de.MinEdgeRing = er;
        }
    }
}