using System.Collections;
using System.Collections.Generic;

// In .NET 2.0+, we can at least use SortedDictionary<TKey, TValue>
#if NET20

namespace NetTopologySuite.Utilities
{
    // In .NET 4.0, we can use SortedSet<T>, just AddMany / UnionWith
    #if NET40

    public static class SortedSetExtensions
    {
        public static void AddMany<T>(this SortedSet<T> set, IEnumerable<T> items)
        {
            set.UnionWith(items);
        }
    }

    #else

    // Implement SortedSet<T> using SortedDictionary<TKey, TValue>.
    // Interesting bit of trivia: .NET 4.0+ does the exact same thing,
    // but the other way around.
    public sealed class SortedSet<T> : ICollection<T>
    {
        private readonly SortedDictionary<T, bool> dict = new SortedDictionary<T, bool>();

        public SortedSet()
        {
        }

        public SortedSet(IEnumerable<T> items)
        {
            this.AddMany(items);
        }

        public int Count
        {
            get { return this.dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T item)
        {
            this.dict[item] = true;
        }

        public bool Add(T item)
        {
            bool alreadyPresent = this.dict.ContainsKey(item);
            if (alreadyPresent)
                return false;

            this.dict.Add(item, true);
            return true;
        }

        public bool Contains(T item)
        {
            return this.dict.ContainsKey(item);
        }

        public void AddMany(IEnumerable<T> items)
        {
            foreach (T item in items)
                this.dict[item] = true;
        }

        public void CopyTo(T[] array, int index)
        {
            this.dict.Keys.CopyTo(array, index);
        }

        public void Clear()
        {
            this.dict.Clear();
        }

        public bool Remove(T item)
        {
            return this.dict.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.dict.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    #endif // NET40
}

#endif // NET20
