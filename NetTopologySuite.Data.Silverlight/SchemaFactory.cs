using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Data.InternalImpl;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public class SchemaFactory : ISchemaFactory
    {
        public SchemaFactory(IPropertyInfoFactory propertyFactory)
        {
            PropertyFactory = propertyFactory;
        }

        public ISchema Create(IEnumerable<IPropertyInfo> properties, IPropertyInfo key)
        {
            return new Schema(this, properties, key);
        }

        public IPropertyInfoFactory PropertyFactory
        {
            get; private set;
        }
    }
}