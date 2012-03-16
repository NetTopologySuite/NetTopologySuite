using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Buffer.Validate;
using NetTopologySuite.Operation.Linemerge;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class BufferFunctions
    {
        public static IGeometry Buffer(IGeometry g, double distance) { return g.Buffer(distance); }

        public static IGeometry BufferWithParams(IGeometry g, Double? distance,
                int? quadrantSegments, int? capStyle, int? joinStyle, Double? mitreLimit)
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

        public static IGeometry BufferOffsetCurve(IGeometry g, double distance)
        {
            return BuildCurveSet(g, distance, new BufferParameters());
        }

        public static IGeometry BufferOffsetCurveWithParams(IGeometry g, Double? distance,
                int? quadrantSegments, int? capStyle, int? joinStyle, Double? mitreLimit)
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
            foreach(var line in lines)
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

    }

    public class BufferByUnionFunctions
    {

        public static IGeometry ComponentBuffers(IGeometry g, double distance)	
	    {		
		    var bufs = new List<IGeometry>();
		    foreach (var comp in new GeometryCollectionEnumerator((IGeometryCollection)g))
		    {
		        if (comp is IGeometryCollection) continue;
		        bufs.Add(comp.Buffer(distance));
		    }
            return FunctionsUtil.getFactoryOrDefault(g)
    				.CreateGeometryCollection(GeometryFactory.ToGeometryArray(bufs));
	}

        public static IGeometry BufferByComponents(IGeometry g, double distance)
        {
            return ComponentBuffers(g, distance).Union();
        }

        /**
         * Buffer polygons by buffering the individual boundary segments and
         * either unioning or differencing them.
         * 
         * @param g
         * @param distance
         * @return
         */
        public static IGeometry BufferBySegments(IGeometry g, double distance)
        {
            var segs = LineHandlingFunctions.ExtractSegments(g);
            var posDist = Math.Abs(distance);
            var segBuf = BufferByComponents(segs, posDist);
            if (distance < 0.0)
                return g.Difference(segBuf);
            return g.Union(segBuf);
        }

        public static IGeometry BufferByChains(IGeometry g, double distance, int maxChainSize)
        {
            if (maxChainSize <= 0)
                throw new ArgumentOutOfRangeException("maxChainSize", "Maximum Chain Size must be specified as an input parameter");
            var segs = LineHandlingFunctions.ExtractChains(g, maxChainSize);
            double posDist = Math.Abs(distance);
            var segBuf = BufferByComponents(segs, posDist);
            if (distance < 0.0)
                return g.Difference(segBuf);
            return g.Union(segBuf);
        }
    }

    public class LineHandlingFunctions
    {

        public static IGeometry MergeLines(IGeometry g)
        {
            var merger = new LineMerger();
            merger.Add(g);
            var lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry SequenceLines(IGeometry g)
        {
            var ls = new LineSequencer();
            ls.Add(g);
            return ls.GetSequencedLineStrings();
        }

        public static IGeometry ExtractLines(IGeometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(lines);
        }
        
        public static IGeometry ExtractSegments(IGeometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var segments = new List<IGeometry>();
            foreach (ILineString line in lines)
            {
                for (var i = 1; i < line.NumPoints; i++)
                {
                    var seg = g.Factory.CreateLineString(
                        new[] { line.GetCoordinateN(i - 1), line.GetCoordinateN(i) }
                      );
                    segments.Add(seg);
                }
            }
            return g.Factory.BuildGeometry(segments);
        }
        public static IGeometry ExtractChains(IGeometry g, int maxChainSize)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var chains = new List<IGeometry>();
            foreach (ILineString line in lines)
            {
                for (var i = 0; i < line.NumPoints - 1; i += maxChainSize)
                {
                    var chain = ExtractChain(line, i, maxChainSize);
                    chains.Add(chain);
                }
            }
            return g.Factory.BuildGeometry(chains);
        }

        private static ILineString ExtractChain(ILineString line, int index, int maxChainSize)
        {
            var size = maxChainSize + 1;
            if (index + size > line.NumPoints)
                size = line.NumPoints - index;
            var pts = new Coordinate[size];
            for (var i = 0; i < size; i++)
            {
                pts[i] = line.GetCoordinateN(index + i);
            }
            return line.Factory.CreateLineString(pts);
        }
    }

}