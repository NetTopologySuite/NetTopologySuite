using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// A node of an <see cref="AbstractSTRtree{T, TItem}"/>. A node is one of:
    /// <list type="bullet">
    /// <item>empty</item>
    /// <item><description>an <i>interior node</i> containing child <see cref="AbstractNode{T, TItem}"/>s</description></item>
    /// <item><description>a <i>leaf node</i> containing data items (<see cref="ItemBoundable{T, TItem}"/>s).</description></item>
    /// </list>
    /// A node stores the bounds of its children, and its level within the index tree.
    /// </summary>
    [Serializable]
    public abstract class AbstractNode<T, TItem> : IBoundable<T, TItem> where T : IIntersectable<T>, IExpandable<T>
    {
        private IList<IBoundable<T, TItem>> _childBoundables = new List<IBoundable<T, TItem>>();
        private T _bounds;
        private readonly int _level;

        /// <summary>
        /// Constructs an AbstractNode at the given level in the tree
        /// </summary>
        /// <param name="level">
        /// 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </param>
        protected AbstractNode(int level)
        {
            _level = level;
        }

        /// <summary>
        /// Returns either child <see cref="AbstractNode{T, TItem}"/>s, or if this is a leaf node, real data (wrapped
        /// in <see cref="ItemBoundable{T, TItem}"/>s).
        /// </summary>
        public IList<IBoundable<T, TItem>> ChildBoundables
        {
            get { return _childBoundables; }
            set { _childBoundables = value; }
        }

        /// <summary>
        /// Returns a representation of space that encloses this Boundable,
        /// preferably not much bigger than this Boundable's boundary yet fast to
        /// test for intersection with the bounds of other Boundables. The class of
        /// object returned depends on the subclass of AbstractSTRtree.
        /// </summary>
        /// <returns>
        /// An Envelope (for STRtrees), an Interval (for SIRtrees), or other
        /// object (for other subclasses of AbstractSTRtree).
        /// </returns>
        protected abstract T ComputeBounds();

        /// <summary>
        /// Gets the bounds of this node
        /// </summary>
        public T Bounds
        {
            get
            {
                if (_bounds == null)
                {
                    _bounds = ComputeBounds();
                }
                return _bounds;
            }
        }

        public TItem Item => default(TItem);

        /// <summary>
        /// Returns 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </summary>
        public int Level => _level;

        /// <summary>
        /// Gets the count of the <see cref="IBoundable{T, TItem}"/>s at this node.
        /// </summary>
        public int Count => _childBoundables.Count;

        /// <summary>
        /// Tests whether there are any <see cref="IBoundable{T, TItem}"/>s at this node.
        /// </summary>
        public bool IsEmpty => _childBoundables.Count == 0;

        /// <summary>
        /// Adds either an AbstractNode, or if this is a leaf node, a data object
        /// (wrapped in an ItemBoundable).
        /// </summary>
        /// <param name="childBoundable">The child to add.</param>
        public void AddChildBoundable(IBoundable<T, TItem> childBoundable)
        {
            Assert.IsTrue(_bounds == null);
            _childBoundables.Add(childBoundable);
        }
    }
}
