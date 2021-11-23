using System;
using System.Runtime.CompilerServices;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Functions for computing precision model scale factors
    /// that ensure robust geometry operations.
    /// In particular, these can be used to
    /// automatically determine appropriate scale factors for operations 
    /// using limited-precision noding (such as <see cref="OverlayNG"/>).
    /// <para/>
    /// WARNING: the <c>inherentScale</c> and <c>robustScale</c> 
    /// functions can be very slow, due to the method used to determine
    /// number of decimal places of a number.
    /// These are not recommended for production use.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class PrecisionUtility
    {
        /// <summary>
        /// A number of digits of precision which leaves some computational "headroom"
        /// to ensure robust evaluation of certain double-precision floating point geometric operations.
        /// <para/>
        /// This value should be less than the maximum decimal precision of double-precision values (16).
        /// </summary>
        public static int MAX_ROBUST_DP_DIGITS = 14;

        /// <summary>
        /// Computes a safe scale factor for a numeric value.
        /// A safe scale factor ensures that rounded 
        /// number has no more than <see cref="MAX_ROBUST_DP_DIGITS"/> 
        /// digits of precision.
        /// </summary>
        /// <param name="value">A numeric value.</param>
        /// <returns>A safe scale factor for the value</returns>
        public static double SafeScale(double value)
        {
            return PrecisionScale(value, MAX_ROBUST_DP_DIGITS);
        }

        /// <summary>
        /// Computes a safe scale factor for a geometry.
        /// A safe scale factor ensures that rounded 
        /// number has no more than <see cref="MAX_ROBUST_DP_DIGITS"/> 
        /// digits of precision.
        /// </summary>
        /// <param name="geom">A geometry.</param>
        /// <returns>A safe scale factor for the geometry ordinates</returns>
        public static double SafeScale(Geometry geom)
        {
            if (geom == null)
            {
                throw new ArgumentNullException(nameof(geom));
            }

            return SafeScale(MaxBoundMagnitude(geom.EnvelopeInternal));
        }

        /// <summary>
        /// Computes a safe scale factor for two geometry.
        /// A safe scale factor ensures that rounded 
        /// number has no more than <see cref="MAX_ROBUST_DP_DIGITS"/> 
        /// digits of precision.
        /// </summary>
        /// <param name="a">A geometry.</param>
        /// <param name="b">A geometry (which may be <c>null</c>).</param>
        /// <returns>A safe scale factor for the geometry ordinates</returns>
        public static double SafeScale(Geometry a, Geometry b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            double maxBnd = MaxBoundMagnitude(a.EnvelopeInternal);
            if (b != null)
            {
                double maxBndB = MaxBoundMagnitude(b.EnvelopeInternal);
                maxBnd = Math.Max(maxBnd, maxBndB);
            }

            double scale = SafeScale(maxBnd);
            return scale;
        }

        /// <summary>
        /// Determines the maximum magnitude (absolute value) of the bounds of an
        /// of an envelope.
        /// This is equal to the largest ordinate value
        /// which must be accommodated by a scale factor.
        /// </summary>
        /// <param name="env">An envelope</param>
        /// <returns>The value of the maximum bound magnitude</returns>
        private static double MaxBoundMagnitude(Envelope env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            return MathUtil.Max(
                Math.Abs(env.MaxX),
                Math.Abs(env.MaxY),
                Math.Abs(env.MinX),
                Math.Abs(env.MinY)
            );
        }

        // TODO: move to PrecisionModel?
        /// <summary>
        /// Computes the scale factor which will
        /// produce a given number of digits of precision(significant digits)
        /// when used to round the given number.
        /// <para/>
        /// For example: to provide 5 decimal digits of precision
        /// for the number 123.456 the precision scale factor is 100;
        /// for 3 digits of precision the scale factor is 1;
        /// for 2 digits of precision the scale factor is 0.1. 
        /// <para/>
        /// Rounding to the scale factor can be performed with <see cref="PrecisionModel.MakePrecise(double)"/>
        /// </summary>
        /// <param name="value">A number to be rounded</param>
        /// <param name="precisionDigits">The number of digits of precision required</param>
        /// <returns>The scale factor which provides the required number of digits of precision</returns>
        /// <seealso cref="PrecisionModel.MakePrecise(Coordinate)"/>
        private static double PrecisionScale(
            double value, int precisionDigits)
        {
            // the smallest power of 10 greater than the value
            int magnitude = (int) (Math.Log(value) / Math.Log(10) + 1.0);
            int precDigits = precisionDigits - magnitude;

            double scaleFactor = Math.Pow(10.0, precDigits);
            return scaleFactor;
        }

        /// <summary>
        /// Computes the inherent scale of a number.
        /// The inherent scale is the scale factor for rounding
        /// which preserves <b>all</b> digits of precision 
        /// (significant digits)
        /// present in the numeric value.
        /// In other words, it is the scale factor which does not
        /// change the numeric value when rounded:
        /// <code>
        ///   num = round( num, inherentScale(num) )
        /// </code>
        /// </summary>
        /// <param name="value">A number</param>
        /// <returns>The inherent scale factor of the number</returns>
        public static double InherentScale(double value)
        {
            int numDec = NumberOfDecimals(value);
            double scaleFactor = Math.Pow(10.0, numDec);
            return scaleFactor;
        }

        /// <summary>
        /// Computes the inherent scale of a geometry.
        /// The inherent scale is the scale factor for rounding
        /// which preserves <b>all</b> digits of precision 
        /// (significant digits)
        /// present in the geometry ordinates.
        /// <para/>
        /// This is the maximum inherent scale
        /// of all ordinate values in the geometry.
        /// <para/>
        /// WARNING: this is <b>very</b> slow.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns>The inherent scale factor in the geometry's ordinates</returns>
        public static double InherentScale(Geometry geom)
        {
            if (geom == null)
            {
                throw new ArgumentNullException(nameof(geom));
            }

            var scaleFilter = new InherentScaleFilter();
            geom.Apply(scaleFilter);
            return scaleFilter.Scale;
        }

        /// <summary>
        /// Computes the inherent scale of two geometries.
        /// The inherent scale is the scale factor for rounding
        /// which preserves <b>all</b> digits of precision 
        /// (significant digits)
        /// present in the geometry ordinates.
        /// <para/>
        /// This is the maximum inherent scale
        /// of all ordinate values in the geometries.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geomety (which may be <c>null</c>)</param>
        /// <returns>The inherent scale factor in the geometries' ordinates</returns>
        public static double InherentScale(Geometry a, Geometry b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            double scale = InherentScale(a);
            if (b != null)
            {
                double scaleB = InherentScale(b);
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

        /// <summary>
        /// Determines the 
        /// number of decimal places represented in a double-precision
        /// number (as determined by .NET).
        /// This uses the .NET double-precision print routine 
        /// to determine the number of decimal places,
        /// This is likely not optimal for performance, 
        /// but should be accurate and portable. 
        /// </summary>
        /// <param name="value">A numeric value</param>
        /// <returns>The number of decimal places in the value</returns>
        private static unsafe int NumberOfDecimals(double value)
        {
            // NaN, infinities, and many values closer to 0 than to 1 can fail this test.
            const double DecimalMaxValueAsDouble = 79228162514264337593543950335d;
            if (Math.Abs(value) < DecimalMaxValueAsDouble)
            {
                decimal valueAsDecimal = (decimal)value;

                // decimal does not guarantee a perfectly faithful conversion from double even when
                // it would be possible to do so (and it's not always possible to do so), so we need
                // to do at least some form of round-trip equality testing.  dotnet/runtime#42775
                // tracks the "even when it would be possible to do so" part of this issue.
                //
                // in practice, most values generated *completely* at random would tend to fail this
                // test every time, truncating one fractional digit from them tends to get them to
                // pass a visible amount of the time, and truncating two fractional digits gets them
                // to pass the test (seemingly) every time, so as long as both immediate neighboring
                // values on the number line are only representable in decimal with more digits, we
                // can just extract the scale byte from the converted-to-decimal representation and
                // get a 20-25x speedup compared to skipping right to the slow path every time.
                // given that this method is probably intended to be used primarily with values that
                // come from a source with a fixed (but unknown) precision that's significantly less
                // than the precision limit of a double-precision floating-point value, it seems
                // worth paying 4% to 5% more in cases that hit the slow-path in order to buy that
                // 20x-25x speedup elsewhere (airbreather 2020-09-26).
                if (value == (double)valueAsDecimal)
                {
                    // System.Decimal layout matches the layout of a Win32 DECIMAL, so the flags are
                    // in the first 32 bits of the value:
                    // https://github.com/dotnet/runtime/blob/cd4cc97e4c099f637061afe2b6c546483ffd3073/src/libraries/System.Private.CoreLib/src/System/Decimal.cs#L104-L108
                    // so we can rely on the scale living at the same spot:
                    // https://github.com/dotnet/runtime/blob/cd4cc97e4c099f637061afe2b6c546483ffd3073/src/libraries/System.Private.CoreLib/src/System/Decimal.cs#L70-L76
                    return ((byte*)&valueAsDecimal)[BitConverter.IsLittleEndian ? 2 : 1];
                }
            }

            // take the slow path: format to a string, get rid of scientific notation, and count the
            // number of digits to the right of the decimal point (or return zero if there is no
            // decimal point).
            return NumberOfDecimalsSlow(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int NumberOfDecimalsSlow(double value)
        {
            /*
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

        /// <summary>
        /// Applies the inherent scale calculation 
        /// to every ordinate in a geometry.
        /// <para/>
        /// WARNING: this is <b>very</b> slow.
        /// </summary>
        /// <author>Martin Davis</author>
        private sealed class InherentScaleFilter : IEntireCoordinateSequenceFilter
        {

            private int _maxNumberOfDecimalsSoFar = 0;

            public double Scale
            {
                get => Math.Pow(10, _maxNumberOfDecimalsSoFar);
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
                _maxNumberOfDecimalsSoFar = Math.Max(_maxNumberOfDecimalsSoFar, NumberOfDecimals(value));
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

        /// <summary>
        /// Determines a precision model to
        /// use for robust overlay operations for one geometry.
        /// The precision scale factor is chosen to maximize
        /// output precision while avoiding round-off issues.
        /// <para/>
        /// NOTE: this is a heuristic determination, so is not guaranteed to
        /// eliminate precision issues.
        /// <para/>
        /// WARNING: this is <b>very</b> slow.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <returns>A suitable precision model for overlay</returns>
        public static PrecisionModel RobustPM(Geometry a)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            double scale = PrecisionUtility.RobustScale(a);
            return new PrecisionModel(scale);
        }

        /// <summary>
        /// Determines a scale factor which maximizes
        /// the digits of precision and is
        /// safe to use for overlay operations.
        /// The robust scale is the minimum of the
        /// inherent scale and the safe scale factors.
        /// <para/>
        /// WARNING: this is <b>very</b> slow.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>A scale factor for use in overlay operations</returns>
        public static double RobustScale(Geometry a, Geometry b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            double inherentScale = InherentScale(a, b);
            double safeScale = SafeScale(a, b);
            return RobustScale(inherentScale, safeScale);
        }

        /// <summary>
        /// Determines a scale factor which maximizes
        /// the digits of precision and is
        /// safe to use for overlay operations.
        /// The robust scale is the minimum of the
        /// inherent scale and the safe scale factors.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <returns>A scale factor for use in overlay operations</returns>
        public static double RobustScale(Geometry a)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            double inherentScale = InherentScale(a);
            double safeScale = SafeScale(a);
            return RobustScale(inherentScale, safeScale);
        }

        private static double RobustScale(double inherentScale, double safeScale)
        {
            /*
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

    }
}
