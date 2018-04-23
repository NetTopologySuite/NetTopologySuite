namespace NetTopologySuite.Index.Sweepline
{
    /// <summary>
    /// 
    /// </summary>
    public class SweepLineInterval
    {
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
            this.Min = min < max ? min : max;
            this.Max = max > min ? max : min;
            this.Item = item;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// 
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// 
        /// </summary>
        public object Item { get; }
    }
}
