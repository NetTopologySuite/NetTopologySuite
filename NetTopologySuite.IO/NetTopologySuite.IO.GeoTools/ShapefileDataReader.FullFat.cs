using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    public partial class ShapefileDataReader : IEnumerable, IDataReader, IDataRecord
    {
        ArrayList _columnValues;


        /// <summary>
        /// Return geometry feature of the shapefile.
        /// </summary>
        public IGeometry Geometry
        {
            get { return geometry; }
        }


        /// <summary>
        /// 
        /// </summary>
        internal class ShapefileDataReaderEnumerator : IEnumerator
        {
            private readonly ShapefileDataReader _parent;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public ShapefileDataReaderEnumerator(ShapefileDataReader parent)
            {
                _parent = parent;
                _parent.Reset();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reset()
            {
                _parent.Reset();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                return _parent.Read();
            }

            /// <summary>
            /// 
            /// </summary>
            public object Current
            {
                get
                {
                    return new RowStructure(_parent._dbaseFields, _parent._columnValues);
                }
            }
        }


        /// <summary>
        /// Advances the data reader to the next result, when reading the shapefile.
        /// </summary>
        /// <returns>false</returns>
        public bool NextResult()
        {
            return false;
        }



        /// <summary>
        /// Advances the IDataReader to the next record.
        /// </summary>
        /// <returns>true if there are more rows; otherwise, false.</returns>
        public bool Read()
        {
            bool moreDbfRecords = _dbfEnumerator.MoveNext();
            bool moreShpRecords = _shpEnumerator.MoveNext();

            if (!moreDbfRecords)
            {
                int a = 0;
                a++;
            }
            if (!moreShpRecords)
            {
                int b = 0;
                b++;
            }
            _moreRecords = moreDbfRecords && moreShpRecords;

            // get current shape 
            geometry = (IGeometry)_shpEnumerator.Current;

            // get current dbase record
            _columnValues = (ArrayList)_dbfEnumerator.Current;

            return _moreRecords; // moreDbfRecords && moreShpRecords;
        }


        /// <summary>
        /// Returns a DataTable that describes the column metadata of the IDataReader.
        /// </summary>
        /// <returns>A DataTable that describes the column metadata.</returns>
        public DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not applicable for this data reader.
        /// </summary>
        /// <value>Always -1 for this data reader.</value>
        public int RecordsAffected
        {
            /*
            * RecordsAffected is only applicable to batch statements
            * that include inserts/updates/deletes. The sample always
            * returns -1.
            */
            get
            {
                return -1;
            }
        }



        /// <summary>
        /// Always return a value of zero since nesting is not supported.
        /// </summary>
        /// <value>The level of nesting.</value>
        /// <remarks>The outermost table has a depth of zero.</remarks>
        public int Depth
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the numbers of records in the Shapefile.
        /// </summary>
        public int RecordCount
        {
            get
            {
                return _recordCount;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i)
        {
            string strValue = _columnValues[i].ToString().Trim();
            Int32 value;
            Int32.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i)
        {
            return _columnValues[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i)
        {
            return _dbaseFields[i].Type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values)
        {
            int i = 0;
            for (i = 0; i < values.Length; i++)
                values[i] = _columnValues[i];
            return i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i)
        {
            return this._dbaseFields[i].Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i)
        {
            string strValue = _columnValues[i].ToString().Trim();
            Int64 value;
            Int64.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i)
        {
            string strValue = _columnValues[i].ToString().Trim();
            double value;
            Double.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBoolean(int i)
        {
            return (bool)_columnValues[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i)
        {
            Guid value = Guid.Empty;
            try
            {
                string strValue = _columnValues[i].ToString().Trim();
                value = new Guid(strValue);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i)
        {
            string strValue = _columnValues[i].ToString().Trim();
            DateTime value;
            DateTime.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name)
        {
            for (int i = 0; i < _dbfReader.GetHeader().NumFields; i++)
                if (0 == CultureAwareCompare(_dbfReader.GetHeader().Fields[i].Name, name))
                    return i;
            // Throw an exception if the ordinal cannot be found.
            throw new IndexOutOfRangeException("Could not find specified column in results.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i)
        {
            return _dbaseFields[i].Type.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i)
        {
            string strValue = _columnValues[i].ToString().Trim();
            float value;
            float.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i)
        {
            /*
            * The sample code does not support this method. Normally,
            * this would be used to expose nested tables and
            * other hierarchical data.
            */
            throw new NotSupportedException("GetData not supported.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldoffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            //HACK:
            string str = _columnValues[i].ToString();
            str.CopyTo((int)fieldoffset, buffer, 0, length);
            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i)
        {
            return _columnValues[i].ToString().Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i)
        {
            return (char)_columnValues[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i)
        {
            string strValue = _columnValues[i].ToString().Trim();
            Int16 value;
            Int16.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                int ordinal = this.GetOrdinal(name);
                return _columnValues[ordinal];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object this[int i]
        {
            get
            {
                return _columnValues[i];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int FieldCount
        {
            get
            {
                return _dbaseFields.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return new ShapefileDataReaderEnumerator(this);
        }


        /// <summary>
        /// Gets the header for the Shapefile.
        /// </summary>
        public ShapefileHeader ShapeHeader
        {
            get
            {
                return _shpHeader;
            }
        }

        /// <summary>
        /// Gets the header for the Dbase file.
        /// </summary>
        public DbaseFileHeader DbaseHeader
        {
            get
            {
                return this._dbfHeader;
            }
        }

        /// <summary>
        /// Implementation specific methods.
        /// </summary>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        /// <returns></returns>
        private int CultureAwareCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
        }
    }
}