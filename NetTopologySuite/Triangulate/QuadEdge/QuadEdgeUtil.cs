namespace NetTopologySuite.Triangulate.QuadEdge
{
    using System.Collections.Generic;

    public class QuadEdgeUtil
    {
        /// <summary>
        /// Gets all edges which are incident on the origin of the given edge.
        /// </summary>
        /// <param name="start">the edge to start at</param>
        /// <returns>a List of edges which have their origin at the origin of the given
        /// edge</returns>
        public static IList<QuadEdge> FindEdgesIncidentOnOrigin(QuadEdge start)
        {
            var incEdge = new List<QuadEdge>();

            var qe = start;
            do
            {
                incEdge.Add(qe);
                qe = qe.ONext;
            } while (qe != start);

            return incEdge;
        }

    }
}