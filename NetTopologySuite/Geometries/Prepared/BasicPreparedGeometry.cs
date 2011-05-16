using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
/**
 * A base class for {@link PreparedGeometry} subclasses.
 * Contains default implementations for methods, which simply delegate
 * to the equivalent {@link Geometry} methods.
 * This class may be used as a "no-op" class for Geometry types
 * which do not have a corresponding {@link PreparedGeometry} implementation.
 * 
 * @author Martin Davis
 *
 */
public class BasicPreparedGeometry : IPreparedGeometry
{
  private readonly IGeometry _baseGeom;
  private readonly IList<ICoordinate> _representativePts;  // List<Coordinate>

  public BasicPreparedGeometry(IGeometry geom) 
  {
    _baseGeom = geom;
    _representativePts = ComponentCoordinateExtracter.GetCoordinates(geom);
  }

  public IGeometry Geometry { get { return _baseGeom; } }

    ///<summary>
    /// Gets the list of representative points for this geometry. 
    /// One vertex is included for every component of the geometry
    /// (i.e. including one for every ring of polygonal geometries)
    ///</summary>
  public IList<ICoordinate> RepresentativePoints 
  {
      get { return _representativePts; }
  }
  
    ///<summary>
    /// Tests whether any representative of the target geometry intersects the test geometry.
    /// This is useful in A/A, A/L, A/P, L/P, and P/P cases.
    ///</summary>
    /// <param name="testGeom">The test geometry</param>
    /// <returns>true if any component intersects the areal test geometry</returns>
	public bool IsAnyTargetComponentInTest(IGeometry testGeom)
	{
		var locator = new PointLocator();
	    foreach (ICoordinate representativePoint in RepresentativePoints)
	    {
            if (locator.Intersects(representativePoint, testGeom))
                return true;
	    }
		return false;
	}

  /**
   * Determines whether a Geometry g interacts with 
   * this geometry by testing the geometry envelopes.
   *  
   * @param g a Geometry
   * @return true if the envelopes intersect
   */
  protected bool EnvelopesIntersect(IGeometry g)
  {
    if (! _baseGeom.EnvelopeInternal.Intersects(g.EnvelopeInternal))
      return false;
    return true;
  }
  
  /**
   * Determines whether the envelope of 
   * this geometry covers the Geometry g.
   * 
   *  
   * @param g a Geometry
   * @return true if g is contained in this envelope
   */
  protected bool EnvelopeCovers(IGeometry g)
  {
      if (!_baseGeom.EnvelopeInternal.Covers(g.EnvelopeInternal))
      return false;
    return true;
  }
  
  /**
   * Default implementation.
   */
  public virtual bool Contains(IGeometry g)
  {
    return _baseGeom.Contains(g);
  }

  /**
   * Default implementation.
   */
  public virtual bool ContainsProperly(IGeometry g)
  {
  	// since raw relate is used, provide some optimizations
  	
    // short-circuit test
      if (!_baseGeom.EnvelopeInternal.Contains(g.EnvelopeInternal))
      return false;
  	
    // otherwise, compute using relate mask
    return _baseGeom.Relate(g, "T**FF*FF*");
  }

  /**
   * Default implementation.
   */
  public bool CoveredBy(IGeometry g)
  {
    return _baseGeom.CoveredBy(g);
  }

  /**
   * Default implementation.
   */
  public virtual bool Covers(IGeometry g)
  {
    return _baseGeom.Covers(g);
  }

  /**
   * Default implementation.
   */
  public bool Crosses(IGeometry g)
  {
    return _baseGeom.Crosses(g);
  }
  
  public bool Disjoint(IGeometry g)
  {
    return ! Intersects(g);
  }
  
  /**
   * Default implementation.
   */
  public virtual bool Intersects(IGeometry g)
  {
    return _baseGeom.Intersects(g);
  }
  
  /**
   * Default implementation.
   */
  public bool Overlaps(IGeometry g)
  {
    return _baseGeom.Overlaps(g);
  }
  
  /**
   * Default implementation.
   */
  public bool Touches(IGeometry g)
  {
    return _baseGeom.Touches(g);
  }
  
  /**
   * Default implementation.
   */
  public bool Within(IGeometry g)
  {
    return _baseGeom.Within(g);
  }
  
  public override string ToString()
  {
  	return _baseGeom.ToString();
  }
}
}