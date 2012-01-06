using System;

namespace NetTopologySuite.Data
{
    public interface IPropertyInfo : IEquatable<IPropertyInfo>
    {
        IPropertyInfoFactory Factory { get; }
        Type PropertyType { get; }
        String Name { get; }
        IValue CreateValue(object value);
    }

    public interface IPropertyInfo<TPropertyType>
        : IPropertyInfo, IEquatable<IPropertyInfo<TPropertyType>>
    {
        IValue<TPropertyType> CreateValue(TPropertyType value);
        new IValue<TPropertyType> CreateValue(object value);
    }

    public interface IStringPropertyInfo : IPropertyInfo<String>
    {
        int MaxLength { get; set; }
    }


    public interface IDecimalPropertyInfo : IPropertyInfo<Decimal>
    {
        int Precision { get; set; }
    }
}