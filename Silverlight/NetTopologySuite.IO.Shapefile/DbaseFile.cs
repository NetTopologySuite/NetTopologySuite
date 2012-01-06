// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// Note:
// Good stuff on DBase format: http://www.clicketyclick.dk/databases/xbase/format/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using GeoAPI.Geometries;
using NetTopologySuite.Data;
using NetTopologySuite.Shapefile;

namespace NetTopologySuite.Shapefile
{
    /// <summary>
    /// Represents a dBase file used to store attribute data in a Shapefile.
    /// </summary>
    internal partial class DbaseFile : IDisposable
    {
        #region Instance fields

        private readonly String _filename;
        private DbaseHeader _header;
        //private FeatureDataTable<UInt32> _baseTable;
        private Boolean _headerIsParsed;
        private DbaseReader _reader;
        private DbaseWriter _writer;
        private Stream _dbaseFileStream;
        private Boolean _isDisposed;
        private Boolean _isOpen;
        private readonly IGeometryFactory _geoFactory;
        private readonly Object _headerParseSync = new Object();
        private ISchema _baseTable;

        #endregion

        // TODO: [codekaizen] instead of passing an IGeometryFactory to 
        // this DbaseFile class, it might be more sensible to create a 
        // FeatureDataTable factory. This is especially true if we extract 
        // interfaces from FeatureDataTable in order to allow some other 
        // implementation to be used.
        internal DbaseFile(String fileName, IGeometryFactory geoFactory, IStorageManager storageManager, ISchemaFactory schemaFactory)
            : this(fileName, geoFactory, true, storageManager, schemaFactory)
        { }

        private DbaseFile(String fileName, IGeometryFactory geoFactory, Boolean checkExists, IStorageManager storageManager, ISchemaFactory schemaFactory)
        {
            _geoFactory = geoFactory;
            SchemaFactory = schemaFactory;
            StorageManager = storageManager;

            if (checkExists && !StorageManager.FileExists(fileName))
            {
                throw new ArgumentException("Cannot find file", "fileName");
            }

            _headerIsParsed = false;
            _filename = fileName;
        }

        public ISchemaFactory SchemaFactory
        {
            get;
            protected set;
        }

        #region Dispose Pattern

        ~DbaseFile()
        {
            dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        #endregion

        /// <summary>
        /// Gets a value which indicates if this Object is disposed: 
        /// <see langword="true"/> if it is, <see langword="false"/> otherwise
        /// </summary>
        /// <seealso cref="Dispose"/>
        internal Boolean IsDisposed
        {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }

        private void dispose(Boolean disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing) // Do deterministic finalization of managed resources
            {
                //if (_baseTable != null) _baseTable.Dispose();
                //_baseTable = null;

                if (_reader != null) _reader.Dispose();
                _reader = null;

                if (_writer != null) _writer.Dispose();
                _writer = null;
            }

            _isOpen = false;
        }
        #endregion

        #region Properties

        #region Columns Property
        public ICollection<DbaseField> Columns
        {
            get { return Header == null ? null : Header.Columns; }
            //set
            //{
            //    if (_header != null)
            //    {
            //        throw new InvalidOperationException(
            //            "Can't set columns after schema has been defined.");
            //    }

            //    _header.Columns = value;
            //}
        }
        #endregion

        internal CultureInfo CultureInfo
        {
            get
            {
                checkState();
                return DbaseLocaleRegistry.GetCulture(Header.LanguageDriver);
            }
        }

