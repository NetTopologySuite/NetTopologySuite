using System;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IValueFactory
    {
        IValue CreateValue(Type targetType, object value);
        IValue<T> CreateValue<T>(object value);
        IValue<T> CreateValue<T>(T value);
        bool HasConverter(Type from, Type to);
        void AddConverter(ICustomConverter converter);
    }

    public interface ICustomConverter
    {
        Type TargetType { get; }
        Type SourceType { get; }

        Object Convert(object source);
    }

    public interface ICustomConverter<in TSource, out TTarget> :ICustomConverter
    {
        TTarget Convert(TSource source);
    }

}