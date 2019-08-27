using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Computes the minimum clearance of a geometry or
    /// set of geometries.<para/>
    /// The <b>Minimum Clearance</b> is a measure of
    /// what magnitude of perturbation of its vertices can be tolerated
    /// by a geometry before it becomes topologically invalid.
    /// <para/>
    /// This class uses an inefficient O(N^2) scan.
    /// It is primarily for testing purposes.
    /// </summary>
    /// <seealso cref="MinimumClearance"/>
    /// <author>Martin Davis</author>
    public class SimpleMinimumClearance
    {
        public static double GetDistance(Geometry g)
        {
            var rp = new SimpleMinimumClearance(g);
            return rp.GetDistance();
        }

        public static Geometry GetLine(Geometry g)
        {
            var rp = new SimpleMinimumClearance(g);
            return rp.GetLine();
        }

        private readonly Geometry _inputGeom;
        private double _minClearance;
        private Coordinate[] _minClearancePts;

        public SimpleMinimumClearance(Geometry geom)
        {
            _inputGeom = geom;
        }

        public double GetDistance()
        {
            Compute();
            return _minClearance;
        }

        public LineString GetLine()
        {
            Compute();
            return _inputGeom.Factory.CreateLineString(_minClearancePts);
        }

        private void Compute()
        {
            if (_minClearancePts != null) return;
            _minClearancePts = new Coordinate[2];
            _minClearance = double.MaxValue;
            _inputGeom.Apply(new VertexCoordinateFilter(this, _inputGeom));
        }

        private void UpdateClearance(double candidateValue, Coordinate p0, Coordinate p1)
        {
            if (candidateValue < _minClearance)
            {
                _minClearance = candidateValue;
                _minClearancePts[0] = p0.Copy();
                _minClearancePts[1] = p1.Copy();
            }
        }

        private void UpdateClearance(double candidateValue, Coordinate p,
                                     Coordinate seg0, Coordinate seg1)
        {
            if (candidateValue < _minClearance)
            {
                _minClearance = candidateValue;
                _minClearancePts[0] = p.Copy();
                var seg = new LineSegment(seg0, seg1);
                _minClearancePts[1] = seg.ClosestPoint(p).Copy();
            }
        }

        private class VertexCoordinateFilter : ICoordinateFilter
        {
            private readonly SimpleMinimumClearance _smc;
            private readonly Geometry _inputGeometry;

            public VertexCoordinateFilter(SimpleMinimumClearance smc, Geometry inputGeometry)
            {
                _smc = smc;
                _inputGeometry = inputGeometry;
            }

            public void Filter(Coordinate coord)
            {
                _inputGeometry.Apply(new ComputeMCCoordinateSequenceFilter(_smc, coord));
            }
        }

        private class ComputeMCCoordinateSequenceFilter : ICoordinateSequenceFilter
        {
            private readonly SimpleMinimumClearance _smc;
            private readonly Coordinate _queryPt;

            public ComputeMCCoordinateSequenceFilter(SimpleMinimumClearance smc, Coordinate queryPt)
            {
                _smc = smc;
                _queryPt = queryPt;
            }

            public void Filter(CoordinateSequence seq, int i)
            {
                // compare to vertex
                CheckVertexDistance(seq.GetCoordinate(i));

                // compare to segment, if this is one
                if (i > 0)
                {
                    CheckSegmentDistance(seq.GetCoordinate(i - 1), seq.GetCoordinate(i));
                }
            }

            private void CheckVertexDistance(Coordinate vertex)
            {
                double vertexDist = vertex.Distance(_queryPt);
                if (vertexDist > 0)
                {
                    _smc.UpdateClearance(vertexDist, _queryPt, vertex);
                }
            }

            private void CheckSegmentDistance(Coordinate seg0, Coordinate seg1)
            {
                if (_queryPt.Equals2D(seg0) || _queryPt.Equals2D(seg1))
                    return;
                double segDist = DistanceComputer.PointToSegment(_queryPt, seg1, seg0);
                if (segDist > 0)
                    _smc.UpdateClearance(segDist, _queryPt, seg1, seg0);
            }

            public bool Done => false;

            public bool GeometryChanged => false;
        }
    }
}
