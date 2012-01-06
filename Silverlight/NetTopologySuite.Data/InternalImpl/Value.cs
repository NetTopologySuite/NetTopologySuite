using System;

namespace NetTopologySuite.Data.InternalImpl
{
    internal class Value<T> : IValue<T>
    {
        private readonly T _value;
        internal Value(T value)
        {
            _value = value;
        }

        T IValue<T>.Value
        {
            get { return _value; }
        }

        object IValue.Value
        {
            get { return ((IValue<T>)this).Value; }
        }

        public Type Type
        {
            get { return typeof (T); }
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}