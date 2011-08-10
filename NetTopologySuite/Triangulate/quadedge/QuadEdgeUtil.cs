namespace NetTopologySuite.Triangulate.QuadEdge
{
    using System.Collections.Generic;

    public class QuadEdgeUtil
    {
        /**
	 * Gets all edges which are incident on the origin of the given edge.
	 * 
	 * @param start
	 *          the edge to start at
	 * @return a List of edges which have their origin at the origin of the given
	 *         edge
	 */

        public static List<QuadEdge> findEdgesIncidentOnOrigin(QuadEdge start)
        {
            var incEdge = new List<QuadEdge>();

            QuadEdge qe = start;
            do
            {
                incEdge.Add(qe);
                qe = qe.oNext();
            } while (qe != start);

            return incEdge;
        }

    }
}