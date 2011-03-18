using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GisSharpBlog.NetTopologySuite.Data
{
    internal class Schema : ISchema, IEquatable<Schema>
    {
        private readonly IPropertyInfo _idProperty;
        private readonly Dictionary<string, int> _nameIndexMap;
        private readonly ReadOnlyCollection<IPropertyInfo> _properties;
        private readonly IRecordFactory _recordFactory;


        internal Schema(ISchemaFactory schemaFactory, IEnumerable<IPropertyInfo> properties, IPropertyInfo idProperty)
        {
            SchemaFactory = schemaFactory;
            _properties = new ReadOnlyCollection<IPropertyInfo>(properties.ToList());
            _idProperty = idProperty;
            _recordFactory = new RecordFactory(this);
            _nameIndexMap = properties.Select((a, b) => new
                                                            {
                                                                a.Name,
                                                                Index = b
                                                            }).ToDictionary(a => a.Name, b => b.Index);
        }

        #region IEquatable<Schema> Members

        public bool Equals(Schema other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._idProperty, _idProperty) && Equals(other._nameIndexMap, _nameIndexMap) &&
                   Equals(other._properties, _properties) && Equals(other._recordFactory, _recordFactory);
        }

        #endregion

        #region ISchema Members

        public ReadOnlyCollection<IPropertyInfo> Properties
        {
            get { return _properties; }
        }

        public IPropertyInfo IdProperty
        {
            get { return _idProperty; }
        }

        public IRecordFactory RecordFactory
        {
            get { return _recordFactory; }
        }

        public int GetOrdinal(string propertyName)
        {
            return _nameIndexMap[propertyName];
        }

        public string GetName(int index)
        {
            if (index > _properties.Count - 1)
                throw new IndexOutOfRangeException();
            return Property(index).Name;
        }

        public IPropertyInfo Property(int index)
        {
            if (index > _properties.Count - 1)
                throw new IndexOutOfRangeException();
            return _properties[index];
        }

        public IPropertyInfo Property(string name)
        {
            return _properties[_nameIndexMap[name]];
        }

        public Type GetPropertyType(int index)
        {
            return _properties[index].PropertyType;
        }

        public bool HasIdProperty
        {
            get { return _idProperty != null; }
        }

        public bool Equals(ISchema other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.IdProperty, _idProperty) &&
                       Equals(other.Properties, _properties) && Equals(other.RecordFactory, _recordFactory);

        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Schema)) return false;
            return Equals((Schema)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_idProperty != null ? _idProperty.GetHashCode() : 0);
                result = (result * 397) ^ (_nameIndexMap != null ? _nameIndexMap.GetHashCode() : 0);
                result = (result * 397) ^ (_properties != null ? _properties.GetHashCode() : 0);
                result = (result * 397) ^ (_recordFactory != null ? _recordFactory.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(Schema left, Schema right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Schema left, Schema right)
        {
            return !Equals(left, right);
        }

        public ISchemaFactory SchemaFactory
        {
            get; private set;
        }

        public IPropertyInfo this[int index]
        {
            get { return _properties[index]; }
        }

        public IPropertyInfo this[string propertyName]
        {
            get { return _properties[_nameIndexMap[propertyName]]; }
        }
    }
}