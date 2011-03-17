using System;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IPropertyInfo : IEquatable<IPropertyInfo>
    {
        Type PropertyType { get; }
        String Name { get; }
    }
}