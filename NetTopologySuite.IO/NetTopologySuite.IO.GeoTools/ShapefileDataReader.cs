using System;
using System.Collections;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.IO.Streams;

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


        /// <summary>
        /// Initializes a new instance of the ShapefileDataReader class.
        /// </summary>
        /// <param name="filename">The shapefile to read (minus the .shp extension)</param>
        ///<param name="geometryFactory">The GeometryFactory to use.</param>
        public ShapefileDataReader(string filename, IGeometryFactory geometryFactory)
            :this(filename, geometryFactory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShapefileDataReader class.
        /// </summary>
        /// <param name="filename">The shapefile to read (minus the .shp extension)</param>
        /// <param name="geometryFactory">The GeometryFactory to use.</param>
        /// <param name="encoding">The encoding to use for reading the attribute data</param>
        public ShapefileDataReader(string filename, IGeometryFactory geometryFactory, Encoding encoding)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (geometryFactory == null)
                throw new ArgumentNullException("geometryFactory");

            _open = true;

            string dbfFile = Path.ChangeExtension(filename, "dbf");
            _dbfReader = encoding != null 
                ? new DbaseFileReader(dbfFile, encoding) 
                : new DbaseFileReader(dbfFile);

            string shpFile = Path.ChangeExtension(filename, "shp");
            _shpReader = new ShapefileReader(shpFile, geometryFactory);

            DbaseHeader = _dbfReader.GetHeader();
            RecordCount = DbaseHeader.NumRecords;

            // copy dbase fields to our own array. 
            //Insert into the first position, the shape column
            _dbaseFields = new DbaseFieldDescriptor[DbaseHeader.Fields.Length + 1];
            _dbaseFields[0] = DbaseFieldDescriptor.ShapeField();
            for (int i = 0; i < DbaseHeader.Fields.Length; i++)
                _dbaseFields[i + 1] = DbaseHeader.Fields[i];

            ShapeHeader = _shpReader.Header;
            _dbfEnumerator = _dbfReader.GetEnumerator();
            _shpEnumerator = _shpReader.GetEnumerator();
            _moreRecords = true;
        }

        public ShapefileDataReader(IStreamProviderRegistry streamProviderRegistry, IGeometryFactory geometryFactory)
        {
            if (streamProviderRegistry==null)
                throw new ArgumentNullException("streamProviderRegistry");
            if (geometryFactory == null)
                throw new ArgumentNullException("geometryFactory");
            _open = true;

            _dbfReader = new DbaseFileReader(streamProviderRegistry);
            _shpReader = new ShapefileReader(streamProviderRegistry, geometryFactory);

            DbaseHeader = _dbfReader.GetHeader();
            RecordCount = DbaseHeader.NumRecords;

            // copy dbase fields to our own array. Insert into the first position, the shape column
            _dbaseFields = new DbaseFieldDescriptor[DbaseHeader.Fields.Length + 1];
            _dbaseFields[0] = DbaseFieldDescriptor.ShapeField();
            for (int i = 0; i < DbaseHeader.Fields.Length; i++)
                _dbaseFields[i + 1] = DbaseHeader.Fields[i];

            ShapeHeader = _shpReader.Header;
            _dbfEnumerator = _dbfReader.GetEnumerator();
            _shpEnumerator = _shpReader.GetEnumerator();
            _moreRecords = true;
        }

        bool _moreRecords = false;

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
        public bool IsClosed => !_open;

        /// <summary>
        /// Closes the IDataReader 0bject.
        /// </summary>
        public void Close()
        {
            _open = false;
        }
    }
}
