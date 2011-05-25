using System;
using System.IO;
using System.IO.IsolatedStorage;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
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

        private Stream GetStream(string filePath)
        {
            return IsolatedStorageFile.OpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public IGeometryCollection ReadRawStream(Stream stream)
        {
            return Read(stream);
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
            {
                if (_file != null)
                    _file.Dispose();
            }

        }
        ~MyShapeFileReader()
        {
            Dispose(false);
        }
    }
}