using System.Collections;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary> 
    /// A node of the STR tree. The children of this node are either more nodes
    /// (AbstractNodes) or real data (ItemBoundables). If this node contains real data
    /// (rather than nodes), then we say that this node is a "leaf node".
    /// </summary>
    public abstract class AbstractNode : IBoundable 
    {
        private ArrayList childBoundables = new ArrayList();
        private object bounds = null;
        private int level;

        /// <summary> 
        /// Constructs an AbstractNode at the given level in the tree
        /// </summary>
        /// <param name="level">
        /// 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </param>
        public AbstractNode(int level) 
        {
            this.level = level;
        }

        /// <summary> 
        /// Returns either child AbstractNodes, or if this is a leaf node, real data (wrapped
        /// in ItemBoundables).
        /// </summary>
        public IList ChildBoundables
        {
            get
            {
                return childBoundables;
            }
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
        protected abstract object ComputeBounds();

        /// <summary>
        /// 
        /// </summary>
        public object Bounds
        {
            get
            {
                if (bounds == null)
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
        public int Level
        {
            get
            {
                return level;
            }
        }

        /// <summary>
        /// Adds either an AbstractNode, or if this is a leaf node, a data object
        /// (wrapped in an ItemBoundable).
        /// </summary>
        /// <param name="childBoundable"></param>
        public void AddChildBoundable(IBoundable childBoundable) 
        {
            Assert.IsTrue(bounds == null);
            childBoundables.Add(childBoundable);
        }
    }
}
