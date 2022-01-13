using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// The base class for nodes in a <c>Quadtree</c>.
    /// </summary>
    [Serializable]
    public abstract class NodeBase<T>
    {
        /// <summary>
        /// Gets the index of the subquad that wholly contains the given envelope.
        /// If none does, returns -1.
        /// </summary>
        /// <returns>The index of the subquad that wholly contains the given envelope <br/>
        /// or -1 if no subquad wholly contains the envelope</returns>
        public static int GetSubnodeIndex(Envelope env, double centreX, double centreY)
        {
            int subnodeIndex = -1;
            if (env.MinX >= centreX)
            {
                if (env.MinY >= centreY)
                    subnodeIndex = 3;
                if (env.MaxY <= centreY)
                    subnodeIndex = 1;
            }
            if (env.MaxX <= centreX)
            {
                if (env.MinY >= centreY)
                    subnodeIndex = 2;
                if (env.MaxY <= centreY)
                    subnodeIndex = 0;
            }
            return subnodeIndex;
        }

        /// <summary>
        ///
        /// </summary>
        private SynchonizedList _items = new SynchonizedList();

        /// <summary>
        /// subquads are numbered as follows:
        /// 2 | 3
        /// --+--
        /// 0 | 1
        /// </summary>
        protected Node<T>[] Subnode = new Node<T>[4];

        /// <summary>
        ///
        /// </summary>
        public IList<T> Items
        {
            get => _items;
            protected set
            {
                var slValue = value as SynchonizedList;
                if (slValue == null && value != null)
                    slValue = new SynchonizedList(value);
                _items = slValue;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool HasItems
        {
            get
            {
                // return !items.IsEmpty;
                if (_items.Count == 0)
                    return false;
                return true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Removes a single item from this subtree.
        /// </summary>
        /// <param name="itemEnv">The envelope containing the item.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found and removed.</returns>
        public bool Remove(Envelope itemEnv, T item)
        {
            // use envelope to restrict nodes scanned
            if (!IsSearchMatch(itemEnv))
                return false;

            bool found = false;
            for (int i = 0; i < 4; i++)
            {
                if (Subnode[i] != null)
                {
                    found = Subnode[i].Remove(itemEnv, item);
                    if (found)
                    {
                        // trim subtree if empty
                        if (Subnode[i].IsPrunable)
                            Subnode[i] = null;
                        break;
                    }
                }
            }

            // if item was found lower down, don't need to search for it here
            if (found)
                return true;

            // otherwise, try and remove the item from the list of items in this node
            if(_items.Contains(item))
            {
                _items.Remove(item);
                found = true;
            }
            return found;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsPrunable => !(HasChildren || HasItems);

        /// <summary>
        ///
        /// </summary>
        public bool HasChildren
        {
            get
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Subnode[i] != null)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating that this node is empty, i.e. it does not contain an items or sub-nodes.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                bool isEmpty = true;
                if(_items.Count != 0)
                    isEmpty = false;
                else
                {
                    for (int i = 0; i < 4; i++)
                        if (Subnode[i] != null)
                            if (!Subnode[i].IsEmpty)
                                isEmpty = false;
                }

                return isEmpty;
            }
        }

        /// <summary>
        /// Insert items in <c>this</c> into the parameter!
        /// </summary>
        /// <param name="resultItems">IList for adding items.</param>
        /// <returns>Parameter IList with <c>this</c> items.</returns>
        public IList<T> AddAllItems(ref IList<T> resultItems)
        {
            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            // resultItems.addAll(this.items);
            foreach (var o in _items)
                resultItems.Add(o);
            for (int i = 0; i < 4; i++)
                if (Subnode[i] != null)
                    Subnode[i].AddAllItems(ref resultItems);
            return resultItems;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <returns></returns>
        protected abstract bool IsSearchMatch(Envelope searchEnv);

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="resultItems"></param>
        public void AddAllItemsFromOverlapping(Envelope searchEnv, ref IList<T> resultItems)
        {
            if (!IsSearchMatch(searchEnv))
                return;

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
                foreach (var o in _items)
                    resultItems.Add(o);
            for (int i = 0; i < 4; i++)
                if (Subnode[i] != null)
                    Subnode[i].AddAllItemsFromOverlapping(searchEnv, ref resultItems);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="visitor"></param>
        public void Visit(Envelope searchEnv, IItemVisitor<T> visitor)
        {
            if (!IsSearchMatch(searchEnv))
                return;

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            VisitItems(searchEnv, visitor);

            for (int i = 0; i < 4; i++)
                if (Subnode[i] != null)
                    Subnode[i].Visit(searchEnv, visitor);
        }

        /// <summary>
        ///
        /// </summary>  
        /// <param name="searchEnv"></param>
        /// <param name="visitor"></param>
        private void VisitItems(Envelope searchEnv, IItemVisitor<T> visitor)
        {
            // would be nice to filter items based on search envelope, but can't until they contain an envelope
            foreach (var item in _items)
                visitor.VisitItem(item);
        }

        public IEnumerable<T> Query(Envelope searchEnv, Func<T, bool> predicate)
        {
            if (!IsSearchMatch(searchEnv))
                yield break;

            foreach (var item in Items)
                if (predicate(item))
                    yield return item;

            for (int i = 0; i < 4; i++)
            {
                if (Subnode[i] == null) continue;
                foreach (var item in Subnode[i].Query(searchEnv, predicate))
                    yield return item;
            }
        }


        /// <summary>
        ///
        /// </summary>
        public int Depth
        {
            get
            {
                int maxSubDepth = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (Subnode[i] != null)
                    {
                        int sqd = Subnode[i].Depth;
                        if (sqd > maxSubDepth)
                            maxSubDepth = sqd;
                    }
                }
                return maxSubDepth + 1;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int Count
        {
            get
            {
                int subSize = 0;
                for (int i = 0; i < 4; i++)
                    if (Subnode[i] != null)
                        subSize += Subnode[i].Count;
                return subSize + _items.Count;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int NodeCount
        {
            get
            {
                int subSize = 0;
                for (int i = 0; i < 4; i++)
                    if (Subnode[i] != null)
                        subSize += Subnode[i].Count;
                return subSize + 1;
            }
        }

        [Serializable]
        private class SynchonizedList : IList<T>, ISerializable
        {
            private readonly IList<T> _items;

            [NonSerialized]
            private readonly object _syncRoot;

            public SynchonizedList()
                : this(new List<T>())
            { }

            public SynchonizedList(int capacity)
                : this(new List<T>(capacity))
            { }

            public SynchonizedList(IList<T> items)
            {
                _items = items;
                _syncRoot = new object();
            }

            internal SynchonizedList(SerializationInfo info, StreamingContext context)
            {
                string itemsTypeName = (string)info.GetValue("_itemsType", typeof(string));
                var itemsType = Type.GetType(itemsTypeName);
                _items = (IList<T>)info.GetValue("_items", itemsType);
                _syncRoot = new object();
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("_itemsType", _items.GetType().FullName);
                info.AddValue("_items", _items);
            }

            public T this[int index]
            {
                get
                {
                    lock (_syncRoot)
                        return _items[index];
                }
                set
                {
                    lock (_syncRoot)
                        _items[index] = value; }
            }

            public int Count { get { lock (_syncRoot) return _items.Count; } }

            public bool IsReadOnly
            {
                get
                {
                    lock(_syncRoot)
                        return _items.IsReadOnly;
                }
            }

            public void Add(T item)
            {
                lock (_syncRoot)
                    _items.Add(item);
            }

            public void Clear()
            {
                lock (_syncRoot)
                    _items.Clear();
            }

            public bool Contains(T item)
            {
                lock (_syncRoot)
                    return _items.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                lock (_syncRoot)
                    _items.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new SynchonizedEnumerator(_items, _syncRoot); ;
            }

            public int IndexOf(T item)
            {
                lock (_syncRoot)
                    return _items.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                lock (_syncRoot)
                    _items.Insert(index, item);
            }

            public bool Remove(T item)
            {
                lock (_syncRoot)
                    return _items.Remove(item);
            }

            public void RemoveAt(int index)
            {
                lock (_syncRoot)
                    _items.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public object SyncRoot { get { return _syncRoot; } }

            private class SynchonizedEnumerator : IEnumerator<T>
            {
                private int _index;
                private IList<T> _items;
                private object _syncRoot;

                public SynchonizedEnumerator(IList<T> items, object syncRoot)
                {
                    Monitor.Enter(syncRoot);
                    _syncRoot = syncRoot;
                    _items = items;
                    _index = -1;
                }

                public T Current
                {
                    get
                    {
                        if (_index < 0 || _index >= _items.Count)
                            return default;

                        return _items[_index];
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    Monitor.Exit(_syncRoot);
                }

                public bool MoveNext()
                {
                    _index++;
                    return _index < _items.Count;
                }

                public void Reset()
                {
                    _index = -1;
                }
            }
        }
    }
}
