using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// A ring of edges which may contain nodes of degree > 2.
    /// </summary>
    /// <remarks>
    /// A <see cref="MaximalEdgeRing{TCoordinate}"/> may represent two different spatial entities:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// a single polygon possibly containing inversions (if the ring is oriented CW)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a single hole possibly containing exversions (if the ring is oriented CCW)    
    /// </description>
    /// </item>
    /// </list>
    /// If the <see cref="MaximalEdgeRing{TCoordinate}"/> represents a polygon,
    /// the interior of the polygon is strongly connected.
    /// These are the form of rings used to define polygons under some spatial data models.
    /// However, under the OGC SFS model, <see cref="MinimalEdgeRing{TCoordinate}"/>s are required.
    /// A MaximalEdgeRing can be converted to a list of MinimalEdgeRings using
    /// <see cref="BuildMinimalRings"/>.
    /// </remarks>
    public class MaximalEdgeRing<TCoordinate> : EdgeRing<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        public MaximalEdgeRing(DirectedEdge<TCoordinate> start, IGeometryFactory<TCoordinate> geometryFactory)
            : base(start, geometryFactory)
        {
        }

        public override DirectedEdge<TCoordinate> GetNext(DirectedEdge<TCoordinate> de)
        {
            return de.Next;
        }

        public override void SetEdgeRing(DirectedEdge<TCoordinate> de, EdgeRing<TCoordinate> er)
        {
            de.EdgeRing = er;
        }

        /// <summary> 
        /// For all nodes in this EdgeRing,
        /// link the DirectedEdges at the node to form minimalEdgeRings
        /// </summary>
        public void LinkDirectedEdgesForMinimalEdgeRings()
        {
            DirectedEdge<TCoordinate> de = StartingEdge;

            do
            {
                Node<TCoordinate> node = de.Node;
                ((DirectedEdgeStar<TCoordinate>) node.Edges).LinkMinimalDirectedEdges(this);
                de = de.Next;
            } while (de != StartingEdge);
        }

        private IList<MinimalEdgeRing<TCoordinate>> _minEdgeRings;
        public IEnumerable<MinimalEdgeRing<TCoordinate>> BuildMinimalRings()
        {
            if ( _minEdgeRings != null )
                foreach (MinimalEdgeRing<TCoordinate> minEdgeRing in _minEdgeRings)
                    yield return minEdgeRing;

            _minEdgeRings = new List<MinimalEdgeRing<TCoordinate>>();
            DirectedEdge<TCoordinate> de = StartingEdge;

            do
            {
                if (de.MinEdgeRing == null)
                {
                    MinimalEdgeRing<TCoordinate> mer = new MinimalEdgeRing<TCoordinate>(de, GeometryFactory);
                    _minEdgeRings.Add(mer);
                    yield return mer;
                }

                de = de.Next;
            } while (de != StartingEdge);
        }
    }
}