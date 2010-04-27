using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer.Validate
{
    ///<summary>
    /// Validates that the result of a standard buffer operation
    /// is geometrically correct, within a computed tolerance.
    /// This checks only buffer operations which
    /// use the default parameters of round corners and end-caps.
    ///     * 
    /// This is a heuristic test, and may return false positive results

    /// (I.e. it may fail to detect an invalid result.)
    /// It should never return a false negative result, however
    /// (I.e. it should never report a valid result as invalid.)
    /// <p>
    /// This test may be (much) more expensive than the original
    ///  buffer computation.
    /// 
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class BufferResultValidator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        private static Boolean Verbose;

        /**
         * Maximum allowable fraction of buffer _distance the 
         * actual _distance can differ by.
         * 1% sometimes causes an error - 1.2% should be safe.
         */
        private const double MaxEnvDiffFrac = .012;

        ///<summary>
        ///</summary>
        ///<param name="g"></param>
        ///<param name="distance"></param>
        ///<param name="result"></param>
        ///<returns></returns>
        public static Boolean IsValid(IGeometry<TCoordinate> g, double distance, IGeometry<TCoordinate> result)
        {
            BufferResultValidator<TCoordinate> validator = new BufferResultValidator<TCoordinate>(g, distance, result);
            if (validator.IsValid())
                return true;
            return false;
        }

        /**
         * Checks whether the geometry buffer is valid, 
         * and returns an error message if not.
         * 
         * @param g
         * @param _distance
         * @param _result
         * @return an appropriate error message
         * @return null if the buffer is valid
         */
        ///<summary>
        ///</summary>
        ///<param name="g"></param>
        ///<param name="distance"></param>
        ///<param name="result"></param>
        ///<returns></returns>
        public static String IsValidMsg(IGeometry<TCoordinate> g, double distance, IGeometry<TCoordinate> result)
        {
            BufferResultValidator<TCoordinate> validator = new BufferResultValidator<TCoordinate>(g, distance, result);
            if (!validator.IsValid())
                return validator.ErrorMessage;
            return null;
        }

        private readonly IGeometry<TCoordinate> _input;
        private readonly double _distance;
        private readonly IGeometry<TCoordinate> _result;
        private Boolean _isValid = true;
        private String _errorMsg;
        private TCoordinate _errorLocation;

        ///<summary>
        ///</summary>
        ///<param name="input"></param>
        ///<param name="distance"></param>
        ///<param name="result"></param>
        public BufferResultValidator(IGeometry<TCoordinate> input, double distance, IGeometry<TCoordinate> result)
        {
            _input = input;
            _distance = distance;
            _result = result;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public Boolean IsValid()
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

        ///<summary>
        ///</summary>
        public String ErrorMessage
        {
            get { return _errorMsg; }
        }

        ///<summary>
        ///</summary>
        public TCoordinate ErrorLocation
        {
            get { return _errorLocation; }
        }

        private void Report(String checkName)
        {
            if (!Verbose) return;
            Console.WriteLine(String.Format("Check {0}: {1}", checkName, (_isValid ? "passed" : "FAILED")));
        }

        private void CheckPolygonal()
        {
            if (!(_result is IPolygon<TCoordinate>
                    || _result is IMultiPolygon<TCoordinate>))
                _isValid = false;
            _errorMsg = "Result is not polygonal";
            Report("Polygonal");
        }

        private void CheckExpectedEmpty()
        {
            // can't check areal features
            if (_input.Dimension >= Dimensions.Surface) return;
            // can't check positive distances
            if (_distance > 0.0) return;

            // at this point can expect an empty _result
            if (!_result.IsEmpty)
            {
                _isValid = false;
                _errorMsg = "Result is non-empty";
            }
            Report("ExpectedEmpty");
        }

        private void CheckEnvelope()
        {
            if (_distance < 0.0) return;

            double padding = _distance * MaxEnvDiffFrac;
            if (padding == 0.0) padding = 0.001;

            IExtents<TCoordinate> expectedEnv = _input.Factory.CreateExtents(_input.Extents);
            expectedEnv.ExpandBy(_distance);

            IExtents<TCoordinate> bufEnv = _result.Factory.CreateExtents(_result.Extents);
            bufEnv.ExpandBy(padding);

            if (!bufEnv.Contains(expectedEnv))
            {
                _isValid = false;
                _errorMsg = "Buffer envelope is incorrect";
            }
            Report("Envelope");
        }

        private void CheckArea()
        {
            double inputArea = _input is ISurface ? ((ISurface)_input).Area : 0d;
            double resultArea = _result is ISurface ? ((ISurface)_result).Area : 0d;

            if (_distance > 0.0
                    && inputArea > resultArea)
            {
                _isValid = false;
                _errorMsg = "Area of positive buffer is smaller than _input";
            }
            if (_distance < 0.0
                    && inputArea < resultArea)
            {
                _isValid = false;
                _errorMsg = "Area of negative buffer is larger than _input";
            }
            Report("Area");
        }

        private void CheckDistance()
        {
            BufferDistanceValidator<TCoordinate> distValid = new BufferDistanceValidator<TCoordinate>(_input, _distance, _result);
            if (!distValid.IsValid())
            {
                _isValid = false;
                _errorMsg = distValid.ErrorMessage;
                _errorLocation = distValid.ErrorLocation;
            }
            Report("Distance");
        }
    }
}