using System.Collections.Generic;
using NetTopologySuite.Data.InternalImpl;

namespace NetTopologySuite.Data
{
    internal class RecordFactory : IRecordFactory
    {

        internal RecordFactory(ISchema schema)
        {
            Schema = schema;
        }

        public ISchema Schema { get; private set; }

        public IRecord Create(IDictionary<IPropertyInfo, IValue> values)
        {
            return new Record(Schema, values);
        }
    }
}