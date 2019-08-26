using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Provides versions of Geometry spatial functions which use
    /// enhanced precision techniques to reduce the likelihood of robustness problems.
    /// </summary>
    public class EnhancedPrecisionOp
    {
        /// <summary>
        /// Only static methods!
        /// </summary>
        private EnhancedPrecisionOp() { }

        /// <summary>
        /// Computes the set-theoretic intersection of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic intersection of the input Geometries.</returns>
        public static Geometry Intersection(Geometry geom0, Geometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                var result = geom0.Intersection(geom1);
                return result;
            }
            catch (ApplicationException ex)
            {
                originalEx = ex;
            }
            /*
             * If we are here, the original op encountered a precision problem
             * (or some other problem).  Retry the operation with
             * enhanced precision to see if it succeeds
             */
            try
            {
                var cbo = new CommonBitsOp(true);
                var resultEP = cbo.Intersection(geom0, geom1);
                // check that result is a valid point after the reshift to original precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (ApplicationException)
            {
                throw originalEx;
            }
        }

        /// <summary>
        /// Computes the set-theoretic union of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic union of the input Geometries.</returns>
        public static Geometry Union(Geometry geom0, Geometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                var result = geom0.Union(geom1);
                return result;
            }
            catch (ApplicationException ex)
            {
                originalEx = ex;
            }
            /*
             * If we are here, the original op encountered a precision problem
             * (or some other problem).  Retry the operation with
             * enhanced precision to see if it succeeds
             */
            try
            {
                var cbo = new CommonBitsOp(true);
                var resultEP = cbo.Union(geom0, geom1);
                // check that result is a valid point after the reshift to original precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (ApplicationException)
            {
                throw originalEx;
            }
        }

        /// <summary>
        /// Computes the set-theoretic difference of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic difference of the input Geometries.</returns>
        public static Geometry Difference(Geometry geom0, Geometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                var result = geom0.Difference(geom1);
                return result;
            }
            catch (ApplicationException ex)
            {
                originalEx = ex;
            }
            /*
             * If we are here, the original op encountered a precision problem
             * (or some other problem).  Retry the operation with
             * enhanced precision to see if it succeeds
             */
            try
            {
                var cbo = new CommonBitsOp(true);
                var resultEP = cbo.Difference(geom0, geom1);
                // check that result is a valid point after the reshift to original precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (ApplicationException)
            {
                throw originalEx;
            }
        }

        /// <summary>
        /// Computes the set-theoretic symmetric difference of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic symmetric difference of the input Geometries.</returns>
        public static Geometry SymDifference(Geometry geom0, Geometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                var result = geom0.SymmetricDifference(geom1);
                return result;
            }
            catch (ApplicationException ex)
            {
                originalEx = ex;
            }
            /*
             * If we are here, the original op encountered a precision problem
             * (or some other problem).  Retry the operation with
             * enhanced precision to see if it succeeds
             */
            try
            {
                var cbo = new CommonBitsOp(true);
                var resultEP = cbo.SymDifference(geom0, geom1);
                // check that result is a valid point after the reshift to original precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (ApplicationException)
            {
                throw originalEx;
            }
        }
    }
}
