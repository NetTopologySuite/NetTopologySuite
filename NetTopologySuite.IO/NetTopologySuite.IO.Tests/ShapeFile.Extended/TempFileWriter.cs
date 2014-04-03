using System;
using System.IO;

namespace NetTopologySuite.IO.Tests.ShapeFile.Extended
{
    public class TempFileWriter : IDisposable
    {
        public TempFileWriter(string fileName, byte[] data)
        {
            Path = System.IO.Path.GetFullPath(fileName);
            File.WriteAllBytes(Path, data);
        }

        public TempFileWriter(string fileName, string data)
        {
            Path = System.IO.Path.GetFullPath(fileName);
            File.WriteAllText(Path, data);
        }

        ~TempFileWriter()
        {
            InternalDispose();
        }

        public void Dispose()
        {
            InternalDispose();
            GC.SuppressFinalize(this);
        }

        private void InternalDispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch { }
        }

        public string Path { get; private set; }
    }
}
