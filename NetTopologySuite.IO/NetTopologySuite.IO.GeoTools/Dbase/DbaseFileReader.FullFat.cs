using NetTopologySuite.IO.Common.Streams;
using System;
using System.IO;

namespace NetTopologySuite.IO
{
    public partial class DbaseFileReader
    {
        private partial class DbaseFileEnumerator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbaseFileEnumerator"/> class.
            /// </summary>
            /// <param name="parent"></param>
            public DbaseFileEnumerator(DbaseFileReader parent)
            {
                _parent = parent;
                Stream stream = parent._dataStreamProvider.DataStream.OpenRead();
                _dbfStream = new BinaryReader(stream, parent._header.Encoding);
                ReadHeader();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _dbfStream.Close();
            }
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="filename"></param>
        public DbaseFileReader(string filename) : this(new ShapefileStreamProvider(null, new FileStreamProvider(filename, true), false, true))
        {


        }


        public DbaseFileReader(IDataStreamProvider dataStreamProvider)
        {
            if (dataStreamProvider == null)
            {
                throw new ArgumentNullException(nameof(dataStreamProvider));
            }
            _dataStreamProvider = dataStreamProvider;

        }


        /// <summary>
        /// Gets the header information for the dbase file.
        /// </summary>
        /// <returns>DbaseFileHeader contain header and field information.</returns>
        public DbaseFileHeader GetHeader()
        {
            if (_header == null)
            {
                using (var stream = _dataStreamProvider.DataStream.OpenRead())
                using (var dbfStream = new BinaryReader(stream))
                {
                    _header = new DbaseFileHeader();
                    // read the header
                    _header.ReadHeader(dbfStream, _dataStreamProvider.DataStream is FileStreamProvider ? ((FileStreamProvider)_dataStreamProvider.DataStream).Path : null);
                }
            }
            return _header;
        }
    }
}
