﻿using GeoAPI.Geometries;
using NetTopologySuite.EdgeGraph;

namespace NetTopologySuite.Dissolve
{
    /// <summary>
    /// A graph containing <see cref="DissolveHalfEdge"/>s.
    /// </summary>
    public class DissolveEdgeGraph : EdgeGraph.EdgeGraph
    {
        protected override HalfEdge CreateEdge(Coordinate p0)
        {
            return new DissolveHalfEdge(p0);
        }
    }
}