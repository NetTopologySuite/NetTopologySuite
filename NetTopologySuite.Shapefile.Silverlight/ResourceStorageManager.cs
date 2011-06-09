using System;
using System.IO;
using System.Windows;

namespace NetTopologySuite.Shapefile
{
    public class ResourceStorageManager : IStorageManager
    {
        public bool FileExists(string path)
        {
            return Application.GetResourceStream(new Uri(path, UriKind.Relative)) != null;
        }

        public Stream OpenRead(string path)
        {
            return Application.GetResourceStream(new Uri(path, UriKind.Relative)).Stream;
        }

        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (mode != FileMode.Open || access != FileAccess.Read)
                throw new InvalidOperationException("Resources can only be opened ReadOnly");
            return OpenRead(path);
        }

        public void FileDelete(string path)
        {
            throw new InvalidOperationException("Readonly storage");
        }

        public string ReadAllText(string path)
        {
            using (StreamReader reader = new StreamReader(OpenRead(path)))
                return reader.ReadToEnd();
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
                Disposed = true;
            if (disposing)
            {

            }
        }

        ~ResourceStorageManager()
        {
            Dispose(false);
        }

        protected bool Disposed { get; set; }
    }
}