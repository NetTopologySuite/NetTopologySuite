using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Base class for STRtree and SIRtree. STR-packed R-trees are described in:
    /// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// <para>
    /// This implementation is based on Boundables rather than just AbstractNodes,
    /// because the STR algorithm operates on both nodes and
    /// data, both of which are treated here as Boundables.
    /// </para>
    /// </summary>
    public abstract class AbstractSTRtree
    {
        /// <returns>
        /// A test for intersection between two bounds, necessary because subclasses
        /// of AbstractSTRtree have different implementations of bounds.
        /// </returns>
        protected interface IIntersectsOp
        {
            /// <summary>
            /// For STRtrees, the bounds will be Envelopes; 
            /// for SIRtrees, Intervals;
            /// for other subclasses of AbstractSTRtree, some other class.
            /// </summary>
            /// <param name="aBounds">The bounds of one spatial object.</param>
            /// <param name="bBounds">The bounds of another spatial object.</param>                        
            /// <returns>Whether the two bounds intersect.</returns>
            bool Intersects(object aBounds, object bBounds);
        }

        /// <summary>
        /// 
        /// </summary>
        protected AbstractNode root;

        private bool built;
        private readonly ArrayList itemBoundables = new ArrayList();
        private readonly int nodeCapacity;

        /// <summary> 
        /// Constructs an AbstractSTRtree with the specified maximum number of child
        /// nodes that a node may have.
        /// </summary>
        /// <param name="nodeCapacity"></param>
        protected AbstractSTRtree(int nodeCapacity)
        {
            Assert.IsTrue(nodeCapacity > 1, "Node capacity must be greater than 1");
            this.nodeCapacity = nodeCapacity;
        }

        /// <summary> 
        /// Creates parent nodes, grandparent nodes, and so forth up to the root
        /// node, for the data that has been inserted into the tree. Can only be
        /// called once, and thus can be called only after all of the data has been
        /// inserted into the tree.
        /// </summary>
        public void Build()
        {
            Assert.IsTrue(!built);
            root = (itemBoundables.Count == 0) ?
                CreateNode(0) : CreateHigherLevels(itemBoundables, -1);
            built = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected abstract AbstractNode CreateNode(int level);

        /// <summary>
        /// Sorts the childBoundables then divides them into groups of size M, where
        /// M is the node capacity.
        /// </summary>
        /// <param name="childBoundables"></param>
        /// <param name="newLevel"></param>
        protected virtual IList CreateParentBoundables(IList childBoundables, int newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);
            var parentBoundables = new ArrayList();
            parentBoundables.Add(CreateNode(newLevel));
            var sortedChildBoundables = new ArrayList(childBoundables);
            sortedChildBoundables.Sort(GetComparer());
            for (var i = sortedChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                var childBoundable = (IBoundable)i.Current;
                if (LastNode(parentBoundables).ChildBoundables.Count == NodeCapacity)
                    parentBoundables.Add(CreateNode(newLevel));
                LastNode(parentBoundables).AddChildBoundable(childBoundable);
            }
            return parentBoundables;
        }

        protected AbstractNode LastNode(IList nodes)
        {
            return (AbstractNode)nodes[nodes.Count - 1];
        }

        protected int CompareDoubles(double a, double b)
        {
            return a > b ? 1 : a < b ? -1 : 0;
        }

        /// <summary>
        /// Creates the levels higher than the given level.
        /// </summary>
        /// <param name="boundablesOfALevel">The level to build on.</param>
        /// <param name="level">the level of the Boundables, or -1 if the boundables are item
        /// boundables (that is, below level 0).</param>
        /// <returns>The root, which may be a ParentNode or a LeafNode.</returns>
        private AbstractNode CreateHigherLevels(IList boundablesOfALevel, int level)
        {
            Assert.IsTrue(boundablesOfALevel.Count != 0);
            var parentBoundables = CreateParentBoundables(boundablesOfALevel, level + 1);
            if (parentBoundables.Count == 1)
                return (AbstractNode)parentBoundables[0];
            return CreateHigherLevels(parentBoundables, level + 1);
        }

        protected AbstractNode Root
        {
            get { return root; }
        }

        /// <summary> 
        /// Returns the maximum number of child nodes that a node may have.
        /// </summary>
        public int NodeCapacity
        {
            get { return nodeCapacity; }
        }

        public int Count
        {
            get
            {
                if (!built)
                    Build();
                if (itemBoundables.Count == 0)
                    return 0;
                return GetSize(root);
            }
        }

        protected int GetSize(AbstractNode node)
        {
            var size = 0;
            for (var i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                var childBoundable = (IBoundable)i.Current;
                if (childBoundable is AbstractNode)
                    size += GetSize((AbstractNode)childBoundable);
                else if (childBoundable is ItemBoundable)
                    size += 1;
            }
            return size;
        }

        public int Depth
        {
            get
            {
                if (!built)
                    Build();
                return itemBoundables.Count == 0 ? 0 : GetDepth(root);
            }
        }

        protected int GetDepth(AbstractNode node)
        {
            var maxChildDepth = 0;
            for (var i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                var childBoundable = (IBoundable)i.Current;
                if (!(childBoundable is AbstractNode))
                    continue;
                var childDepth = GetDepth((AbstractNode)childBoundable);
                if (childDepth > maxChildDepth)
                    maxChildDepth = childDepth;
            }
            return maxChildDepth + 1;
        }

        protected void Insert(object bounds, object item)
        {
            Assert.IsTrue(!built, "Cannot insert items into an STR packed R-tree after it has been built.");
            itemBoundables.Add(new ItemBoundable(bounds, item));
        }

        /// <summary>
        /// Also builds the tree, if necessary.
        /// </summary>
        /// <param name="searchBounds"></param>
        protected IList Query(object searchBounds)
        {
            if (!built)
                Build();
            var matches = new ArrayList();
            if (itemBoundables.Count == 0)
            {
                Assert.IsTrue(root.Bounds == null);
                return matches;
            }
            if (IntersectsOp.Intersects(root.Bounds, searchBounds))
                Query(searchBounds, root, matches);
            return matches;
        }

        protected void Query(Object searchBounds, IItemVisitor visitor)
        {
            if (!built)
                Build();

            if (itemBoundables.Count == 0)
                Assert.IsTrue(root.Bounds == null);

            if (IntersectsOp.Intersects(root.Bounds, searchBounds))
                Query(searchBounds, root, visitor);
        }

        private void Query(object searchBounds, AbstractNode node, IList matches)
        {
            foreach (var obj in node.ChildBoundables)
            {
                var childBoundable = (IBoundable)obj;
                if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
                    continue;

                if (childBoundable is AbstractNode)
                    Query(searchBounds, (AbstractNode)childBoundable, matches);
                else if (childBoundable is ItemBoundable)
                    matches.Add(((ItemBoundable)childBoundable).Item);
                else Assert.ShouldNeverReachHere();
            }
        }

        private void Query(object searchBounds, AbstractNode node, IItemVisitor visitor)
        {
            foreach (var obj in node.ChildBoundables)
            {
                var childBoundable = (IBoundable)obj;
                if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
                    continue;
                if (childBoundable is AbstractNode)
                    Query(searchBounds, (AbstractNode)childBoundable, visitor);
                else if (childBoundable is ItemBoundable)
                    visitor.VisitItem(((ItemBoundable)childBoundable).Item);
                else Assert.ShouldNeverReachHere();
            }
        }

        /// <summary>
        /// Gets a tree structure (as a nested list) 
        /// corresponding to the structure of the items and nodes in this tree.
        /// The returned Lists contain either Object items, 
        /// or Lists which correspond to subtrees of the tree
        /// Subtrees which do not contain any items are not included.
        /// Builds the tree if necessary.
        /// </summary>
        public IList ItemsTree()
        {
            if (!built) { Build(); }

            var valuesTree = ItemsTree(root);
            return valuesTree ?? new ArrayList();
        }

        private IList ItemsTree(AbstractNode node)
        {
            var valuesTreeForNode = new ArrayList();

            foreach (IBoundable childBoundable in node.ChildBoundables)
            {
                if (childBoundable is AbstractNode)
                {
                    var valuesTreeForChild = ItemsTree((AbstractNode)childBoundable);
                    // only add if not null (which indicates an item somewhere in this tree
                    if (valuesTreeForChild != null)
                        valuesTreeForNode.Add(valuesTreeForChild);
                }
                else if (childBoundable is ItemBoundable)
                    valuesTreeForNode.Add(((ItemBoundable) childBoundable).Item);
                else Assert.ShouldNeverReachHere();
            }
            
            return valuesTreeForNode.Count <= 0 ? null : valuesTreeForNode;
        }


        /// <returns>
        /// A test for intersection between two bounds, necessary because subclasses
        /// of AbstractSTRtree have different implementations of bounds.
        /// </returns>
        protected abstract IIntersectsOp IntersectsOp { get; }

        protected bool Remove(object searchBounds, object item)
        {
            if (!built)
                Build();
            if (itemBoundables.Count == 0)
                Assert.IsTrue(root.Bounds == null);
            return IntersectsOp.Intersects(root.Bounds, searchBounds) && Remove(searchBounds, root, item);
        }

        private bool RemoveItem(AbstractNode node, object item)
        {
            IBoundable childToRemove = null;
            for (var i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                var childBoundable = (IBoundable)i.Current;
                if (childBoundable is ItemBoundable)
                    if (((ItemBoundable)childBoundable).Item == item)
                        childToRemove = childBoundable;
            }
            if (childToRemove != null)
            {
                node.ChildBoundables.Remove(childToRemove);
                return true;
            }
            return false;
        }

        private bool Remove(object searchBounds, AbstractNode node, object item)
        {
            // first try removing item from this node
            var found = RemoveItem(node, item);
            if (found)
                return true;
            AbstractNode childToPrune = null;
            // next try removing item from lower nodes
            for (var i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                var childBoundable = (IBoundable)i.Current;
                if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
                    continue;
                if (!(childBoundable is AbstractNode))
                    continue;
                found = Remove(searchBounds, (AbstractNode)childBoundable, item);
                // if found, record child for pruning and exit
                if (!found)
                    continue;
                childToPrune = (AbstractNode)childBoundable;
                break;
            }
            // prune child if possible
            if (childToPrune != null)
                if (childToPrune.ChildBoundables.Count == 0)
                    node.ChildBoundables.Remove(childToPrune);
            return found;
        }

        protected IList BoundablesAtLevel(int level)
        {
            IList boundables = new ArrayList();
            BoundablesAtLevel(level, root, ref boundables);
            return boundables;
        }

        private void BoundablesAtLevel(int level, AbstractNode top, ref IList boundables)
        {
            Assert.IsTrue(level > -2);
            if (top.Level == level)
            {
                boundables.Add(top);
                return;
            }
            for (var i = top.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                var boundable = (IBoundable)i.Current;
                if (boundable is AbstractNode)
                    BoundablesAtLevel(level, (AbstractNode)boundable, ref boundables);
                else
                {
                    Assert.IsTrue(boundable is ItemBoundable);
                    if (level == -1)
                        boundables.Add(boundable);
                }
            }
        }

        protected abstract IComparer GetComparer();
    }
}
