using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Counts the number of rings containing each vertex.
    /// Vertices which are contained by 3 or more rings are nodes in the coverage topology
    /// (although not the only ones -
    /// boundary vertices with 3 or more incident edges are also nodes).
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class VertexRingCounter : IEntireCoordinateSequenceFilter
    {

        public static IDictionary<Coordinate, int> Count(Geometry[] geoms)
        {
            var vertexRingCount = new Dictionary<Coordinate, int>();
            var counter = new VertexRingCounter(vertexRingCount);
            foreach (var geom in geoms)
            {
                geom.Apply(counter);
            }
            return vertexRingCount;
        }

        private readonly IDictionary<Coordinate, int> _vertexRingCount;

        public VertexRingCounter(IDictionary<Coordinate, int> vertexCount)
        {
            _vertexRingCount = vertexCount;
        }

        public void Filter(CoordinateSequence seq)
        {
            //-- for rings don't double-count duplicate endpoint
            int i = CoordinateSequences.IsRing(seq) ? 1 : 0;
            for (; i < seq.Count; i++)
            {
                var v = seq.GetCoordinate(i);
                if (_vertexRingCount.TryGetValue(v, out int count))
                    count++;
                else
                    count = 1;
                _vertexRingCount[v] = count;
            }

        }

        public bool Done => false;

        public bool GeometryChanged => false;
    }
}
