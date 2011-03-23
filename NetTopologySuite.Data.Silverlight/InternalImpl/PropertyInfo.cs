using System;

namespace GisSharpBlog.NetTopologySuite.Data.InternalImpl
{
    internal class PropertyInfo<T> : IPropertyInfo<T>, IEquatable<PropertyInfo<T>>
    {
        private IPropertyInfoFactory _factory;

        internal PropertyInfo(IPropertyInfoFactory factory, string name)
        {
            _factory = factory;
            Guard.IsNotNull(name, "name");

            Name = name;
        }

        public Type PropertyType
        {
            get
            {
                return typeof(T);
            }
        }

        public string Name { get; private set; }

        public bool Equals(IPropertyInfo other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name && PropertyType == other.PropertyType;
        }

        public bool Equals(PropertyInfo<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.PropertyType, PropertyType) && Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PropertyInfo<T>)) return false;
            return Equals((PropertyInfo<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PropertyType.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(PropertyInfo<T> left, PropertyInfo<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyInfo<T> left, PropertyInfo<T> right)
        {
            return !Equals(left, right);
        }

        bool IEquatable<PropertyInfo<T>>.Equals(PropertyInfo<T> other)
        {
            throw new NotImplementedException();
        }


        public bool Equals(IPropertyInfo<T> other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name
                && Factory.Equals(other.Factory);
        }

        public IValue<T> CreateValue(T value)
        {
            return Factory.ValueFactory.CreateValue(value);
        }

        public IValue<T> CreateValue(object value)
        {
            return (IValue<T>)Factory.ValueFactory.CreateValue(typeof(T), value);
        }

        public IPropertyInfoFactory Factory
        {
            get { return _factory; }
        }


        IValue IPropertyInfo.CreateValue(object value)
        {
            return CreateValue(value);
        }
    }
}