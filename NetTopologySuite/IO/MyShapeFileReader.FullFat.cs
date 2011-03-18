using System.IO;

namespace GisSharpBlog.NetTopologySuite.IO
{
    public partial class MyShapeFileReader
    {
        public Stream GetStream(string filePath)
        {
            return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}