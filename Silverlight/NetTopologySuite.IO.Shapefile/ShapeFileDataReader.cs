// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Data;

namespace NetTopologySuite.Shapefile
{
    /// <summary>
    /// Provides a fast-forward, read-only data stream to feature data
    /// from a <see cref="NetTopologySuite.Shapefile"/>.
    /// </summary>
    public class ShapeFileDataReader : IEnumerable<IRecord> //IFeatureDataReader
    {
        #region Instance fields

        private readonly Int32 _fieldCount;
        private readonly IEnumerator<UInt32> _objectEnumerator;
        private readonly ISchema _schemaTable;
        private readonly ShapeFileProvider _shapeFile;
        private IRecord _currentFeature;

        #endregion

        #region Object Construction / Disposal

        internal ShapeFileDataReader(ShapeFileProvider source)
        {
            _shapeFile = source;
            _schemaTable = source.GetSchemaTable();
            _fieldCount = _schemaTable.Properties.Count;

            // TODO: now that we are accessing the geometry each time, perhaps a feature
            // query here would save a disk access
            _objectEnumerator = source.ExecuteOidQuery(null).GetEnumerator();
        }

        #region Dispose Pattern

        /// <summary>
        /// Gets a value indicating whether the ShapeFileDataReader has been disposed or not.
        /// </summary>
        public Boolean IsDisposed { get; private set; }

        ~ShapeFileDataReader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        private void Dispose(Boolean disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                //jd:commented out because it closes the parent ShapefileProvider which 
                //then causes an exception when a new query occurs e.g zoom in
                //_shapeFile.Close();            
            }

            OnDisposed();
        }

        private void OnDisposed()
        {
            EventHandler e = Disposed;

            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
        }

        public event EventHandler Disposed;

        #endregion

        #endregion

        public IGeometry Geometry
        {
            get
            {
                checkState();
                var geom = _currentFeature.GetValue<IGeometry>("Geom");
                return geom == null
                           ? null
                           : (IGeometry)geom.Clone();
            }
        }

        public IEnvelope Extents
        {
            get
            {
                checkState();

                var geom = _currentFeature.GetValue<IGeometry>("Geom");
                return geom == null
                           ? null
                           : (IEnvelope)geom.EnvelopeInternal.Clone();
            }
        }

        public Boolean HasOid
        {
            get { return true; }
        }


        public Type OidType
        {
            get { return typeof(UInt32); }
        }

        //public ICoordinateTransformation CoordinateTransformation { get; set; }

        public Int32 Depth
        {
            get
            {
                checkDisposed();
                return 0;
            }
        }

        public Boolean IsClosed
        {
            get { return IsDisposed; }
        }

        public Int32 RecordsAffected
        {
            get { return -1; }
        }

        public Int32 FieldCount
        {
            get { return _fieldCount; }
        }

        public Object this[String name]
        {
            get
            {
                checkState();
                return _currentFeature[name];
            }
        }

        public Object this[Int32 i]
        {
            get
            {
                checkState();
                checkIndex(i);

                return _currentFeature[i];
            }
        }

        #region Private helper methods

        private void checkIndex(Int32 i)
        {
            if (i >= FieldCount)
            {
                throw new IndexOutOfRangeException("Index must be less than FieldCount.");
            }
        }

        private void checkState()
        {
            checkDisposed();
            if (_currentFeature == null)
            {
                throw new InvalidOperationException("The Read method must be called before accessing values.");
            }
        }

        private void checkDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());
        }

        #endregion

        #region IEnumerable<IRecord> Members

        public IEnumerator<IRecord> GetEnumerator()
        {
            while (Read())
            {
                yield return _currentFeature;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public Object GetOid()
        {
            checkState();
            return _objectEnumerator.Current;
        }

        public void Close()
        {
            Dispose();
        }

        public ISchema GetSchemaTable()
        {
            checkDisposed();
            return _schemaTable;
        }

        public Boolean NextResult()
        {
            checkState();
            return false;
        }

        public Boolean Read()
        {
            checkDisposed();

            Boolean reading = _objectEnumerator.MoveNext();

            if (reading)
            {
                _currentFeature = _shapeFile.GetFeatureByOid(_objectEnumerator.Current);
            }

            return reading;
        }

        public Boolean GetBoolean(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToBoolean(_currentFeature[i]);
        }

        public Byte GetByte(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToByte(_currentFeature[i]);
        }

        public Int64 GetBytes(Int32 i, Int64 fieldOffset, Byte[] buffer, Int32 bufferoffset, Int32 length)
        {
            throw new NotSupportedException();
        }

        public Char GetChar(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToChar(_currentFeature[i]);
        }

        public Int64 GetChars(Int32 i, Int64 fieldoffset, Char[] buffer, Int32 bufferoffset, Int32 length)
        {
            throw new NotSupportedException();
        }


        public String GetDataTypeName(Int32 i)
        {
            checkState();
            checkIndex(i);

            return GetFieldType(i).Name;
        }

        public DateTime GetDateTime(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToDateTime(_currentFeature[i]);
        }

        public decimal GetDecimal(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToDecimal(_currentFeature[i]);
        }

        public Double GetDouble(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToDouble(_currentFeature[i]);
        }

        public Type GetFieldType(Int32 i)
        {
            checkState();
            checkIndex(i);
            return _currentFeature.Schema.GetPropertyType(i);
        }

        public Single GetFloat(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToSingle(_currentFeature[i]);
        }

        public Guid GetGuid(Int32 i)
        {
            checkState();
            checkIndex(i);

            Object feature = _currentFeature[i];

            if (feature is Guid)
            {
                return (Guid)feature;
            }

            var featureAsString = feature as String;

            if (featureAsString != null)
            {
                return new Guid(featureAsString);
            }

            var featureAsBytes = feature as Byte[];

            if (featureAsBytes != null)
            {
                return new Guid(featureAsBytes);
            }

            throw new InvalidCastException(
                String.Format("Invalid cast from '{0}' to 'Guid'", feature.GetType()));
        }

        public Int16 GetInt16(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToInt16(_currentFeature[i]);
        }

        public Int32 GetInt32(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToInt32(_currentFeature[i]);
        }

        public Int64 GetInt64(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToInt64(_currentFeature[i]);
        }

        public String GetName(Int32 i)
        {
            checkDisposed();
            return _schemaTable.GetName(i);
        }

        public Int32 GetOrdinal(String name)
        {
            checkDisposed();

            if (name == null) throw new ArgumentNullException("name");

            Int32 fieldCount = FieldCount;

            for (Int32 i = 0; i < fieldCount; i++)
            {
                if (name.Equals(GetName(i)))
                {
                    return i;
                }
            }

            for (Int32 i = 0; i < fieldCount; i++)
            {
                if (String.Compare(name, GetName(i), StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException("Column name not found");
        }

        public String GetString(Int32 i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToString(_currentFeature[i]);
        }

        public Object GetValue(Int32 i)
        {
            checkState();
            checkIndex(i);

            return _currentFeature[i];
        }

        public Int32 GetValues(Object[] values)
        {
            checkState();

            if (values == null) throw new ArgumentNullException("values");

            Int32 count = values.Length > FieldCount ? FieldCount : values.Length;

            for (Int32 i = 0; i < count; i++)
            {
                values[i] = this[i];
            }

            return count;
        }

        public Boolean IsDBNull(Int32 i)
        {
            checkState();
            checkIndex(i);

            return _currentFeature.IsUnset(i);
        }
    }
}