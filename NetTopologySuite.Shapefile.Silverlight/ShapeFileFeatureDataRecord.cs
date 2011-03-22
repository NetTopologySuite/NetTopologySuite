using System;
using System.Collections.Generic;
using System.Data;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.ShapeFile
{
    internal class ShapeFileFeatureDataRecord : IFeatureDataRecord
    {
        //private readonly IDictionary<Int32, Object> _rowValues = new HybridDictionary<Int32, Object>();
        private readonly IDictionary<Int32, Object> _rowValues = new Dictionary<Int32, Object>();
        //private readonly Dictionary<Int32, String> _rowColumns = new Dictionary<Int32, String>();
        private readonly List<DbaseField> _rowColumns;
        private IGeometry _geometry;
        private readonly Int32 _oidColumn = 0;
        private static readonly DbaseField _oidField =
            new DbaseField(null, ShapeFileConstants.IdColumnName, typeof(UInt32), 0, 0, 0, 0);
        private static readonly ShapeFileFeatureDataRecord _idOnlyRecord =
            new ShapeFileFeatureDataRecord(new DbaseField[] { _oidField });
        //private readonly Int32 _columnCount;

        public static DbaseField OidField
        {
            get { return _oidField; }
        }

        public static ShapeFileFeatureDataRecord IdOnlyRecord
        {
            get { return _idOnlyRecord; }
        }

        public ShapeFileFeatureDataRecord(IList<DbaseField> columns)
        {
            if (columns == null) throw new ArgumentNullException("columns");
            _rowColumns = new List<DbaseField>(columns.Count + 1);
            _rowColumns.Add(OidField);
            _rowColumns.AddRange(columns);
        }

        public void SetColumnValue(Int32 columnOrdinal, Object value)
        {
            if (_rowValues.ContainsKey(columnOrdinal))
            {
                throw new InvalidOperationException("Column with ordinal " + columnOrdinal + " already exists.");
            }

            _rowValues[columnOrdinal] = value;
        }

        public void SetColumnValues(Int32 startOrdinal, Int32 endOrdinal, Object[] values)
        {
            for (Int32 i = startOrdinal, j = 0; i < endOrdinal; i++, j++)
            {
                _rowValues[i] = values[j];
            }
        }

        #region Implementation of IDataRecord

        public string GetName(int i)
        {
            checkIndex(i);
            return _rowColumns[i].ColumnName;
        }

        public string GetDataTypeName(int i)
        {
            checkIndex(i);
            return _rowColumns[i].DataType.Name;
        }

        public Type GetFieldType(int i)
        {
            checkIndex(i);
            return _rowColumns[i].DataType;
        }

        public Object GetValue(int i)
        {
            checkIndex(i);
            return _rowColumns[i];
        }

        public int GetValues(Object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                return 0;
            }

            Int32 objectCount = Math.Min(values.Length, _rowColumns.Count);

            List<KeyValuePair<Int32, Object>> sortedValues = new List<KeyValuePair<Int32, Object>>(_rowValues);
            sortedValues.Sort(delegate(KeyValuePair<Int32, Object> a, KeyValuePair<Int32, Object> b)
            {
                return a.Key.CompareTo(b.Key);
            });

            for (int i = 0; i < objectCount; i++)
            {
                values[i] = sortedValues[i].Value;
            }

            return objectCount;
        }

        public int GetOrdinal(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            foreach (DbaseField column in _rowColumns)
            {
                if (name.Equals(column.ColumnName))
                {
                    return column.Ordinal + 1; //jd: oid column is at index 0
                }
            }

            return -1;
        }

        public bool GetBoolean(int i)
        {
            checkIndex(i);

            return (Boolean)_rowValues[i];
        }

        public byte GetByte(int i)
        {
            checkIndex(i);

            return (Byte)_rowValues[i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new System.NotImplementedException();
        }

        public char GetChar(int i)
        {
            checkIndex(i);

            return (Char)_rowValues[i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new System.NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            checkIndex(i);

            return (Guid)_rowValues[i];
        }

        public short GetInt16(int i)
        {
            checkIndex(i);

            return (Int16)_rowValues[i];
        }

        public int GetInt32(int i)
        {
            checkIndex(i);

            return (Int32)_rowValues[i];
        }

        public long GetInt64(int i)
        {
            checkIndex(i);

            return (Int64)_rowValues[i];
        }

        public float GetFloat(int i)
        {
            checkIndex(i);

            return (Single)_rowValues[i];
        }

        public double GetDouble(int i)
        {
            checkIndex(i);

            return (Double)_rowValues[i];
        }

        public string GetString(int i)
        {
            checkIndex(i);

            return (String)_rowValues[i];
        }

        public decimal GetDecimal(int i)
        {
            checkIndex(i);

            return (Decimal)_rowValues[i];
        }

        public DateTime GetDateTime(int i)
        {
            checkIndex(i);

            return (DateTime)_rowValues[i];
        }

        public IDataReader GetData(int i)
        {
            throw new System.NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            checkIndex(i);

            Object value;
            return !_rowValues.TryGetValue(i, out value) && DBNull.Value.Equals(value);
        }

        public int FieldCount
        {
            get { return _rowColumns.Count; }
        }

        Object IDataRecord.this[int i]
        {
            get
            {
                checkIndex(i);
                return _rowValues[i];
            }
        }

        Object IDataRecord.this[string name]
        {
            get { return _rowValues[GetOrdinal(name)]; }
        }

        #endregion

        #region Implementation of IFeatureDataRecord

        public IGeometry Geometry
        {
            get { return _geometry; }
            set { _geometry = value; }
        }

        public IExtents Extents
        {
            get { return _geometry == null ? null : _geometry.Extents; }
        }

        public Object GetOid()
        {
            return _rowValues[_oidColumn];
        }

        public bool HasOid
        {
            get
            {
                return true;
            }
        }

        public bool IsFullyLoaded
        {
            get
            {
                return true;
            }
        }

        public Type OidType
        {
            get
            {
                return typeof(UInt32);
            }
        }

        public ICoordinateTransformation CoordinateTransformation
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        #endregion

        private void checkIndex(int i)
        {
            if (i < 0 || i >= _rowColumns.Count)
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
