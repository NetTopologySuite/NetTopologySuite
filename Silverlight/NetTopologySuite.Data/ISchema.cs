using System;
using System.Collections.ObjectModel;

namespace NetTopologySuite.Data
{
    public interface ISchema : IEquatable<ISchema>
    {

        IPropertyInfo this[int index] { get; }
        IPropertyInfo this[string propertyName] { get; }
        ISchemaFactory SchemaFactory { get; }

        ReadOnlyCollection<IPropertyInfo> Properties { get; }
        IPropertyInfo IdProperty { get; }

        IRecordFactory RecordFactory { get; }

        int GetOrdinal(string propertyName);
        string GetName(int index);
        Type GetPropertyType(int index);

        IPropertyInfo Property(int index);
        IPropertyInfo Property(string name);

        bool HasIdProperty { get; }
        bool ContainsProperty(IPropertyInfo propertyInfo);
    }
}