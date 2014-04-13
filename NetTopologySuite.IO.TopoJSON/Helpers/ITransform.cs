namespace NetTopologySuite.IO.TopoJSON.Helpers
{
    public interface ITransform
    {
        bool Quantized { get; }
        double[] Scale { get; }
        double[] Translate { get; }
    }
}