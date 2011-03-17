using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GisSharpBlog.NetTopologySuite.Data
{
    internal class ValueFactory : IValueFactory
    {
        private static readonly Dictionary<Type, Func<object, IValue>> _innerFactories =
            new Dictionary<Type, Func<object, IValue>>();

        #region IValueFactory Members

        public IValue CreateValue(Type targetType, object value)
        {
            if (!_innerFactories.ContainsKey(targetType))
            {
                lock (_innerFactories)
                {
                    if (!_innerFactories.ContainsKey(targetType))
                        _innerFactories.Add(targetType, CreateDelegate(targetType));
                }
            }

            return _innerFactories[targetType](value);
        }

        public IValue<T> CreateValue<T>(object value)
        {
            return CreateValue((T) Convert.ChangeType(value, typeof (T), CultureInfo.InvariantCulture));
        }

        public IValue<T> CreateValue<T>(T value)
        {
            return new Value<T>(value);
        }

        #endregion

        private Func<object, IValue> CreateDelegate(Type targetType)
        {
            MethodInfo mif =
                typeof (ValueFactory).GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public)
                    .Where(
                        a =>
                        a.Name == "CreateValue"
                        && a.GetGenericArguments().Length == 1
                        && a.GetParameters()[0].ParameterType == typeof (object)).FirstOrDefault();

            if (mif == null)
                throw new MissingMemberException();

            mif = mif.MakeGenericMethod(targetType);

            ParameterExpression pex = Expression.Parameter(typeof (object));
            ConstantExpression instance = Expression.Constant(this, typeof (ValueFactory));

            return Expression.Lambda<Func<object, IValue>>(Expression.Call(instance, mif, pex), pex).Compile();
        }
    }
}