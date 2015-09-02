using System;
using System.IO;

namespace NetTopologySuite.IO.Common.Streams
{
    public class ShapefileStreamProvider : ICombinedStreamProvider
    {
        public ShapefileStreamProvider(string path, bool validateShapePath = false, bool validateDataPath = false, bool validateIndexPath = false)
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

        public ShapefileStreamProvider(IStreamProvider shapeStream, IStreamProvider dataStream,
            bool validateShapeProvider = false, bool validateDataProvider = false, bool validateIndexPath = false) :
                this(shapeStream, dataStream, null, validateShapeProvider, validateDataProvider, false)
        {
        }

        public ShapefileStreamProvider(IStreamProvider shapeStream, IStreamProvider dataStream,
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

        public IStreamProvider DataStream { get; }

        public IStreamProvider ShapeStream { get; }

        public IStreamProvider IndexStream { get; }
    }
}