using GeoAPI.Geometries;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /// <summary>
    ///     Finds the approximate maximum distance from a buffer curve to
    ///     the originating geometry.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The approximate maximum distance is determined by testing
    ///         all vertices in the buffer curve, as well
    ///         as midpoints of the curve segments.
    ///         Due to the way buffer curves are constructed, this
    ///         should be a very close approximation.
    ///     </para>
    ///     <para>
    ///         This is similar to the Discrete Oriented Hausdorff distance
    ///         from the buffer curve to the input.
    ///     </para>
    /// </remarks>
    /// <author>mbdavis</author>
    public class BufferCurveMaximumDistanceFinder
    {
        private readonly IGeometry _inputGeom;

        public BufferCurveMaximumDistanceFinder(IGeometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        public PointPairDistance DistancePoints { get; } = new PointPairDistance();

        public double FindDistance(IGeometry bufferCurve)
        {
            ComputeMaxVertexDistance(bufferCurve);
            computeMaxMidpointDistance(bufferCurve);
            return DistancePoints.Distance;
        }

        private void ComputeMaxVertexDistance(IGeometry curve)
        {
            var distFilter = new MaxPointDistanceFilter(_inputGeom);
            curve.Apply(distFilter);
            DistancePoints.SetMaximum(distFilter.MaxPointDistance);
        }

        private void computeMaxMidpointDistance(IGeometry curve)
        {
            var distFilter = new MaxMidpointDistanceFilter(_inputGeom);
            curve.Apply(distFilter);
            DistancePoints.SetMaximum(distFilter.MaxPointDistance);
        }

        public class MaxPointDistanceFilter : ICoordinateFilter
        {
            private readonly IGeometry geom;
            private readonly PointPairDistance minPtDist = new PointPairDistance();

            public MaxPointDistanceFilter(IGeometry geom)
            {
                this.geom = geom;
            }

            public PointPairDistance MaxPointDistance { get; } = new PointPairDistance();

            public void Filter(Coordinate pt)
            {
                minPtDist.Initialize();
                DistanceToPointFinder.ComputeDistance(geom, pt, minPtDist);
                MaxPointDistance.SetMaximum(minPtDist);
            }
        }

        public class MaxMidpointDistanceFilter
            : ICoordinateSequenceFilter
        {
            private readonly IGeometry geom;
            private readonly PointPairDistance minPtDist = new PointPairDistance();

            public MaxMidpointDistanceFilter(IGeometry geom)
            {
                this.geom = geom;
            }

            public PointPairDistance MaxPointDistance { get; } = new PointPairDistance();

            public void Filter(ICoordinateSequence seq, int index)
            {
                if (index == 0)
                    return;

                var p0 = seq.GetCoordinate(index - 1);
                var p1 = seq.GetCoordinate(index);
                var midPt = new Coordinate(
                    (p0.X + p1.X)/2,
                    (p0.Y + p1.Y)/2);

                minPtDist.Initialize();
                DistanceToPointFinder.ComputeDistance(geom, midPt, minPtDist);
                MaxPointDistance.SetMaximum(minPtDist);
            }

            public bool GeometryChanged => false;

            public bool Done => false;
        }
    }
}