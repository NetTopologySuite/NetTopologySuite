using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Precision
{
    /// <summary>
    /// Provides versions of Geometry spatial functions which use
    /// enhanced precision techniques to reduce the likelihood of robustness problems.
    /// </summary>
    public static class EnhancedPrecisionOp
    {
        /// <summary>
        /// Computes the set-theoretic intersection of two 
        /// <see cref="Geometry{TCoordinate}"/>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>
        /// The Geometry representing the set-theoretic intersection of the input Geometries.
        /// </returns>
        public static IGeometry Intersection(IGeometry geom0, IGeometry geom1)
        {
            NtsException originalEx;

            try
            {
                IGeometry result = geom0.Intersection(geom1);
                return result;
            }
            catch (NtsException ex)
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
                {
                    throw originalEx;
                }

                return resultEP;
            }
            catch (NtsException)
            {
                throw originalEx;
            }
        }

        /// <summary>
        /// Computes the set-theoretic union of two <see cref="Geometry{TCoordinate}"/>s, 
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>
        /// The Geometry representing the set-theoretic union of the input Geometries.
        /// </returns>
        public static IGeometry Union(IGeometry geom0, IGeometry geom1)
        {
            NtsException originalEx;

            try
            {
                IGeometry result = geom0.Union(geom1);
                return result;
            }
            catch (NtsException ex)
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
                {
                    throw originalEx;
                }

                return resultEP;
            }
            catch (NtsException)
            {
                throw originalEx;
            }
        }

        /// <summary>
        /// Computes the set-theoretic difference of two <see cref="Geometry{TCoordinate}"/>s, 
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>
        /// The Geometry representing the set-theoretic difference of the input Geometries.
        /// </returns>
        public static IGeometry Difference(IGeometry geom0, IGeometry geom1)
        {
            NtsException originalEx;

            try
            {
                IGeometry result = geom0.Difference(geom1);
                return result;
            }
            catch (NtsException ex)
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
                {
                    throw originalEx;
                }

                return resultEP;
            }
            catch (NtsException)
            {
                throw originalEx;
            }
        }

        /// <summary>
        /// Computes the set-theoretic symmetric difference of two <see cref="Geometry{TCoordinate}"/>s, 
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>
        /// The Geometry representing the set-theoretic symmetric difference of the input Geometries.
        /// </returns>
        public static IGeometry SymDifference(IGeometry geom0, IGeometry geom1)
        {
             NtsException originalEx = null;

            try
            {
                IGeometry result = geom0.SymmetricDifference(geom1);
                return result;
            }
            catch (NtsException ex)
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
                {
                    throw originalEx;
                }

                return resultEP;
            }
            catch (NtsException)
            {
                throw originalEx;
            }
        }
    }
}