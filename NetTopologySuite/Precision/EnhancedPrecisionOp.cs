using System;
using GeoAPI.Geometries;
#if !HAS_SYSTEM_APPLICATIONEXCEPTION
using ApplicationException = System.Exception;
#endif
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
        public static IGeometry Intersection(IGeometry geom0, IGeometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                IGeometry result = geom0.Intersection(geom1);
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
                CommonBitsOp cbo = new CommonBitsOp(true);
                IGeometry resultEP = cbo.Intersection(geom0, geom1);
                // check that result is a valid point after the reshift to orginal precision
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
        public static IGeometry Union(IGeometry geom0, IGeometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                IGeometry result = geom0.Union(geom1);
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
                CommonBitsOp cbo = new CommonBitsOp(true);
                IGeometry resultEP = cbo.Union(geom0, geom1);
                // check that result is a valid point after the reshift to orginal precision
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
        public static IGeometry Difference(IGeometry geom0, IGeometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                IGeometry result = geom0.Difference(geom1);
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
                CommonBitsOp cbo = new CommonBitsOp(true);
                IGeometry resultEP = cbo.Difference(geom0, geom1);
                // check that result is a valid point after the reshift to orginal precision
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
        public static IGeometry SymDifference(IGeometry geom0, IGeometry geom1)
        {
            ApplicationException originalEx;
            try
            {
                IGeometry result = geom0.SymmetricDifference(geom1);
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
                CommonBitsOp cbo = new CommonBitsOp(true);
                IGeometry resultEP = cbo.SymDifference(geom0, geom1);
                // check that result is a valid point after the reshift to orginal precision
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
        /// Computes the buffer of a <c>Geometry</c>, using enhanced precision.
        /// This method should no longer be necessary, since the buffer algorithm
        /// now is highly robust.
        /// </summary>
        /// <param name="geom">The first Geometry.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <returns>The Geometry representing the buffer of the input Geometry.</returns>
        [Obsolete("This method should no longer be necessary, since the buffer algorithm now is highly robust.")]
        public static IGeometry Buffer(IGeometry geom, double distance)
        {
            ApplicationException originalEx;
            try
            {
                IGeometry result = geom.Buffer(distance);
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
                CommonBitsOp cbo = new CommonBitsOp(true);
                IGeometry resultEP = cbo.Buffer(geom, distance);
                // check that result is a valid point after the reshift to orginal precision
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
