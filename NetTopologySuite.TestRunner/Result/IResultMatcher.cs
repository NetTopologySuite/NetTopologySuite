using System;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Result
{
    public interface IResultMatcher
    {
        /// <summary>
        /// Tests whether the actual and expected results match well enough for the test to be considered as passed.
        /// </summary>
        /// <param name="geom">The target geometry</param>
        /// <param name="opName">The operation performed</param>
        /// <param name="args">The input arguments to the operation</param>
        /// <param name="actualResult">The actual computed result</param>
        /// <param name="expectedResult">The expected result of the test</param>
        /// <param name="tolerance">The tolerance for the test</param>
        /// <returns>True if the actual and expected results match</returns>
        bool IsMatch(IGeometry geom, String opName, Object[] args,
                     IResult actualResult, IResult expectedResult,
                     double tolerance);
    }
    
    /// <summary>
    /// An interface for classes which can determine whether
    /// two <see typeref="TestResult"/>s match, within a given <tt>tolerance</tt>.
    /// The matching may also take into account the original input parameters
    /// to the geometry method.
    /// </summary>
    public interface IResultMatcher<TResult> : IResultMatcher where TResult : IResult
    {
        /// <summary>
        /// Tests whether the actual and expected results match well enough for the test to be considered as passed.
        /// </summary>
        /// <param name="geom">The target geometry</param>
        /// <param name="opName">The operation performed</param>
        /// <param name="args">The input arguments to the operation</param>
        /// <param name="actualResult">The actual computed result</param>
        /// <param name="expectedResult">The expected result of the test</param>
        /// <param name="tolerance">The tolerance for the test</param>
        /// <returns>True if the actual and expected results match</returns>
        bool IsMatch(IGeometry geom, String opName, Object[] args,
                     TResult actualResult, TResult expectedResult,
                     double tolerance);
    }
}