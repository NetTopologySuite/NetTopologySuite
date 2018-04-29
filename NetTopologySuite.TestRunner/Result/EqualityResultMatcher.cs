using System;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Result
{
    /// <summary>
    /// A <seealso cref="IResultMatcher{TResult}"/>  which compares result for equality, 
    /// up to the given tolerance.
    /// </summary>
    public class EqualityResultMatcher<TResult> : IResultMatcher<TResult>
        where TResult : class, IResult
    {
        /// <inheritdoc/>
        /// <remarks>Tests whether the two results are equal within the given tolerance.
        /// The input parameters are not considered.
        /// </remarks>
        public bool IsMatch(IGeometry geom, String opName, Object[] args,
                            TResult actualResult, TResult expectedResult,
                            double tolerance)
        {
            return actualResult.Equals(expectedResult, tolerance);
        }

        public bool IsMatch(IGeometry geom, string opName, object[] args, IResult actualResult, IResult expectedResult, double tolerance)
        {
            return IsMatch(geom, opName, args, actualResult as TResult, expectedResult as TResult, tolerance);
        }
    }
}