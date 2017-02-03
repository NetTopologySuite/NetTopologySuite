using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using NetTopologySuite.IO.Streams;

namespace NetTopologySuite.IO
{
    /// <summary>
    ///     Class that allows records in a dbase file to be enumerated.
    /// </summary>
    public partial class DbaseFileReader : IEnumerable
    {
        private readonly IStreamProvider _streamProvider;
        private readonly IStreamProvider _encodingProvider;

        private DbaseFileHeader _header;

        #region Implementation of IEnumerable

        /// <summary>
        ///     Gets the object that allows iterating through the members of the collection.
        /// </summary>
        /// <returns>
        ///     An object that implements the IEnumerator interface.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return new DbaseFileEnumerator(this);
        }

        #endregion

        /// <summary>
        /// </summary>
        private partial class DbaseFileEnumerator : IEnumerator, IDisposable
        {
            private ArrayList _arrayList;
            private readonly BinaryReader _dbfReader;
            protected string[] _fieldNames = null;
            private DbaseFileHeader _header;
            private int _iCurrentRecord;
            private readonly DbaseFileReader _parent;
            private int _readPosition;

            #region Implementation of IEnumerator

            /// <summary>
            ///     Sets the enumerator to its initial position, which is
            ///     before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            ///     The collection was modified after the enumerator was created.
            /// </exception>
            public void Reset()
            {
                _dbfReader.BaseStream.Seek(_header.HeaderLength, SeekOrigin.Begin);
                _iCurrentRecord = 0;
                //throw new InvalidOperationException();
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     true if the enumerator was successfully advanced to the next element;
            ///     false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            ///     The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
                _iCurrentRecord++;
                if (_iCurrentRecord <= _header.NumRecords)
                    _arrayList = Read();
                bool more = !(_iCurrentRecord > _header.NumRecords);
                return more;
            }

            /// <summary>
            ///     Gets the current element in the collection.
            /// </summary>
            /// <value></value>
            /// <returns>The current element in the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">
            ///     The enumerator is positioned before the first element of the collection
            ///     or after the last element.
            /// </exception>
            public object Current
            {
                get { return _arrayList; }
            }

            /// <summary>
            /// </summary>
            protected void ReadHeader()
            {
                _header = new DbaseFileHeader();
                // read the header
                _header.ReadHeader(_dbfReader, _parent._encodingProvider);

                // how many records remain
                _readPosition = _header.HeaderLength;
            }

            /// <summary>
            ///     Read a single dbase record
            /// </summary>
            /// <returns>
            ///     The read shapefile record,
            ///     or null if there are no more records.
            /// </returns>
            private ArrayList Read()
            {
                ArrayList attrs = null;

                var foundRecord = false;
                while (!foundRecord)
                {
                    // retrieve the record length
                    var tempNumFields = _header.NumFields;

                    // storage for the actual values
                    attrs = new ArrayList(tempNumFields);

                    // read the deleted flag
                    var tempDeleted = _dbfReader.ReadChar();

                    // read the record length
                    var tempRecordLength = 1; // for the deleted character just read.

                    // read the Fields
                    for (var j = 0; j < tempNumFields; j++)
                    {
                        // find the length of the field.
                        var tempFieldLength = _header.Fields[j].Length;
                        tempRecordLength = tempRecordLength + tempFieldLength;

                        // find the field type
                        var tempFieldType = _header.Fields[j].DbaseType;

                        // read the data.
                        object tempObject = null;
                        switch (tempFieldType)
                        {
                            case 'L': // logical data type, one character (T,t,F,f,Y,y,N,n)
                                var tempChar = (char)_dbfReader.ReadByte();
                                if ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'))
                                    tempObject = true;
                                else tempObject = false;
                                break;

                            case 'C': // character record.

                                if (_header.Encoding == null)
                                {
                                    var sbuffer = _dbfReader.ReadChars(tempFieldLength);
                                    tempObject = new string(sbuffer).Trim().Replace("\0", string.Empty);
                                        //.ToCharArray();
                                }
                                else
                                {
                                    var buf = _dbfReader.ReadBytes(tempFieldLength);
                                    tempObject = _header.Encoding.GetString(buf, 0, buf.Length).Trim();
                                }
                                break;

                            case 'D': // date data type.
                                var ebuffer = new char[8];
                                ebuffer = _dbfReader.ReadChars(8);
                                var tempString = new string(ebuffer, 0, 4);

                                int year;
                                if (
                                    !int.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                        out year))
                                    break;
                                tempString = new string(ebuffer, 4, 2);

                                int month;
                                if (
                                    !int.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                        out month))
                                    break;
                                tempString = new string(ebuffer, 6, 2);

                                int day;
                                if (
                                    !int.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                        out day))
                                    break;

                                try
                                {
                                    if (day > 0 && year > 0 && month > 0 && month <= 12)
                                        // don't try to parse date when day is invalid - it will be useless and slow for large files
                                        tempObject = new DateTime(year, month, day);
                                }
                                catch (Exception)
                                {
                                }

                                break;

                            case 'N': // number
                            case 'F': // floating point number
                                var fbuffer = new char[tempFieldLength];
                                fbuffer = _dbfReader.ReadChars(tempFieldLength);
                                tempString = new string(fbuffer);
                                double val;
                                if (double.TryParse(tempString.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture,
                                    out val))
                                {
                                    tempObject = val;
                                }
                                else
                                {
                                    // if we can't format the number, just save it as a string
                                    tempObject = tempString;
                                }
                                break;

                            default:
                                throw new NotSupportedException("Do not know how to parse Field type " + tempFieldType);
                        }
                        attrs.Add(tempObject);
                    }

                    // ensure that the full record has been read.
                    if (tempRecordLength < _header.RecordLength)
                    {
                        var tempbuff = new byte[_header.RecordLength - tempRecordLength];
                        tempbuff = _dbfReader.ReadBytes(_header.RecordLength - tempRecordLength);
                    }

                    // add the row if it is not deleted.
                    if (tempDeleted != '*')
                    {
                        foundRecord = true;
                    }
                }
                return attrs;
            }

            #endregion
        }

        #region Constructors

        #endregion

        #region Methods
        #region Stream provider regsitry utility functions
        static IStreamProviderRegistry CreateStreamProviderRegistry(string dbfPath)
        {
            var dbfStreamProvider = new FileStreamProvider(StreamTypes.Data, dbfPath, true);
            var cpgPath = Path.ChangeExtension(dbfPath, "cpg");
            IStreamProvider cpgStreamProvider = null;
            if (File.Exists(cpgPath))
                cpgStreamProvider = new FileStreamProvider(StreamTypes.DataEncoding, cpgPath, true);

            return new ShapefileStreamProviderRegistry(null, dbfStreamProvider, null, dataEncodingStream: cpgStreamProvider);
        }

        static IStreamProviderRegistry CreateStreamProviderRegistry(string dbfPath, Encoding encoding)
        {
            var dbfStreamProvider = new FileStreamProvider(StreamTypes.Data, dbfPath, true);
            var cpgStreamProvider = new ByteStreamProvider(StreamTypes.DataEncoding, encoding.EncodingName);

            return new ShapefileStreamProviderRegistry(null, dbfStreamProvider, null, dataEncodingStream: cpgStreamProvider);
        }
        #endregion
        #endregion
    }
}