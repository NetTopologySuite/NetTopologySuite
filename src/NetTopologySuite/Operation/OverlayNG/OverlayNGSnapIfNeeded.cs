using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Performs an overlay operation using full precision
    /// if possible, and snap-rounding only as a fall-back for failure.
    /// </summary>
    /// <author>Martin Davis</author>
    public class OverlayNGSnapIfNeeded
    {

        private const double SNAP_TOL_FACTOR = 1e12;

        public static Geometry Intersection(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Intersection);
        }

        public static Geometry Union(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, OverlayNG.UNION);
        }

        public static Geometry Difference(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, OverlayNG.DIFFERENCE);
        }

        public static Geometry SymDifference(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, OverlayNG.SYMDIFFERENCE);
        }

        private static readonly PrecisionModel PmFloat = new PrecisionModel();

        public static Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            Geometry result;
            Exception exOriginal;
            try
            {
                result = OverlayNG.Overlay(geom0, geom1, opCode, PmFloat);

                // Simple noding with no validation
                // There are cases where this succeeds with invalid noding (e.g. STMLF 1608).
                // So currently it is NOT safe to run overlay without noding validation
                //result = OverlayNG.overlay(geom0, geom1, opCode, createFloatingNoder()); 
                return result;
            }
            catch (Exception ex)
            {
                exOriginal = ex;
                // ignore this exception, since the operation will be rerun
                //Console.WriteLine("Overlay failed");
            }
            // on failure retry using snapping noding with a "safe" tolerance
            // if this throws an exception just let it go

            double snapTol = SnapTolerance(geom0, geom1);
            for (int i = 0; i < 5; i++)
            {
                result = OverlaySnapping(geom0, geom1, opCode, snapTol);
                if (result != null) return result;
                snapTol = snapTol * 10;
            }
            Console.WriteLine(geom0);
            Console.WriteLine(geom1);
            throw exOriginal;
        }

        /// <summary>
        /// Creates a noder using simple floating noding
        /// with no validation phase.
        /// This is twice as fast, and should be safe since
        /// OverlayNG is more sensitive to invalid noding.
        /// </summary>
        /// <returns>A floating noder with no validation</returns>
        private static INoder CreateFloatingNoder()
        {
            var noder = new MCIndexNoder();
            var li = new RobustLineIntersector();
            noder.SegmentIntersector = new IntersectionAdder(li);
            return noder;
        }

        private static INoder CreateSnappingNoder(double tolerance)
        {
            var snapNoder = new SnappingNoder(tolerance);
            return snapNoder;
            //return new ValidatingNoder(snapNoder);
        }

        private static Geometry OverlaySnapping(Geometry geom0, Geometry geom1, SpatialFunction opCode, double snapTol)
        {
            Geometry result;
            try
            {
                var noder = CreateSnappingNoder(snapTol);
                //Console.WriteLine("Snapping with " + snapTol);

                result = OverlayNG.Overlay(geom0, geom1, opCode, noder);
                return result;
            }
            catch (TopologyException ex)
            {
                Console.WriteLine("Snapping with " + snapTol + " - FAILED");
                //Console.WriteLine(geom0);
                //Console.WriteLine(geom1);
            }
            return null;
        }

        private static double SnapTolerance(Geometry geom0, Geometry geom1)
        {
            double tol0 = SnapTolerance(geom0);
            double tol1 = SnapTolerance(geom1);
            double snapTol = Math.Max(tol0, tol1);
            return snapTol;
        }

        private static double SnapTolerance(Geometry geom)
        {
            double magnitude = OrdinateMagnitude(geom);
            return magnitude / SNAP_TOL_FACTOR;
        }

        private static double OrdinateMagnitude(Geometry geom)
        {
            var env = geom.EnvelopeInternal;
            double magMax = Math.Max(
                Math.Abs(env.MaxX), Math.Abs(env.MaxY));
            double magMin = Math.Max(
                Math.Abs(env.MinX), Math.Abs(env.MinY));
            return Math.Max(magMax, magMin);
        }

        public static Geometry OverlaySR(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            Geometry result;
            try
            {
                // start with operation using floating PM
                result = OverlayNG.Overlay(geom0, geom1, opCode, PmFloat);
                return result;
            }
            catch (TopologyException ex)
            {
                // ignore this exception, since the operation will be rerun
                //Console.WriteLine("Overlay failed");
            }
            // on failure retry with a "safe" fixed PM
            // this should not throw an exception, but if it does just let it go
            double scaleSafe = PrecisionUtility.SafeScale(geom0, geom1);
            var pmSafe = new PrecisionModel(scaleSafe);
            result = OverlayNG.Overlay(geom0, geom1, opCode, pmSafe);
            return result;
        }

    }
}
