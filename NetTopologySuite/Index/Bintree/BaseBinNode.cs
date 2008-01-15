using System;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary> 
    /// The base class for nodes in a <see cref="BinTree{TItem}"/>.
    /// </summary>
    public abstract class BaseBinNode<TItem> : AbstractNode<Interval, TItem>
        where TItem : IBoundable<Interval>
    {
        /// <summary> 
        /// Returns the index of the subnode that wholely contains the given interval.
        /// If none does, returns -1.
        /// Subnodes are numbered as follows:
        ///     0 | 1
        /// </summary>
        public static Int32 GetSubNodeIndex(Interval interval, Double center)
        {
            Int32 subnodeIndex = -1;

            if (interval.Min >= center)
            {
                subnodeIndex = 1;
            }

            if (interval.Max <= center)
            {
                subnodeIndex = 0;
            }

            return subnodeIndex;
        }

        private Node<TItem> _subNode1;
        private Node<TItem> _subNode2;

        public BaseBinNode(Interval bounds, Int32 level) : base(bounds, level)
        {
            
        }

        //public IList AddAllItemsFromOverlapping(Interval interval, IList resultItems)
        //{
        //    if (!IsSearchMatch(interval))
        //    {
        //        return _items;
        //    }

        //    // resultItems.addAll(items);
        //    foreach (object o in _items)
        //    {
        //        resultItems.Add(o);
        //    }

        //    for (Int32 i = 0; i < 2; i++)
        //    {
        //        if (_subNode[i] != null)
        //        {
        //            _subNode[i].AddAllItemsFromOverlapping(interval, resultItems);
        //        }
        //    }

        //    return _items;
        //}

        //public IEnumerable<TItem> Query(Interval interval)
        //{
        //    if (IsSearchMatch(interval))
        //    {
        //        foreach (TItem item in _items)
        //        {
        //            yield return item;
        //        }
        //    }

        //    if (_subNode1 != null)
        //    {
        //        foreach (TItem item in _subNode1.Query(interval))
        //        {
        //            yield return item;
        //        }
        //    }

        //    if (_subNode2 != null)
        //    {
        //        foreach (TItem item in _subNode2.Query(interval))
        //        {
        //            yield return item;
        //        }
        //    }
        //}

        public Int32 Depth
        {
            get
            {
                Int32 maxSubDepth = 0;

                if (_subNode1 != null)
                {
                    if (_subNode1.Depth > maxSubDepth)
                    {
                        maxSubDepth = _subNode1.Depth;
                    }
                }

                if (_subNode2 != null)
                {
                    if (_subNode2.Depth > maxSubDepth)
                    {
                        maxSubDepth = _subNode2.Depth;
                    }
                }

                return maxSubDepth + 1;
            }
        }

        //public Int32 Count
        //{
        //    get
        //    {
        //        Int32 subSize = 0;

        //        if (_subNode1 != null)
        //        {
        //            subSize += _subNode1.Count;
        //        }

        //        if (_subNode2 != null)
        //        {
        //            subSize += _subNode2.Count;
        //        }

        //        return subSize + ItemsInternal.Count;
        //    }
        //}

        public Int32 NodeCount
        {
            get
            {
                Int32 subCount = 0;

                if (_subNode1 != null)
                {
                    subCount += _subNode1.NodeCount;
                }

                if (_subNode2 != null)
                {
                    subCount += _subNode2.NodeCount;
                }

                return subCount + 1;
            }
        }

        protected Node<TItem> SubNode1
        {
            get { return _subNode1; }
            set { _subNode1 = value; }
        }

        protected Node<TItem> SubNode2
        {
            get { return _subNode2;}
            set { _subNode2 = value; }
        }

        protected Node<TItem> CreateSubNode(Int32 index)
        {
            // create a new subnode in the appropriate interval
            Double min = 0.0;
            Double max = 0.0;

            switch (index)
            {
                case 0:
                    min = Bounds.Min;
                    max = Bounds.Center;
                    break;
                case 1:
                    min = Bounds.Center;
                    max = Bounds.Max;
                    break;
                default:
                    break;
            }

            Interval subInt = new Interval(min, max);
            Node<TItem> node = new Node<TItem>(subInt, Level - 1);
            return node;
        }

        /// <summary>
        /// Get the subnode for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        protected Node<TItem> GetSubNode(Interval interval, Double center)
        {
            Int32 index = GetSubNodeIndex(interval, center);

            if (index < 0)
            {
                return null;
            }
            else if (index == 0)
            {
                if (SubNode1 == null)
                {
                    SubNode1 = CreateSubNode(0);
                }

                return SubNode1;
            }
            else
            {
                if (SubNode2 == null)
                {
                    SubNode2 = CreateSubNode(1);
                }

                return SubNode2;
            }
        }

        /// <summary>
        /// Get the subnode for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        protected Node<TItem> GetSubNode(Int32 index)
        {
            if (index == 0)
            {
                if (SubNode1 == null)
                {
                    SubNode1 = CreateSubNode(0);
                }

                return SubNode1;
            }
            else
            {
                if (SubNode2 == null)
                {
                    SubNode2 = CreateSubNode(1);
                }

                return SubNode2;
            }
        }

        protected void SetSubNode(Int32 index, Node<TItem> node)
        {
            if (index == 0)
            {
                SubNode1 = node;
            }
            else
            {
                SubNode2 = node;
            }
        }
    }
}