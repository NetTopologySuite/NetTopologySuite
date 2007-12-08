using System;
using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary> 
    /// A node of the STR tree. The children of this node are either more nodes
    /// (AbstractNodes) or real data (ItemBoundables). If this node contains real data
    /// (rather than nodes), then we say that this node is a "leaf node".
    /// </summary>
    public abstract class AbstractNode<TBounds> : IBoundable<TBounds>
    {
        private readonly List<IBoundable<TBounds>> _childBoundables = new List<IBoundable<TBounds>>();
        private TBounds bounds = default(TBounds);
        private readonly Int32 _level;

        /// <summary> 
        /// Constructs an AbstractNode at the given level in the tree
        /// </summary>
        /// <param name="level">
        /// 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </param>
        public AbstractNode(Int32 level)
        {
            _level = level;
        }

        /// <summary> 
        /// Returns either child AbstractNodes, or if this is a leaf node, real data (wrapped
        /// in ItemBoundables).
        /// </summary>
        public IList<IBoundable<TBounds>> ChildBoundables
        {
            get { return _childBoundables; }
        }

        /// <summary>
        /// Returns a representation of space that encloses this Boundable,
        /// preferably not much bigger than this Boundable's boundary yet fast to
        /// test for intersection with the bounds of other Boundables. The class of
        /// object returned depends on the subclass of AbstractStrTree.
        /// </summary>
        /// <returns> 
        /// An Envelope (for STRtrees), an Interval (for SIRtrees), or other
        /// object (for other subclasses of AbstractStrTree).
        /// </returns>        
        protected abstract TBounds ComputeBounds();

        public TBounds Bounds
        {
            get
            {
                if (Equals(bounds, default(TBounds)))
                {
                    bounds = ComputeBounds();
                }

                return bounds;
            }
        }

        /// <summary>
        /// Returns 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </summary>
        public Int32 Level
        {
            get { return _level; }
        }

        /// <summary>
        /// Adds either an AbstractNode, or if this is a leaf node, a data object
        /// (wrapped in an ItemBoundable).
        /// </summary>
        public void AddChildBoundable(IBoundable<TBounds> childBoundable)
        {
            Assert.IsTrue(Equals(bounds, default(TBounds)));
            _childBoundables.Add(childBoundable);
        }
    }
}