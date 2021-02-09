namespace NetTopologySuite.Geography.Lib
{
    /// <summary>
    /// An accumulator for sums.
    /// <para/>
    /// This allow many double precision numbers to be added together with twice the
    /// normal precision.  Thus the effective precision of the sum is 106 bits or
    /// about 32 decimal places.
    /// <para/>
    /// The implementation follows J. R. Shewchuk,
    /// <a href="https://doi.org/10.1007/PL00009321"> Adaptive Precision
    /// Floating-Point Arithmetic and Fast Robust Geometric Predicates</a>,
    /// Discrete &amp; Computational Geometry 18(3) 305&ndash;363 (1997).
    /// <para/>
    /// In the documentation of the member functions, <i>sum</i> stands for the
    /// value currently held in the accumulator.
    /// </summary>
    internal class Accumulator
    {
        // _s + _t accumulators for the sum.
        private double _s, _t;
        /**
         * Construct from a double.
         * <p>
         * @param y set <i>sum</i> = <i>y</i>.
         **********************************************************************/
        public Accumulator(double y) { _s = y; _t = 0; }
        /**
         * Construct from another Accumulator.
         * <p>
         * @param a set <i>sum</i> = <i>a</i>.
         **********************************************************************/
        public Accumulator(Accumulator a) { _s = a._s; _t = a._t; }
        /**
         * Set the value to a double.
         * <p>
         * @param y set <i>sum</i> = <i>y</i>.
         **********************************************************************/
        public void Set(double y) { _s = y; _t = 0; }
        /**
         * Return the value held in the accumulator.
         * <p>
         * @return <i>sum</i>.
         **********************************************************************/
        public double Sum() { return _s; }
        /**
         * Return the result of adding a number to <i>sum</i> (but don't change
         * <i>sum</i>).
         * <p>
         * @param y the number to be added to the sum.
         * @return <i>sum</i> + <i>y</i>.
         **********************************************************************/
        public double Sum(double y)
        {
            (double first, double second) p = (0d, 0d);
            AddInternal(ref p, _s, _t, y);
            return p.first;
        }
        /**
         * Internal version of Add, p = [s, t] + y
         * <p>
         * @param s the larger part of the accumulator.
         * @param t the smaller part of the accumulator.
         * @param y the addend.
         * @param p output Pair(<i>s</i>, <i>t</i>) with the result.
         **********************************************************************/
        public static void AddInternal(ref (double first, double second) p, double s, double t, double y)
        {
            // Here's Shewchuk's solution...
            double u;                       // hold exact sum as [s, t, u]
                                            // Accumulate starting at least significant end
            GeoMath.sum(ref p, y, t); y = p.first; u = p.second;
            GeoMath.sum(ref p, y, s); s = p.first; t = p.second;
            // Start is s, t decreasing and non-adjacent.  Sum is now (s + t + u)
            // exactly with s, t, u non-adjacent and in decreasing order (except for
            // possible zeros).  The following code tries to normalize the result.
            // Ideally, we want s = round(s+t+u) and u = round(s+t+u - s).  The
            // following does an approximate job (and maintains the decreasing
            // non-adjacent property).  Here are two "failures" using 3-bit floats:
            //
            // Case 1: s is not equal to round(s+t+u) -- off by 1 ulp
            // [12, -1] - 8 -> [4, 0, -1] -> [4, -1] = 3 should be [3, 0] = 3
            //
            // Case 2: s+t is not as close to s+t+u as it shold be
            // [64, 5] + 4 -> [64, 8, 1] -> [64,  8] = 72 (off by 1)
            //                    should be [80, -7] = 73 (exact)
            //
            // "Fixing" these problems is probably not worth the expense.  The
            // representation inevitably leads to small errors in the accumulated
            // values.  The additional errors illustrated here amount to 1 ulp of the
            // less significant word during each addition to the Accumulator and an
            // additional possible error of 1 ulp in the reported sum.
            //
            // Incidentally, the "ideal" representation described above is not
            // canonical, because s = round(s + t) may not be true.  For example,
            // with 3-bit floats:
            //
            // [128, 16] + 1 -> [160, -16] -- 160 = round(145).
            // But [160, 0] - 16 -> [128, 16] -- 128 = round(144).
            //
            if (s == 0)                 // This implies t == 0,
                s = u;                    // so result is u
            else
                t += u;                   // otherwise just accumulate u to t.
            p.first = s; p.second = t;
        }

        /**
         * Add a number to the accumulator.
         * <p>
         * @param y set <i>sum</i> += <i>y</i>.
         **********************************************************************/
        public void Add(double y)
        {
            (double first, double second) p = (0d, 0d);
            AddInternal(ref p, _s, _t, y);
            _s = p.first; _t = p.second;
        }
        /**
         * Negate an accumulator.
         * <p>
         * Set <i>sum</i> = &minus;<i>sum</i>.
         **********************************************************************/
        public void Negate() { _s = -_s; _t = -_t; }
        /**
         * Take the remainder.
         * <p>
         * @param y the modulus
         * <p>
         * Put <i>sum</i> in the rangle [&minus;<i>y</i>, <i>y</i>].
         **********************************************************************/
        public void Remainder(double y)
        {
            _s = GeoMath.remainder(_s, y);
            Add(0.0);                   // renormalize
        }
    }
}
