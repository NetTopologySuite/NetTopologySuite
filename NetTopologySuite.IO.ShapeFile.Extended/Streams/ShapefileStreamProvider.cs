using System;
using System.IO;

namespace NetTopologySuite.IO.ShapeFile.Extended.Streams
{
    public class ShapefileStreamProvider : ICombinedStreamProvider
    {

        public ShapefileStreamProvider(string path, bool validateShapePath = false, bool validateDataPath = false)
        {
            //if (path == null)
            //{
            //    throw new ArgumentNullException(nameof(path));
            //}

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Path to shapefile can't be null, empty or whitespace");
            }

            ShapeStream = new FileStreamProvider(path, validateShapePath);
            DataStream = new FileStreamProvider(Path.ChangeExtension(path, ".dbf"), validateDataPath);
        }

        public ShapefileStreamProvider(IStreamProvider shapeStream, IStreamProvider dataStream, bool validateShapeProvider = false, bool validateDataProvider = false)
        {
            if (validateShapeProvider && shapeStream == null)
                throw new ArgumentNullException(nameof(shapeStream));
            if (validateDataProvider && dataStream == null)
                throw new ArgumentNullException(nameof(dataStream));

            ShapeStream = shapeStream;
            DataStream = dataStream;
        }

        public IStreamProvider DataStream { get; private set; }

        public IStreamProvider ShapeStream { get; private set; }

    }
}