#if !PCL
using System;
using System.IO;
using System.Text;
using NetTopologySuite.IO.Streams;

namespace NetTopologySuite.IO
{
    public partial class DbaseFileReader
    {
        private partial class DbaseFileEnumerator
        {

        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="path">The path to the Dbase file</param>
        public DbaseFileReader(string path) 
            : this(CreateStreamProviderRegistry(path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="path">The path to the Dbase file</param>
        /// <param name="encoding">The encoding to use</param>
        public DbaseFileReader(string path, Encoding encoding)
            : this(CreateStreamProviderRegistry(path, encoding))
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="streamProvider">A stream provider</param>
        [Obsolete("Do not use!")]
        public DbaseFileReader(IStreamProvider streamProvider)
        {
            if (streamProvider == null)
                throw new ArgumentNullException("streamProvider");

            if (streamProvider.Kind != StreamTypes.Data)
                throw new ArgumentException("Not a data stream provider", "streamProvider");

            _streamProvider = streamProvider;
        }

#region Stream provider regsitry utility functions
        static IStreamProviderRegistry CreateStreamProviderRegistry(string dbfPath)
        {
            var dbfStreamProvider = new FileStreamProvider(StreamTypes.Data, dbfPath, true);
            var cpgPath = Path.ChangeExtension(dbfPath, "cpg");
            IStreamProvider cpgStreamProvider = null;
            if (File.Exists(cpgPath))
                cpgStreamProvider = new FileStreamProvider(StreamTypes.DataEncoding, cpgPath, true);

            return new ShapefileStreamProviderRegistry(null, dbfStreamProvider, null, dataEncodingStream: cpgStreamProvider);
        }

        static IStreamProviderRegistry CreateStreamProviderRegistry(string dbfPath, Encoding encoding)
        {
            var dbfStreamProvider = new FileStreamProvider(StreamTypes.Data, dbfPath, true);
            var cpgStreamProvider = new ByteStreamProvider(StreamTypes.DataEncoding, encoding.EncodingName);

            return new ShapefileStreamProviderRegistry(null, dbfStreamProvider, null, dataEncodingStream: cpgStreamProvider);
        }
#endregion
    }
}
#endif