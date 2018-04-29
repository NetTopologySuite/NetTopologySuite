using System;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Result
{
    /// <summary>
    /// A <seealso cref="IResultMatcher{TResult}"/> which always passes.
    /// This is useful if the expected result of an operation is not known.
    /// </summary>
    public class NullResultMatcher<TResult> : IResultMatcher<TResult>
        where TResult : IResult
    {
        ///<inheritdoc/>
        ///<remarks>Always reports a match.</remarks>
        public bool IsMatch(IGeometry geom, String opName, Object[] args,
                            TResult actualResult, TResult expectedResult,
                            double tolerance)
        {
            return true;
        }

        public bool IsMatch(IGeometry geom, string opName, object[] args, IResult actualResult, IResult expectedResult, double tolerance)
        {
            return true;
        }
    }
}