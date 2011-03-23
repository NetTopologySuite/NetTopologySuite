using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GisSharpBlog.NetTopologySuite.Data.InternalImpl;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public class ValueFactory : IValueFactory
    {
        private static readonly Dictionary<Type, Func<object, IValue>> _innerFactories =
            new Dictionary<Type, Func<object, IValue>>();

        #region IValueFactory Members

        public IValue CreateValue(Type targetType, object value)
        {

            Func<object, IValue> fact;
            if (!_innerFactories.TryGetValue(targetType, out fact))
            {
                lock (_innerFactories)
                {
                    fact = CreateDelegate(targetType);
                    _innerFactories.Add(targetType, fact);
                }
            }
            return fact(value);
        }

        public IValue<T> CreateValue<T>(object value)
        {
            if (value is T)
                return CreateValue((T)value);

            Type valueType = value.GetType();

            ICustomConverter converter;

            if (CustomConverters.TryGetValue(Tuple.Create(valueType, typeof(T)), out converter))
                return CreateValue((T)converter.Convert(value));

            return CreateValue((T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture));
        }

        public IValue<T> CreateValue<T>(T value)
        {
            return new Value<T>(value);
        }

        #endregion

        private Func<object, IValue> CreateDelegate(Type targetType)
        {
            MethodInfo mif =
                typeof(ValueFactory).GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public)
                    .Where(
                        a =>
                        a.Name == "CreateValue"
                        && a.GetGenericArguments().Length == 1
                        && a.GetParameters()[0].ParameterType == typeof(object)).FirstOrDefault();

            if (mif == null)
                throw new MissingMemberException();

            mif = mif.MakeGenericMethod(targetType);

            ParameterExpression pex = Expression.Parameter(typeof(object));
            ConstantExpression instance = Expression.Constant(this, typeof(ValueFactory));

            return Expression.Lambda<Func<object, IValue>>(Expression.Call(instance, mif, pex), pex).Compile();
        }

        private readonly IDictionary<Tuple<Type, Type>, ICustomConverter> _customConverters
            = new Dictionary<Tuple<Type, Type>, ICustomConverter>();

        protected IDictionary<Tuple<Type, Type>, ICustomConverter> CustomConverters
        {
            get { return _customConverters; }
        }


        public bool HasConverter(Type from, Type to)
        {
            return _customConverters.ContainsKey(Tuple.Create(from, to));
        }

        public void AddConverter(ICustomConverter converter)
        {
            _customConverters.Add(Tuple.Create(converter.SourceType, converter.TargetType), converter);
        }
    }
}