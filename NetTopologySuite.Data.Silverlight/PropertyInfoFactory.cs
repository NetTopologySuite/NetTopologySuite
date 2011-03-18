using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public class PropertyInfoFactory : IPropertyInfoFactory
    {

        public PropertyInfoFactory()
            : this(new ValueFactory())
        { }

        public PropertyInfoFactory(IValueFactory valueFactory)
        {
            ValueFactory = valueFactory;
        }

        private static readonly Dictionary<Type, Func<string, IPropertyInfo>> _innerFactories =
            new Dictionary<Type, Func<string, IPropertyInfo>>();

        #region IPropertyInfoFactory Members

        public IPropertyInfo Create(Type propertyType, string name)
        {
            return GetInnerFactory(propertyType)(name);
        }

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

        public IPropertyInfo<TProperty> Create<TProperty>(string name)
        {
            return new PropertyInfo<TProperty>(this, name);
        }

        #endregion

        public IValueFactory ValueFactory
        {
            get;
            private set;
        }
    }
}