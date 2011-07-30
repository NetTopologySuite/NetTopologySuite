namespace NetTopologySuite.Operation.Buffer.Validate
{
    using System;

    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Distance;
    using NetTopologySuite.Geometries.Utilities;

    /// <summary>
    /// Validates that the result of a buffer operation
    /// is geometrically correct, within a computed tolerance.
    /// <p>
    /// This is a heuristic test, and may return false positive results
    /// (I.e. it may fail to detect an invalid result.)
    /// It should never return a false negative result, however
    /// (I.e. it should never report a valid result as invalid.)
    /// <p>
    /// This test may be (much) more expensive than the original
    /// buffer computation.
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    public class BufferResultValidator 
    {
        private IGeometry input;
        private double distance;
        private IGeometry result;
        private bool isValid = true;
        private string errorMsg = null;
        private ICoordinate errorLocation = null;
  
        public BufferResultValidator(IGeometry input, double distance, IGeometry result)
        {
            this.input = input;
            this.distance = distance;
            this.result = result;
        }

        public static bool IsValid(IGeometry g, double distance, IGeometry result)
        {
            BufferResultValidator validator = new BufferResultValidator(g, distance, result);
            return validator.IsValid();
        }

        public bool IsValid()
        {
            CheckPolygonal();
            if (! isValid) return isValid;
            CheckExpectedEmpty();
            if (! isValid) return isValid;
            CheckEnvelope();
            if (! isValid) return isValid;
            CheckArea();
            if (! isValid) return isValid;
  	
            CheckDistance();
  	
            return isValid;
        }
  
        public string GetErrorMessage()
        {
            return errorMsg;
        }
  
        public ICoordinate GetErrorLocation()
        {
            return errorLocation;
        }
  
        private void CheckPolygonal()
        {
            if (! (result is IPolygon 
  		            || result is IMultiPolygon))
            isValid = false;
            errorMsg = "Result is not polygonal";
        }
  
        private void CheckExpectedEmpty()
        {
            // can't check areal features
            if (Convert.ToInt32(input.Dimension) >= 2) return;
            // can't check positive distances
            if (distance > 0.0) return;
  		
            // at this point can expect an empty result
            if (! result.IsEmpty) {
  	            isValid = false;
  	            errorMsg = "Result is non-empty";
            }
        }
  
        private void CheckEnvelope()
        {
            if (distance < 0.0) return;
  	
            double padding = distance * 0.01;
            if (padding == 0.0) padding = 0.001;

            Envelope expectedEnv = new Envelope(input.EnvelopeInternal);
            expectedEnv.ExpandBy(distance);
  	
            Envelope bufEnv = new Envelope(result.EnvelopeInternal);
            bufEnv.ExpandBy(padding);

            if (! bufEnv.Contains(expectedEnv)) {
  	            isValid = false;
  	            errorMsg = "Buffer envelope is incorrect";
            }
        }
  
        private void CheckArea()
        {
            double inputArea = input.Area;
            double resultArea = result.Area;
  	
            if (distance > 0.0
  		            && inputArea > resultArea) {
  	            isValid = false;
  	            errorMsg = "Area of positive buffer is smaller than input";
            }
            if (distance < 0.0
  		            && inputArea < resultArea) {
  	            isValid = false;
  	            errorMsg = "Area of negative buffer is larger than input";
            }
        }
  
        private void CheckDistance()
        {
            BufferDistanceValidator distValid = new BufferDistanceValidator(input, distance, result);
            if (! distValid.IsValid()) {
  	            isValid = false;
  	            errorMsg = "Buffer curve is incorrect distance from input";
  	            errorLocation = distValid.GetErrorLocation();
            }
        }
    }
}
