using System;

namespace NetTopologySuite.Data
{
    public interface IPropertyInfoFactory :IEquatable<IPropertyInfoFactory>
    {
        IValueFactory ValueFactory { get; }

        IPropertyInfo Create(Type propertyType, string name);
        IPropertyInfo<TProperty> Create<TProperty>(string name);
    }
}