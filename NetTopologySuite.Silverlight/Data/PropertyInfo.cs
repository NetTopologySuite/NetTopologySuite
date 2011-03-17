using System;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Data
{
    internal class PropertyInfo : IPropertyInfo, IEquatable<PropertyInfo>
    {
        public PropertyInfo(string name, Type propType)
        {
            Guard.IsNotNull(name, "name");
            Guard.IsNotNull(propType, "propType");

            Name = name;
            PropertyType = propType;
        }

        public Type PropertyType { get; private set; }

        public string Name { get; private set; }

        public bool Equals(IPropertyInfo other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name && PropertyType == other.PropertyType;
        }

        public bool Equals(PropertyInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.PropertyType, PropertyType) && Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PropertyInfo)) return false;
            return Equals((PropertyInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PropertyType.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(PropertyInfo left, PropertyInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyInfo left, PropertyInfo right)
        {
            return !Equals(left, right);
        }
    }
}