        private Stream DataStream
        {
            get { return _dbaseFileStream; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Text.Encoding"/> used for parsing strings 
        /// from the DBase DBF file.
        /// </summary>
        /// <remarks>
        /// If the encoding type isn't set, the dbase driver will try to determine 
        /// the correct <see cref="System.Text.Encoding"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the property is 
        /// fetched and/or set and Object has been disposed.
        /// </exception>
        internal Encoding Encoding
        {
            get
            {
                checkState();
                return DbaseLocaleRegistry.GetEncoding(Header.LanguageDriver);
            }
        }

        /// <summary>
        /// Gets a value which indicates if the reader is open: 
        /// true if it is, false otherwise.
        /// </summary>
        internal Boolean IsOpen
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// Number of records in the DBF file.
        /// </summary>
        internal UInt32 RecordCount
        {
            get
            {
                checkState();
                return Header.RecordCount;
            }
        }
        #endregion

        #region Methods

        internal static DbaseFile CreateDbaseFile(String fileName, ISchema schema, CultureInfo culture, Encoding encoding, IGeometryFactory geoFactory, IStorageManager storageManager)
        {
            DbaseFile file = new DbaseFile(fileName, geoFactory, false, storageManager, schema.SchemaFactory);
            Byte languageDriverCode = DbaseLocaleRegistry.GetLanguageDriverCode(culture, encoding);
            file._header = new DbaseHeader(languageDriverCode, DateTime.Now, 0);

            file._header.Columns = new List<DbaseField>(DbaseSchema.GetFields(schema, file._header));
            file._headerIsParsed = true;
            file.Open();
            file.Save();
            file.Close();

            return file;
        }

        internal void Save()
        {
            _writer.BeginWrite();
            _writer.WriteFullHeader(Header);
            _writer.EndWrite();
        }

        //internal void AddRow(DataRow row)
        //{
        //    if (row == null) throw new ArgumentNullException("row");

        //    DataStream.Seek((Int32)ComputeByteOffsetToRecord(Header.RecordCount),
        //                    SeekOrigin.Begin);

        //    _writer.BeginWrite();
        //    _writer.WriteRow(row);
        //    Header.LastUpdate = DateTime.Now;
        //    Header.RecordCount++;
        //    _writer.UpdateHeader(Header);
        //    _writer.EndWrite();
        //}

        //internal void AddRows(DataTable table)
        //{
        //    if (table == null) throw new ArgumentNullException("table");

        //    _writer.BeginWrite();

        //    foreach (DataRow row in table.Rows)
        //    {
        //        _writer.WriteRow(row);
        //        Header.RecordCount++;
        //    }

        //    Header.LastUpdate = DateTime.Now;
        //    _writer.UpdateHeader(Header);
        //    _writer.EndWrite();
        //}

        /// <summary>
        /// Closes the xBase file.
        /// </summary>
        /// <seealso cref="IsOpen"/>
        /// <seealso cref="Open()"/>
        /// <seealso cref="Dispose" />
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the method is called and
        /// Object has been disposed.
        /// </exception>
        public void Close()
        {
            (this as IDisposable).Dispose();
        }

        internal Int64 ComputeByteOffsetToRecord(UInt32 row)
        {
            return Header.HeaderLength + ((row) * Header.RecordLength);
        }

        //internal void DeleteRow(UInt32 rowIndex)
        //{
        //    // TODO: implement DbaseFile.DeleteRow
        //    throw new NotImplementedException("Not implemented in this version.");
        //}

        /// <summary>
        /// Gets the feature at the specified Object ID
        /// </summary>
        /// <param name="oid">Row index. Zero-based.</param>
        /// <param name="schema">The feature table containing the schema used to 
        /// read the row.</param>
        /// <returns>The feature row for the given row index</returns>
        /// <exception cref="InvalidDbaseFileOperationException">
        /// Thrown if this reader is closed (check <see cref="IsOpen"/> before calling), 
        /// or if the column is an unsupported type.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="oid"/> 
        /// &lt; 0 or <paramref name="oid"/> &gt;= <see cref="RecordCount"/></exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schema"/> is 
        /// null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the method is called and 
        /// Object has been disposed</exception>
        internal IRecord GetAttributes(UInt32 oid, ISchema schema)
        {
            checkState();

            if (!_isOpen)
            {
                throw new InvalidDbaseFileOperationException("An attempt was made to read " +
                                                             "from a closed dBase file");
            }

            if (oid <= 0 || oid > RecordCount)
            {
                throw new ArgumentOutOfRangeException("Invalid DataRow requested " +
                                                      "at index " + oid);
            }

            if (_reader.IsRowDeleted(oid)) // is record marked deleted?
            {
                return null;
            }

            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            IDictionary<IPropertyInfo, IValue> values = new Dictionary<IPropertyInfo, IValue>();

            foreach (DbaseField field in Header.Columns)
            {
                try
                {
                    IPropertyInfo propertyInfo = schema.Property(field.ColumnName);
                    values.Add(propertyInfo, propertyInfo.CreateValue(_reader.GetValue(oid, field)));
                }
                catch (NotSupportedException)
                {
                    String message = String.Format("Column type {0} is not supported.",
                                                   field.DataType);
                    throw new InvalidDbaseFileOperationException(message);
                }
            }

            return schema.RecordFactory.Create(values);

        }

        internal ISchema GetSchemaTable()
        {
            checkState();
            return _baseTable ?? (_baseTable = Header.GetSchemaTable(SchemaFactory));
        }

        //internal void SetTableSchema(FeatureDataTable target, SchemaMergeAction mergeAction)
        //{
        //    _baseTable.MergeSchemaTo(target, mergeAction);
        //}

        ///// <summary>
        ///// Returns an empty <see cref="FeatureDataTable"/> 
        ///// with the same schema as this DBase file.
        ///// </summary>
        //internal FeatureDataTable<UInt32> NewTable
        //{
        //    get
        //    {
        //        checkState();
        //        DbaseHeader h = Header;//hack to ensure _baseTable exists
        //        return _baseTable.Clone();
        //    }
        //}

        public DbaseHeader Header
        {
            get
            {
                if (!_headerIsParsed && !_isOpen)
                {
                    using (Stream dbfStream = openDbfFileStream(WriteAccess.ReadOnly))
                    {
                        syncReadHeader(SchemaFactory, dbfStream);
                    }
                }

                return _header;
            }
        }

        /// <summary>
        /// Opens the <see cref="DbaseReader"/> on the file 
        /// specified in the constructor.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the method is called 
        /// and Object has been disposed.
        /// </exception>
        internal void Open()
        {
            Open(WriteAccess.ReadOnly);
        }

        /// <summary>
        /// Opens the <see cref="DbaseFile"/> on the file name
        /// specified in the constructor, 
        /// with a value determining if the file is locked for 
        /// exclusive read access or not.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the method is called 
        /// and Object has been disposed.
        /// </exception>
        internal void Open(WriteAccess writeAccess)
        {
            checkState();

            // TODO: implement asynchronous access
            _dbaseFileStream = openDbfFileStream(writeAccess);

            _isOpen = true;

            syncReadHeader(SchemaFactory, DataStream);

            _reader = new DbaseReader(this);
            if (writeAccess != WriteAccess.ReadOnly)
                _writer = new DbaseWriter(this);
            // TODO: NullBinaryWriter
        }

        //internal void UpdateRow(UInt32 rowIndex, DataRow row)
        //{
        //    if (row == null) throw new ArgumentNullException("row");

        //    if (rowIndex < 0 || rowIndex >= Header.RecordCount)
        //    {
        //        throw new ArgumentOutOfRangeException("rowIndex");
        //    }

        //    DataStream.Seek(ComputeByteOffsetToRecord(rowIndex), SeekOrigin.Begin);
        //    _writer.BeginWrite();
        //    _writer.WriteRow(row);
        //    Header.LastUpdate = DateTime.Now;
        //    _writer.UpdateHeader(Header);
        //    _writer.EndWrite();
        //}
        #endregion

        #region Private helper methods

        private void checkState()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    "Attempt to access a disposed DbaseReader Object");
            }
        }

