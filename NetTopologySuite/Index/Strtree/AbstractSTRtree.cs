using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Base class for STRtree and SIRtree. 
    /// <remarks>
    /// <para>
    /// STR-packed R-trees are described in:
    /// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </para>
    /// <para>
    /// This implementation is based on Boundables rather than just AbstractNodes,
    /// because the STR algorithm operates on both nodes and
    /// data, both of which are treated here as Boundables.
    /// </para>
    /// </remarks>
    /// </summary>
    public abstract class AbstractStrTree
    {
        /// <returns>
        /// A test for intersection between two bounds, necessary because subclasses
        /// of AbstractStrTree have different implementations of bounds.
        /// </returns>
        protected interface IIntersectsOp
        {
            /// <summary>
            /// For STRtrees, the bounds will be Envelopes; 
            /// for SIRtrees, Intervals;
            /// for other subclasses of AbstractStrTree, some other class.
            /// </summary>
            /// <param name="aBounds">The bounds of one spatial object.</param>
            /// <param name="bBounds">The bounds of another spatial object.</param>                        
            /// <returns>Whether the two bounds intersect.</returns>
            Boolean Intersects(object aBounds, object bBounds);
        }

        protected AbstractNode root;

        private Boolean built = false;
        private ArrayList itemBoundables = new ArrayList();
        private Int32 nodeCapacity;

        /// <summary> 
        /// Constructs an AbstractStrTree with the specified maximum number of child
        /// nodes that a node may have.
        /// </summary>
        public AbstractStrTree(Int32 nodeCapacity)
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
            root = (itemBoundables.Count == 0)
                       ? CreateNode(0)
                       : CreateHigherLevels(itemBoundables, -1);
            built = true;
        }

        protected abstract AbstractNode CreateNode(Int32 level);

        /// <summary>
        /// Sorts the childBoundables then divides them into groups of size M, where
        /// M is the node capacity.
        /// </summary>
        protected virtual IList CreateParentBoundables(IList childBoundables, Int32 newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);
            ArrayList parentBoundables = new ArrayList();
            parentBoundables.Add(CreateNode(newLevel));
            ArrayList sortedChildBoundables = new ArrayList(childBoundables);
            sortedChildBoundables.Sort(GetComparer());

            for (IEnumerator i = sortedChildBoundables.GetEnumerator(); i.MoveNext();)
            {
                IBoundable childBoundable = (IBoundable) i.Current;

                if (LastNode(parentBoundables).ChildBoundables.Count == NodeCapacity)
                {
                    parentBoundables.Add(CreateNode(newLevel));
                }

                LastNode(parentBoundables).AddChildBoundable(childBoundable);
            }

            return parentBoundables;
        }

        protected AbstractNode LastNode(IList nodes)
        {
            return (AbstractNode) nodes[nodes.Count - 1];
        }

        protected Int32 CompareDoubles(Double a, Double b)
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
        private AbstractNode CreateHigherLevels(IList boundablesOfALevel, Int32 level)
        {
            Assert.IsTrue(boundablesOfALevel.Count != 0);
            IList parentBoundables = CreateParentBoundables(boundablesOfALevel, level + 1);
           
            if (parentBoundables.Count == 1)
            {
                return (AbstractNode) parentBoundables[0];
            }

            return CreateHigherLevels(parentBoundables, level + 1);
        }

        protected AbstractNode Root
        {
            get { return root; }
        }

        /// <summary> 
        /// Returns the maximum number of child nodes that a node may have.
        /// </summary>
        public Int32 NodeCapacity
        {
            get { return nodeCapacity; }
        }

        public Int32 Count
        {
            get
            {
                if (!built)
                {
                    Build();
                }

                if (itemBoundables.Count == 0)
                {
                    return 0;
                }

                return GetSize(root);
            }
        }

        protected Int32 GetSize(AbstractNode node)
        {
            Int32 size = 0;

            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext();)
            {
                IBoundable childBoundable = (IBoundable) i.Current;

                if (childBoundable is AbstractNode)
                {
                    size += GetSize((AbstractNode) childBoundable);
                }
                else if (childBoundable is ItemBoundable)
                {
                    size += 1;
                }
            }

            return size;
        }

        public Int32 Depth
        {
            get
            {
                if (!built)
                {
                    Build();
                }

                if (itemBoundables.Count == 0)
                {
                    return 0;
                }

                return GetDepth(root);
            }
        }

        protected Int32 GetDepth(AbstractNode node)
        {
            Int32 maxChildDepth = 0;

            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext();)
            {
                IBoundable childBoundable = (IBoundable) i.Current;

                if (childBoundable is AbstractNode)
                {
                    Int32 childDepth = GetDepth((AbstractNode) childBoundable);

                    if (childDepth > maxChildDepth)
                    {
                        maxChildDepth = childDepth;
                    }
                }
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
        protected IList Query(object searchBounds)
        {
            if (!built)
            {
                Build();
            }
          
            ArrayList matches = new ArrayList();
          
            if (itemBoundables.Count == 0)
            {
                Assert.IsTrue(root.Bounds == null);
                return matches;
            }
           
            if (IntersectsOp.Intersects(root.Bounds, searchBounds))
            {
                Query(searchBounds, root, matches);
            }
          
            return matches;
        }

        /// <summary>
        /// Also builds the tree, if necessary.
        /// </summary>
        protected void Query(Object searchBounds, IItemVisitor visitor)
        {
            if (!built)
            {
                Build();
            }

            if (itemBoundables.Count == 0)
            {
                Assert.IsTrue(root.Bounds == null);
            }

            if (IntersectsOp.Intersects(root.Bounds, searchBounds))
            {
                Query(searchBounds, root, visitor);
            }
        }

        private void Query(object searchBounds, AbstractNode node, IList matches)
        {
            foreach (object obj in node.ChildBoundables)
            {
                IBoundable childBoundable = (IBoundable) obj;
              
                if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
                {
                    continue;
                }

                if (childBoundable is AbstractNode)
                {
                    Query(searchBounds, (AbstractNode) childBoundable, matches);
                }
                else if (childBoundable is ItemBoundable)
                {
                    matches.Add(((ItemBoundable) childBoundable).Item);
                }
                else
                {
                    Assert.ShouldNeverReachHere();
                }
            }
        }

        private void Query(object searchBounds, AbstractNode node, IItemVisitor visitor)
        {
            foreach (object obj in node.ChildBoundables)
            {
                IBoundable childBoundable = (IBoundable) obj;
               
                if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
                {
                    continue;
                }
               
                if (childBoundable is AbstractNode)
                {
                    Query(searchBounds, (AbstractNode) childBoundable, visitor);
                }
                else if (childBoundable is ItemBoundable)
                {
                    visitor.VisitItem(((ItemBoundable) childBoundable).Item);
                }
                else
                {
                    Assert.ShouldNeverReachHere();
                }
            }
        }

        /// <returns>
        /// A test for intersection between two bounds, necessary because subclasses
        /// of AbstractStrTree have different implementations of bounds.
        /// </returns>
        protected abstract IIntersectsOp IntersectsOp { get; }

        /// <summary>
        /// Also builds the tree, if necessary.
        /// </summary>
        protected Boolean Remove(object searchBounds, object item)
        {
            if (!built)
            {
                Build();
            }
           
            if (itemBoundables.Count == 0)
            {
                Assert.IsTrue(root.Bounds == null);
            }
            
            if (IntersectsOp.Intersects(root.Bounds, searchBounds))
            {
                return Remove(searchBounds, root, item);
            }
           
            return false;
        }

        private Boolean RemoveItem(AbstractNode node, object item)
        {
            IBoundable childToRemove = null;
            
            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext();)
            {
                IBoundable childBoundable = (IBoundable) i.Current;
                if (childBoundable is ItemBoundable)
                {
                    if (((ItemBoundable) childBoundable).Item == item)
                    {
                        childToRemove = childBoundable;
                    }
                }
            }

            if (childToRemove != null)
            {
                node.ChildBoundables.Remove(childToRemove);
                return true;
            }

            return false;
        }

        private Boolean Remove(object searchBounds, AbstractNode node, object item)
        {
            // first try removing item from this node
            Boolean found = RemoveItem(node, item);

            if (found)
            {
                return true;
            }

            AbstractNode childToPrune = null;

            // next try removing item from lower nodes
            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext();)
            {
                IBoundable childBoundable = (IBoundable) i.Current;
                if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
                {
                    continue;
                }

                if (childBoundable is AbstractNode)
                {
                    found = Remove(searchBounds, (AbstractNode) childBoundable, item);
                    // if found, record child for pruning and exit
                    if (found)
                    {
                        childToPrune = (AbstractNode) childBoundable;
                        break;
                    }
                }
            }

            // prune child if possible
            if (childToPrune != null)
            {
                if (childToPrune.ChildBoundables.Count == 0)
                {
                    node.ChildBoundables.Remove(childToPrune);
                }
            }

            return found;
        }

        protected IList BoundablesAtLevel(Int32 level)
        {
            IList boundables = new ArrayList();
            BoundablesAtLevel(level, root, ref boundables);
            return boundables;
        }

        /// <param name="level">-1 to get items.</param>
        private void BoundablesAtLevel(Int32 level, AbstractNode top, ref IList boundables)
        {
            Assert.IsTrue(level > -2);
            
            if (top.Level == level)
            {
                boundables.Add(top);
                return;
            }

            for (IEnumerator i = top.ChildBoundables.GetEnumerator(); i.MoveNext();)
            {
                IBoundable boundable = (IBoundable) i.Current;
                
                if (boundable is AbstractNode)
                {
                    BoundablesAtLevel(level, (AbstractNode) boundable, ref boundables);
                }
                else
                {
                    Assert.IsTrue(boundable is ItemBoundable);
                    
                    if (level == -1)
                    {
                        boundables.Add(boundable);
                    }
                }
            }

            return;
        }

        protected abstract IComparer GetComparer();
    }
}