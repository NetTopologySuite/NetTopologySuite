using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IMemoryRecordSet : IList<IRecord>
    {
        ISchema Schema { get; }

    }
}