        private Stream openDbfFileStream(WriteAccess writeAccess)
        {
            FilePermissions @params = ShapeFileProvider.GetPermissions(writeAccess);
            return StorageManager.Open(_filename, @params.FileMode, @params.FileAccess, @params.FileShare);
            //return new FileStream(_filename,
            //                      @params.FileMode,
            //                      @params.FileAccess,
            //                      @params.FileShare,
            //                      4096,
            //                      FileOptions.None);
        }

        private void syncReadHeader(ISchemaFactory schemaFactory, Stream dbfStream)
        {
            if (_headerIsParsed)
            {
                return;
            }

            lock (_headerParseSync)
            {
                if (!_headerIsParsed)
                {
                    parseHeader(schemaFactory, dbfStream);
                }
            }
        }

        private void parseHeader(ISchemaFactory schemaFactory, Stream dbfStream)
        {
            _header = DbaseHeader.ParseDbfHeader(new NondisposingStream(dbfStream));
            _baseTable = DbaseSchema.GetFeatureTableForFields(schemaFactory, _header.Columns, _geoFactory);
            _headerIsParsed = true;
        }

        //            // Binary Tree not working yet on Mono 
        //            // see bug: http://bugzilla.ximian.com/show_bug.cgi?id=78502
        //#if !MONO
        //            /// <summary>
        //            /// Indexes a DBF column in a binary tree (B-Tree) [NOT COMPLETE]
        //            /// </summary>
        //            /// <typeparam name="TValue">datatype to be indexed</typeparam>
        //            /// <param name="columnId">Column to index</param>
        //            /// <returns>A <see cref="SharpMap.Indexing.BinaryTree.BinaryTree{T, UInt32}"/> data 
        //            /// structure indexing values in a column to a row index</returns>
        //            /// <exception cref="ObjectDisposedException">Thrown when the method is called and 
        //            /// Object has been disposed</exception>
        //            private BinaryTree<UInt32, TValue> createDbfIndex<TValue>(Int32 columnId) where TValue : IComparable<TValue>
        //            {
        //                if (_isDisposed)
        //                {
        //                    throw new ObjectDisposedException(
        //                        "Attempt to access a disposed DbaseReader Object");
        //                }

