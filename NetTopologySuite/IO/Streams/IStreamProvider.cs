using System.IO;

namespace NetTopologySuite.IO.Streams
{
    public interface IStreamProvider
    {
        bool UnderlyingStreamIsReadonly { get; }

        Stream OpenRead();
        Stream OpenWrite(bool truncate);

        string Kind { get; }
    }
}
