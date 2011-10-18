using System;
using System.IO;
using System.IO.IsolatedStorage;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

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
            Guard.IsNotNull(filename, "filename");
            Guard.IsNotNull(geometryFactory, "geometryFactory");

            _filename = filename;
            _geometryFactory = geometryFactory;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // read header information. note, we open the file, read the header information and then
                // close the file. This means the file is not opened again until GetEnumerator() is requested.
                // For each call to GetEnumerator() a new BinaryReader is created.
                using (
                    var stream = new IsolatedStorageFileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read,
                                                               isf))
                {
                    using (var shpBinaryReader = new BigEndianBinaryReader(stream))
                    {
                        _mainHeader = new ShapefileHeader(shpBinaryReader);
                        shpBinaryReader.Close();
                    }
                }
            }
        }

        #region Nested type: ShapefileEnumerator

        /// <summary>
        /// Summary description for ShapefileEnumerator.
        /// </summary>
        private partial class ShapefileEnumerator
        {
            private IsolatedStorageFile _isolatedStorageFile;
            /// <summary>
            /// Initializes a new instance of the <see cref="ShapefileEnumerator"/> class.
            /// </summary>
            /// <param name="shapefile"></param>
            public ShapefileEnumerator(ShapefileReader shapefile)
            {
                _parent = shapefile;

                _isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();

                // create a file stream for each enumerator that is given out. This allows the same file
                // to have one or more enumerator. If we used the parents stream - than only one IEnumerator 
                // could be given out.
                var stream = new IsolatedStorageFileStream(_parent._filename,
                                                           FileMode.Open, FileAccess.Read,
                                                           FileShare.Read, _isolatedStorageFile);


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
                _isolatedStorageFile.Dispose();
            }
        }

        #endregion
    }
}