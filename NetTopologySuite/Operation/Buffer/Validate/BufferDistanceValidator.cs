namespace NetTopologySuite.Operation.Buffer.Validate
{
    using System.Collections.Generic;

    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Distance;
    using NetTopologySuite.Geometries.Utilities;

    /// <summary>
    /// Validates that a given buffer curve lies an appropriate distance
    /// from the input generating it.
    /// </summary>
    /// <author>
    /// mbdavis
    /// </author>
    public class BufferDistanceValidator 
    {
        private const double MAX_DISTANCE_DIFF_FRAC = .01;
	
        private IGeometry input;
        private double distance;
        private IGeometry result;
  
        private double minValidDistance;
        private double maxValidDistance;
  
        private bool isValid = true;
        private ICoordinate errorLocation = null;
  
        public BufferDistanceValidator(IGeometry input, double distance, IGeometry result)
        {
            this.input = input;
            this.distance = distance;
            this.result = result;
        }
  
        public bool IsValid()
        {
            double distDelta = MAX_DISTANCE_DIFF_FRAC * distance;
            minValidDistance = distance - distDelta;
            maxValidDistance = distance + distDelta;
  	
            // can't use this test if either is empty
            if (input.IsEmpty || result.IsEmpty)
  	            return true;
  	
            if (distance > 0.0) {
  	            CheckPositiveValid();
            }
            else
            {
  	            CheckNegativeValid();
            }
            return isValid;
        }
  
        public ICoordinate GetErrorLocation()
        {
            return errorLocation;
        }
  
        private void CheckPositiveValid()
        {
            IGeometry bufCurve = result.Boundary;
            CheckMinimumDistance(input, bufCurve, minValidDistance);
            if (! isValid) return;

            CheckMaximumDistance(input, bufCurve, maxValidDistance);
        }
  
        private void CheckNegativeValid()
        {
            // Assert: only polygonal inputs can be checked for negative buffers
  	
            // MD - could generalize this to handle GCs too
            if (! (input is IPolygon 
  		            || input is IMultiPolygon
  		            || input is IGeometryCollection
  		            )) {
  	            return;
            }
            IGeometry inputCurve = GetPolygonLines(input);
            CheckMinimumDistance(inputCurve, result, minValidDistance);
            if (! isValid) return;
  	
            CheckMaximumDistance(inputCurve, result, maxValidDistance);
        }
  
        private IGeometry GetPolygonLines(IGeometry g)
        {
            var lines = new List<ILineString>();
            LinearComponentExtracter lineExtracter = new LinearComponentExtracter(lines);
            var polys = PolygonExtracter.GetPolygons(g);
            foreach (var poly in polys)
            {
  	            poly.Apply(lineExtracter);
            }

            var lineGeometries = lines as IList<IGeometry>;

            return g.Factory.BuildGeometry(lineGeometries);
        }
  
        /// <summary> 
        /// Checks that the buffer curve is at least minDist from the input curve.
        /// </summary>
        /// <param name="inputCurve" />
        /// <param name="buf" />
        /// <param name="minDist" />
        private void CheckMinimumDistance(IGeometry inputCurve, IGeometry buf, double minDist)
        {
            DistanceOp distOp = new DistanceOp(input, buf, minDist);
            double dist = distOp.Distance();
            if (dist < minDist) {
  	            isValid = false;
  	            errorLocation = distOp.ClosestPoints()[1];
            }
        }

        /// <summary> 
        /// Checks that the Hausdorff distance from g1 to g2
        /// is less than the given maximum distance.
        /// This uses the oriented Hausdorff distance measure; it corresponds to finding
        /// the point on g1 which is furthest from <i>some</i> point on g2.
        /// </summary>
        /// <param name="g1">a geometry</param>
        /// <param name="g2">a geometry</param>
        /// <param name="maxDist">the maximum distance that a buffer result can be from the input</param>  
        private void CheckMaximumDistance(IGeometry input, IGeometry buf, double maxDist)
        {
            // TODO: to be developed
        }
    }
}
