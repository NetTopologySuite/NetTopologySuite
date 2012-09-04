using System;
using System.IO;
using System.IO.IsolatedStorage;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    public partial class DbaseFileReader
    {
        private partial class DbaseFileEnumerator
        {
            private IsolatedStorageFile _isolatedStorageFile;

            /// <summary>
            /// Initializes a new instance of the <see cref="DbaseFileEnumerator"/> class.
            /// </summary>
            /// <param name="parent"></param>
            public DbaseFileEnumerator(DbaseFileReader parent)
            {
                _isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();

                _parent = parent;
                IsolatedStorageFileStream stream = new IsolatedStorageFileStream(parent._filename, FileMode.Open, FileAccess.Read, FileShare.Read, _isolatedStorageFile);
                _dbfStream = new BinaryReader(stream, PlatformUtilityEx.GetDefaultEncoding());
                ReadHeader();
            }


            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _dbfStream.Close();
                _isolatedStorageFile.Dispose();
            }

            #endregion
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="filename"></param>
        public DbaseFileReader(string filename)
        {
            Guard.IsNotNull(filename, "filename");
            // check for the file existing here, otherwise we will not get an error
            //until we read the first record or read the header.);
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.FileExists(filename))
                {
                    throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
                }
            }
            _filename = filename;

        }

        /// <summary>
        /// Gets the header information for the dbase file.
        /// </summary>
        /// <returns>DbaseFileHeader contain header and field information.</returns>
        public DbaseFileHeader GetHeader()
        {
            if (_header == null)
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(_filename, FileMode.Open,
                                                                                         FileAccess.Read, isf))
                    {
                        using (BinaryReader dbfStream = new BinaryReader(stream))
                        {

                            _header = new DbaseFileHeader();
                            // read the header
                            _header.ReadHeader(dbfStream, string.Empty);

                            dbfStream.Close();
                            stream.Close();
                        }
                    }
                }
            }
            return _header;
        }

    }
}
