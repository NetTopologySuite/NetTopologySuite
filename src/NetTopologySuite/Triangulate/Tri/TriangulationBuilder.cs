using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.Tri
{
    /// <summary>
    /// Builds a triangulation from a set of <see cref="Tri"/>s
    /// by populating the links to adjacent triangles.
    /// </summary>
    /// <author>Martin Davis</author>
    public class TriangulationBuilder
    {

        /// <summary>
        /// Computes the triangulation of a set of <see cref="Tri"/>s.
        /// </summary>
        /// <param name="triList">An enumeration of <see cref="Tri"/>s</param>
        public static void Build(IEnumerable<Tri> triList)
        {
            new TriangulationBuilder(triList);
        }

        private readonly Dictionary<TriEdge, Tri> _triMap;

        /// <summary>
        /// Creates an instance of this class and computes the triangulation of a set of <see cref="Tri"/>s.
        /// </summary>
        /// <param name="triList">An enumeration of <see cref="Tri"/>s</param>
        private TriangulationBuilder(IEnumerable<Tri> triList)
        {
            _triMap = new Dictionary<TriEdge, Tri>();
            foreach (var tri in triList)
                Add(tri);
        }

        private Tri Find(Coordinate p0, Coordinate p1)
        {
            var e = new TriEdge(p0, p1);
            if (_triMap.TryGetValue(e, out var res))
                return res;
            return null;
        }

        private void Add(Tri tri)
        {
            var p0 = tri.GetCoordinate(0);
            var p1 = tri.GetCoordinate(1);
            var p2 = tri.GetCoordinate(2);

            // get adjacent triangles, if any
            var n0 = Find(p0, p1);
            var n1 = Find(p1, p2);
            var n2 = Find(p2, p0);

            tri.SetAdjacent(n0, n1, n2);
            AddAdjacent(tri, n0, p0, p1);
            AddAdjacent(tri, n1, p1, p2);
            AddAdjacent(tri, n2, p2, p0);
        }

        private void AddAdjacent(Tri tri, Tri adj, Coordinate p0, Coordinate p1)
        {
            /*
             * If adjacent is null, this tri is first one to be recorded for edge
             */
            if (adj == null)
            {
                _triMap.Add(new TriEdge(p0, p1), tri);
                return;
            }
            adj.SetAdjacent(p1, tri);
        }
    }
}
