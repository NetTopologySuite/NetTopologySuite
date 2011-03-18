using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IRecordSource : IEnumerable<IRecord>
    {
        ISchema Schema { get; }

        IMemoryRecordSet ToInMemorySet();
        IMemoryRecordSet ToInMemorySet(Expression<Func<IRecord, bool>> predicate);
    }
}