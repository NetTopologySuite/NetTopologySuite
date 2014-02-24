using System;
using System.Collections;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Creates a IDataReader that can be used to enumerate through an ESRI shape file.
    /// </summary>
    /// <remarks>	
    /// To create a ShapefileDataReader, use the static methods on the Shapefile class.
    /// </remarks>
    public partial class ShapefileDataReader : IDisposable
    {
        bool _open = false;
        readonly DbaseFieldDescriptor[] _dbaseFields;
        readonly DbaseFileReader _dbfReader;
        readonly ShapefileReader _shpReader;
        readonly IEnumerator _dbfEnumerator;
        readonly IEnumerator _shpEnumerator;
        readonly ShapefileHeader _shpHeader;
        readonly DbaseFileHeader _dbfHeader;
        readonly int _recordCount = 0;


        /// <summary>
        /// Initializes a new instance of the ShapefileDataReader class.
        /// </summary>
        /// <param name="filename">The shapefile to read (minus the .shp extension)</param>
        ///<param name="geometryFactory">The GeometryFactory to use.</param>
        public ShapefileDataReader(string filename, IGeometryFactory geometryFactory)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (geometryFactory == null)
                throw new ArgumentNullException("geometryFactory");
            _open = true;

            string dbfFile = Path.ChangeExtension(filename, "dbf");
            _dbfReader = new DbaseFileReader(dbfFile);
            string shpFile = Path.ChangeExtension(filename, "shp");
            _shpReader = new ShapefileReader(shpFile, geometryFactory);

            _dbfHeader = _dbfReader.GetHeader();
            _recordCount = _dbfHeader.NumRecords;

            // copy dbase fields to our own array. Insert into the first position, the shape column
            _dbaseFields = new DbaseFieldDescriptor[_dbfHeader.Fields.Length + 1];
            _dbaseFields[0] = DbaseFieldDescriptor.ShapeField();
            for (int i = 0; i < _dbfHeader.Fields.Length; i++)
                _dbaseFields[i + 1] = _dbfHeader.Fields[i];

            _shpHeader = _shpReader.Header;
            _dbfEnumerator = _dbfReader.GetEnumerator();
            _shpEnumerator = _shpReader.GetEnumerator();
            _moreRecords = true;
        }
        bool _moreRecords = false;

        IGeometry geometry = null;

        public void Reset()
        {
            _dbfEnumerator.Reset();
            _shpEnumerator.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!IsClosed)
                Close();
            ((IDisposable)_shpEnumerator).Dispose();
            ((IDisposable)_dbfEnumerator).Dispose();
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <value>true if the data reader is closed; otherwise, false.</value>
        /// <remarks>IsClosed and RecordsAffected are the only properties that you can call after the IDataReader is closed.</remarks>
        public bool IsClosed
        {
            get
            {
                return !_open;
            }
        }

        /// <summary>
        /// Closes the IDataReader 0bject.
        /// </summary>
        public void Close()
        {
            _open = false;
        }
    }
}
