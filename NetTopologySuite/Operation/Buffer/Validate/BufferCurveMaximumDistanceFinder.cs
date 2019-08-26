using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /// <summary>
    /// Finds the approximate maximum distance from a buffer curve to
    /// the originating geometry.
    /// </summary>
    /// <remarks><para>The approximate maximum distance is determined by testing
    /// all vertices in the buffer curve, as well
    /// as midpoints of the curve segments.
    /// Due to the way buffer curves are constructed, this
    /// should be a very close approximation.</para>
    /// <para>This is similar to the Discrete Oriented Hausdorff distance
    /// from the buffer curve to the input.</para>
    /// </remarks>
    /// <author>mbdavis</author>
    public class BufferCurveMaximumDistanceFinder
    {
        private readonly Geometry _inputGeom;
        private readonly PointPairDistance _maxPtDist = new PointPairDistance();

        public BufferCurveMaximumDistanceFinder(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        public double FindDistance(Geometry bufferCurve)
        {
            ComputeMaxVertexDistance(bufferCurve);
            computeMaxMidpointDistance(bufferCurve);
            return _maxPtDist.Distance;
        }

        public PointPairDistance DistancePoints => _maxPtDist;

        private void ComputeMaxVertexDistance(Geometry curve)
        {
            var distFilter = new MaxPointDistanceFilter(_inputGeom);
            curve.Apply(distFilter);
            _maxPtDist.SetMaximum(distFilter.MaxPointDistance);
        }

        private void computeMaxMidpointDistance(Geometry curve)
        {
            var distFilter = new MaxMidpointDistanceFilter(_inputGeom);
            curve.Apply(distFilter);
            _maxPtDist.SetMaximum(distFilter.MaxPointDistance);
        }

        public class MaxPointDistanceFilter : ICoordinateFilter
        {
            private readonly PointPairDistance maxPtDist = new PointPairDistance();
            private readonly PointPairDistance minPtDist = new PointPairDistance();
            private readonly Geometry geom;

            public MaxPointDistanceFilter(Geometry geom)
            {
                this.geom = geom;
            }

            public void Filter(Coordinate pt)
            {
                minPtDist.Initialize();
                DistanceToPointFinder.ComputeDistance(geom, pt, minPtDist);
                maxPtDist.SetMaximum(minPtDist);
            }

            public PointPairDistance MaxPointDistance => maxPtDist;
        }

        public class MaxMidpointDistanceFilter
          : ICoordinateSequenceFilter
        {
            private readonly PointPairDistance maxPtDist = new PointPairDistance();
            private readonly PointPairDistance minPtDist = new PointPairDistance();
            private readonly Geometry geom;

            public MaxMidpointDistanceFilter(Geometry geom)
            {
                this.geom = geom;
            }

            public void Filter(CoordinateSequence seq, int index)
            {
                if (index == 0)
                    return;

                var p0 = seq.GetCoordinate(index - 1);
                var p1 = seq.GetCoordinate(index);
                var midPt = new Coordinate(
                        (p0.X + p1.X) / 2,
                        (p0.Y + p1.Y) / 2);

                minPtDist.Initialize();
                DistanceToPointFinder.ComputeDistance(geom, midPt, minPtDist);
                maxPtDist.SetMaximum(minPtDist);
            }

            public bool GeometryChanged => false;

            public bool Done => false;

            public PointPairDistance MaxPointDistance => maxPtDist;
        }

    }
}