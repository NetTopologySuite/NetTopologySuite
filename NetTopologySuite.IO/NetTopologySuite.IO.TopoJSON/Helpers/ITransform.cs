namespace NetTopologySuite.IO.Helpers
{
    public interface ITransform
    {
        bool Quantized { get; }
        double[] Scale { get; }
        double[] Translate { get; }
    }
}