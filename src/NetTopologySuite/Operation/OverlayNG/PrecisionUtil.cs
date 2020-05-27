using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Functions for computing precision model scale factors
     * that ensure robust geometry operations.
     * In particular, these can be used to
     * automatically determine appropriate scale factors for operations 
     * using limited-precision noding (such as {@link OverlayNG}).
     * 
     * @author Martin Davis
     *
     */
    public class PrecisionUtil
    {
        /**
         * A number of digits of precision which leaves some computational "headroom"
         * to ensure robust evaluation of certain double-precision floating point geometric operations.
         * 
         * This value should be less than the maximum decimal precision of double-precision values (16).
         */
        public static int MAX_ROBUST_DP_DIGITS = 14;

        /**
         * Determines a precision model to 
         * use for robust overlay operations.
         * The precision scale factor is chosen to maximize 
         * output precision while avoiding round-off issues.
         * <p>
         * NOTE: this is a heuristic determination, so is not guaranteed to 
         * eliminate precision issues.
         * <p>
         * WARNING: this is quite slow.
         * 
         * @param a a geometry
         * @param b a geometry
         * @return a suitable precision model for overlay
         */
        public static PrecisionModel RobustPM(Geometry a, Geometry b)
        {
            double scale = PrecisionUtil.RobustScale(a, b);
            return new PrecisionModel(scale);
        }

        /**
         * Determines a precision model to 
         * use for robust overlay operations for one geometry.
         * The precision scale factor is chosen to maximize 
         * output precision while avoiding round-off issues.
         * <p>
         * NOTE: this is a heuristic determination, so is not guaranteed to 
         * eliminate precision issues.
         * <p>
         * WARNING: this is quite slow.
         * 
         * @param a a geometry
         * @return a suitable precision model for overlay
         */
        public static PrecisionModel RobustPM(Geometry a)
        {
            double scale = PrecisionUtil.RobustScale(a);
            return new PrecisionModel(scale);
        }

        /**
         * Determines a scale factor which maximizes 
         * the digits of precision and is 
         * safe to use for overlay operations.
         * The robust scale is the minimum of the 
         * inherent scale and the safe scale factors.
         * 
         * @param a a geometry 
         * @param b a geometry
         * @return a scale factor for use in overlay operations
         */
        public static double RobustScale(Geometry a, Geometry b)
        {
            double inherentScale = InherentScale(a, b);
            double safeScale = SafeScale(a, b);
            return RobustScale(inherentScale, safeScale);
        }

        /**
         * Determines a scale factor which maximizes 
         * the digits of precision and is 
         * safe to use for overlay operations.
         * The robust scale is the minimum of the 
         * inherent scale and the safe scale factors.
         * 
         * @param a a geometry 
         * @return a scale factor for use in overlay operations
         */
        public static double RobustScale(Geometry a)
        {
            double inherentScale = InherentScale(a);
            double safeScale = SafeScale(a);
            return RobustScale(inherentScale, safeScale);
        }

        private static double RobustScale(double inherentScale, double safeScale)
        {
            /**
             * Use safe scale if lower, 
             * since it is important to preserve some precision for robustness
             */
            if (inherentScale <= safeScale)
            {
                return inherentScale;
            }

            //System.out.println("Scale = " + scale);
            return safeScale;
        }

        /**
         * Computes a safe scale factor for a numeric value.
         * A safe scale factor ensures that rounded 
         * number has no more than {@link MAX_PRECISION_DIGITS} 
         * digits of precision.
         * 
         * @param value a numeric value
         * @return a safe scale factor for the value
         */
        public static double SafeScale(double value)
        {
            return PrecisionScale(value, MAX_ROBUST_DP_DIGITS);
        }

        /**
         * Computes a safe scale factor for a geometry.
         * A safe scale factor ensures that the rounded 
         * ordinates have no more than {@link MAX_PRECISION_DIGITS} 
         * digits of precision.
         * 
         * @param geom a geometry
         * @return a safe scale factor for the geometry ordinates
         */
        public static double SafeScale(Geometry geom)
        {
            return SafeScale(MaxBoundMagnitude(geom.EnvelopeInternal));
        }

        /**
         * Computes a safe scale factor for two geometries.
         * A safe scale factor ensures that the rounded 
         * ordinates have no more than {@link MAX_PRECISION_DIGITS} 
         * digits of precision.
         * 
         * @param a a geometry
         * @param b a geometry (which may be null)
         * @return a safe scale factor for the geometry ordinates
         */
        public static double SafeScale(Geometry a, Geometry b)
        {
            double maxBnd = MaxBoundMagnitude(a.EnvelopeInternal);
            if (b != null)
            {
                double maxBndB = MaxBoundMagnitude(b.EnvelopeInternal);
                maxBnd = Math.Max(maxBnd, maxBndB);
            }

            double scale = PrecisionUtil.SafeScale(maxBnd);
            return scale;
        }

        /**
         * Determines the maximum magnitude (absolute value) of the bounds of an
         * of an envelope.
         * This is equal to the largest ordinate value
         * which must be accommodated by a scale factor.
         * 
         * @param env an envelope
         * @return the value of the maximum bound magnitude
         */
        private static double MaxBoundMagnitude(Envelope env)
        {
            return MathUtil.Max(
                Math.Abs(env.MaxX),
                Math.Abs(env.MaxY),
                Math.Abs(env.MinX),
                Math.Abs(env.MinY)
            );
        }

        // TODO: move to PrecisionModel?
        /**
         * Computes the scale factor which will
         * produce a given number of digits of precision (significant digits)
         * when used to round the given number.
         * <p>
         * For example: to provide 5 decimal digits of precision
         * for the number 123.456 the precision scale factor is 100;
         * for 3 digits of precision the scale factor is 1;
         * for 2 digits of precision the scale factor is 0.1. 
         * <p>
         * Rounding to the scale factor can be performed with {@link PrecisionModel#round}
         * 
         * @param value a number to be rounded
         * @param precisionDigits the number of digits of precision required
         * @return scale factor which provides the required number of digits of precision 
         * 
         * @see PrecisionModel.round
         */
        private static double PrecisionScale(
            double value, int precisionDigits)
        {
            // the smallest power of 10 greater than the value
            int magnitude = (int) (Math.Log(value) / Math.Log(10) + 1.0);
            int precDigits = precisionDigits - magnitude;

            double scaleFactor = Math.Pow(10.0, precDigits);
            return scaleFactor;
        }

        /**
         * Computes the inherent scale of a number.
         * The inherent scale is the scale factor for rounding
         * which preserves <b>all</b> digits of precision 
         * (significant digits)
         * present in the numeric value.
         * In other words, it is the scale factor which does not
         * change the numeric value when rounded:
         * <pre>
         *   num = round( num, inherentScale(num) )
         * </pre>
         * 
         * @param value a number
         * @return the inherent scale factor of the number
         */
        public static double InherentScale(double value)
        {
            int numDec = NumberOfDecimals(value);
            double scaleFactor = Math.Pow(10.0, numDec);
            return scaleFactor;
        }

        /**
         * Computes the inherent scale of a geometry.
         * The inherent scale is the scale factor for rounding
         * which preserves <b>all</b> digits of precision 
         * (significant digits)
         * present in the geometry ordinates.
         * <p>
         * This is the maximum inherent scale
         * of all ordinate values in the geometry.
         *  
         * @param value a number
         * @return the inherent scale factor of the number
         */
        public static double InherentScale(Geometry geom)
        {
            var scaleFilter = new InherentScaleFilter();
            geom.Apply(scaleFilter);
            return scaleFilter.Scale;
        }

        /**
         * Computes the inherent scale of two geometries.
         * The inherent scale is the scale factor for rounding
         * which preserves <b>all</b> digits of precision 
         * (significant digits)
         * present in the geometry ordinates.
         * <p>
         * This is the maximum inherent scale
         * of all ordinate values in the geometries.
         * 
         * @param a a geometry
         * @param b a geometry
         * @return the inherent scale factor of the two geometries
         */
        public static double InherentScale(Geometry a, Geometry b)
        {
            double scale = PrecisionUtil.InherentScale(a);
            if (b != null)
            {
                double scaleB = PrecisionUtil.InherentScale(b);
                scale = Math.Max(scale, scaleB);
            }

            return scale;
        }

        /*
        // this doesn't work
        private static int BADnumDecimals(double value) {
          double val = Math.abs(value);
          double frac = val - Math.floor(val);
          int numDec = 0;
          while (frac > 0 && numDec < MAX_PRECISION_DIGITS) {
            double mul10 = 10 * frac;
            frac = mul10 - Math.floor(mul10);
            numDec ++;
          }
          return numDec;
        }
        */

        /**
         * Determines the 
         * number of decimal places represented in a double-precision
         * number (as determined by Java).
         * This uses the Java double-precision print routine 
         * to determine the number of decimal places,
         * This is likely not optimal for performance, 
         * but should be accurate and portable. 
         * 
         * @param value a numeric value
         * @return the number of decimal places in the value
         */
        private static int NumberOfDecimals(double value)
        {
            /**
             * Ensure that scientific notation is NOT used
             * (it would skew the number of fraction digits)
             */
            string s = OrdinateFormat.Default.Format(value);
            if (s.EndsWith(".0"))
                return 0;
            int len = s.Length;
            int decIndex = s.IndexOf('.');
            if (decIndex <= 0)
                return 0;
            return len - decIndex - 1;
        }

        /**
         * Applies the inherent scale calculation 
         * to every ordinate in a geometry.
         * 
         * @author Martin Davis
         *
         */
        private class InherentScaleFilter : IEntireCoordinateSequenceFilter
        {

            private double _scale = 0;

            public double Scale
            {
                get => _scale;
            }

            public void Filter(CoordinateSequence sequence)
            {
                for (int i = 0; i < sequence.Count; i++)
                {
                    UpdateScaleMax(sequence.GetX(i));
                    UpdateScaleMax(sequence.GetY(i));
                }
            }

            private void UpdateScaleMax(double value)
            {
                double scaleVal = PrecisionUtil.InherentScale(value);
                if (scaleVal > _scale)
                {
                    //System.out.println("Value " + value + " has scale: " + scaleVal);
                    _scale = scaleVal;
                }
            }

            public bool Done
            {
                get => false;
            }

            public bool GeometryChanged
            {
                get => false;
            }
        }
    }
}
