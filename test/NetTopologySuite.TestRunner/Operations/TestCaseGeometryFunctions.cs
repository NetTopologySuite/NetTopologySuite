using System;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Precision;
using NetTopologySuite.Simplify;
using SpatialFunction = NetTopologySuite.Operation.Overlay.SpatialFunction;

namespace Open.Topology.TestRunner.Operations
{
    public class TestCaseGeometryFunctions
    {
        public static Geometry bufferMitredJoin(Geometry g, double distance)
        {
            var bufParams = new BufferParameters();
            bufParams.JoinStyle = JoinStyle.Mitre;

            return BufferOp.Buffer(g, distance, bufParams);
        }

        public static Geometry densify(Geometry g, double distance)
        {
            return Densifier.Densify(g, distance);
        }

        public static double minClearance(Geometry g)
        {
            return MinimumClearance.GetDistance(g);
        }

        public static Geometry minClearanceLine(Geometry g)
        {
            return MinimumClearance.GetLine(g);
        }

        private static Geometry polygonize(Geometry g, bool extractOnlyPolygonal)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(lines);
            return polygonizer.GetGeometry();
        }

        public static Geometry polygonize(Geometry g)
        {
            return polygonize(g, false);
        }

        public static Geometry polygonizeValidPolygonal(Geometry g)
        {
            return polygonize(g, true);
        }

        public static Geometry simplifyDP(Geometry g, double distance)
        {
            return DouglasPeuckerSimplifier.Simplify(g, distance);
        }

        public static Geometry simplifyTP(Geometry g, double distance)
        {
            return TopologyPreservingSimplifier.Simplify(g, distance);
        }

        public static Geometry reducePrecision(Geometry g, double scaleFactor)
        {
            return GeometryPrecisionReducer.Reduce(g, new PrecisionModel(scaleFactor));
        }

        public static Geometry intersectionNG(Geometry geom0, Geometry geom1)
        {
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.Intersection);
        }

        public static Geometry unionNG(Geometry geom0, Geometry geom1)
        {
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.Union);
        }

        public static Geometry differenceNG(Geometry geom0, Geometry geom1)
        {
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.Difference);
        }

        public static Geometry symDifferenceNG(Geometry geom0, Geometry geom1)
        {
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.SymDifference);
        }

        public static Geometry intersectionSR(Geometry geom0, Geometry geom1, double scale)
        {
            var pm = new PrecisionModel(scale);
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.Intersection, pm);
        }

        public static Geometry unionSr(Geometry geom0, Geometry geom1, double scale)
        {
            var pm = new PrecisionModel(scale);
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.Union, pm);
        }

        public static Geometry differenceSR(Geometry geom0, Geometry geom1, double scale)
        {
            var pm = new PrecisionModel(scale);
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.Difference, pm);
        }

        public static Geometry symDifferenceSR(Geometry geom0, Geometry geom1, double scale)
        {
            var pm = new PrecisionModel(scale);
            return OverlayNG.Overlay(geom0, geom1, SpatialFunction.SymDifference, pm);
        }

        public static double unionArea(Geometry geom)
        {
            return geom.Union().Area;
        }

        public static double unionLength(Geometry geom)
        {
            return geom.Union().Length;
        }

        public static bool overlayAreaTest(Geometry a, Geometry b)
        {
            double areaDelta = AreaDelta(a, b);
            return areaDelta < 1e-6;
        }

        /// <summary>
        /// Computes the maximum area delta value resulting from identity equations over the
        /// overlay operations. The delta value is normalized to the total area of the geometries.
        /// If the overlay operations are computed correctly the area delta is expected to be very
        /// small (e.g. &lt; 1e-6).
        /// </summary>
        private static double AreaDelta(Geometry a, Geometry b)
        {
            double areaA = a?.Area ?? 0;
            double areaB = b?.Area ?? 0;

            // if an input is non-polygonal delta is 0
            if (areaA == 0 || areaB == 0)
                return 0;

            double areaU = a.Union(b).Area;
            double areaI = a.Intersection(b).Area;
            double areaDab = a.Difference(b).Area;
            double areaDba = b.Difference(a).Area;
            double areaSD = a.SymmetricDifference(b).Area;

            double maxDelta = 0;

            // & : intersection
            // - : difference
            // + : union
            // ^ : symdifference

            // A = ( A & B ) + ( A - B )
            double delta = Math.Abs(areaA - areaI - areaDab);
            if (delta > maxDelta)
                maxDelta = delta;

            // B = ( A & B ) + ( B - A )
            delta = Math.Abs(areaB - areaI - areaDba);
            if (delta > maxDelta)
                maxDelta = delta;

            //  ( A ^ B ) = ( A - B ) + ( B - A )
            delta = Math.Abs(areaDab + areaDba - areaSD);
            if (delta > maxDelta)
                maxDelta = delta;

            //  ( A + B ) = ( A & B ) + ( A ^ B )
            delta = Math.Abs(areaI + areaSD - areaU);
            if (delta > maxDelta)
                maxDelta = delta;

            //  ( A + B ) = ( A & B ) + ( A - B ) + ( A - B )
            delta = Math.Abs(areaU - areaI - areaDab - areaDba);
            if (delta > maxDelta)
                maxDelta = delta;

            // normalize the area delta value
            return maxDelta / (areaA + areaB);
        }
    }
}
