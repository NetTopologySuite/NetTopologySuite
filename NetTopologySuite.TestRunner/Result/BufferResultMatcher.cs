using System;
using System.Globalization;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;

namespace Open.Topology.TestRunner.Result
{
    /// <summary>
    /// A <see cref="IResultMatcher{GeometryResult}"/> which compares the results of
    /// buffer operations for equality, up to the given tolerance.
    /// All other operations are delagated to the 
    /// standard <see cref="EqualityResultMatcher{GeometryResult}"/> algorithm.
    /// </summary>
    /// <author>mbdavis</author>
    public class BufferResultMatcher : IResultMatcher<GeometryResult>
    {
        private readonly IResultMatcher<GeometryResult> _defaultMatcher = new EqualityResultMatcher<GeometryResult>();

        /// <summary>
        /// Tests whether the two results are equal within the given
        /// tolerance. The input parameters are not considered.
        /// </summary>
        /// <param name="geom">The target geometry</param>
        /// <param name="opName">The operation performed</param>
        /// <param name="args">The input arguments to the operation</param>
        /// <param name="actualResult">The actual computed result</param>
        /// <param name="expectedResult">The expected result of the test</param>
        /// <param name="tolerance">The tolerance for the test</param>
        /// <returns>true if the actual and expected results are considered equal</returns>
        public bool IsMatch(IGeometry geom, String opName, Object[] args,
                            GeometryResult actualResult, GeometryResult expectedResult,
                            double tolerance)
        {
            if (String.Compare(opName, "buffer", true) != 0)
                return _defaultMatcher.IsMatch(geom, opName, args, actualResult, expectedResult, tolerance);

            double distance;
            double.TryParse(((String) args[0]), NumberStyles.Any, CultureInfo.InvariantCulture, out distance);
            
            return IsBufferResultMatch(actualResult.Value, expectedResult.Value, distance);
        }

        private const double MaxRelativeAreaDifference = 1.0E-3;
        private const double MaxHausdorffDistanceFactor = 100;

        /**
         * The minimum distance tolerance which will be used.
         * This is required because densified vertices do no lie precisely on their parent segment.
         */
        private const double MinDistanceTolerance = 1.0e-8;

        public bool IsBufferResultMatch(IGeometry actualBuffer, IGeometry expectedBuffer, double distance)
        {
            if (actualBuffer.IsEmpty && expectedBuffer.IsEmpty)
                return true;

            /**
             * MD - need some more checks here - symDiffArea won't catch very small holes ("tears") 
             * near the edge of computed buffers (which can happen in current version of JTS (1.8)).  
             * This can probably be handled by testing
             * that every point of the actual buffer is at least a certain distance away from the 
             * geometry boundary.  
            */
            if (!IsSymDiffAreaInTolerance(actualBuffer, expectedBuffer))
                return false;

            if (!IsBoundaryHausdorffDistanceInTolerance(actualBuffer, expectedBuffer, distance))
                return false;

            return true;
        }

        public bool IsSymDiffAreaInTolerance(IGeometry actualBuffer, IGeometry expectedBuffer)
        {
            double area = expectedBuffer.Area;
            var diff = actualBuffer.SymmetricDifference(expectedBuffer);
            //		System.out.println(diff);
            double areaDiff = diff.Area;

            // can't get closer than difference area = 0 !  This also handles case when symDiff is empty
            if (areaDiff <= 0.0)
                return true;

            double frac = Double.PositiveInfinity;
            if (area > 0.0)
                frac = areaDiff/area;

            return frac < MaxRelativeAreaDifference;
        }

        public bool IsBoundaryHausdorffDistanceInTolerance(IGeometry actualBuffer, IGeometry expectedBuffer,
                                                           double distance)
        {
            var actualBdy = actualBuffer.Boundary;
            var expectedBdy = expectedBuffer.Boundary;

            var haus = new DiscreteHausdorffDistance(actualBdy, expectedBdy) {DensifyFraction = 0.25};
            double maxDistanceFound = haus.OrientedDistance();
            double expectedDistanceTol = Math.Abs(distance)/MaxHausdorffDistanceFactor;
            if (expectedDistanceTol < MinDistanceTolerance)
                expectedDistanceTol = MinDistanceTolerance;
            if (maxDistanceFound > expectedDistanceTol)
                return false;
            return true;
        }

        public bool IsMatch(IGeometry geom, string opName, object[] args, IResult actualResult, IResult expectedResult, double tolerance)
        {
            return IsMatch(geom, opName, args, actualResult as GeometryResult, expectedResult as GeometryResult,
                           tolerance);
        }
    }
}