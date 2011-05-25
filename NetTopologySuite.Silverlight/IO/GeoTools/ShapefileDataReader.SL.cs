using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GeoAPI.Geometries;
using NetTopologySuite.Data;

namespace NetTopologySuite.IO
{
    public partial class ShapefileDataReader : IRecordSource
    {
        public ShapefileDataReader(ISchemaFactory schemaFactory)
        {
            SchemaFactory = schemaFactory;

        }

        protected ISchemaFactory SchemaFactory
        {
            get;
            set;
        }

        protected IValueFactory ValueFactory
        {
            get
            {
                return SchemaFactory.PropertyFactory.ValueFactory;
            }
        }

        private List<object> _columnValues;

        private ISchema _schema;

        #region IRecordSource Members

        public ISchema Schema
        {
            get { return _schema ?? (_schema = CreateSchema()); }
        }

        public IEnumerator<IRecord> GetEnumerator()
        {
            return new SLShapeFileDataEnumerator(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IMemoryRecordSet ToInMemorySet()
        {
            return new MemoryRecordSet(Schema, this);
        }

        public IMemoryRecordSet ToInMemorySet(Expression<Func<IRecord, bool>> predicate)
        {
            return new MemoryRecordSet(Schema, this.Where(predicate.Compile()));
        }

        #endregion

        private ISchema CreateSchema()
        {
            var props = new List<IPropertyInfo> { SchemaFactory.PropertyFactory.Create<IGeometry>("Geom") };
            props.AddRange(_dbfHeader.Fields.Select(a => SchemaFactory.PropertyFactory.Create(a.Type, a.Name)));

            return SchemaFactory.Create(props, props.FirstOrDefault(a => a.Name == "OID"));
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
            _columnValues = (List<object>)_dbfEnumerator.Current;

            return _moreRecords; // moreDbfRecords && moreShpRecords;
        }

        #region Nested type: SLShapeFileDataEnumerator

        internal class SLShapeFileDataEnumerator : IEnumerable<IRecord>
        {
            private readonly ShapefileDataReader _shapefileDataReader;

            public SLShapeFileDataEnumerator(ShapefileDataReader shapefileDataReader)
            {
                _shapefileDataReader = shapefileDataReader;
            }

            #region IEnumerable<IRecord> Members

            public IEnumerator<IRecord> GetEnumerator()
            {
                while (_shapefileDataReader.Read())
                {
                    var values = new Dictionary<IPropertyInfo, IValue>
                                     {
                                         {
                                             _shapefileDataReader.Schema.Properties[0],
                                            _shapefileDataReader.ValueFactory.CreateValue((IGeometry) _shapefileDataReader._shpEnumerator.Current)
                                             }
                                     };

                    for (int i = 1; i < _shapefileDataReader.Schema.Properties.Count; i++)
                        values.Add(_shapefileDataReader.Schema.Properties[i],
                                   _shapefileDataReader.ValueFactory.CreateValue(
                                       _shapefileDataReader.Schema.Properties[i].PropertyType,
                                       _shapefileDataReader._columnValues[i - 1]));


                    yield return _shapefileDataReader.Schema.RecordFactory.Create(values);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
}