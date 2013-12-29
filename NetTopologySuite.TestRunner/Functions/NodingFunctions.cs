using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Precision;
using NetTopologySuite.Utilities;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public static class NodingFunctions
    {
        public static IGeometry NodeWithPointwisePrecision(IGeometry geom, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var roundedGeom = SimpleGeometryPrecisionReducer.Reduce(geom, pm);

            var geomList = new List<IGeometry>();
            geomList.Add(roundedGeom);

            var noder = new GeometryNoder(pm);
            var lines = noder.Node(geomList);

            return FunctionsUtil.GetFactoryOrDefault(geom).BuildGeometry(CollectionUtil.Cast<IGeometry>((ICollection)lines));
        }

        /// <summary>
        /// Reduces precision pointwise, then snap-rounds.
        /// Note that output set may not contain non-unique linework
        /// (and thus cannot be used as input to Polygonizer directly).
        /// UnaryUnion is one way to make the linework unique.
        /// </summary>
        /// <param name="geom">A geometry containing linework to node</param>
        /// <param name="scaleFactor">the precision model scale factor to use</param>
        /// <returns>The noded, snap-rounded linework</returns>
        public static IGeometry SnapRoundWithPointwisePrecisionReduction(IGeometry geom, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var roundedGeom = GeometryPrecisionReducer.ReducePointwise(geom, pm);

            var geomList = new List<IGeometry>();
            geomList.Add(roundedGeom);

            var noder = new GeometryNoder(pm);
            var lines = noder.Node(geomList);

            return FunctionsUtil.GetFactoryOrDefault(geom).BuildGeometry(CollectionUtil.Cast<IGeometry>((ICollection)lines));
        }

        public static IGeometry CheckNoding(Geometry geom)
        {
            var segs = CreateSegmentStrings(geom);
            var nv = new FastNodingValidator(segs);
            nv.FindAllIntersections = true;
            var res = nv.IsValid;
            var intPts = nv.Intersections;
            var pts = new IPoint[intPts.Count];
            for (var i = 0; i < intPts.Count; i++)
            {
                var coord = intPts[i];
                // use default factory in case intersections are not fixed
                pts[i] = FunctionsUtil.GetFactoryOrDefault(null).CreatePoint(coord);
            }
            return FunctionsUtil.GetFactoryOrDefault(null).CreateMultiPoint(
                pts);
        }

        public static IGeometry MCIndexNodingWithPrecision(Geometry geom, double scaleFactor)
        {
            List<ISegmentString> segs = CreateNodedSegmentStrings(geom);

            LineIntersector li = new RobustLineIntersector();
            li.PrecisionModel = new PrecisionModel(scaleFactor);
            INoder noder = new MCIndexNoder(new IntersectionAdder(li));
            noder.ComputeNodes(segs);
            IList<ISegmentString> nodedSegStrings = noder.GetNodedSubstrings();
            return FromSegmentStrings(nodedSegStrings);
        }

        public static IGeometry MCIndexNoding(Geometry geom)
        {
            IList<ISegmentString> segs = CreateNodedSegmentStrings(geom);
            INoder noder = new MCIndexNoder(new IntersectionAdder(new RobustLineIntersector()));
            noder.ComputeNodes(segs);
            IList<ISegmentString> nodedSegStrings = noder.GetNodedSubstrings();
            return FromSegmentStrings(nodedSegStrings);
        }

        /// <summary>
        /// Runs a ScaledNoder on input.
        /// Input vertices should be rounded to precision model.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="scaleFactor"></param>
        /// <returns>The noded geometry</returns>
        public static IGeometry ScaledNoding(Geometry geom, double scaleFactor)
        {
            var segs = CreateSegmentStrings(geom);
            var fixedPM = new PrecisionModel(scaleFactor);
            var noder = new ScaledNoder(new MCIndexSnapRounder(new PrecisionModel(1.0)),
                fixedPM.Scale);
            noder.ComputeNodes(segs);
            var nodedSegStrings = noder.GetNodedSubstrings();
            return FromSegmentStrings(nodedSegStrings);
        }

        private static List<ISegmentString> CreateSegmentStrings(Geometry geom)
        {
            var segs = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (ILineString line in lines)
            {
                segs.Add(new BasicSegmentString(line.Coordinates, null));
            }
            return segs;
        }

        private static List<ISegmentString> CreateNodedSegmentStrings(Geometry geom)
        {
            var segs = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (ILineString line in lines)
            {
                segs.Add(new NodedSegmentString(line.Coordinates, null));
            }
            return segs;
        }

        private static IGeometry FromSegmentStrings(IList<ISegmentString> segStrings)
        {
            var lines = new ILineString[segStrings.Count];
            int index = 0;
            foreach (var ss in segStrings)
            {
                var line = FunctionsUtil.GetFactoryOrDefault(null).CreateLineString(ss.Coordinates);
                lines[index++] = line;
            }
            return FunctionsUtil.GetFactoryOrDefault(null).CreateMultiLineString(lines);
        }
    }
}