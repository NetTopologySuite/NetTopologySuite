namespace System.Collections.Generic
{
    public interface IReadOnlyCollection<T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }
    }

    public interface IReadOnlyList<T> : IEnumerable<T>, IReadOnlyCollection<T>, IEnumerable
    {
        T this[int index] { get; }
    }
}