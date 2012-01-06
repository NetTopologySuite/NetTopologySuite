using System.Collections.Generic;

namespace NetTopologySuite.Data
{
    public interface IRecordFactory
    {
        ISchema Schema { get; }

        IRecord Create(IDictionary<IPropertyInfo, IValue> values);
    }
}