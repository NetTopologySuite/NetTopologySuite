namespace NetTopologySuite.Index.Sweepline
{
    /// <summary>
    ///
    /// </summary>
    public class SweepLineInterval
    {
        private readonly double min;
        private readonly double max;
        private readonly object item;

        /// <summary>
        ///
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public SweepLineInterval(double min, double max) : this(min, max, null) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="item"></param>
        public SweepLineInterval(double min, double max, object item)
        {
            this.min = min < max ? min : max;
            this.max = max > min ? max : min;
            this.item = item;
        }

        /// <summary>
        ///
        /// </summary>
        public double Min => min;

        /// <summary>
        ///
        /// </summary>
        public double Max => max;

        /// <summary>
        ///
        /// </summary>
        public object Item => item;
    }
}
