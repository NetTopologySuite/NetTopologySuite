using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Performs an overlay operation using <see cref="OverlayNG"/>,
    /// increasing robustness by using a series of
    /// increasingly robust (but slower) noding strategies.
    /// <para/>
    /// The noding strategies used are:
    /// <list type="number">
    /// <item><description>A simple fast noder using <see cref="PrecisionModels.Floating"/> precision</description></item>
    /// <item><description>A <see cref="SnappingNoder"/> using an automatically-determined snap tolerance</description></item>
    /// <item><description>First snapping each geometry to itself, and then overlaying them wih a <see cref="SnappingNoder"/></description></item>
    /// <item><description>The above two strategies are repeated with increasing snap tolerance, up to a limit</description></item>
    /// <item><description>Finally a <see cref="SnapRoundingNoder"/> is used with a automatically-determined scale factor.</description></item>
    /// </list>
    /// If all of the above heuristics fail to compute a valid overlay,
    /// the original <see cref="TopologyException"/> is thrown.
    /// In practice this should be extremely unlikely to occur.
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
    public sealed class OverlayNGRobust
    {
        /// <summary>
        /// Computes the unary union of a geometry using robust computation.
        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <returns>The union result</returns>
        public static Geometry Union(Geometry geom)
        {
            var op = new UnaryUnionOp(geom) {
                UnionStrategy = OverlayUnionStrategy
            };
            return op.Union();
        }

        /// <summary>
        /// Computes the unary union of a collection of geometries using robust computation.
        /// </summary>
        /// <param name="geoms">An enumeration of geometries to union</param>
        /// <returns>The union result</returns>
        public static Geometry Union(IEnumerable<Geometry> geoms)
        {
            var op = new UnaryUnionOp(geoms) {
                UnionStrategy = OverlayUnionStrategy
            };
            return op.Union();
        }

        /// <summary>
        /// Computes the unary union of a collection of geometries using robust computation.
        /// </summary>
        /// <param name="geoms">An enumeration of geometries to union</param>
        /// <param name="geomFact">The geometry factory to use</param>
        /// <returns>The union of the geometries</returns>
        public static Geometry Union(IEnumerable<Geometry> geoms, GeometryFactory geomFact)
        {
            var op = new UnaryUnionOp(geoms, geomFact) {
                UnionStrategy = OverlayUnionStrategy
            };
            return op.Union();
        }

        private static readonly UnionStrategy OverlayUnionStrategy =
            new UnionStrategy((g0, g1) => Overlay(g0, g1, SpatialFunction.Union), true);

        /// <summary>
        /// Overlay two geometries, using heuristics to ensure
        /// computation completes correctly.
        /// In practice the heuristics are observed to be fully correct.
        /// </summary>
        /// <param name="geom0">A geometry</param>
        /// <param name="geom1">A geometry</param>
        /// <param name="opCode">The overlay operation code</param>
        /// <returns>The overlay result geometry</returns>
        public static Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            if (geom0 == null)
            {
                throw new ArgumentNullException(nameof(geom0));
            }

            switch (opCode)
            {
                case SpatialFunction.Intersection:
                case SpatialFunction.Union:
                case SpatialFunction.Difference:
                case SpatialFunction.SymDifference:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(opCode), opCode, "Only Intersection, Union, Difference, and SymDifference are recognized at this time.");
            }

            /*
             * First try overlay with a PrecisionModels.Floating noder, which is
             * fast and causes least change to geometry coordinates
             * By default the noder is validated, which is required in order
             * to detect certain invalid noding situations which otherwise
             * cause incorrect overlay output.
             */
            try
            {
                return OverlayNG.Overlay(geom0, geom1, opCode);
            }
            catch (Exception ex)
            {
                /*
                 * On failure retry using snapping noding with a "safe" tolerance.
                 * if this throws an exception just let it go,
                 * since it is something that is not a TopologyException
                 */
                try
                {
                    if (OverlaySnapTries(geom0, geom1, opCode) is Geometry result)
                    {
                        return result;
                    }
                }
                catch (Exception ex2)
                {
                    throw new AggregateException(ex, ex2);
                }

                /*
                 * On failure retry using snap-rounding with a heuristic scale factor (grid size).
                 */
                try
                {
                    if (OverlaySR(geom0, geom1, opCode) is Geometry result)
                    {
                        return result;
                    }
                }
                catch (Exception ex2)
                {
                    throw new AggregateException(ex, ex2);
                }

                /*
                 * Just can't get overlay to work, so throw original error.
                 */
                throw;
            }
        }


        private const int NUM_SNAP_TRIES = 5;

        /// <summary>
        /// Attempt overlay using snapping with repeated tries with increasing snap tolerances.
        /// </summary>
        /// <param name ="geom0"></param>
        /// <param name ="geom1"></param>
        /// <param name ="opCode"></param>
        /// <returns>The computed overlay result, or null if the overlay fails</returns>
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

        /// <summary>
        /// Attempt overlay using a <see cref="SnappingNoder"/>.
        /// </summary>
        /// <param name="geom0"></param>
        /// <param name="geom1"></param>
        /// <param name="opCode"></param>
        /// <param name="snapTol"></param>
        /// <returns>The computed overlay result, or null if the overlay fails</returns>
        private static Geometry OverlaySnapping(Geometry geom0, Geometry geom1, SpatialFunction opCode, double snapTol)
        {
            try
            {
                return OverlaySnapTol(geom0, geom1, opCode, snapTol);
            }
            catch (TopologyException ex)
            {
                //---- ignore exception, return null to indicate failure

                //System.out.println("Snapping with " + snapTol + " - FAILED");
                //log("Snapping with " + snapTol + " - FAILED", geom0, geom1);
            }
            return null;
        }

        /// <summary>
        /// Attempt overlay with first snapping each geometry individually.
        /// </summary>
        /// <param name="geom0"></param>
        /// <param name="geom1"></param>
        /// <param name="opCode"></param>
        /// <param name="snapTol"></param>
        /// <returns>The computed overlay result, or null if the overlay fails</returns>
        private static Geometry OverlaySnapBoth(Geometry geom0, Geometry geom1, SpatialFunction opCode, double snapTol)
        {
            try
            {
                var snap0 = SnapSelf(geom0, snapTol);
                var snap1 = SnapSelf(geom1, snapTol);
                //log("Snapping BOTH with " + snapTol, geom0, geom1);

                return OverlaySnapTol(snap0, snap1, opCode, snapTol);
            }
            catch (TopologyException ex)
            {
                //---- ignore exception, return null result to indicate failure
            }
            return null;
        }

        /// <summary>
        /// Self-snaps a geometry by running a union operation with it as the only input.
        /// This helps to remove narrow spike/gore artifacts to simplify the geometry,
        /// which improves robustness.
        /// Collapsed artifacts are removed from the result to allow using
        /// it in further overlay operations.
        /// </summary>
        /// <param name="geom">Geometry to self-snap</param>
        /// <param name="snapTol">Snap tolerance</param>
        /// <returns>The snapped geometry (homogenous)</returns>
        private static Geometry SnapSelf(Geometry geom, double snapTol)
        {
            var ov = new OverlayNG(geom, null);
            var snapNoder = new SnappingNoder(snapTol);
            ov.Noder = snapNoder;
            /*
             * Ensure the result is not mixed-dimension,
             * since it will be used in further overlay computation.
             * It may however be lower dimension, if it collapses completely due to snapping.
             */
            ov.StrictMode = true;
            return ov.GetResult();
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
            if (geom == null || geom.IsEmpty) return 0;
            var env = geom.EnvelopeInternal;
            double magMax = Math.Max(
                Math.Abs(env.MaxX), Math.Abs(env.MaxY));
            double magMin = Math.Max(
                Math.Abs(env.MinX), Math.Abs(env.MinY));
            return Math.Max(magMax, magMin);
        }

        //===============================================

        /// <summary>
        /// Attempt Overlay using Snap-Rounding with an automatically-determined
        /// scale factor.
        /// </summary>
        /// <param name="geom0"></param>
        /// <param name="geom1"></param>
        /// <param name="opCode"></param>
        /// <returns>the computed overlay result, or null if the overlay fails</returns>
        public static Geometry OverlaySR(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            Geometry result;
            try
            {
                //System.out.println("OverlaySnapIfNeeded: trying snap-rounding");
                double scaleSafe = PrecisionUtility.SafeScale(geom0, geom1);
                var pmSafe = new PrecisionModel(scaleSafe);
                result = OverlayNG.Overlay(geom0, geom1, opCode, pmSafe);
                return result;
            }
            catch (TopologyException ex)
            {
                //---- ignore exception, return null result to indicate failure
            }
            return null;
        }

    }
}
