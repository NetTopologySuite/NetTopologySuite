using System;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// A pair of {@link Boundable}s, whose leaf items 
    /// support a distance metric between them.
    /// Used to compute the distance between the members,
    /// and to expand a member relative to the other
    /// in order to produce new branches of the 
    /// Branch-and-Bound evaluation tree.
    /// Provides an ordering based on the distance between the members,
    /// which allows building a priority queue by minimum distance.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class BoundablePair : IComparable<BoundablePair>
    {
        private readonly IBoundable _boundable1;
        private readonly IBoundable _boundable2;
        private readonly double _distance;
        private readonly IItemDistance _itemDistance;
        //private double _maxDistance = -1.0;

        /// <summary>
        /// Creates an instance of this class with the given <see cref="IBoundable"/>s and the <see cref="IItemDistance"/> function.
        /// </summary>
        /// <param name="boundable1">The first boundable</param>
        /// <param name="boundable2">The second boundable</param>
        /// <param name="itemDistance">The item distance function</param>
        public BoundablePair(IBoundable boundable1, IBoundable boundable2, IItemDistance itemDistance)
        {
            _boundable1 = boundable1;
            _boundable2 = boundable2;
            _itemDistance = itemDistance;
            _distance = GetDistance();
        }

        /// <summary>
        /// Gets one of the member <see cref="IBoundable"/>s in the pair 
        /// (indexed by [0, 1]).
        /// </summary>
        /// <param name="i">The index of the member to return (0 or 1)</param>
        /// <returns>The chosen member</returns>
        public IBoundable GetBoundable(int i)
        {
            return i == 0 ? _boundable1 : _boundable2;
        }

        /// <summary>
        /// Computes the distance between the <see cref="IBoundable"/>s in this pair.
        /// The boundables are either composites or leaves.
        /// If either is composite, the distance is computed as the minimum distance
        /// between the bounds.  
        /// If both are leaves, the distance is computed by <see cref="IItemDistance.Distance(ItemBoundable, ItemBoundable)"/>.
        /// </summary>
        /// <returns>The distance between the <see cref="IBoundable"/>s in this pair.</returns>
        private double GetDistance()
        {
            // if items, compute exact distance
            if (IsLeaves)
            {
                return _itemDistance.Distance((ItemBoundable) _boundable1,
                                              (ItemBoundable) _boundable2);
            }
            // otherwise compute distance between bounds of boundables
            return ((IEnvelope) _boundable1.Bounds).Distance(((IEnvelope) _boundable2.Bounds));
        }


        /*
        public double GetMaximumDistance()
        {
          if (_maxDistance < 0.0)
              _maxDistance = MaxDistance();
          return _maxDistance;
        }
        */

        /*
        private double MaxDistance()
        {
          return maximumDistance( 
              (IEnvelope) boundable1.Bounds,
              (IEnvelope) boundable2.Bounds);      	
        }
  
        private static double MaximumDistance(IEnvelope env1, IEnvelope env2)
        {
          double minx = Math.Min(env1.GetMinX(), env2.GetMinX());
          double miny = Math.Min(env1.GetMinY(), env2.GetMinY());
          double maxx = Math.Max(env1.GetMaxX(), env2.GetMaxX());
          double maxy = Math.Max(env1.GetMaxY(), env2.GetMaxY());
          var min = new Coordinate(minx, miny);
          var max = new Coordinate(maxx, maxy);
          return min.Distance(max);
        }
        */

        /**
         * Gets the minimum possible distance between the Boundables in
         * this pair. 
         * If the members are both items, this will be the
         * exact distance between them.
         * Otherwise, this distance will be a lower bound on 
         * the distances between the items in the members.
         * 
         * @return the exact or lower bound distance for this pair
         */

        /// <summary>
        /// Gets the minimum possible distance between the Boundables in
        /// this pair. 
        /// If the members are both items, this will be the
        /// exact distance between them.
        /// Otherwise, this distance will be a lower bound on 
        /// the distances between the items in the members.
        /// </summary>
        /// <returns>The exact or lower bound distance for this pair</returns>
        public double Distance
        {
            get { return _distance; }
        }

        /**
         * Compares two pairs based on their minimum distances
         */

        /// <summary>
        /// Compares two pairs based on their minimum distances
        /// </summary>
        public int CompareTo(BoundablePair o)
        {
            BoundablePair nd = o;
            if (Distance < nd.GetDistance()) return -1;
            if (Distance > nd.Distance) return 1;
            return 0;
        }

        /// <summary>
        /// Tests if both elements of the pair are leaf nodes
        /// </summary>
        public bool IsLeaves
        {
            get { return !(IsComposite(_boundable1) || IsComposite(_boundable2)); }
        }

        public static bool IsComposite(Object item)
        {
            return (item is AbstractNode);
        }

        private static double Area(IBoundable b)
        {
            return ((IEnvelope) b.Bounds).Area;
        }

        /// <summary>
        /// For a pair which is not a leaf 
        /// (i.e. has at least one composite boundable)
        /// computes a list of new pairs 
        /// from the expansion of the larger boundable.
        /// </summary>
        public void ExpandToQueue(PriorityQueue<BoundablePair> priQ, double minDistance)
        {
            bool isComp1 = IsComposite(_boundable1);
            bool isComp2 = IsComposite(_boundable2);

            /**
             * HEURISTIC: If both boundable are composite,
             * choose the one with largest area to expand.
             * Otherwise, simply expand whichever is composite.
             */
            if (isComp1 && isComp2)
            {
                if (Area(_boundable1) > Area(_boundable2))
                {
                    Expand(_boundable1, _boundable2, priQ, minDistance);
                    return;
                }
                Expand(_boundable2, _boundable1, priQ, minDistance);
                return;
            }
            if (isComp1)
            {
                Expand(_boundable1, _boundable2, priQ, minDistance);
                return;
            }
            if (isComp2)
            {
                Expand(_boundable2, _boundable1, priQ, minDistance);
                return;
            }

            throw new ArgumentException("neither boundable is composite");
        }

        private void Expand(IBoundable bndComposite, IBoundable bndOther,
                            PriorityQueue<BoundablePair> priQ, double minDistance)
        {
            var children = ((AbstractNode) bndComposite).ChildBoundables;
            foreach (IBoundable child in children)
            {
                var bp = new BoundablePair(child, bndOther, _itemDistance);
                // only add to queue if this pair might contain the closest points
                // MD - it's actually faster to construct the object rather than called distance(child, bndOther)!
                if (bp.GetDistance() < minDistance)
                {
                    priQ.Add(bp);
                }
            }
        }
    }
}