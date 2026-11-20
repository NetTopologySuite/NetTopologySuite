using NetTopologySuite.Geometries;

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
        public bool IsMatch(Geometry geom, string opName, object[] args,
                            TResult actualResult, TResult expectedResult,
                            double tolerance)
        {
            return actualResult.Equals(expectedResult, tolerance);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// A result of a kind this matcher was not made for is not a match. The upstream matcher
        /// is not generic and answers the same, because the comparison it delegates to rejects a
        /// result it cannot read; here the cast has to be checked, or the answer becomes a
        /// NullReferenceException on the line below.
        /// </remarks>
        public bool IsMatch(Geometry geom, string opName, object[] args, IResult actualResult, IResult expectedResult, double tolerance)
        {
            if (!(actualResult is TResult actual))
                return false;

            return IsMatch(geom, opName, args, actual, expectedResult as TResult, tolerance);
        }
    }
}