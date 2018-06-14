using System;
using IList = System.Collections.Generic.IList<object>;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// A query-only R-tree created using the Sort-Tile-Recursive (STR) algorithm.
    /// For two-dimensional spatial data.
    /// The STR packed R-tree is simple to implement and maximizes space
    /// utilization; that is, as many leaves as possible are filled to capacity.
    /// Overlap between nodes is far less than in a basic R-tree. However, once the
    /// tree has been built (explicitly or on the first call to #query), items may
    /// not be added or removed.
    /// Described in: P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class STRtree<TItem> : AbstractSTRtree<Envelope, TItem>, ISpatialIndex<TItem>
    {
        private static readonly AnonymousXComparerImpl XComparer = new AnonymousXComparerImpl();
        private static readonly AnonymousYComparerImpl YComparer = new AnonymousYComparerImpl();

        private class AnonymousXComparerImpl : Comparer<IBoundable<Envelope, TItem>>
        {
            public override int Compare(IBoundable<Envelope, TItem> o1, IBoundable<Envelope, TItem> o2)
            {
                return CompareDoubles(CentreX(o1.Bounds),
                                      CentreX(o2.Bounds));
            }
        }

        private class AnonymousYComparerImpl : Comparer<IBoundable<Envelope, TItem>>
        {
            public override int Compare(IBoundable<Envelope, TItem> o1, IBoundable<Envelope, TItem> o2)
            {
                return CompareDoubles(CentreY(o1.Bounds),
                                      CentreY(o2.Bounds));
            }
        }

#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
        [Serializable]
#endif
        private class AnonymousAbstractNodeImpl : AbstractNode<Envelope, TItem>
        {
            public AnonymousAbstractNodeImpl(int nodeCapacity) :
                base(nodeCapacity)
            {
            }

            protected override Envelope ComputeBounds()
            {
                /*Envelope*/var bounds = new Envelope() /*= null*/;
                foreach (var childBoundable in ChildBoundables)
                {
                    /*
                    if (bounds == null)
                        bounds = new Envelope(childBoundable.Bounds);
                    else */
                        bounds.ExpandToInclude(childBoundable.Bounds);
                }
                //return bounds;
                return bounds.IsNull ? null : bounds;
            }
        }

        private static readonly IIntersectsOp IntersectsOperation = new AnonymousIntersectsOpImpl();
        private class AnonymousIntersectsOpImpl : IIntersectsOp
        {
            public bool Intersects(Envelope aBounds, Envelope bBounds)
            {
                return aBounds.Intersects(bBounds);
            }
        }

        private const int DefaultNodeCapacity = 10;

        /// <summary>
        /// Constructs an STRtree with the default (10) node capacity.
        /// </summary>
        public STRtree() : this(DefaultNodeCapacity)
        {
        }

        /// <summary>
        /// Constructs an STRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        /// <remarks>The minimum recommended capacity setting is 4.</remarks>
        public STRtree(int nodeCapacity) :
            base(nodeCapacity)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double Avg(double a, double b)
        {
            return (a + b)/2d;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double CentreX(Envelope e)
        {
            return Avg(e.MinX, e.MaxX);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double CentreY(Envelope e)
        {
            return Avg(e.MinY, e.MaxY);
        }

        /// <summary>
        /// Creates the parent level for the given child level. First, orders the items
        /// by the x-values of the midpoints, and groups them into vertical slices.
        /// For each slice, orders the items by the y-values of the midpoints, and
        /// group them into runs of size M (the node capacity). For each run, creates
        /// a new (parent) node.
        /// </summary>
        /// <param name="childBoundables"></param>
        /// <param name="newLevel"></param>
        protected override IList<IBoundable<Envelope, TItem>> CreateParentBoundables(IList<IBoundable<Envelope, TItem>> childBoundables, int newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);
            int minLeafCount = (int) Math.Ceiling((childBoundables.Count/(double) NodeCapacity));
            var sortedChildBoundables = new List<IBoundable<Envelope, TItem>>(childBoundables);
            sortedChildBoundables.Sort(XComparer);
            var verticalSlices = VerticalSlices(sortedChildBoundables,
                                                    (int) Math.Ceiling(Math.Sqrt(minLeafCount)));
            var tempList = CreateParentBoundablesFromVerticalSlices(verticalSlices, newLevel);
            return tempList;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="verticalSlices"></param>
        /// <param name="newLevel"></param>
        /// <returns></returns>
        private List<IBoundable<Envelope, TItem>> CreateParentBoundablesFromVerticalSlices(IList<IBoundable<Envelope, TItem>>[] verticalSlices, int newLevel)
        {
            Assert.IsTrue(verticalSlices.Length > 0);
            var parentBoundables = new List<IBoundable<Envelope, TItem>>();
            for (int i = 0; i < verticalSlices.Length; i++)
            {
                var tempList = CreateParentBoundablesFromVerticalSlice(verticalSlices[i], newLevel);
                foreach (var o in tempList)
                    parentBoundables.Add(o);
            }
            return parentBoundables;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="childBoundables"></param>
        /// <param name="newLevel"></param>
        /// <returns></returns>
        protected IList<IBoundable<Envelope, TItem>> CreateParentBoundablesFromVerticalSlice(IList<IBoundable<Envelope, TItem>> childBoundables, int newLevel)
        {
            return base.CreateParentBoundables(childBoundables, newLevel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="childBoundables">Must be sorted by the x-value of the envelope midpoints.</param>
        /// <param name="sliceCount"></param>
        protected IList<IBoundable<Envelope, TItem>>[] VerticalSlices(IList<IBoundable<Envelope, TItem>> childBoundables, int sliceCount)
        {
            int sliceCapacity = (int) Math.Ceiling(childBoundables.Count/(double) sliceCount);
            var slices = new IList<IBoundable<Envelope, TItem>>[sliceCount];
            var i = childBoundables.GetEnumerator();
            for (int j = 0; j < sliceCount; j++)
            {
                slices[j] = new List<IBoundable<Envelope, TItem>>();
                int boundablesAddedToSlice = 0;
                /*
                 *          Diego Guidi says:
                 *          the line below introduce an error:
                 *          the first element at the iteration (not the first) is lost!
                 *          This is simply a different implementation of Iteration in .NET against Java
                 */
                // while (i.MoveNext() && boundablesAddedToSlice < sliceCapacity)
                while (boundablesAddedToSlice < sliceCapacity && i.MoveNext())
                {
                    var childBoundable = i.Current;
                    slices[j].Add(childBoundable);
                    boundablesAddedToSlice++;
                }
            }
            return slices;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected override AbstractNode<Envelope, TItem> CreateNode(int level)
        {
            return new AnonymousAbstractNodeImpl(level);
        }

        /// <summary>
        ///
        /// </summary>
        protected override IIntersectsOp IntersectsOp => IntersectsOperation;

        /// <summary>
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="item"></param>
        public new void Insert(Envelope itemEnv, TItem item)
        {
            if (itemEnv.IsNull)
                return;
            base.Insert(itemEnv, item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        /// <param name="searchEnv"></param>
        public new IList<TItem> Query(Envelope searchEnv)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an object, not an Envelope. [Jon Aquino 10/24/2003]
            return base.Query(searchEnv);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="visitor"></param>
        public new void Query(Envelope searchEnv, IItemVisitor<TItem> visitor)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an Object, not an Envelope. [Jon Aquino 10/24/2003]
            base.Query(searchEnv, visitor);
        }

        /// <summary>
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemEnv">The Envelope of the item to remove.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found.</returns>
        public new bool Remove(Envelope itemEnv, TItem item)
        {
            return base.Remove(itemEnv, item);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override IComparer<IBoundable<Envelope, TItem>> GetComparer()
        {
            return YComparer;
        }

        /// <summary>
        /// Finds the two nearest items in the tree,
        /// using <see cref="IItemDistance{Envelope, TItem}"/> as the distance metric.
        /// A Branch-and-Bound tree traversal algorithm is used
        /// to provide an efficient search.
        /// </summary>
        /// <param name="itemDist">A distance metric applicable to the items in this tree</param>
        /// <returns>The pair of the nearest items</returns>
        public TItem[] NearestNeighbour(IItemDistance<Envelope, TItem> itemDist)
        {
            var bp = new BoundablePair<TItem>(Root, Root, itemDist);
            return NearestNeighbour(bp);
        }

        /// <summary>
        /// Finds k items in this tree which are the top k nearest neighbors to the given <c>item</c>,
        /// using <c>itemDist</c> as the distance metric.
        /// A Branch-and-Bound tree traversal algorithm is used
        /// to provide an efficient search.
        /// This method implements the KNN algorithm described in the following paper:
        /// <para/>
        /// Roussopoulos, Nick, Stephen Kelley, and Frédéric Vincent. "Nearest neighbor queries."
        /// ACM sigmod record. Vol. 24. No. 2. ACM, 1995.
        /// <para/>
        /// The query <c>item</c> does <b>not</b> have to be
        /// contained in the tree, but it does
        /// have to be compatible with the <c>itemDist</c>
        /// distance metric.
        /// </summary>
        /// <param name="env">The envelope of the query item</param>
        /// <param name="item">The item to find the nearest neighbour of</param>
        /// <param name="itemDist">A distance metric applicable to the items in this tree and the query item</param>
        /// <param name="k">The K nearest items in kNearestNeighbour</param>
        ///<returns>K nearest items in this tree</returns>
        public TItem[] NearestNeighbour(Envelope env, TItem item, IItemDistance<Envelope, TItem> itemDist, int k)
        {
            var bnd = new ItemBoundable<Envelope, TItem> (env, item);
            var bp = new BoundablePair<TItem>(Root, bnd, itemDist);
            return NearestNeighbour(bp, k);
        }

        /// <summary>
        /// Finds the item in this tree which is nearest to the given <paramref name="item"/>,
        /// using <see cref="IItemDistance{Envelope,TItem}"/> as the distance metric.
        /// A Branch-and-Bound tree traversal algorithm is used
        /// to provide an efficient search.
        /// <para/>
        /// The query <paramref name="item"/> does <b>not</b> have to be
        /// contained in the tree, but it does
        /// have to be compatible with the <paramref name="itemDist"/>
        /// distance metric.
        /// </summary>
        /// <param name="env">The envelope of the query item</param>
        /// <param name="item">The item to find the nearest neighbour of</param>
        /// <param name="itemDist">A distance metric applicable to the items in this tree and the query item</param>
        /// <returns>The nearest item in this tree</returns>
        public TItem NearestNeighbour(Envelope env, TItem item, IItemDistance<Envelope, TItem> itemDist)
        {
            var bnd = new ItemBoundable<Envelope, TItem>(env, item);
            var bp = new BoundablePair<TItem>(Root, bnd, itemDist);
            return NearestNeighbour(bp)[0];
        }

        /// <summary>
        /// Finds the two nearest items from this tree
        /// and another tree,
        /// using <see cref="IItemDistance{Envelope, TItem}"/> as the distance metric.
        /// A Branch-and-Bound tree traversal algorithm is used
        /// to provide an efficient search.
        /// The result value is a pair of items,
        /// the first from this tree and the second
        /// from the argument tree.
        /// </summary>
        /// <param name="tree">Another tree</param>
        /// <param name="itemDist">A distance metric applicable to the items in the trees</param>
        /// <returns>The pair of the nearest items, one from each tree</returns>
        public TItem[] NearestNeighbour(STRtree<TItem> tree, IItemDistance<Envelope, TItem> itemDist)
        {
            var bp = new BoundablePair<TItem>(Root, tree.Root, itemDist);
            return NearestNeighbour(bp);
        }

        private static TItem[] NearestNeighbour(BoundablePair<TItem> initBndPair)
        {
            return NearestNeighbour(initBndPair, double.PositiveInfinity);
        }

        private TItem[] NearestNeighbour(BoundablePair<TItem> initBndPair, int k)
        {
            return NearestNeighbour(initBndPair, double.PositiveInfinity, k);
        }

        private static TItem[] NearestNeighbour(BoundablePair<TItem> initBndPair, double maxDistance)
        {
            double distanceLowerBound = maxDistance;
            BoundablePair<TItem> minPair = null;

            // initialize internal structures
            var priQ = new PriorityQueue<BoundablePair<TItem>>();

            // initialize queue
            priQ.Add(initBndPair);

            while (!priQ.IsEmpty() && distanceLowerBound > 0.0)
            {
                // pop head of queue and expand one side of pair
                var bndPair = priQ.Poll();
                double currentDistance = bndPair.Distance; //bndPair.GetDistance();

                /**
                 * If the distance for the first node in the queue
                 * is >= the current minimum distance, all other nodes
                 * in the queue must also have a greater distance.
                 * So the current minDistance must be the true minimum,
                 * and we are done.
                 */
                if (currentDistance >= distanceLowerBound)
                    break;

                /**
                 * If the pair members are leaves
                 * then their distance is the exact lower bound.
                 * Update the distanceLowerBound to reflect this
                 * (which must be smaller, due to the test
                 * immediately prior to this).
                 */
                if (bndPair.IsLeaves)
                {
                    // assert: currentDistance < minimumDistanceFound
                    distanceLowerBound = currentDistance;
                    minPair = bndPair;
                }
                else
                {
                    // testing - does allowing a tolerance improve speed?
                    // Ans: by only about 10% - not enough to matter
                    /*
                    double maxDist = bndPair.getMaximumDistance();
                    if (maxDist * .99 < lastComputedDistance)
                      return;
                    //*/

                    /**
                     * Otherwise, expand one side of the pair,
                     * (the choice of which side to expand is heuristically determined)
                     * and insert the new expanded pairs into the queue
                     */
                    bndPair.ExpandToQueue(priQ, distanceLowerBound);
                }
            }
            if (minPair != null)
            // done - return items with min distance
            return new[]
                       {
                           ((ItemBoundable<Envelope, TItem>) minPair.GetBoundable(0)).Item,
                           ((ItemBoundable<Envelope, TItem>) minPair.GetBoundable(1)).Item
                       };
            return null;
        }

        private TItem[] NearestNeighbour(BoundablePair<TItem> initBndPair, double maxDistance, int k)
        {
            double distanceLowerBound = maxDistance;

            // initialize internal structures
            var priQ = new PriorityQueue<BoundablePair<TItem>>();

            // initialize queue
            priQ.Add(initBndPair);

            var kNearestNeighbors = new PriorityQueue<BoundablePair<TItem>>(k, new BoundablePairDistanceComparer<TItem>(false));

            while (!priQ.IsEmpty() && distanceLowerBound >= 0.0)
            {
                // pop head of queue and expand one side of pair
                var bndPair = priQ.Poll();
                double currentDistance = bndPair.Distance;

                /**
                 * If the distance for the first node in the queue
                 * is >= the current maximum distance in the k queue , all other nodes
                 * in the queue must also have a greater distance.
                 * So the current minDistance must be the true minimum,
                 * and we are done.
                 */

                if (currentDistance >= distanceLowerBound)
                {
                    break;
                }
                /**
                 * If the pair members are leaves
                 * then their distance is the exact lower bound.
                 * Update the distanceLowerBound to reflect this
                 * (which must be smaller, due to the test
                 * immediately prior to this).
                 */
                if (bndPair.IsLeaves)
                {
                    // assert: currentDistance < minimumDistanceFound

                    if (kNearestNeighbors.Size < k)
                    {
                        kNearestNeighbors.Add(bndPair);
                    }
                    else
                    {

                        if (kNearestNeighbors.Peek().Distance > currentDistance)
                        {
                            kNearestNeighbors.Poll();
                            kNearestNeighbors.Add(bndPair);
                        }
                        /*
                         * minDistance should be the farthest point in the K nearest neighbor queue.
                         */
                        distanceLowerBound = kNearestNeighbors.Peek().Distance;

                    }
                }
                else
                {
                    // testing - does allowing a tolerance improve speed?
                    // Ans: by only about 10% - not enough to matter
                    /*
                    double maxDist = bndPair.getMaximumDistance();
                    if (maxDist * .99 < lastComputedDistance)
                      return;
                    //*/

                    /**
                     * Otherwise, expand one side of the pair,
                     * (the choice of which side to expand is heuristically determined)
                     * and insert the new expanded pairs into the queue
                     */
                    bndPair.ExpandToQueue(priQ, distanceLowerBound);
                }
            }
            // done - return items with min distance

            return GetItems(kNearestNeighbors);
        }

        private static TItem[] GetItems(PriorityQueue<BoundablePair<TItem>> kNearestNeighbors)
        {
            /**
             * Iterate the K Nearest Neighbour Queue and retrieve the item from each BoundablePair
             * in this queue
             */
            var items = new TItem[kNearestNeighbors.Size];
            var resultIterator = kNearestNeighbors.GetEnumerator();
            int count = 0;
            while (resultIterator.MoveNext())
            {
                items[count] = ((ItemBoundable<Envelope, TItem>)resultIterator.Current.GetBoundable(0)).Item;
                count++;
            }
            return items;
        }

    }
}