        //                BinaryTree<UInt32, TValue> tree = new BinaryTree<UInt32, TValue>();

        //                for (UInt32 i = 0; i < _header.RecordCount; i++)
        //                {
        //                    tree.Add(new BinaryTree<UInt32, TValue>.ItemValue(i, (TValue)GetValue(i, columnId)));
        //                }

        //                return tree;
        //            }
        //#endif

        //    #region Lucene Indexing (EXPERIMENTAL)

        //    /*
        ///// <summary>
        ///// Creates an index on the columns for faster searching [EXPERIMENTAL - Requires Lucene dependencies]
        ///// </summary>
        ///// <returns></returns>
        //private String createLuceneIndex()
        //{
        //    String dir = this._filename + ".idx";
        //    if (!System.IO.Directory.Exists(dir))
        //        System.IO.Directory.CreateDirectory(dir);
        //    Lucene.Net.Index.IndexWriter iw = new Lucene.Net.Index.IndexWriter(dir,new Lucene.Net.Analysis.Standard.StandardAnalyzer(),true);

        //    for (UInt32 i = 0; i < this._NumberOfRecords; i++)
        //    {
        //        FeatureDataRow dr = GetFeature(i,this.NewTable);
        //        Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
        //        // Add the Object-id as a field, so that index can be maintained.
        //        // This field is not stored with document, it is indexed, but it is not
        //        // tokenized prior to indexing.
        //        //doc.Add(Lucene.Net.Documents.Field.UnIndexed("SharpMap_oid", i.ToString())); //Add OID index

        //        foreach(System.Data.DataColumn col in dr.Table.Columns) //Add and index values from DBF
        //        {
        //            if(col.DataType.Equals(typeof(String)))
        //                // Add the contents as a valued Text field so it will get tokenized and indexed.
        //                doc.Add(Lucene.Net.Documents.Field.UnStored(col.ColumnName,(String)dr[col]));
        //            else
        //                doc.Add(Lucene.Net.Documents.Field.UnStored(col.ColumnName, dr[col].ToString()));
        //        }
        //        iw.AddDocument(doc);
        //    }
        //    iw.Optimize();
        //    iw.Close();
        //    return this._filename + ".idx";
        //}
        //*/

        #endregion

        public IStorageManager StorageManager { get; protected set; }
    }
}
