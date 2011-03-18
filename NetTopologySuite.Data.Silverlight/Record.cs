using System;
using System.Collections;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Data
{
    internal class Record : IRecord
    {
        private readonly ISchema _schema;
        private readonly IDictionary<IPropertyInfo, IValue> _values;

        internal Record(ISchema schema, IDictionary<IPropertyInfo, IValue> values)
        {
            _schema = schema;
            _values = values;
        }

        #region IRecord Members

        public IValue Id
        {
            get { return _values[_schema.IdProperty]; }
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public IValue GetValue(int index)
        {
            return _values[_schema.Property(index)];
        }

        public IValue GetValue(string name)
        {
            return _values[_schema.Property(name)];
        }

        public T GetValue<T>(int index)
        {
            return ((IValue<T>)GetValue(index)).Value;
        }

        public T GetValue<T>(string name)
        {
            return ((IValue<T>)GetValue(name)).Value;
        }

        public IEnumerator<KeyValuePair<IPropertyInfo, IValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IValue GetValue(IPropertyInfo propertyInfo)
        {
            return _values[propertyInfo];
        }

        public T GetValue<T>(IPropertyInfo propertyInfo)
        {
            if (!(propertyInfo is IPropertyInfo<T>))
                throw new ArgumentException();

            return ((IValue<T>)GetValue(propertyInfo)).Value;
        }

        public IValue this[int index]
        {
            get { return GetValue(index); }
        }

        public IValue this[string propertyName]
        {
            get { return GetValue(propertyName); }
        }

        public IValue this[IPropertyInfo propertyInfo]
        {
            get { return GetValue(propertyInfo); }
        }

        #endregion



        public T GetId<T>()
        {
            return ((IValue<T>)Id).Value;
        }

        public IValue GetId()
        {
            return Id;
        }
    }
}