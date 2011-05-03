using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    public class IsolatedStorageManager : IStorageManager
    {
        private readonly IsolatedStorageFile _isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();

        public bool FileExists(string path)
        {
            return _isolatedStorageFile.FileExists(path);
        }

        public Stream OpenRead(string path)
        {
            return Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (disposing)
                _isolatedStorageFile.Dispose();
        }

        ~IsolatedStorageManager()
        {
            Dispose(false);
        }


        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return _isolatedStorageFile.OpenFile(path, mode, access, share);
        }


        public void FileDelete(string path)
        {
            _isolatedStorageFile.DeleteFile(path);
        }


        public string ReadAllText(string path)
        {
            using (var srt = new StreamReader(_isolatedStorageFile.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                return srt.ReadToEnd();
            }
        }
    }
}