using System;
using System.Collections.ObjectModel;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface ISchema : IEquatable<ISchema>
    {
        ReadOnlyCollection<IPropertyInfo> Properties { get; }
        IPropertyInfo IdProperty { get; }

        IRecordFactory RecordFactory { get; }

        int GetOrdinal(string propertyName);
        string GetName(int index);
        Type GetPropertyType(int index);

        IPropertyInfo Property(int index);
        IPropertyInfo Property(string name);

        bool HasIdProperty { get; }
    }
}