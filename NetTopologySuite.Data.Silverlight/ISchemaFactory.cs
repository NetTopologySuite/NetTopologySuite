using System.Collections.Generic;

namespace NetTopologySuite.Data
{
    public interface ISchemaFactory
    {
        IPropertyInfoFactory PropertyFactory { get; }
        ISchema Create(IEnumerable<IPropertyInfo> properties, IPropertyInfo key);
    }
}