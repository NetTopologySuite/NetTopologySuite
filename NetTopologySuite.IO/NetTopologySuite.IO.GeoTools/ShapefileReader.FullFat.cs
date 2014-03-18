using System;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    public partial class ShapefileReader
    {
        /// <summary>
        /// Initializes a new instance of the Shapefile class with the given parameters.
        /// </summary>
        /// <param name="filename">The filename of the shape file to read (with .shp).</param>
        /// <param name="geometryFactory">The GeometryFactory to use when creating Geometry objects.</param>
        public ShapefileReader(string filename, IGeometryFactory geometryFactory)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (geometryFactory == null)
                throw new ArgumentNullException("geometryFactory");

            _filename = filename;
            _geometryFactory = geometryFactory;

            // read header information. note, we open the file, read the header information and then
            // close the file. This means the file is not opened again until GetEnumerator() is requested.
            // For each call to GetEnumerator() a new BinaryReader is created.
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var shpBinaryReader = new BigEndianBinaryReader(stream))
                {
                    _mainHeader = new ShapefileHeader(shpBinaryReader);
                }
            }
        }

        #region Nested type: ShapefileEnumerator

        /// <summary>
        /// Summary description for ShapefileEnumerator.
        /// </summary>
        private partial class ShapefileEnumerator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ShapefileEnumerator"/> class.
            /// </summary>
            /// <param name="shapefile"></param>
            public ShapefileEnumerator(ShapefileReader shapefile)
            {
                _parent = shapefile;

                // create a file stream for each enumerator that is given out. This allows the same file
                // to have one or more enumerator. If we used the parents stream - than only one IEnumerator 
                // could be given out.
                var stream = new FileStream(_parent._filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                _shpBinaryReader = new BigEndianBinaryReader(stream);

                // skip header - since parent has already read this.
                _shpBinaryReader.ReadBytes(100);
                ShapeGeometryType type = _parent._mainHeader.ShapeType;
                _handler = Shapefile.GetShapeHandler(type);
                if (_handler == null)
                    throw new NotSupportedException("Unsuported shape type:" + type);
            }


            /// <summary>
            /// Performs application-defined tasks associated with freeing, 
            /// releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _shpBinaryReader.Close();
            }
        }

        #endregion
    }
}