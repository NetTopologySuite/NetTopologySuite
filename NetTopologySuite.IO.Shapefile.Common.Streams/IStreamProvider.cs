using System.IO;

namespace NetTopologySuite.IO.Common.Streams
{
    public interface IStreamProvider
    {
        bool UnderlyingStreamIsReadonly { get; }

        Stream OpenRead();
        Stream OpenWrite(bool truncate);
    }
}
