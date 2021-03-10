using System;
using System.Collections.Generic;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Flatbush
{
    public sealed class FlatbushSpatialIndex<T> : ISpatialIndex<T>
    {
        private readonly Flatbush _flatbush;

        private readonly T[] _items;

        private bool _built;

        public FlatbushSpatialIndex(int numItems)
            : this(numItems, 16)
        {
        }

        public FlatbushSpatialIndex(int numItems, int nodeSize)
        {
            if (numItems <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numItems), numItems, "Must be positive.");
            }

            if (nodeSize < 2 || nodeSize >= 65536)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeSize), nodeSize, "Must be between 2 (inclusive) and 65536 (exclusive).");
            }

            _flatbush = new Flatbush(numItems, nodeSize);
            _items = new T[numItems];
        }

        public void Insert(Envelope itemEnv, T item)
        {
            if (itemEnv is null)
            {
                throw new ArgumentNullException(nameof(itemEnv));
            }

            lock (_flatbush)
            {
                EnsureNotBuilt();
                _items[_flatbush.Add(itemEnv.MinX, itemEnv.MinY, itemEnv.MaxX, itemEnv.MaxY)] = item;
            }
        }

        public IList<T> Query(Envelope searchEnv)
        {
            if (searchEnv is null)
            {
                throw new ArgumentNullException(nameof(searchEnv));
            }

            Build();

            var items = _items;
            var result = new List<T>();

            // ReSharper disable once InconsistentlySynchronizedField
            _flatbush.Search(searchEnv.MinX, searchEnv.MinY, searchEnv.MaxX, searchEnv.MaxY, Visit);
            return result;

            void Visit(int index)
            {
                result.Add(items[index]);
            }
        }

        public void Query(Envelope searchEnv, IItemVisitor<T> visitor)
        {
            if (searchEnv is null)
            {
                throw new ArgumentNullException(nameof(searchEnv));
            }

            if (visitor is null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            Build();
            var items = _items;

            // ReSharper disable once InconsistentlySynchronizedField
            _flatbush.Search(searchEnv.MinX, searchEnv.MinY, searchEnv.MaxX, searchEnv.MaxY, Visit);

            void Visit(int index)
            {
                visitor.VisitItem(items[index]);
            }
        }

        public void Build()
        {
            if (_built)
            {
                return;
            }

            lock (_flatbush)
            {
                if (_built)
                {
                    return;
                }

                _flatbush.Finish();
                _built = true;
            }
        }

        bool ISpatialIndex<T>.Remove(Envelope itemEnv, T item)
        {
            throw new NotSupportedException("Removing items is not supported at any time.");
        }

        private void EnsureNotBuilt()
        {
            if (_built)
            {
                throw new InvalidOperationException("The spatial index may not be modified after it has already been built.");
            }
        }
    }
}
