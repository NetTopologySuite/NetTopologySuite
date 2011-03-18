using System;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IPropertyInfoFactory
    {
        IValueFactory ValueFactory { get; }

        IPropertyInfo Create(Type propertyType, string name);
        IPropertyInfo<TProperty> Create<TProperty>(string name);
    }
}