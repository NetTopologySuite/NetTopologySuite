using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Buffer.Validate;

namespace Open.Topology.TestRunner.Functions
{
    public static class BufferFunctions
    {
        public static Geometry Buffer(Geometry g, double distance)
        {
            return g.Buffer(distance);
        }

        public static Geometry BufferWithParams(Geometry g, double? distance,
                                                 int? quadrantSegments, int? capStyle, int? joinStyle,
                                                 double? mitreLimit)
        {
            double dist = 0;
            if (distance != null) dist = distance.Value;

            var bufParams = new BufferParameters();
            if (quadrantSegments != null) bufParams.QuadrantSegments = quadrantSegments.Value;
            if (capStyle != null) bufParams.EndCapStyle = (EndCapStyle)capStyle.Value;
            if (joinStyle != null) bufParams.JoinStyle = (JoinStyle)joinStyle.Value;
            if (mitreLimit != null) bufParams.MitreLimit = mitreLimit.Value;

            return BufferOp.Buffer(g, dist, bufParams);
        }

        public static Geometry BufferWithSimplify(Geometry g, double? distance,
            double? simplifyFactor)
        {
            double dist = 0;
            if (distance != null) dist = distance.Value;

            var bufParams = new BufferParameters();
            if (simplifyFactor != null)
                bufParams.SimplifyFactor = simplifyFactor.Value;
            return BufferOp.Buffer(g, dist, bufParams);
        }

        public static Geometry BufferOffsetCurve(Geometry g, double distance)
        {
            return BuildCurveSet(g, distance, new BufferParameters());
        }

        public static Geometry BufferOffsetCurveWithParams(Geometry g, double? distance,
            int? quadrantSegments, int? capStyle, int? joinStyle,
            double? mitreLimit)
        {
            double dist = 0;
            if (distance != null) dist = distance.Value;

            var bufParams = new BufferParameters();
            if (quadrantSegments != null) bufParams.QuadrantSegments = quadrantSegments.Value;
            if (capStyle != null) bufParams.EndCapStyle = (EndCapStyle)capStyle.Value;
            if (joinStyle != null) bufParams.JoinStyle = (JoinStyle)joinStyle.Value;
            if (mitreLimit != null) bufParams.MitreLimit = mitreLimit.Value;

            return BuildCurveSet(g, dist, bufParams);
        }

        private static Geometry BuildCurveSet(Geometry g, double dist, BufferParameters bufParams)
        {
            // --- now construct curve
            var ocb = new OffsetCurveBuilder(g.Factory.PrecisionModel, bufParams);
            var ocsb = new OffsetCurveSetBuilder(g, dist, ocb);
            var curves = ocsb.GetCurves();

            var lines = new List<Geometry>();
            foreach (var ss in curves)
            {
                var pts = ss.Coordinates;
                lines.Add(g.Factory.CreateLineString(pts));
            }
            var curve = g.Factory.BuildGeometry(lines);
            return curve;
        }

        public static Geometry BufferLineSimplifier(Geometry g, double distance)
        {
            return BuildBufferLineSimplifiedSet(g, distance);
        }

        private static Geometry BuildBufferLineSimplifiedSet(Geometry g, double distance)
        {
            var simpLines = new List<Geometry>();

            var lines = new List<Geometry>();
            LinearComponentExtracter.GetLines(g, lines);
            foreach (var line in lines)
            {
                var pts = line.Coordinates;
                simpLines.Add(g.Factory.CreateLineString(BufferInputLineSimplifier.Simplify(pts, distance)));
            }
            var simpGeom = g.Factory.BuildGeometry(simpLines);
            return simpGeom;
        }

        public static Geometry BufferValidated(Geometry g, double distance)
        {
            var buf = g.Buffer(distance);
            string errMsg = BufferResultValidator.IsValidMessage(g, distance, buf);
            if (errMsg != null)
                throw new InvalidOperationException(errMsg);
            return buf;
        }

        public static Geometry BufferValidatedGeom(Geometry g, double distance)
        {
            var buf = g.Buffer(distance);
            var validator = new BufferResultValidator(g, distance, buf);
            bool isValid = validator.IsValid();
            return validator.ErrorIndicator;
        }

        public static Geometry SingleSidedBufferCurve(Geometry geom, double distance)
        {
            var bufParam = new BufferParameters();
            bufParam.IsSingleSided = true;
            var ocb = new OffsetCurveBuilder(
                geom.Factory.PrecisionModel, bufParam
                );
            var pts = ocb.GetLineCurve(geom.Coordinates, distance);
            var curve = geom.Factory.CreateLineString(pts);
            return curve;
        }

        public static Geometry SingleSidedBuffer(Geometry geom, double distance)
        {
            var bufParams = new BufferParameters { IsSingleSided = true };
            return BufferOp.Buffer(geom, distance, bufParams);
        }

        public static Geometry BufferEach(Geometry g, double distance)
        {
            return GeometryMapper.Map(g, gin => gin.Buffer(distance));
        }

        public static Geometry VariableBuffer(Geometry line,
        double startDist,
        double endDist) {
            if (line is Polygon poly) {
                line = poly.ExteriorRing;
            }
            return NetTopologySuite.Operation.Buffer.VariableBuffer.Buffer(line, startDist, endDist);
        }

        public static Geometry VariableBufferMid(Geometry line,
        double startDist,
        double midDist)  
        {
            if (line is Polygon poly) {
                line = poly.ExteriorRing;
            }
            return NetTopologySuite.Operation.Buffer.VariableBuffer.Buffer(line, startDist, midDist, startDist);
        }

}
}
