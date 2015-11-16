using System;
using System.IO;

namespace NetTopologySuite.IO.Streams
{
    /// <summary>
    /// A stream provider registry for an ESRI Shapefile dataset
    /// </summary>
    public class ShapefileStreamProviderRegistry : IStreamProviderRegistry
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="path">The path to the shapefile</param>
        /// <param name="validateShapePath">A value indicating that the <paramref name="path"/> must be validated</param>
        /// <param name="validateDataPath">A value indicating that the data file modified <paramref name="path"/> must be validated</param>
        /// <param name="validateIndexPath">A value indicating that the shape index modified <paramref name="path"/> must be validated</param>
        public ShapefileStreamProviderRegistry(string path, bool validateShapePath = false, bool validateDataPath = false, bool validateIndexPath = false)
        {
#if NET40
            if (string.IsNullOrWhiteSpace(path))
#else
            if (string.IsNullOrEmpty(path))
#endif
            {
                throw new ArgumentNullException("path", "Path to shapefile can't be null, empty or whitespace");
            }

            ShapeStream = new FileStreamProvider(StreamTypes.Shape, Path.ChangeExtension(path, ".shp"), validateShapePath);
            DataStream = new FileStreamProvider(StreamTypes.Data, Path.ChangeExtension(path, ".dbf"), validateDataPath);
            IndexStream = new FileStreamProvider(StreamTypes.Index, Path.ChangeExtension(path, ".shx"), validateIndexPath);
        }

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
        public ShapefileStreamProviderRegistry(IStreamProvider shapeStream, IStreamProvider dataStream,
            IStreamProvider indexStream,
            bool validateShapeProvider = false, bool validateDataProvider = false, bool validateIndexProvider = false)
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
        }

        private IStreamProvider DataStream { get; set; }

        private IStreamProvider ShapeStream { get; set; }

        private IStreamProvider IndexStream { get; set; }

        public IStreamProvider this[string streamType]
        {
            get
            {
                switch (streamType)
                {
                    case StreamTypes.Data:
                        return DataStream;
                    case StreamTypes.Index:
                        return IndexStream;
                    case StreamTypes.Shape:
                        return ShapeStream;
                    default:
                        throw new ArgumentException(
                            string.Format("Unknown stream type: '{0}'", streamType),
                            "streamType");
                }
            }
        }
    }
}