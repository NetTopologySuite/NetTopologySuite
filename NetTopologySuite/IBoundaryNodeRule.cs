using System;

namespace GisSharpBlog.NetTopologySuite
{
    public interface IBoundaryNodeRule
    {
        Boolean IsInBoundary(int boundaryCount);
    }

  ///**
  // * The Mod-2 Boundary Node Rule (which is the rule specified in the OGC SFS).
  // * @see Mod2BoundaryNodeRule
  // */
  //public static final BoundaryNodeRule MOD2_BOUNDARY_RULE = new Mod2BoundaryNodeRule();

  ///**
  // * The Endpoint Boundary Node Rule.
  // * @see EndPointBoundaryNodeRule
  // */
  //public static final BoundaryNodeRule ENDPOINT_BOUNDARY_RULE = new EndPointBoundaryNodeRule();

  ///**
  // * The MultiValent Endpoint Boundary Node Rule.
  // * @see MultiValentEndPointBoundaryNodeRule
  // */
  //public static final BoundaryNodeRule MULTIVALENT_ENDPOINT_BOUNDARY_RULE = new MultiValentEndPointBoundaryNodeRule();

  ///**
  // * The Monovalent Endpoint Boundary Node Rule.
  // * @see MonoValentEndPointBoundaryNodeRule
  // */
  //public static final BoundaryNodeRule MONOVALENT_ENDPOINT_BOUNDARY_RULE = new MonoValentEndPointBoundaryNodeRule();

  ///**
  // * The Boundary Node Rule specified by the OGC Simple Features Specification,
  // * which is the same as the Mod-2 rule.
  // * @see Mod2BoundaryNodeRule
  // */
  //public static final BoundaryNodeRule OGC_SFS_BOUNDARY_RULE = MOD2_BOUNDARY_RULE;

  /**
   * A {@link BoundaryNodeRule} specifies that points are in the
   * boundary of a lineal geometry iff
   * the point lies on the boundary of an odd number
   * of components.
   * Under this rule {@link LinearRing}s and closed
   * {@link LineString}s have an empty boundary.
   * <p>
   * This is the rule specified by the <i>OGC SFS</i>,
   * and is the default rule used in JTS.
   *
   * @author Martin Davis
   * @version 1.7
   */
  public class Mod2BoundaryNodeRule : IBoundaryNodeRule
  {
    public Boolean IsInBoundary(int boundaryCount)
    {
      // the "Mod-2 Rule"
      return boundaryCount % 2 == 1;
    }
  }

  /**
   * A {@link BoundaryNodeRule} which specifies that any points which are endpoints
   * of lineal components are in the boundary of the
   * parent geometry.
   * This corresponds to the "intuitive" topological definition
   * of boundary.
   * Under this rule {@link LinearRing}s have a non-empty boundary
   * (the common endpoint of the underlying LineString).
   * <p>
   * This rule is useful when dealing with linear networks.
   * For example, it can be used to check
   * whether linear networks are correctly noded.
   * The usual network topology constraint is that linear segments may touch only at endpoints.
   * In the case of a segment touching a closed segment (ring) at one point,
   * the Mod2 rule cannot distinguish between the permitted case of touching at the
   * node point and the invalid case of touching at some other interior (non-node) point.
   * The EndPoint rule does distinguish between these cases,
   * so is more appropriate for use.
   *
   * @author Martin Davis
   * @version 1.7
   */
  ///<summary>
  /// 
  ///</summary>
  public class EndPointBoundaryNodeRule : IBoundaryNodeRule
  {
    public Boolean IsInBoundary(int boundaryCount)
    {
      return boundaryCount > 0;
    }
  }

  /**
   * A {@link BoundaryNodeRule} which determines that only
   * endpoints with valency greater than 1 are on the boundary.
   * This corresponds to the boundary of a {@link MultiLineString}
   * being all the "attached" endpoints, but not
   * the "unattached" ones.
   *
   * @author Martin Davis
   * @version 1.7
   */
  public class MultiValentEndPointBoundaryNodeRule : IBoundaryNodeRule
  {
    public Boolean IsInBoundary(int boundaryCount)
    {
      return boundaryCount > 1;
    }
  }

  ///<summary>
  /// A {@link BoundaryNodeRule} which determines that only
  /// endpoints with valency of exactly 1 are on the boundary.
  /// This corresponds to the boundary of a {@link MultiLineString}
  /// being all the "unattached" endpoints.
  ///</summary>
  public class MonoValentEndPointBoundaryNodeRule : IBoundaryNodeRule
  {
    public Boolean IsInBoundary(int boundaryCount)
    {
      return boundaryCount == 1;
    }
  }

}
