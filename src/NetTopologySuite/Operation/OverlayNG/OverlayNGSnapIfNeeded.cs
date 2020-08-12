using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Performs an overlay operation, increasing robustness by using a series of
    /// increasingly aggressive(and slower) noding strategies.
    /// <para/>
    /// The noding strategies used are:
    /// <list type="number">
    /// <item><description>A simple fast noder using <see cref="PrecisionModels.Floating"/> precision</description></item>
    /// <item><description>A <see cref="SnappingNoder"/> using an automatically-determined snap tolerance</description></item>
    /// <item><description>First snapping each geometry to itself, and then overlaying them wih a <see cref="SnappingNoder"/></description></item>
    /// <item><description>The above two strategies are repeated with increasing snap tolerance, up to a limit</description></item>
    /// </list>
    /// If the above heuristics still fail to compute a valid overlay,
    /// the original <see cref="TopologyException"/> is thrown.
    /// <para/>
    /// This algorithm relies on each overlay operation execution
    /// throwing a <see cref="TopologyException"/> if it is unable
    /// to compute the overlay correctly.
    /// Generally this occurs because the noding phase does
    /// not produce a valid noding.
    /// This requires the use of a <see cref="ValidatingNoder"/>
    /// in order to check the results of using a floating noder.
    /// </summary>
    /// <author>Martin Davis</author>
    public class OverlayNGSnapIfNeeded
    {

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

        public static Geometry Union(Geometry a)
        {
            var unionSRFun = new UnionStrategy((g0, g1) => Overlay(g0, g1, SpatialFunction.Union), true);
            var op = new UnaryUnionOp(a) {UnionStrategy = unionSRFun};
            return op.Union();
        }

    private static readonly PrecisionModel PmFloat = new PrecisionModel();

        public static Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            Geometry result;
            Exception exOriginal;

            /*
             * First try overlay with a PrecisionModels.Floating noder, which is
             * fastest and causes least change to geometry coordinates
             * By default the noder is validated, which is required in order
             * to detect certain invalid noding situations which otherwise
             * cause incorrect overlay output.
             */
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
                /*
                 * Capture original exception,
                 * so it can be rethrown if the remaining strategies all fail.
                 */
                exOriginal = ex;
            }

            /*
             * On failure retry using snapping noding with a "safe" tolerance.
             * if this throws an exception just let it go,
             * since it is something that is not a TopologyException
             */
            result = OverlaySnapTries(geom0, geom1, opCode);
            if (result != null)
                return result;

            throw exOriginal;
        }


        private const int NUM_SNAP_TRIES = 5;

        private static Geometry OverlaySnapTries(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            Geometry result;
            double snapTol = SnapTolerance(geom0, geom1);

            for (int i = 0; i < NUM_SNAP_TRIES; i++)
            {

                result = OverlaySnapping(geom0, geom1, opCode, snapTol);
                if (result != null) return result;

                /*
                 * Now try snapping each input individually, 
                 * and then doing the overlay.
                 */
                result = OverlaySnapBoth(geom0, geom1, opCode, snapTol);
                if (result != null) return result;

                // increase the snap tolerance and try again
                snapTol = snapTol * 10;
            }
            // failed to compute overlay
            return null;
        }

        private static Geometry OverlaySnapping(Geometry geom0, Geometry geom1, SpatialFunction opCode, double snapTol)
        {
            try
            {
                return OverlaySnapTol(geom0, geom1, opCode, snapTol);
            }
            catch (TopologyException ex)
            {
                //---- ignore this exception, just return a null result

                //System.out.println("Snapping with " + snapTol + " - FAILED");
                //log("Snapping with " + snapTol + " - FAILED", geom0, geom1);
            }
            return null;
        }

        private static Geometry OverlaySnapBoth(Geometry geom0, Geometry geom1, SpatialFunction opCode, double snapTol)
        {
            try
            {
                var snap0 = OverlaySnapTol(geom0, null, OverlayNG.UNION, snapTol);
                var snap1 = OverlaySnapTol(geom1, null, OverlayNG.UNION, snapTol);
                //log("Snapping BOTH with " + snapTol, geom0, geom1);

                return OverlaySnapTol(snap0, snap1, opCode, snapTol);
            }
            catch (TopologyException ex)
            {
                //---- ignore this exception, just return a null result
            }
            return null;
        }

        private static Geometry OverlaySnapTol(Geometry geom0, Geometry geom1, SpatialFunction opCode, double snapTol)
        {
            var snapNoder = new SnappingNoder(snapTol);
            return OverlayNG.Overlay(geom0, geom1, opCode, snapNoder);
        }

        //============================================

        /// <summary>
        /// A factor for a snapping tolerance distance which
        /// should allow noding to be computed robustly.
        /// </summary>
        private const double SnapTolFactor = 1e12;

        /// <summary>
        /// Computes a heuristic snap tolerance distance
        /// for overlaying a pair of geometries using a <see cref="SnappingNoder"/>.
        /// </summary>
        /// <param name="geom0"></param>
        /// <param name="geom1"></param>
        /// <returns></returns>
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
            return magnitude / SnapTolFactor;
        }

        /// <summary>
        /// Computes the largest magnitude of the ordinates of a geometry,
        /// based on the geometry envelope.
        /// </summary>
        /// <param name="geom"></param>
        /// <returns>The magnitude of the largest ordinate</returns>
        private static double OrdinateMagnitude(Geometry geom)
        {
            if (geom == null) return 0;
            var env = geom.EnvelopeInternal;
            double magMax = Math.Max(
                Math.Abs(env.MaxX), Math.Abs(env.MaxY));
            double magMin = Math.Max(
                Math.Abs(env.MinX), Math.Abs(env.MinY));
            return Math.Max(magMax, magMin);
        }

        //===============================================

        private static void Log(string msg, Geometry geom0, Geometry geom1)
        {
            Console.WriteLine(msg);
            Console.WriteLine(geom0);
            Console.WriteLine(geom1);
        }

        /**
         * Creates a noder using simple floating noding 
         * with no validation phase.
         * This is twice as fast, but can cause
         * invalid overlay results.
         * 
         * @return a floating noder with no validation
         */
        /*
        private static Noder createFloatingNoValidNoder() {
          MCIndexNoder noder = new MCIndexNoder();
          LineIntersector li = new RobustLineIntersector();
          noder.setSegmentIntersector(new IntersectionAdder(li));
          return noder;
        }
        */

        /// <summary>
        /// Overlay using Snap-Rounding with an automatically-determined
        /// scale factor.
        /// <para/>
        /// NOTE: currently this strategy is not used, since all known
        /// test cases work using one of the Snapping strategies.
        /// </summary>
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
