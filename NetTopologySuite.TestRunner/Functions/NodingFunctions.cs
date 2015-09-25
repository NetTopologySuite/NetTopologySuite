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

        public static bool IsNodingValid(IGeometry geom)
        {
            FastNodingValidator nv = new FastNodingValidator(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return nv.IsValid;
        }

        public static IGeometry FindSingleNodePoint(IGeometry geom)
        {
            FastNodingValidator nv = new FastNodingValidator(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            bool temp = nv.IsValid;
            var intPts = nv.Intersections;
            if (intPts.Count == 0) return null;
            return FunctionsUtil.GetFactoryOrDefault((IGeometry)null).CreatePoint((Coordinate)intPts[0]);
        }

        public static IGeometry FindNodePoints(IGeometry geom)
        {
            IList<Coordinate> intPts = FastNodingValidator.ComputeIntersections(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return FunctionsUtil.GetFactoryOrDefault((IGeometry)null).CreateMultiPoint(CoordinateArrays.ToCoordinateArray(intPts));
        }

        public static int InteriorIntersectionCount(IGeometry geom)
        {
            InteriorIntersectionFinder intCounter = InteriorIntersectionFinder.CreateIntersectionCounter(new RobustLineIntersector());
            INoder noder = new MCIndexNoder(intCounter);
            noder.ComputeNodes(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return intCounter.Count;
        }

        public static IGeometry MCIndexNodingWithPrecision(IGeometry geom, double scaleFactor)
        {
            IPrecisionModel fixedPM = new PrecisionModel(scaleFactor);

            LineIntersector li = new RobustLineIntersector();
            li.PrecisionModel = fixedPM;

            INoder noder = new MCIndexNoder(new IntersectionAdder(li));
            noder.ComputeNodes(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return SegmentStringUtil.ToGeometry(noder.GetNodedSubstrings(), geom.Factory);
        }

        public static IGeometry MCIndexNoding(IGeometry geom)
        {
            INoder noder = new MCIndexNoder(new IntersectionAdder(new RobustLineIntersector()));
            noder.ComputeNodes(SegmentStringUtil.ExtractNodedSegmentStrings(geom));
            return SegmentStringUtil.ToGeometry(noder.GetNodedSubstrings(), geom.Factory);
        }

        /// <summary>
        /// Runs a ScaledNoder on input.
        /// Input vertices should be rounded to precision model.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="scaleFactor"></param>
        /// <returns>The noded geometry</returns>
        public static IGeometry ScaledNoding(IGeometry geom, double scaleFactor)
        {
            var segs = CreateSegmentStrings(geom);
            var fixedPM = new PrecisionModel(scaleFactor);
            var noder = new ScaledNoder(new MCIndexSnapRounder(new PrecisionModel(1.0)),
                fixedPM.Scale);
            noder.ComputeNodes(segs);
            var nodedSegStrings = noder.GetNodedSubstrings();
            return SegmentStringUtil.ToGeometry(nodedSegStrings, geom.Factory);
        }

        private static List<ISegmentString> CreateSegmentStrings(IGeometry geom)
        {
            var segs = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (ILineString line in lines)
            {
                segs.Add(new BasicSegmentString(line.Coordinates, null));
            }
            return segs;
        }
    }
}