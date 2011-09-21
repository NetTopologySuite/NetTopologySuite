namespace NetTopologySuite.Index.Strtree
{
    using System;
    using System.Collections.Generic;

    using GeoAPI.Geometries;

    using NetTopologySuite.Utilities;

    /**
 * A pair of {@link Boundable}s, whose leaf items 
 * support a distance metric between them.
 * Used to compute the distance between the members,
 * and to expand a member relative to the other
 * in order to produce new branches of the 
 * Branch-and-Bound evaluation tree.
 * Provides an ordering based on the distance between the members,
 * which allows building a priority queue by minimum distance.
 * 
 * @author Martin Davis
 *
 */
class BoundablePair : IComparable<BoundablePair>
  //implements Comparable
{
  private readonly IBoundable _boundable1;
  private readonly IBoundable _boundable2;
  private readonly double _distance;
  private readonly IItemDistance _itemDistance;
  //private double maxDistance = -1.0;
  
  public BoundablePair(IBoundable boundable1, IBoundable boundable2, IItemDistance itemDistance)
  {
    this._boundable1 = boundable1;
    this._boundable2 = boundable2;
    this._itemDistance = itemDistance;
    _distance = this.GetDistance();
  }
  
  /**
   * Gets one of the member {@link Boundable}s in the pair 
   * (indexed by [0, 1]).
   * 
   * @param i the index of the member to return (0 or 1)
   * @return the chosen member
   */
  public IBoundable GetBoundable(int i)
  {
    if (i == 0) return this._boundable1;
    return this._boundable2;
  }
  
  /**
   * Computes the distance between the {@link Boundable}s in this pair.
   * The boundables are either composites or leaves.
   * If either is composite, the distance is computed as the minimum distance
   * between the bounds.  
   * If both are leaves, the distance is computed by {@link #itemDistance(ItemBoundable, ItemBoundable)}.
   * 
   * @return
   */
  private double GetDistance()
  {
    // if items, compute exact distance
    if (this.IsLeaves()) {
      return this._itemDistance.Distance((ItemBoundable) this._boundable1,
          (ItemBoundable) this._boundable2);
    }
    // otherwise compute distance between bounds of boundables
    return ((IEnvelope) this._boundable1.Bounds).Distance(
        ((IEnvelope) this._boundable2.Bounds));
  }

  
  /*
  public double getMaximumDistance()
  {
  	if (maxDistance < 0.0)
  		maxDistance = maxDistance();
  	return maxDistance;
  }
  */
  
  /*
  private double maxDistance()
  {
    return maximumDistance( 
        (Envelope) boundable1.getBounds(),
        (Envelope) boundable2.getBounds());      	
  }
  
  private static double maximumDistance(Envelope env1, Envelope env2)
  {
  	double minx = Math.min(env1.getMinX(), env2.getMinX());
  	double miny = Math.min(env1.getMinY(), env2.getMinY());
  	double maxx = Math.max(env1.getMaxX(), env2.getMaxX());
  	double maxy = Math.max(env1.getMaxY(), env2.getMaxY());
    Coordinate min = new Coordinate(minx, miny);
    Coordinate max = new Coordinate(maxx, maxy);
    return min.distance(max);
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
  public double Distance { get { return _distance; } }
  
  /**
   * Compares two pairs based on their minimum distances
   */
  public int CompareTo(BoundablePair o)
  {
    BoundablePair nd = o;
    if (this.Distance < nd.GetDistance()) return -1;
    if (this.Distance > nd.Distance) return 1;
    return 0;
  }

  /**
   * Tests if both elements of the pair are leaf nodes
   * 
   * @return true if both pair elements are leaf nodes
   */
  public bool IsLeaves()
  {
    return ! (IsComposite(this._boundable1) || IsComposite(this._boundable2));
  }
  
  public static bool IsComposite(Object item)
  {
    return (item is AbstractNode); 
  }
  
  private static double Area(IBoundable b)
  {
    return ((IEnvelope) b.Bounds).Area;
  }
  
  /**
   * For a pair which is not a leaf 
   * (i.e. has at least one composite boundable)
   * computes a list of new pairs 
   * from the expansion of the larger boundable.
   * 
   * @return a List of new pairs
   */
  public void ExpandToQueue(PriorityQueue<BoundablePair> priQ, double minDistance)
  {
    bool isComp1 = IsComposite(this._boundable1);
    bool isComp2 = IsComposite(this._boundable2);
    
    /**
     * HEURISTIC: If both boundable are composite,
     * choose the one with largest area to expand.
     * Otherwise, simply expand whichever is composite.
     */
    if (isComp1 && isComp2) {
      if (Area(this._boundable1) > Area(this._boundable2)) {
        this.Expand(this._boundable1, this._boundable2, priQ, minDistance);
        return;
      }
        this.Expand(this._boundable2, this._boundable1, priQ, minDistance);
        return;
    }
      if (isComp1) {
          this.Expand(this._boundable1, this._boundable2, priQ, minDistance);
          return;
      }
      if (isComp2) {
          this.Expand(this._boundable2, this._boundable1, priQ, minDistance);
          return;
      }

      throw new ArgumentException("neither boundable is composite");
  }
  
  private void Expand(IBoundable bndComposite, IBoundable bndOther,
      PriorityQueue<BoundablePair> priQ, double minDistance)
  {
    var children = ((AbstractNode) bndComposite).ChildBoundables;
    foreach (IBoundable child in  children)
    {
      var bp = new BoundablePair(child, bndOther, this._itemDistance);
      // only add to queue if this pair might contain the closest points
      // MD - it's actually faster to construct the object rather than called distance(child, bndOther)!
      if (bp.GetDistance() < minDistance) {
        priQ.Add(bp);
      }
    }
  }
}
}