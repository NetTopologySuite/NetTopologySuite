using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Precision;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class NodingFunctions
    {
        public static bool IsNodingValid(Geometry geom)
        {
            var nv = new FastNodingValidator(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return nv.IsValid;
        }

        public static bool IsSegmentNodingValid(Geometry geom)
        {
            var intFinder = NodingIntersectionFinder
                .CreateInteriorIntersectionCounter(new RobustLineIntersector());
            ProcessNodes(geom, intFinder);
            return 0 == intFinder.Count;
        }

        public static Geometry FindOneNode(Geometry geom)
        {
            var nv = new FastNodingValidator(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            bool temp = nv.IsValid;
            var intPts = nv.Intersections;
            if (intPts.Count == 0) return null;
            return FunctionsUtil.GetFactoryOrDefault((Geometry)null).CreatePoint((Coordinate)intPts[0]);
        }

        public static Geometry FindNodes(Geometry geom)
        {
            var intPts = FastNodingValidator.ComputeIntersections(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return FunctionsUtil.GetFactoryOrDefault((Geometry)null).CreateMultiPointFromCoords(CoordinateArrays.ToCoordinateArray(intPts));
        }

        private static Coordinate[] Dedup(IList<Coordinate> ptsList)
        {
            var ptsNoDup = new List<Coordinate>(new HashSet<Coordinate>(ptsList));
            var pts = CoordinateArrays.ToCoordinateArray(ptsNoDup);
            return pts;
        }

        public static Geometry FindInteriorNodes(Geometry geom)
        {
            var intFinder = NodingIntersectionFinder.CreateInteriorIntersectionsFinder(new RobustLineIntersector());
            ProcessNodes(geom, intFinder);
            var intPts = intFinder.Intersections;
            return FunctionsUtil.GetFactoryOrDefault((Geometry)null)
                .CreateMultiPointFromCoords(Dedup(intPts));
        }

        public static int IntersectionCount(Geometry geom)
        {
            var intCounter = NodingIntersectionFinder.CreateIntersectionCounter(new RobustLineIntersector());
            ProcessNodes(geom, intCounter);
            return intCounter.Count;
        }
        public static int InteriorIntersectionCount(Geometry geom)
        {
            var intCounter = NodingIntersectionFinder.CreateInteriorIntersectionCounter(new RobustLineIntersector());
            ProcessNodes(geom, intCounter);
            return intCounter.Count;
        }

        private static void ProcessNodes(Geometry geom, NodingIntersectionFinder intFinder)
        {
            var noder = new MCIndexNoder(intFinder);
            noder.ComputeNodes(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
        }

        public static Geometry MCIndexNodingWithPrecision(Geometry geom, double scaleFactor)
        {
            var fixedPM = new PrecisionModel(scaleFactor);

            LineIntersector li = new RobustLineIntersector();
            li.PrecisionModel = fixedPM;

            INoder noder = new MCIndexNoder(new IntersectionAdder(li));
            noder.ComputeNodes(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return SegmentStringUtil.ToGeometry(noder.GetNodedSubstrings(), geom.Factory);
        }

        public static Geometry MCIndexNoding(Geometry geom)
        {
            INoder noder = new MCIndexNoder(new IntersectionAdder(new RobustLineIntersector()));
            noder.ComputeNodes(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return SegmentStringUtil.ToGeometry(noder.GetNodedSubstrings(), geom.Factory);
        }

        /// <summary>
        /// Reduces precision pointwise, then snap-rounds.
        /// Note that output set may not contain non-unique linework
        /// (and thus cannot be used as input to Polygonizer directly).
        /// UnaryUnion is one way to make the linework unique.
        /// </summary>
        /// <param name="geom">A geometry containing linework to node</param>
        /// <param name="scaleFactor">The precision model scale factor to use</param>
        /// <returns>The noded, snap-rounded linework</returns>
        public static Geometry SnapRoundWithPrecision(
            Geometry geom, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var roundedGeom = GeometryPrecisionReducer.ReducePointwise(geom, pm);

            var geomList = new List<Geometry>();
            geomList.Add(roundedGeom);

            var noder = new GeometryNoder(pm);
            var lines = noder.Node(geomList);

            return FunctionsUtil.GetFactoryOrDefault(geom).BuildGeometry(lines);
        }
        /// <summary>
        /// Runs a ScaledNoder on input.
        /// Input vertices should be rounded to precision model.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="scaleFactor"></param>
        /// <returns>The noded geometry</returns>
        public static Geometry ScaledNoding(Geometry geom, double scaleFactor)
        {
            var segs = CreateSegmentStrings(geom);
            var fixedPM = new PrecisionModel(scaleFactor);
            var noder = new ScaledNoder(new MCIndexSnapRounder(new PrecisionModel(1.0)),
                fixedPM.Scale);
            noder.ComputeNodes(segs);
            var nodedSegStrings = noder.GetNodedSubstrings();
            return SegmentStringUtil.ToGeometry(nodedSegStrings, geom.Factory);
        }

        private static List<ISegmentString> CreateSegmentStrings(Geometry geom)
        {
            var segs = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (LineString line in lines)
            {
                segs.Add(new BasicSegmentString(line.Coordinates, null));
            }
            return segs;
        }
    }
}
