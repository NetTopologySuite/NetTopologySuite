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
            //    throw new ArgumentNullException(nameof(path));
            //}

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path to shapefile can't be null, empty or whitespace");
            }

            ShapeStream = new FileStreamProvider(Path.ChangeExtension(path, ".shp"), validateShapePath);
            DataStream = new FileStreamProvider(Path.ChangeExtension(path, ".dbf"), validateDataPath);
            IndexStream = new FileStreamProvider(Path.ChangeExtension(path, ".shx"), validateIndexPath);
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
                throw new ArgumentNullException(nameof(shapeStream));
            if (validateDataProvider && dataStream == null)
                throw new ArgumentNullException(nameof(dataStream));
            if (validateIndexProvider && indexStream == null)
                throw new ArgumentNullException(nameof(indexStream));

            ShapeStream = shapeStream;
            DataStream = dataStream;
            IndexStream = indexStream;
        }

        private IStreamProvider DataStream { get; }

        private IStreamProvider ShapeStream { get; }

        private IStreamProvider IndexStream { get; }

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
                        throw new NotImplementedException();
                }
            }
        }
    }
}