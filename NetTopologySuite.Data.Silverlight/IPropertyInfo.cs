using System;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IPropertyInfo : IEquatable<IPropertyInfo>
    {
        IPropertyInfoFactory Factory { get; }
        Type PropertyType { get; }
        String Name { get; }
    }

    public interface IPropertyInfo<TPropertyType>
        : IPropertyInfo, IEquatable<IPropertyInfo<TPropertyType>>
    {
        IValue<TPropertyType> CreateValue(TPropertyType value);
        IValue<TPropertyType> CreateValue(object value);
    }
}