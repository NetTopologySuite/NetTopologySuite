using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    internal sealed class VertexCounter : IEntireCoordinateSequenceFilter
    {

        public static IDictionary<Coordinate, int> Count(Geometry[] geoms)
        {
            var vertexCount = new Dictionary<Coordinate, int>();
            var counter = new VertexCounter(vertexCount);
            foreach (var geom in geoms)
            {
                geom.Apply(counter);
            }
            return vertexCount;
        }

        private readonly IDictionary<Coordinate, int> _vertexCount;

        public VertexCounter(IDictionary<Coordinate, int> vertexCount)
        {
            _vertexCount = vertexCount;
        }

        public void Filter(CoordinateSequence seq)
        {
            //-- for rings don't double-count duplicate endpoint
            int i = CoordinateSequences.IsRing(seq) ? 1 : 0;
            for (; i < seq.Count; i++)
            {
                var v = seq.GetCoordinate(i);
                if (_vertexCount.TryGetValue(v, out int count))
                    count++;
                else
                    count = 1;
                _vertexCount[v] = count;
            }

        }

        public bool Done => false;

        public bool GeometryChanged => false;
    }
}
