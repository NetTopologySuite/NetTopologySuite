using System.Text;

namespace NetTopologySuite.Encodings
{
    public interface IEncodingRegistry
    {
        Encoding ASCII { get; }
        Encoding UTF8 { get; }
        Encoding Unicode { get; }
        Encoding GetEncoding(int codePage);
        int GetCodePage(Encoding encoding);
    }
}