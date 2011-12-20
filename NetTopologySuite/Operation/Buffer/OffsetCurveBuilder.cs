using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Buffer
{
    /**
     * Computes the raw offset curve for a
     * single {@link Geometry} component (ring, line or point).
     * A raw offset curve line is not noded -
     * it may contain self-intersections (and usually will).
     * The final buffer polygon is computed by forming a topological graph
     * of all the noded raw curves and tracing outside contours.
     * The points in the raw curve are rounded
     * to a given {@link PrecisionModel}.
     *
     * @version 1.7
     */

    public class OffsetCurveBuilder
    {
        private double _distance;
        private readonly IPrecisionModel _precisionModel;
        private readonly IBufferParameters _bufParams;

        public OffsetCurveBuilder(
            IPrecisionModel precisionModel,
            IBufferParameters bufParams
            )
        {
            _precisionModel = precisionModel;
            _bufParams = bufParams;
        }

        /**
   * Gets the buffer parameters being used to generate the curve.
   *
   * @return the buffer parameters being used
   */

        public IBufferParameters BufferParameters
        {
            get { return _bufParams; }
        }

        /**
   * This method handles single points as well as LineStrings.
   * LineStrings are assumed <b>not</b> to be closed (the function will not
   * fail for closed lines, but will generate superfluous line caps).
   *
   * @param inputPts the vertices of the line to offset
   * @param distance the offset distance
   *
   * @return a Coordinate array representing the curve
   * @return null if the curve is empty
   */

        public Coordinate[] GetLineCurve(Coordinate[] inputPts, double distance)
        {
            _distance = distance;

            // a zero or negative width buffer of a line/point is empty
            if (distance < 0.0 && !_bufParams.IsSingleSided) return null;
            if (distance == 0.0) return null;

            var posDistance = Math.Abs(distance);
            var segGen = GetSegmentGenerator(posDistance);
            if (inputPts.Length <= 1)
            {
                ComputePointCurve(inputPts[0], segGen);
            }
            else
            {
                if (_bufParams.IsSingleSided)
                {
                    var isRightSide = distance < 0.0;
                    ComputeSingleSidedBufferCurve(inputPts, isRightSide, segGen);
                }
                else
                    ComputeLineBufferCurve(inputPts, segGen);
            }

            var lineCoord = segGen.GetCoordinates();
            return lineCoord;
        }

        /**
   * This method handles the degenerate cases of single points and lines,
   * as well as rings.
   *
   * @return a Coordinate array representing the curve
   * @return null if the curve is empty
   */

        public Coordinate[] GetRingCurve(Coordinate[] inputPts, Positions side, double distance)
        {
            _distance = distance;
            if (inputPts.Length <= 2)
                return GetLineCurve(inputPts, distance);

            // optimize creating ring for for zero distance
            if (distance == 0.0)
            {
                return CopyCoordinates(inputPts);
            }
            OffsetSegmentGenerator segGen = GetSegmentGenerator(distance);
            ComputeRingBufferCurve(inputPts, side, segGen);
            return segGen.GetCoordinates();
        }

        public Coordinate[] GetOffsetCurve(Coordinate[] inputPts, double distance)
        {
            _distance = distance;

            // a zero width offset curve is empty
            if (distance == 0.0) return null;

            var isRightSide = distance < 0.0;
            double posDistance = Math.Abs(distance);
            OffsetSegmentGenerator segGen = GetSegmentGenerator(posDistance);
            if (inputPts.Length <= 1)
            {
                ComputePointCurve(inputPts[0], segGen);
            }
            else
            {
                ComputeOffsetCurve(inputPts, isRightSide, segGen);
            }
            Coordinate[] curvePts = segGen.GetCoordinates();
            // for right side line is traversed in reverse direction, so have to reverse generated line
            if (isRightSide)
                CoordinateArrays.Reverse(curvePts);
            return curvePts;
        }

        private static Coordinate[] CopyCoordinates(Coordinate[] pts)
        {
            var copy = new Coordinate[pts.Length];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = new Coordinate(pts[i]);
            }
            return copy;
        }

        private OffsetSegmentGenerator GetSegmentGenerator(double distance)
        {
            return new OffsetSegmentGenerator(_precisionModel, _bufParams, distance);
        }

        /**
   * Use a value which results in a potential distance error which is
   * significantly less than the error due to
   * the quadrant segment discretization.
   * For QS = 8 a value of 100 is reasonable.
   * This should produce a maximum of 1% distance error.
   */
        private const double SimplifyFactor = 100.0;

        /**
   * Computes the distance tolerance to use during input
   * line simplification.
   *
   * @param distance the buffer distance
   * @return the simplification tolerance
   */

        private static double SimplifyTolerance(double bufDistance)
        {
            return bufDistance / SimplifyFactor;
        }

        private void ComputePointCurve(Coordinate pt, OffsetSegmentGenerator segGen)
        {
            switch (_bufParams.EndCapStyle)
            {
                case EndCapStyle.Round:
                    segGen.CreateCircle(pt);
                    break;
                case EndCapStyle.Square:
                    segGen.CreateSquare(pt);
                    break;
                // otherwise curve is empty (e.g. for a butt cap);
            }
        }

        private void ComputeLineBufferCurve(Coordinate[] inputPts, OffsetSegmentGenerator segGen)
        {
            var distTol = SimplifyTolerance(_distance);

            //--------- compute points for left side of line
            // Simplify the appropriate side of the line before generating
            var simp1 = BufferInputLineSimplifier.Simplify(inputPts, distTol);
            // MD - used for testing only (to eliminate simplification)
            //    Coordinate[] simp1 = inputPts;

            var n1 = simp1.Length - 1;
            segGen.InitSideSegments(simp1[0], simp1[1], Positions.Left);
            for (int i = 2; i <= n1; i++)
            {
                segGen.AddNextSegment(simp1[i], true);
            }
            segGen.AddLastSegment();
            // add line cap for end of line
            segGen.AddLineEndCap(simp1[n1 - 1], simp1[n1]);

            //---------- compute points for right side of line
            // Simplify the appropriate side of the line before generating
            var simp2 = BufferInputLineSimplifier.Simplify(inputPts, -distTol);
            // MD - used for testing only (to eliminate simplification)
            //    Coordinate[] simp2 = inputPts;
            var n2 = simp2.Length - 1;

            // since we are traversing line in opposite order, offset position is still LEFT
            segGen.InitSideSegments(simp2[n2], simp2[n2 - 1], Positions.Left);
            for (var i = n2 - 2; i >= 0; i--)
            {
                segGen.AddNextSegment(simp2[i], true);
            }
            segGen.AddLastSegment();
            // add line cap for start of line
            segGen.AddLineEndCap(simp2[1], simp2[0]);

            segGen.CloseRing();
        }

        /*
  private void OLDcomputeLineBufferCurve(Coordinate[] inputPts)
  {
    int n = inputPts.length - 1;

    // compute points for left side of line
    initSideSegments(inputPts[0], inputPts[1], Position.LEFT);
    for (int i = 2; i <= n; i++) {
      addNextSegment(inputPts[i], true);
    }
    addLastSegment();
    // add line cap for end of line
    addLineEndCap(inputPts[n - 1], inputPts[n]);

    // compute points for right side of line
    initSideSegments(inputPts[n], inputPts[n - 1], Position.LEFT);
    for (int i = n - 2; i >= 0; i--) {
      addNextSegment(inputPts[i], true);
    }
    addLastSegment();
    // add line cap for start of line
    addLineEndCap(inputPts[1], inputPts[0]);

    vertexList.closeRing();
  }
  */

        private void ComputeSingleSidedBufferCurve(Coordinate[] inputPts, bool isRightSide,
                                                   OffsetSegmentGenerator segGen)
        {
            var distTol = SimplifyTolerance(_distance);

            if (isRightSide)
            {
                // add original line
                segGen.AddSegments(inputPts, true);

                //---------- compute points for right side of line
                // Simplify the appropriate side of the line before generating
                var simp2 = BufferInputLineSimplifier.Simplify(inputPts, -distTol);
                // MD - used for testing only (to eliminate simplification)
                // Coordinate[] simp2 = inputPts;
                var n2 = simp2.Length - 1;

                // since we are traversing line in opposite order, offset position is still LEFT
                segGen.InitSideSegments(simp2[n2], simp2[n2 - 1], Positions.Left);
                segGen.AddFirstSegment();
                for (var i = n2 - 2; i >= 0; i--)
                {
                    segGen.AddNextSegment(simp2[i], true);
                }
            }
            else
            {
                // add original line
                segGen.AddSegments(inputPts, false);

                //--------- compute points for left side of line
                // Simplify the appropriate side of the line before generating
                var simp1 = BufferInputLineSimplifier.Simplify(inputPts, distTol);
                // MD - used for testing only (to eliminate simplification)
                //      Coordinate[] simp1 = inputPts;

                var n1 = simp1.Length - 1;
                segGen.InitSideSegments(simp1[0], simp1[1], Positions.Left);
                segGen.AddFirstSegment();
                for (var i = 2; i <= n1; i++)
                {
                    segGen.AddNextSegment(simp1[i], true);
                }
            }
            segGen.AddLastSegment();
            segGen.CloseRing();
        }

        private void ComputeOffsetCurve(Coordinate[] inputPts, Boolean isRightSide, OffsetSegmentGenerator segGen)
        {
            var distTol = SimplifyTolerance(_distance);

            if (isRightSide)
            {
                //---------- compute points for right side of line
                // Simplify the appropriate side of the line before generating
                var simp2 = BufferInputLineSimplifier.Simplify(inputPts, -distTol);
                // MD - used for testing only (to eliminate simplification)
                // Coordinate[] simp2 = inputPts;
                var n2 = simp2.Length - 1;

                // since we are traversing line in opposite order, offset position is still LEFT
                segGen.InitSideSegments(simp2[n2], simp2[n2 - 1], Positions.Left);
                segGen.AddFirstSegment();
                for (var i = n2 - 2; i >= 0; i--)
                {
                    segGen.AddNextSegment(simp2[i], true);
                }
            }
            else
            {
                //--------- compute points for left side of line
                // Simplify the appropriate side of the line before generating
                var simp1 = BufferInputLineSimplifier.Simplify(inputPts, distTol);
                // MD - used for testing only (to eliminate simplification)
                // Coordinate[] simp1 = inputPts;

                var n1 = simp1.Length - 1;
                segGen.InitSideSegments(simp1[0], simp1[1], Positions.Left);
                segGen.AddFirstSegment();
                for (var i = 2; i <= n1; i++)
                {
                    segGen.AddNextSegment(simp1[i], true);
                }
            }
            segGen.AddLastSegment();
        }

        private void ComputeRingBufferCurve(Coordinate[] inputPts, Positions side, OffsetSegmentGenerator segGen)
        {
            // simplify input line to improve performance
            var distTol = SimplifyTolerance(_distance);
            // ensure that correct side is simplified
            if (side == Positions.Right)
                distTol = -distTol;
            var simp = BufferInputLineSimplifier.Simplify(inputPts, distTol);
            // MD - used for testing only (to eliminate simplification)
            // Coordinate[] simp = inputPts;

            var n = simp.Length - 1;
            segGen.InitSideSegments(simp[n - 1], simp[0], side);
            for (var i = 1; i <= n; i++)
            {
                var addStartPoint = i != 1;
                segGen.AddNextSegment(simp[i], addStartPoint);
            }
            segGen.CloseRing();
        }
    }
}