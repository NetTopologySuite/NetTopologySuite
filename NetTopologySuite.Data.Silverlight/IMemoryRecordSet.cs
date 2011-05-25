using System.Collections.Generic;

namespace NetTopologySuite.Data
{
    public interface IMemoryRecordSet : IList<IRecord>
    {
        ISchema Schema { get; }

    }
}