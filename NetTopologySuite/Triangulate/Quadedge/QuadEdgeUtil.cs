using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// Utilities for working with <see cref="QuadEdge{TCoordinate}"/>s.
    ///</summary>
    public class QuadEdgeUtil<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Gets all edges which are incident on the origin of the given edge.
        ///</summary>
        ///<param name="start">the edge to start at</param>
        ///<returns>an enumeration of edges which have their origin at the origin of the given edge</returns>
        public static IEnumerable<QuadEdge<TCoordinate>> FindEdgesIncidentOnOrigin(QuadEdge<TCoordinate> start)
        {
            QuadEdge<TCoordinate> qe = start;
            do
            {
                yield return qe;
                qe = qe.OriginNext;
            } while (qe != start);
        }

    }
}
