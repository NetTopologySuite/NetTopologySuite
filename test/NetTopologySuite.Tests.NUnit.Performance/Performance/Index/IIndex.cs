using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    /// <summary>Adapter for different kinds of indexes</summary>
    public interface IIndex<T>
    {
        void Insert(Envelope itemEnv, T item);
        IList<T> Query(Envelope searchEnv);
        void FinishInserting();
    }
}
