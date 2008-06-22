using System;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// This class represnts an ESRI Shapefile.
	/// </summary>
	public class ShapefileReader : IEnumerable
	{        
		/// <summary>
		/// Summary description for ShapefileEnumerator.
		/// </summary>
        private class ShapefileEnumerator : IEnumerator, IDisposable
        {
            private ShapefileReader _parent;
            private IGeometry _geometry;
            private ShapeHandler _handler;
            private BigEndianBinaryReader _shpBinaryReader = null;

            /// <summary>
            /// Initializes a new instance of the ShapefileEnumerator class.
            /// </summary>
            /// <param name="shapefile"></param>
            public ShapefileEnumerator(ShapefileReader shapefile)
            {                
                _parent = shapefile;

                // create a file stream for each enumerator that is given out. This allows the same file
                // to have one or more enumerator. If we used the parents stream - than only one IEnumerator 
                // could be given out.
                FileStream stream = new FileStream(_parent._filename, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
                _shpBinaryReader = new BigEndianBinaryReader(stream);

                // skip header - since parent has already read this.
                _shpBinaryReader.ReadBytes(100);
                ShapeGeometryTypes type = _parent._mainHeader.ShapeType;
                _handler = Shapefile.GetShapeHandler(type);
                if (_handler == null) 
                    throw new NotSupportedException("Unsuported shape type:" + type);
            }

            ~ShapefileEnumerator()
            {
                _shpBinaryReader.Close();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reset()
            {
                _shpBinaryReader.BaseStream.Seek(100, SeekOrigin.Begin);
                //throw new InvalidOperationException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_shpBinaryReader.PeekChar() != -1)
                {
                    // Mark Jacquin: add a try catch when some shapefile have extra char at the end but no record
                    try
                    {
                        int recordNumber = _shpBinaryReader.ReadInt32BE();
                        int contentLength = _shpBinaryReader.ReadInt32BE();
                        _geometry = _handler.Read(_shpBinaryReader, _parent._geometryFactory);
                    }
                    catch (Exception) { return false; }
                    return true;
                }
                else
                {
                    // Reached end of file, so close the reader.
                    //_shpBinaryReader.Close();
                    return false;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public object Current
            {
                get
                {
                    return _geometry;
                }
            }

            #region IDisposable Members

            public void Dispose()
            {
                _shpBinaryReader.Close();
            }

            #endregion
        }

		private ShapefileHeader _mainHeader = null;
		private IGeometryFactory _geometryFactory = null;
		private string _filename;
		
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
			FileStream stream = new FileStream(filename, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
			BigEndianBinaryReader shpBinaryReader = new BigEndianBinaryReader(stream);
			_mainHeader = new ShapefileHeader(shpBinaryReader);
			shpBinaryReader.Close();
		}

        /// <summary>
        /// Initializes a new instance of the Shapefile class with the given parameter 
        /// and a standard GeometryFactory.
        /// </summary>
        /// <param name="filename">The filename of the shape file to read (with .shp).</param>        
        public ShapefileReader(string filename) : 
            this(filename, new GeometryFactory()) { }        

		/// <summary>
		/// Gets the bounds of the shape file.
		/// </summary>
		public ShapefileHeader Header
		{
			get
			{
				return _mainHeader;
			}
		}	
		
		/// <summary>
		/// Reads the shapefile and returns a GeometryCollection representing all the records in the shapefile.
		/// </summary>
		/// <returns>GeometryCollection representing every record in the shapefile.</returns>
		public IGeometryCollection ReadAll()
		{
			ArrayList list = new ArrayList();
            ShapeGeometryTypes type = _mainHeader.ShapeType;
			ShapeHandler handler = Shapefile.GetShapeHandler(type);
			if (handler == null) 
				throw new NotSupportedException("Unsupported shape type:" + type);

			int i = 0;
			foreach (IGeometry geometry in this)
			{                
				list.Add(geometry);
				i++;
			}
			
	        IGeometry[] geomArray = GeometryFactory.ToGeometryArray(list);
			return _geometryFactory.CreateGeometryCollection(geomArray);
		}		
		
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
		{
			return new ShapefileEnumerator(this);
		}
	}
}
