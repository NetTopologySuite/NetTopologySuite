using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /// <summary>
    /// Validates that a given buffer curve lies an appropriate distance
    /// from the input generating it.
    /// </summary>
    /// <remarks>
    /// Useful only for round buffers (cap and join).
    /// Can be used for either positive or negative distances.
    /// <para></para>
    /// <para>This is a heuristic test, and may return false positive results
    /// (I.e. it may fail to detect an invalid result.)
    /// It should never return a false negative result, however
    /// (I.e. it should never report a valid result as invalid.)</para>
    /// </remarks>
    /// <author>mbdavis</author>
    public class BufferDistanceValidator
    {
        public static bool Verbose;
        /*
         * Maximum allowable fraction of buffer distance the
         * actual distance can differ by.
         * 1% sometimes causes an error - 1.2% should be safe.
         */
        private const double MaxDistanceDiffFrac = .012;

        private readonly Geometry _input;
        private readonly double _bufDistance;
        private readonly Geometry _result;

        private double _minValidDistance;
        private double _maxValidDistance;

        private double _minDistanceFound;
        private double _maxDistanceFound;

        private bool _isValid = true;
        private string _errMsg;
        private Coordinate _errorLocation;
        private Geometry _errorIndicator;

        public BufferDistanceValidator(Geometry input, double bufDistance, Geometry result)
        {
            _input = input;
            _bufDistance = bufDistance;
            _result = result;
        }

        public bool IsValid()
        {
            double posDistance = Math.Abs(_bufDistance);
            double distDelta = MaxDistanceDiffFrac * posDistance;
            _minValidDistance = posDistance - distDelta;
            _maxValidDistance = posDistance + distDelta;

            // can't use this test if either is empty
            if (_input.IsEmpty || _result.IsEmpty)
                return true;

            if (_bufDistance > 0.0)
            {
                CheckPositiveValid();
            }
            else
            {
                CheckNegativeValid();
            }
            if (Verbose)
            {
                // ReSharper disable once RedundantStringFormatCall
                // String.Format needed to build 2.0 release!
                Debug.WriteLine(string.Format("Min Dist= {0}  err= {1}  Max Dist= {2}  err= {3}",
                    _minDistanceFound,
                    1.0 - _minDistanceFound / _bufDistance,
                    _maxDistanceFound,
                    _maxDistanceFound / _bufDistance - 1.0)
                  );
            }
            return _isValid;
        }

        public string ErrorMessage => _errMsg;

        public Coordinate ErrorLocation => _errorLocation;

        /// <summary>
        /// Gets a geometry which indicates the location and nature of a validation failure.
        /// <para>
        /// The indicator is a line segment showing the location and size
        /// of the distance discrepancy.
        /// </para>
        /// </summary>
        /// <returns>A geometric error indicator
        /// or <c>null</c>, if no error was found</returns>
        public Geometry ErrorIndicator => _errorIndicator;

        private void CheckPositiveValid()
        {
            var bufCurve = _result.Boundary;
            CheckMinimumDistance(_input, bufCurve, _minValidDistance);
            if (!_isValid) return;

            CheckMaximumDistance(_input, bufCurve, _maxValidDistance);
        }

        private void CheckNegativeValid()
        {
            // Assert: only polygonal inputs can be checked for negative buffers

            // MD - could generalize this to handle GCs too
            if (!(_input is Polygon
                    || _input is MultiPolygon
                    || _input is GeometryCollection
                    ))
            {
                return;
            }
            var inputCurve = GetPolygonLines(_input);
            CheckMinimumDistance(inputCurve, _result, _minValidDistance);
            if (!_isValid) return;

            CheckMaximumDistance(inputCurve, _result, _maxValidDistance);
        }

        private static Geometry GetPolygonLines(Geometry g)
        {
            var lines = new List<Geometry>();
            var lineExtracter = new LinearComponentExtracter(lines);
            var polys = PolygonExtracter.GetPolygons(g);
            foreach (var poly in polys)
            {
                poly.Apply(lineExtracter);
            }
            return g.Factory.BuildGeometry(polys);
        }

        /// <summary>
        /// Checks that two geometries are at least a minimum distance apart.
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <param name="minDist">The minimum distance the geometries should be separated by</param>
        private void CheckMinimumDistance(Geometry g1, Geometry g2, double minDist)
        {
            var distOp = new DistanceOp(g1, g2, minDist);
            _minDistanceFound = distOp.Distance();

            if (_minDistanceFound < minDist)
            {
                _isValid = false;
                var pts = distOp.NearestPoints();
                _errorLocation = pts[1];
                _errorIndicator = g1.Factory.CreateLineString(pts);
                _errMsg = "Distance between buffer curve and input is too small "
                    + "(" + _minDistanceFound
                    + " at " + WKTWriter.ToLineString(pts[0], pts[1]) + " )";
            }
        }

        /// <summary>
        /// Checks that the furthest distance from the buffer curve to the input
        /// is less than the given maximum distance.
        /// </summary>
        /// <remarks>
        /// This uses the Oriented Hausdorff distance metric. It corresponds to finding
        /// the point on the buffer curve which is furthest from <i>some</i> point on the input.
        /// </remarks>
        /// <param name="input">A geometry</param>
        /// <param name="bufCurve">A geometry</param>
        /// <param name="maxDist">The maximum distance that a buffer result can be from the input</param>
        private void CheckMaximumDistance(Geometry input, Geometry bufCurve, double maxDist)
        {
            //    BufferCurveMaximumDistanceFinder maxDistFinder = new BufferCurveMaximumDistanceFinder(input);
            //    maxDistanceFound = maxDistFinder.findDistance(bufCurve);

            var haus = new DiscreteHausdorffDistance(bufCurve, input);
            haus.DensifyFraction = 0.25;
            _maxDistanceFound = haus.OrientedDistance();

            if (_maxDistanceFound > maxDist)
            {
                _isValid = false;
                var pts = haus.Coordinates;
                _errorLocation = pts[1];
                _errorIndicator = input.Factory.CreateLineString(pts);
                _errMsg = "Distance between buffer curve and input is too large "
                  + "(" + _maxDistanceFound
                  + " at " + WKTWriter.ToLineString(pts[0], pts[1]) + ")";
            }
        }

        /*
        private void OLDcheckMaximumDistance(Geometry input, Geometry bufCurve, double maxDist)
        {
          BufferCurveMaximumDistanceFinder maxDistFinder = new BufferCurveMaximumDistanceFinder(input);
          maxDistanceFound = maxDistFinder.findDistance(bufCurve);

          if (maxDistanceFound > maxDist) {
            isValid = false;
            PointPairDistance ptPairDist = maxDistFinder.getDistancePoints();
            errorLocation = ptPairDist.getCoordinate(1);
            errMsg = "Distance between buffer curve and input is too large "
              + "(" + ptPairDist.getDistance()
              + " at " + ptPairDist.ToString() +")";
          }
        }
        */
    }
}
