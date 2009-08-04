namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    // TODO: replace this with a generator rather than a visitor
    public interface ISweepLineOverlapAction
    {
        void Overlap(SweepLineInterval s0, SweepLineInterval s1);
    }
}