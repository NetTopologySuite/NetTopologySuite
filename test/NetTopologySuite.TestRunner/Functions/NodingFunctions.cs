using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Precision;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class NodingFunctions
    {
        public static bool IsNodingValid(Geometry geom)
        {
            var nv = new FastNodingValidator(SegmentStringUtil.ExtractBasicSegmentStrings(geom));
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
            var nv = new FastNodingValidator(SegmentStringUtil.ExtractBasicSegmentStrings(geom));
            bool temp = nv.IsValid;
            var intPts = nv.Intersections;
            if (intPts.Count == 0) return null;
            return FunctionsUtil.GetFactoryOrDefault((Geometry)null).CreatePoint((Coordinate)intPts[0]);
        }

        public static Geometry FindNodes(Geometry geom)
        {
            var intPts = FastNodingValidator.ComputeIntersections(
                SegmentStringUtil.ExtractBasicSegmentStrings(geom));
            return FunctionsUtil.GetFactoryOrDefault((Geometry)null)
                .CreateMultiPointFromCoords(CoordinateArrays.ToCoordinateArray(intPts));
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
            noder.ComputeNodes(SegmentStringUtil.ExtractBasicSegmentStrings(geom));
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


        public static Geometry snappingNoder(Geometry geom, Geometry geom2, double snapDistance)
        {
            var segs = SegmentStringUtil.ExtractNodedSegmentStrings(geom);
            if (geom2 != null) {
                foreach (var seg in SegmentStringUtil.ExtractNodedSegmentStrings(geom2))
                    segs.Add(seg);
            }
            var noder = new SnappingNoder(snapDistance);
            noder.ComputeNodes(segs);
            var nodedSegStrings = noder.GetNodedSubstrings();
            return SegmentStringUtil.ToGeometry(nodedSegStrings, FunctionsUtil.GetFactoryOrDefault(geom));
        }

        public static Geometry snapRoundingNoder(Geometry geom, Geometry geom2, double scaleFactor)
        {
            var segs = SegmentStringUtil.ExtractNodedSegmentStrings(geom);
            if (geom2 != null)
            {
                foreach (var seg in SegmentStringUtil.ExtractNodedSegmentStrings(geom2))
                    segs.Add(seg);
            }
            var pm = new PrecisionModel(scaleFactor);
            var noder = new SnapRoundingNoder(pm);
            noder.ComputeNodes(segs);
            var nodedSegStrings = noder.GetNodedSubstrings();
            return SegmentStringUtil.ToGeometry(nodedSegStrings, FunctionsUtil.GetFactoryOrDefault(geom));
        }    }
}
