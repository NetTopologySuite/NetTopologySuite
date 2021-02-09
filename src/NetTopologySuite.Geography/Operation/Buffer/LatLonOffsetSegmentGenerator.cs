using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geography.Lib;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;

namespace NetTopologySuite.Geography.Operation.Buffer
{

    internal class LatLonOffsetSegmentGenerator : OffsetSegmentGenerator
    {
        private readonly Geodesic _geodesic;
        private readonly int latIdx = 1, lonIdx = 0;
        public LatLonOffsetSegmentGenerator(Geodesic geodesic,
            PrecisionModel precisionModel, BufferParameters bufParams, double distance) : base(precisionModel, bufParams, distance)
        {
            _geodesic = geodesic;
        }

        protected override void ComputeOffsetSegment(LineSegment seg, Position side, double distance, LineSegment offset)
        {
            int sideSign = side == Position.Left ? 1 : -1;

            var p = seg.P0;
            double azi1 = GeoMath.ToHeading(AngleUtility.ToDegrees(seg.Angle));
            var gl = _geodesic.Line(p[latIdx], p[lonIdx], azi1);
            var gp = gl.Position(sideSign * Distance);
            offset.P0[latIdx] = gp.lat2;
            offset.P0[lonIdx] = gp.lon2;

            p = seg.P1;
            azi1 = GeoMath.ToHeading(AngleUtility.ToDegrees(seg.Angle));
            gl = _geodesic.Line(p[latIdx], p[lonIdx], azi1);
            gp = gl.Position(sideSign * Distance);
            offset.P1[latIdx] = gp.lat2;
            offset.P1[lonIdx] = gp.lon2;
        }

        protected override void AddDirectedFillet(Coordinate p, double startAngle, double endAngle, OrientationIndex direction, double radius)
        {
            double aziStart = GeoMath.ToHeading(AngleUtility.ToDegrees(startAngle));
            double aziEnd = GeoMath.ToHeading(AngleUtility.ToDegrees(endAngle));

            int directionFactor = direction == OrientationIndex.Clockwise ? 1 : -1;

            double totalAngle = Math.Abs(aziStart - aziEnd);
            int nSegs = (int)(totalAngle / AngleUtility.ToDegrees(FilletAngleQuantum) + 0.5);

            if (nSegs < 1) return;    // no segments because angle is less than increment - nothing to do!

            // choose angle increment so that each segment has equal length
            double angleInc = totalAngle / nSegs;
            var pt = new Coordinate();
            for (int i = 0; i < nSegs; i++)
            {
                double angle = startAngle + directionFactor * i * angleInc;
                var gl = _geodesic.Line(p[latIdx], p[lonIdx], angle);
                var gp = gl.Position(Distance);
                pt[latIdx] = gp.lat2;
                pt[latIdx] = gp.lon2;
                SegList.AddPt(pt);
            }
        }
    }
}
