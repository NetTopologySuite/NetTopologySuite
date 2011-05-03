using System;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface ICustomConverter<in TSource, out TTarget> : ICustomConverter
    {
        TTarget Convert(TSource source);
    }

    public interface ICustomConverter
    {
        Type TargetType { get; }
        Type SourceType { get; }

        Object Convert(object source);
    }
}