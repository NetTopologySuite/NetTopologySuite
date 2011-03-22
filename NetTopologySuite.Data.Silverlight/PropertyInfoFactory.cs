using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public class PropertyInfoFactory : IPropertyInfoFactory
    {
        private static readonly Dictionary<Type, Func<string, IPropertyInfo>> _innerFactories =
            new Dictionary<Type, Func<string, IPropertyInfo>>();

        public PropertyInfoFactory()
            : this(new ValueFactory())
        {
        }

        public PropertyInfoFactory(IValueFactory valueFactory)
        {
            ValueFactory = valueFactory;
        }

        #region IPropertyInfoFactory Members

        public IPropertyInfo Create(Type propertyType, string name)
        {
            return GetInnerFactory(propertyType)(name);
        }

        public IPropertyInfo<TProperty> Create<TProperty>(string name)
        {
            if (typeof(TProperty) == typeof(string))
                return (IPropertyInfo<TProperty>)new StringPropertyInfo(this, name);

            if (typeof(TProperty) == typeof(Decimal))
                return (IPropertyInfo<TProperty>)new DecimalPropertyInfo(this, name);

            return new PropertyInfo<TProperty>(this, name);
        }

        public IValueFactory ValueFactory { get; private set; }

        #endregion

        private Func<string, IPropertyInfo> GetInnerFactory(Type propertyType)
        {
            Func<string, IPropertyInfo> factory;

            if (!_innerFactories.TryGetValue(propertyType, out factory))
            {
                lock (_innerFactories)
                {
                    factory = CreateFactory(propertyType);
                }
            }
            return factory;
        }

        private Func<string, IPropertyInfo> CreateFactory(Type propertyType)
        {
            ParameterExpression pex = Expression.Parameter(typeof(string));
            ConstantExpression self = Expression.Constant(this);

            MethodInfo memberInfo =
                typeof(PropertyInfoFactory)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                    .Where(a => a.Name == "Create" && a.IsGenericMethod)
                    .First()
                    .MakeGenericMethod(propertyType);

            return Expression.Lambda<Func<string, IPropertyInfo>>(
                Expression.Call(self, memberInfo, pex), pex).Compile();
        }

        public bool Equals(IPropertyInfoFactory other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other is PropertyInfoFactory)
                return true;

            return false;
        }
    }
}