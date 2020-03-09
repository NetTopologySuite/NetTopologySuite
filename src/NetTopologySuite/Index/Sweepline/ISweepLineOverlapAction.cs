namespace NetTopologySuite.Index.Sweepline
{
    /// <summary>
    ///
    /// </summary>
    public interface ISweepLineOverlapAction
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="s0"></param>
        /// <param name="s1"></param>
        void Overlap(SweepLineInterval s0, SweepLineInterval s1);
    }
}
