using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IRecord : IEnumerable<KeyValuePair<IPropertyInfo, IValue>>
    {
        IValue this[int index] { get; }
        IValue this[string propertyName] { get; }
        IValue this[IPropertyInfo propertyInfo] { get; }
        IValue Id { get; }
        ISchema Schema { get; }
        IValue GetValue(int index);
        IValue GetValue(string name);
        IValue<T> GetValue<T>(int index);
        IValue<T> GetValue<T>(string name);
        IValue GetValue(IPropertyInfo propertyInfo);
        IValue<T> GetValue<T>(IPropertyInfo propertyInfo);
    }
}