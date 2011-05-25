using System.Collections.Generic;

namespace NetTopologySuite.Data
{
    public interface IRecord : IEnumerable<KeyValuePair<IPropertyInfo, IValue>>
    {
        IValue this[int index] { get; set; }
        IValue this[string propertyName] { get; set; }
        IValue this[IPropertyInfo propertyInfo] { get; set; }
        ISchema Schema { get; }
        IValue GetValue(int index);
        IValue GetValue(string name);
        IValue GetValue(IPropertyInfo propertyInfo);
        void SetValue(int index, IValue value);
        void SetValue(string name, IValue value);
        void SetValue(IPropertyInfo propertyInfo, IValue value);
        void SetValue<T>(IPropertyInfo<T> propertyInfo, IValue<T> value);
        void SetValue(int index, object value);
        void SetValue(string name, object value);
        void SetValue(IPropertyInfo propertyInfo, object value);
        void SetValue<T>(IPropertyInfo<T> propertyInfo, T value);

        T GetValue<T>(int index);
        T GetValue<T>(string name);
        T GetValue<T>(IPropertyInfo propertyInfo);
        T GetId<T>();
        IValue GetId();

        bool IsUnset(int i);
    }
}