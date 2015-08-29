﻿using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Operation.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Buffer.Validate;

namespace Open.Topology.TestRunner.Functions
{
    public static class BufferFunctions
    {
        public static IGeometry Buffer(IGeometry g, double distance)
        {
            return g.Buffer(distance);
        }

        public static IGeometry BufferWithParams(IGeometry g, double? distance,
                                                 int? quadrantSegments, int? capStyle, int? joinStyle,
                                                 Double? mitreLimit)
        {
            double dist = 0;
            if (distance != null) dist = distance.Value;

            IBufferParameters bufParams = new BufferParameters();
            if (quadrantSegments != null) bufParams.QuadrantSegments = quadrantSegments.Value;
            if (capStyle != null) bufParams.EndCapStyle = (EndCapStyle)capStyle.Value;
            if (joinStyle != null) bufParams.JoinStyle = (JoinStyle)joinStyle.Value;
            if (mitreLimit != null) bufParams.MitreLimit = mitreLimit.Value;

            return BufferOp.Buffer(g, dist, bufParams);
        }

        public static IGeometry BufferWithSimplify(IGeometry g, double? distance,
            double? simplifyFactor)
        {
            double dist = 0;
            if (distance != null) dist = distance.Value;

            IBufferParameters bufParams = new BufferParameters();
            if (simplifyFactor != null)
                bufParams.SimplifyFactor = simplifyFactor.Value;
            return BufferOp.Buffer(g, dist, bufParams);
        }

        public static IGeometry BufferOffsetCurve(IGeometry g, double distance)
        {
            return BuildCurveSet(g, distance, new BufferParameters());
        }

        public static IGeometry BufferOffsetCurveWithParams(IGeometry g, Double? distance,
            int? quadrantSegments, int? capStyle, int? joinStyle,
            Double? mitreLimit)
        {
            double dist = 0;
            if (distance != null) dist = distance.Value;

            IBufferParameters bufParams = new BufferParameters();
            if (quadrantSegments != null) bufParams.QuadrantSegments = quadrantSegments.Value;
            if (capStyle != null) bufParams.EndCapStyle = (EndCapStyle)capStyle.Value;
            if (joinStyle != null) bufParams.JoinStyle = (JoinStyle)joinStyle.Value;
            if (mitreLimit != null) bufParams.MitreLimit = mitreLimit.Value;

            return BuildCurveSet(g, dist, bufParams);
        }

        private static IGeometry BuildCurveSet(IGeometry g, double dist, IBufferParameters bufParams)
        {
            // --- now construct curve
            var ocb = new OffsetCurveBuilder(g.Factory.PrecisionModel, bufParams);
            var ocsb = new OffsetCurveSetBuilder(g, dist, ocb);
            var curves = ocsb.GetCurves();

            var lines = new List<IGeometry>();
            foreach (var ss in curves)
            {
                var pts = ss.Coordinates;
                lines.Add(g.Factory.CreateLineString(pts));
            }
            IGeometry curve = g.Factory.BuildGeometry(lines);
            return curve;
        }

        public static IGeometry BufferLineSimplifier(Geometry g, double distance)
        {
            return BuildBufferLineSimplifiedSet(g, distance);
        }

        private static IGeometry BuildBufferLineSimplifiedSet(Geometry g, double distance)
        {
            var simpLines = new List<IGeometry>();

            var lines = new List<IGeometry>();
            LinearComponentExtracter.GetLines(g, lines);
            foreach (var line in lines)
            {
                var pts = line.Coordinates;
                simpLines.Add(g.Factory.CreateLineString(BufferInputLineSimplifier.Simplify(pts, distance)));
            }
            var simpGeom = g.Factory.BuildGeometry(simpLines);
            return simpGeom;
        }

        public static IGeometry BufferValidated(IGeometry g, double distance)
        {
            var buf = g.Buffer(distance);
            var errMsg = BufferResultValidator.IsValidMessage(g, distance, buf);
            if (errMsg != null)
                throw new InvalidOperationException(errMsg);
            return buf;
        }

        public static IGeometry BufferValidatedGeom(IGeometry g, double distance)
        {
            var buf = g.Buffer(distance);
            var validator = new BufferResultValidator(g, distance, buf);
            var isValid = validator.IsValid();
            return validator.ErrorIndicator;
        }

        public static IGeometry SingleSidedBufferCurve(IGeometry geom, double distance)
        {
            IBufferParameters bufParam = new BufferParameters();
            bufParam.IsSingleSided = true;
            var ocb = new OffsetCurveBuilder(
                geom.Factory.PrecisionModel, bufParam
                );
            var pts = ocb.GetLineCurve(geom.Coordinates, distance);
            var curve = geom.Factory.CreateLineString(pts);
            return curve;
        }

        public static IGeometry SingleSidedBuffer(IGeometry geom, double distance)
        {
            IBufferParameters bufParams = new BufferParameters { IsSingleSided = true };
            return BufferOp.Buffer(geom, distance, bufParams);
        }

        public static IGeometry BufferEach(IGeometry g, double distance)
        {
            return GeometryMapper.Map(g, gin => gin.Buffer(distance));
        }
    }
}
