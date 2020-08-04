using NetTopologySuite.Index;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    class CountItemVisitor<T> : IItemVisitor<T>
    {
        public int Count { get; private set; }

        public void VisitItem(T item)
        {
            Count++;
        }
    }
}
