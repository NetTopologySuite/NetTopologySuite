using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace NetTopologySuite.IO.ShapeFile.Extended.Streams
{
    public interface IStreamProvider
    {
        bool UnderlyingStreamIsReadonly { get; }

        Stream OpenRead();
        Stream OpenWrite(bool truncate);
    }
}
