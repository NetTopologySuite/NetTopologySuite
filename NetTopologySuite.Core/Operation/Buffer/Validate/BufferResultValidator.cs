using System.Diagnostics;
using GeoAPI.Geometries;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /// <summary>
    ///     Validates that the result of a buffer operation
    ///     is geometrically correct, within a computed tolerance.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a heuristic test, and may return false positive results
    ///         (I.e. it may fail to detect an invalid result.)
    ///         It should never return a false negative result, however
    ///         (I.e. it should never report a valid result as invalid.)
    ///     </para>
    ///     <para>This test may be (much) more expensive than the original buffer computation.</para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class BufferResultValidator
    {
        /**
         * Maximum allowable fraction of buffer distance the
         * actual distance can differ by.
         * 1% sometimes causes an error - 1.2% should be safe.
         */
        private const double MaxEnvDiffFrac = .012;
        public static bool Verbose;
        private readonly double _distance;

        private readonly IGeometry _input;
        private readonly IGeometry _result;
        private bool _isValid = true;

        public BufferResultValidator(IGeometry input, double distance, IGeometry result)
        {
            _input = input;
            _distance = distance;
            _result = result;
        }

        /// <summary>
        ///     Gets the error message
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        ///     Gets the error location
        /// </summary>
        public Coordinate ErrorLocation { get; private set; }

        /// <summary>
        ///     Gets a geometry which indicates the location and nature of a validation failure.
        ///     <para>
        ///         If the failure is due to the buffer curve being too far or too close
        ///         to the input, the indicator is a line segment showing the location and size
        ///         of the discrepancy.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     A geometric error indicator<br />
        ///     or
        ///     <value>null</value>
        ///     , if no error was found
        /// </returns>
        public IGeometry ErrorIndicator { get; private set; }

        public static bool IsValid(IGeometry g, double distance, IGeometry result)
        {
            var validator = new BufferResultValidator(g, distance, result);
            if (validator.IsValid())
                return true;
            return false;
        }

        /// <summary>
        ///     Checks whether the geometry buffer is valid, and returns an error message if not.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="distance"></param>
        /// <param name="result"></param>
        /// <returns>
        ///     An appropriate error message<br />
        ///     or <c>null</c>if the buffer is valid
        /// </returns>
        public static string IsValidMessage(IGeometry g, double distance, IGeometry result)
        {
            var validator = new BufferResultValidator(g, distance, result);
            if (!validator.IsValid())
                return validator.ErrorMessage;
            return null;
        }

        public bool IsValid()
        {
            CheckPolygonal();
            if (!_isValid) return _isValid;
            CheckExpectedEmpty();
            if (!_isValid) return _isValid;
            CheckEnvelope();
            if (!_isValid) return _isValid;
            CheckArea();
            if (!_isValid) return _isValid;
            CheckDistance();
            return _isValid;
        }

        private void Report(string checkName)
        {
            if (!Verbose) return;
#if !PCL
            Debug.WriteLine("Check " + checkName + ": "
                            + (_isValid ? "passed" : "FAILED"));
#endif
        }

        private void CheckPolygonal()
        {
            if (!(_result is IPolygon
                  || _result is IMultiPolygon))
                _isValid = false;
            ErrorMessage = "Result is not polygonal";
            ErrorIndicator = _result;
            Report("Polygonal");
        }

        private void CheckExpectedEmpty()
        {
            // can't check areal features
            if (_input.Dimension >= Dimension.Surface) return;
            // can't check positive distances
            if (_distance > 0.0) return;

            // at this point can expect an empty result
            if (!_result.IsEmpty)
            {
                _isValid = false;
                ErrorMessage = "Result is non-empty";
                ErrorIndicator = _result;
            }
            Report("ExpectedEmpty");
        }

        private void CheckEnvelope()
        {
            if (_distance < 0.0) return;

            var padding = _distance*MaxEnvDiffFrac;
            if (padding == 0.0) padding = 0.001;

            var expectedEnv = new Envelope(_input.EnvelopeInternal);
            expectedEnv.ExpandBy(_distance);

            var bufEnv = new Envelope(_result.EnvelopeInternal);
            bufEnv.ExpandBy(padding);

            if (!bufEnv.Contains(expectedEnv))
            {
                _isValid = false;
                ErrorMessage = "Buffer envelope is incorrect";
                ErrorIndicator = _result;
            }
            Report("Envelope");
        }

        private void CheckArea()
        {
            var inputArea = _input.Area;
            var resultArea = _result.Area;

            if ((_distance > 0.0)
                && (inputArea > resultArea))
            {
                _isValid = false;
                ErrorMessage = "Area of positive buffer is smaller than input";
                ErrorIndicator = _result;
            }
            if ((_distance < 0.0)
                && (inputArea < resultArea))
            {
                _isValid = false;
                ErrorMessage = "Area of negative buffer is larger than input";
                ErrorIndicator = _result;
            }
            Report("Area");
        }

        private void CheckDistance()
        {
            var distValid = new BufferDistanceValidator(_input, _distance, _result);
            if (!distValid.IsValid())
            {
                _isValid = false;
                ErrorMessage = distValid.ErrorMessage;
                ErrorLocation = distValid.ErrorLocation;
                ErrorIndicator = _result;
            }
            Report("Distance");
        }
    }
}