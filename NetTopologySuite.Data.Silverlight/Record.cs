using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

        public IValue Id
        {
            get { return _values[_schema.IdProperty]; }
        }

        #region IRecord Members

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
            set { SetValue(index, value); }
        }

        public void SetValue(int index, IValue value)
        {
            SetValue(_schema.Property(index), value);
        }

        public IValue this[string propertyName]
        {
            get { return GetValue(propertyName); }
            set { SetValue(propertyName, value); }
        }

        public void SetValue(string propertyName, IValue value)
        {
            SetValue(_schema.Property(propertyName), value);
        }

        public IValue this[IPropertyInfo propertyInfo]
        {
            get { return GetValue(propertyInfo); }
            set { SetValue(propertyInfo, value); }
        }

        public void SetValue(IPropertyInfo propertyInfo, IValue value)
        {
            if (!value.Type.IsAssignableFrom(propertyInfo.PropertyType))
                throw new ArgumentException();
            _values[propertyInfo] = value;
        }

        public T GetId<T>()
        {
            return ((IValue<T>)Id).Value;
        }

        public IValue GetId()
        {
            return Id;
        }


        public void SetValue<T>(IPropertyInfo<T> propertyInfo, IValue<T> value)
        {
            if (!_schema.ContainsProperty(propertyInfo))
                throw new ArgumentException();

            _values[propertyInfo] = value;
        }


        public bool IsUnset(int index)
        {
            return IsUnset(Schema.Property(index));
        }

        #endregion

        public bool IsUnset(string propertyName)
        {
            return IsUnset(Schema.Property(propertyName));
        }

        public bool IsUnset(IPropertyInfo propertyInfo)
        {
            if (!_values.ContainsKey(propertyInfo))
                return true;

            return _values[propertyInfo] == null;
        }


        public void SetValue(int index, object value)
        {
            SetValue(Schema.Property(index), value);
        }

        public void SetValue(string name, object value)
        {
            SetValue(Schema.Property(name), value);
        }

        public void SetValue(IPropertyInfo propertyInfo, object value)
        {
            if (!_schema.ContainsProperty(propertyInfo))
                throw new ArgumentException("propertyInfo");
            _values[propertyInfo] = propertyInfo.CreateValue(value);
        }

        public void SetValue<T>(IPropertyInfo<T> propertyInfo, T value)
        {
            if (!Schema.ContainsProperty(propertyInfo))
                throw new ArgumentException("propertyInfo");
            _values[propertyInfo] = propertyInfo.CreateValue(value);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetType());
            sb.Append(":: ");
            foreach (var prop in _schema.Properties)
            {
                sb.Append(prop.Name);
                sb.Append(":");
                sb.Append(GetValue(prop));
                sb.Append("; ");
            }

            return sb.ToString();
        }
    }
}