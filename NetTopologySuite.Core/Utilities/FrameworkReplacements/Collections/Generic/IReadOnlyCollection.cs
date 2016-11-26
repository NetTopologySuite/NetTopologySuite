namespace System.Collections.Generic
{
    public partial interface IReadOnlyCollection<T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }
    }

    public partial interface IReadOnlyList<T> : IEnumerable<T>, IReadOnlyCollection<T>, IEnumerable
    {
        T this[int index] { get; }
    }

}