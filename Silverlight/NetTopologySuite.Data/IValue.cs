using System;

namespace NetTopologySuite.Data
{
    public interface IValue
    {
        Type Type { get; }
        object Value { get; }
    }

    public interface IValue<out T> : IValue
    {
        new T Value { get; }
    }
}