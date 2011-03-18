using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IRecord : IEnumerable<KeyValuePair<IPropertyInfo, IValue>>
    {
        IValue this[int index] { get; }
        IValue this[string propertyName] { get; }
        IValue this[IPropertyInfo propertyInfo] { get; }
        ISchema Schema { get; }
        IValue GetValue(int index);
        IValue GetValue(string name);
        IValue GetValue(IPropertyInfo propertyInfo);
        T GetValue<T>(int index);
        T GetValue<T>(string name);
        T GetValue<T>(IPropertyInfo propertyInfo);
        T GetId<T>();
        IValue GetId();
    }
}