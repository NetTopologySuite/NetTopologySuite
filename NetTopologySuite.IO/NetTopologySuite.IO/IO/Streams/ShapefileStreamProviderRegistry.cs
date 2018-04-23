using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NetTopologySuite.IO.Streams
{
    /// <summary>
    /// An enumeration of stream types
    /// </summary>
    public static class StreamTypes
    {
        /// <summary>
        /// A shape stream (*.shp)
        /// </summary>
        public const string Shape = "SHAPESTREAM";
        /// <summary>
        /// An index stream (*.shx)
        /// </summary>
        public const string Index = "INDEXSTREAM";
        /// <summary>
        /// A projection string (*.prj)
        /// </summary>
        public const string Projection = "PROJECTIONSTREAM";

        /// <summary>
        /// A data stream (*.dbf)
        /// </summary>
        public const string Data = "DATASTREAM";
        /// <summary>
        /// A data encoding stream (*.cpg)
        /// </summary>
        public const string DataEncoding = "DATAENCODINGSTREAM";

        /// <summary>
        /// A spatial index stream (*.sbn)
        /// </summary>
        public const string SpatialIndex = "SPATIALINDEXSTREAM";
        /// <summary>
        /// A spatial index index stream (*.sbx)
        /// </summary>
        public const string SpatialIndexIndex = "SPATIALINDEXINDEXSTREAM";
    }

    /// <summary>
    /// A stream provider registry for an ESRI Shapefile dataset
    /// </summary>
    public class ShapefileStreamProviderRegistry : IStreamProviderRegistry //, IDisposable
    {
        private IStreamProvider _dataEncodingStream;

#if FEATURE_FILE_IO
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="path">The path to the shapefile</param>
        /// <param name="validateShapePath">A value indicating that the <paramref name="path"/> must be validated</param>
        /// <param name="validateDataPath">A value indicating that the data file modified <paramref name="path"/> must be validated</param>
        /// <param name="validateIndexPath">A value indicating that the shape index modified <paramref name="path"/> must be validated</param>
        public ShapefileStreamProviderRegistry(string path, bool validateShapePath = false, bool validateDataPath = false, bool validateIndexPath = false)
        {
#if HAS_SYSTEM_STRING_ISNULLORWHITESPACE
            if (string.IsNullOrWhiteSpace(path))
#else
            if (string.IsNullOrEmpty(path) || path.All(Char.IsWhiteSpace))
#endif
            {
                throw new ArgumentNullException("path", "Path to shapefile can't be null, empty or whitespace");
            }

            ShapeStream = new FileStreamProvider(StreamTypes.Shape, Path.ChangeExtension(path, ".shp"), validateShapePath);
            /*
            IndexStream = File.Exists(Path.ChangeExtension(path, "shx")) 
                ? (IStreamProvider)new FileStreamProvider(StreamTypes.Index, Path.ChangeExtension(path, ".shx"), validateIndexPath)
                : new NullStreamProvider(StreamTypes.Index);
            */
            IndexStream = new FileStreamProvider(StreamTypes.Index, Path.ChangeExtension(path,".shx"), validateIndexPath);
            DataStream = new FileStreamProvider(StreamTypes.Data, Path.ChangeExtension(path, ".dbf"), validateDataPath);

            var tmpPath = Path.ChangeExtension(path, "prj");
            if (File.Exists(tmpPath))
                ProjectionStream = new FileStreamProvider(StreamTypes.Projection, tmpPath);
            tmpPath = Path.ChangeExtension(path, "cpg");
            if (File.Exists(tmpPath))
                DataEncodingStream = new FileStreamProvider(StreamTypes.DataEncoding, tmpPath);
            tmpPath = Path.ChangeExtension(path, "sbn");
            if (File.Exists(tmpPath))
                SpatialIndexStream = new FileStreamProvider(StreamTypes.SpatialIndex, tmpPath);
            tmpPath = Path.ChangeExtension(path, "sbx");
            if (File.Exists(tmpPath))
                SpatialIndexIndexStream = new FileStreamProvider(StreamTypes.SpatialIndexIndex, tmpPath);
        }
#endif

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="shapeStream">A stream provider for the shape stream</param>
        /// <param name="dataStream">A stream provider for the data stream</param>
        /// <param name="validateShapeProvider">A value indicating that the <paramref name="shapeStream"/> must be validated</param>
        /// <param name="validateDataProvider">A value indicating that the <paramref name="dataStream"/> must be validated</param>
        public ShapefileStreamProviderRegistry(IStreamProvider shapeStream, IStreamProvider dataStream,
            bool validateShapeProvider = false, bool validateDataProvider = false) :
                this(shapeStream, dataStream, null, validateShapeProvider, validateDataProvider)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="shapeStream">A stream provider for the shape stream</param>
        /// <param name="dataStream">A stream provider for the data stream</param>
        /// <param name="indexStream">A stream provider for the shape index stream</param>
        /// <param name="validateShapeProvider">A value indicating that the <paramref name="shapeStream"/> must be validated</param>
        /// <param name="validateDataProvider">A value indicating that the <paramref name="dataStream"/> must be validated</param>
        /// <param name="validateIndexProvider">A value indicating that the <paramref name="indexStream"/> must be validated</param>
        /// <param name="dataEncodingStream"></param>
        /// <param name="projectionStream"></param>
        /// <param name="spatialIndexStream"></param>
        /// <param name="spatialIndexIndexStream"></param>
        public ShapefileStreamProviderRegistry(IStreamProvider shapeStream, IStreamProvider dataStream,
            IStreamProvider indexStream,
            bool validateShapeProvider = false, bool validateDataProvider = false, bool validateIndexProvider = false,
            IStreamProvider dataEncodingStream = null, IStreamProvider projectionStream = null,
            IStreamProvider spatialIndexStream = null, IStreamProvider spatialIndexIndexStream = null)
        {
            if (validateShapeProvider && shapeStream == null)
                throw new ArgumentNullException("shapeStream");
            if (validateDataProvider && dataStream == null)
                throw new ArgumentNullException("dataStream");
            if (validateIndexProvider && indexStream == null)
                throw new ArgumentNullException("indexStream");

            ShapeStream = shapeStream;
            DataStream = dataStream;
            IndexStream = indexStream;
            DataEncodingStream = dataEncodingStream;
            ProjectionStream = projectionStream;
            SpatialIndexStream = spatialIndexStream;
            SpatialIndexIndexStream = spatialIndexIndexStream;
        }

        private IStreamProvider SpatialIndexIndexStream { get; set; }

        private IStreamProvider SpatialIndexStream { get; set; }

        private IStreamProvider ProjectionStream { get; set; }

        private IStreamProvider DataStream { get; set; }

        private IStreamProvider DataEncodingStream
        {
            get => _dataEncodingStream ?? 
                   new ByteStreamProvider(StreamTypes.DataEncoding, "windows-1252", Encoding.UTF8);
            set => _dataEncodingStream = value;
        }

        private IStreamProvider ShapeStream { get; set; }

        private IStreamProvider IndexStream { get; set; }

        /// <summary>
        /// Indexer for a stream provider
        /// </summary>
        /// <param name="streamType">The stream type</param>
        /// <returns>A stream provider</returns>
        public IStreamProvider this[string streamType]
        {
            get
            {
                switch (streamType)
                {
                    case StreamTypes.Data:
                        return DataStream;
                    case StreamTypes.DataEncoding:
                        return DataEncodingStream;

                    case StreamTypes.Shape:
                        return ShapeStream;
                    case StreamTypes.Index:
                        return IndexStream;
                    case StreamTypes.Projection:
                        return ProjectionStream;

                    case StreamTypes.SpatialIndex:
                        return SpatialIndexStream;
                    case StreamTypes.SpatialIndexIndex:
                        return SpatialIndexIndexStream;

                    default:
                        return null;
                        /*
                        throw new ArgumentException(
                            string.Format("Unknown stream type: '{0}'", streamType),
                            "streamType");
                         */
                }
            }
        }

        internal class NullStreamProvider : IStreamProvider
        {
            public NullStreamProvider(string kind)
            {
                Kind = kind;
            }

            public bool UnderlyingStreamIsReadonly => false;

            public Stream OpenRead()
            {
                return null;
            }

            public Stream OpenWrite(bool truncate)
            {
                return null;
            }

            public string Kind { get; private set; }
        }

        /*
        #region IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;

                TryDispose(DataStream);
                TryDispose(DataEncodingStream);
                TryDispose(ShapeStream);
                TryDispose(IndexStream);
                TryDispose(ProjectionStream);

                TryDispose(SpatialIndexStream);
                TryDispose(SpatialIndexIndexStream);
            }
        }

        protected static void TryDispose(IStreamProvider provider)
        {
            if (provider is IDisposable)
                ((IDisposable)provider).Dispose();
        }
        #endregion
        */
    }
}