using System;

namespace GisSharpBlog.NetTopologySuite.Data
{
    internal interface IValueFactory
    {
        IValue CreateValue(Type targetType, object value);
        IValue<T> CreateValue<T>(object value);
        IValue<T> CreateValue<T>(T value);
    }
}