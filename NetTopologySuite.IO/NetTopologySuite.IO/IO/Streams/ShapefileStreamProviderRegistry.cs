using System;
using System.IO;

namespace NetTopologySuite.IO.Streams
{
    public class ShapefileStreamProviderRegistry : IStreamProviderRegistry
    {
        public ShapefileStreamProviderRegistry(string path, bool validateShapePath = false, bool validateDataPath = false, bool validateIndexPath = false)
        {
            //if (path == null)
            //{
            //    throw new ArgumentNullException("path");
            //}

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path", "Path to shapefile can't be null, empty or whitespace");
            }

            ShapeStream = new FileStreamProvider(StreamTypes.Shape, Path.ChangeExtension(path, ".shp"), validateShapePath);
            DataStream = new FileStreamProvider(StreamTypes.Data, Path.ChangeExtension(path, ".dbf"), validateDataPath);
            IndexStream = new FileStreamProvider(StreamTypes.Index, Path.ChangeExtension(path, ".shx"), validateIndexPath);
        }

        public ShapefileStreamProviderRegistry(IStreamProvider shapeStream, IStreamProvider dataStream,
            bool validateShapeProvider = false, bool validateDataProvider = false, bool validateIndexPath = false) :
                this(shapeStream, dataStream, null, validateShapeProvider, validateDataProvider, false)
        {
        }

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