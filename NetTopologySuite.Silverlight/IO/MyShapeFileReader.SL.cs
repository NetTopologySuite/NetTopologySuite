using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace GisSharpBlog.NetTopologySuite.IO
{
    public partial class MyShapeFileReader : IDisposable
    {
        private IsolatedStorageFile _file;

        protected IsolatedStorageFile IsolatedStorageFile
        {
            get
            {
                return _file ?? (_file = IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        public Stream GetStream(string filePath)
        {
            return IsolatedStorageFile.OpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        private bool IsDisposed
        {
            get;
            set;
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (disposing)
                _file.Dispose();

        }
        ~MyShapeFileReader()
        {
            Dispose(false);
        }
    }
}