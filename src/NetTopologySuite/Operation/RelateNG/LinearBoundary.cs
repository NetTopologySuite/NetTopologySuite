using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Determines the boundary points of a linear geometry,
    /// using a <see cref="IBoundaryNodeRule"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class LinearBoundary
    {

        private readonly Dictionary<Coordinate, int> _vertexDegree;
        private readonly IBoundaryNodeRule _boundaryNodeRule;

        public LinearBoundary(IEnumerable<LineString> lines, IBoundaryNodeRule bnRule)
        {
            //assert: dim(geom) == 1
            _boundaryNodeRule = bnRule;
            _vertexDegree = ComputeBoundaryPoints(lines);
            HasBoundary = CheckBoundary(_vertexDegree);
        }

        private bool CheckBoundary(Dictionary<Coordinate, int> vertexDegree)
        {
            foreach (int degree in vertexDegree.Values)
            {
                if (_boundaryNodeRule.IsInBoundary(degree))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasBoundary { get; }

        public bool IsBoundary(Coordinate pt)
        {
            if (!_vertexDegree.ContainsKey(pt))
                return false;
            int degree = _vertexDegree[pt];
            return _boundaryNodeRule.IsInBoundary(degree);
        }

        private static Dictionary<Coordinate, int> ComputeBoundaryPoints(IEnumerable<LineString> lines)
        {
            var vertexDegree = new Dictionary<Coordinate, int>();
            foreach (var line in lines)
            {
                if (line.IsEmpty)
                    continue;
                AddEndpoint(line.CoordinateSequence.First);
                AddEndpoint(line.CoordinateSequence.Last);
            }
            return vertexDegree;

            void AddEndpoint(Coordinate p)
            {
                if (!vertexDegree.TryGetValue(p, out int dim)) dim = 0;
                dim++;
                vertexDegree[p] = dim;
            }
        }

        //private static void AddEndpoint(Coordinate p, Dictionary<Coordinate, int> degree)
        //{
        //    if (!degree.TryGetValue(p, out int dim)) dim = 0;
        //    dim++;
        //    degree[p] = dim;
        //}
    }

}

