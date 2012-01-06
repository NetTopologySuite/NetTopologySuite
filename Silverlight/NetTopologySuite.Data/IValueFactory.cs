using System;
using System.Collections.Generic;

namespace NetTopologySuite.Data
{
    public interface IValueFactory
    {
        IValue CreateValue(Type targetType, object value);
        IValue<T> CreateValue<T>(object value);
        IValue<T> CreateValue<T>(T value);
        bool HasConverter(Type from, Type to);
        void AddConverter(ICustomConverter converter);
    }
}