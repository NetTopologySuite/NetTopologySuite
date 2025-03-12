using NetTopologySuite.Geometries;
using System;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Creates predicate instances for evaluating OGC-standard named topological relationships.
    /// Predicates can be evaluated for geometries using <see cref="RelateNG"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class RelatePredicate
    {
        /// <summary>
        /// Creates a predicate to determine whether two geometries intersect.
        /// <para/>
        /// The <c>Intersects</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The two geometries have at least one point in common</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// at least one of the patterns
        /// <list type="bullet">
        /// <item><description><c>[T********]</c></description></item>
        /// <item><description><c>[*T*******]</c></description></item>
        /// <item><description><c>[***T*****]</c></description></item>
        /// <item><description><c>[****T****]</c></description></item>
        /// </list>
        /// <item><description><c>Disjoint() = false</c>
        /// <br/>(<c>Intersects</c> is the inverse of <c>Disjoint</c>)</description></item></description></item>
        /// </list>
        /// </summary>
        /// <returns>The predicate instance</returns>
        /// <seealso cref="Disjoint"/>
        public static TopologyPredicate Intersects()
        {
            return new IntersectsPredicate();
        }

        private class IntersectsPredicate : BasicPredicate
        {
            public IntersectsPredicate() : base("intersects") { }

            public override bool RequireSelfNoding()
            {
                //-- self-noding is not required to check for a simple interaction
                return false;
            }

            public override bool RequireExteriorCheck(bool isSourceA)
            {
                //-- intersects only requires testing interaction
                return false;
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                Require(envA.Intersects(envB));
            }

            public override void UpdateDimension(Location locA, Location locB, Dimension dimension)
            {
                SetValueIf(true, IsIntersection(locA, locB));
            }

            public override void Finish()
            {
                //-- if no intersecting locations were found
                SetValue(false);
            }
        }

        /// <summary>
        /// Creates a predicate to determine whether two geometries are disjoint.
        /// <para/>
        /// The <c>Disjoint</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The two geometries have no point in common</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// <c>[FF*FF****]</c></description></item>
        /// <item><description><c>Intersects() = false</c>
        /// <br/>(<c>Disjoint</c> is the inverse of <c>Intersects</c>)</description></item>
        /// </list>
        /// </summary>
        /// <returns>The predicate instance</returns>
        /// <seealso cref="Intersects"/>
        public static TopologyPredicate Disjoint()
        {
            return new DisjointPredicate();
        }

        private class DisjointPredicate : BasicPredicate
        {
            public DisjointPredicate() : base("disjoint") { }

            public override bool RequireSelfNoding()
            {
                //-- self-noding is not required to check for a simple interaction
                return false;
            }

            public override bool RequireInteraction()
            {
                //-- ensure entire matrix is computed
                return false;
            }

            public override bool RequireExteriorCheck(bool isSourceA)
            {
                //-- disjoint only requires testing interaction
                return false;
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                SetValueIf(true, envA.Disjoint(envB));
            }

            public override void UpdateDimension(Location locA, Location locB, Dimension dimension)
            {
                SetValueIf(false, IsIntersection(locA, locB));
            }

            public override void Finish()
            {
                //-- if no intersecting locations were found
                SetValue(true);
            }
        }

        /// <summary>
        /// Creates a predicate to determine whether a geometry contains another geometry.
        /// <para/>
        /// The <c>Contains</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of the other geometry is a point of this geometry,
        /// and the interiors of the two geometries have at least one point in common.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// the pattern <c>[T*****FF*]</c></description></item>
        /// <item><description><c>within(B, A) = true </c>
        /// <br/>(<c> contains </c> is the converse of <see cref="Within"/>)</description></item>
        /// </list>
        /// An implication of the definition is that "Geometries do not
        /// contain their boundary".  In other words, if a geometry A is a subset of
        /// the points in the boundary of a geometry B, <c>B.contains(A) = false</c>.
        /// (As a concrete example, take A to be a LineString which lies in the boundary of a Polygon B.)
        /// For a predicate with similar behavior but avoiding
        /// this subtle limitation, see <see cref="Covers"/>.
        /// </summary>
        /// <returns>The predicate instance</returns>
        /// <seealso cref="Within"/>
        public static TopologyPredicate Contains()
        {
            return new ContainsPredicate();
        }

        private class ContainsPredicate : IMPredicate
        {
            public ContainsPredicate() : base("contains") { }

            public override bool RequireCovers(bool isSourceA)
            {
                return isSourceA == RelateGeometry.GEOM_A;
            }

            public override bool RequireExteriorCheck(bool isSourceA)
            {
                //-- only need to check B against Exterior of A
                return isSourceA == RelateGeometry.GEOM_B;
            }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                Require(IsDimsCompatibleWithCovers(dimA, dimB));
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                RequireCovers(envA, envB);
            }

            public override bool IsDetermined
                => IntersectsExteriorOf(RelateGeometry.GEOM_A);

            public override bool ValueIM
                => intMatrix.IsContains();
            
        }

        /// <summary>
        /// Creates a predicate to determine whether a geometry is within another geometry.
        /// <para/>
        /// The <c>Within</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of this geometry is a point of the other geometry,
        /// and the interiors of the two geometries have at least one point in common.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// <c>[T*F**F***]</c></description></item>
        /// <item><description><c>contains(B, A) = true</c>
        /// <br/>(<c>Within</c> is the converse of <see cref="Contains"/>)</description></item>
        /// </list>
        /// An implication of the definition is that
        /// "The boundary of a Geometry is not within the Geometry".
        /// In other words, if a geometry A is a subset of
        /// the points in the boundary of a geometry B, <c>within(B, A) = false</c>
        /// (As a concrete example, take A to be a LineString which lies in the boundary of a Polygon B.)
        /// For a predicate with similar behavior but avoiding
        /// this subtle limitation, see <see cref="CoveredBy"/>.
        /// </summary>
        /// <returns>The predicate instance</returns>
        /// <seealso cref="Contains"/>
        /// <seealso cref="CoveredBy"/>
        public static TopologyPredicate Within()
        {
            return new WithinPredicate();
        }

        private class WithinPredicate : IMPredicate
        {
            public WithinPredicate() : base("within") { }

            public override bool RequireCovers(bool isSourceA)
            {
                return isSourceA == RelateGeometry.GEOM_B;
            }

            public override bool RequireExteriorCheck(bool isSourceA)
            {
                //-- only need to check A against Exterior of B
                return isSourceA == RelateGeometry.GEOM_A;
            }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                Require(IsDimsCompatibleWithCovers(dimB, dimA));
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                RequireCovers(envB, envA);
            }

            public override bool IsDetermined
                => IntersectsExteriorOf(RelateGeometry.GEOM_B);

            public override bool ValueIM
                => intMatrix.IsWithin();
        }

        /// <summary>
        /// Creates a predicate to determine whether a geometry covers another geometry.
        /// <para/>
        /// The <c>Covers</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of the other geometry is a point of this geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// at least one of the following patterns:
        /// <list type="bullet">
        /// <item><c><description>[T*****FF*]</description></c></item>
        /// <item><c><description>[*T****FF*]</description></c></item>
        /// <item><c><description>[***T**FF*]</description></c></item>
        /// <item><c><description>[****T*FF*]</description></c></item>
        /// </list></description></item>
        /// <item><description><c>CoveredBy(b, a) = true</c>
        /// <br/>(<c>Covers</c> is the converse of <see cref="CoveredBy"/>)</description></item>
        /// </list>
        /// If either geometry is empty, the value of this predicate is <c>false</c>.
        /// <para/>
        /// This predicate is similar to <see cref="Contains"/>,
        /// but is more inclusive (i.e. returns <c>true</c> for more cases).
        /// In particular, unlike <c>Contains</c> it does not distinguish between
        /// points in the boundary and in the interior of geometries.
        /// For most cases, <c>Covers</c> should be used in preference to <c>Contains</c>.
        /// As an added benefit, <c>Covers</c> is more amenable to optimization,
        /// and hence should be more performant.
        /// </summary>
        /// <returns>The predicate instance</returns>
        /// <seealso cref="CoveredBy"/>
        public static TopologyPredicate Covers()
        {
            return new CoversPredicate();
        }

        private class CoversPredicate : IMPredicate
        {
            public CoversPredicate() : base("covers") { }

            public override bool RequireCovers(bool isSourceA)
            {
                return isSourceA == RelateGeometry.GEOM_A;
            }

            public override bool RequireExteriorCheck(bool isSourceA)
            {
                //-- only need to check B against Exterior of A
                return isSourceA == RelateGeometry.GEOM_B;
            }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                Require(IsDimsCompatibleWithCovers(dimA, dimB));
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                RequireCovers(envA, envB);
            }

            public override bool IsDetermined
                => IntersectsExteriorOf(RelateGeometry.GEOM_A);

            public override bool ValueIM
                => intMatrix.IsCovers();
        }

        /// <summary>
        /// Creates a predicate to determine whether a geometry is covered by another geometry.
        /// <para/>
        /// The <c>CoveredBy</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of this geometry is a point of the other geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// at least one of the following patterns:
        /// <list type="bullet">
        /// <item><description><c>[T*F**F***]</c></description></item>
        /// <item><description><c>[*TF**F***]</c></description></item>
        /// <item><description><c>[**FT*F***]</c></description></item>
        /// <item><description><c>[**F*TF***]</c></description></item>
        /// </list></description></item>
        /// <item><description><c>Covers(B, A) = true</c>
        /// <br/>(<c>CoveredBy</c> is the converse of <see cref="Covers"/>)</description></item>
        /// </list>
        /// If either geometry is empty, the value of this predicate is <c>false</c>.
        /// <para/>
        /// This predicate is similar to {@link #within},
        /// but is more inclusive (i.e. returns <c>true</c> for more cases).
        /// </summary>
        /// <returns>The predicate instance</returns>
        /// <seealso cref="Covers"/>
        public static TopologyPredicate CoveredBy()
        {
            return new CoveredByPredicate();
        }

        private class CoveredByPredicate : IMPredicate
        {
            public CoveredByPredicate() : base("coveredBy") { }

            public override bool RequireCovers(bool isSourceA)
            {
                return isSourceA == RelateGeometry.GEOM_B;
            }

            public override bool RequireExteriorCheck(bool isSourceA)
            {
                //-- only need to check A against Exterior of B
                return isSourceA == RelateGeometry.GEOM_A;
            }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                Require(IsDimsCompatibleWithCovers(dimB, dimA));
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                RequireCovers(envB, envA);
            }

            public override bool IsDetermined
                => IntersectsExteriorOf(RelateGeometry.GEOM_B);

            public override bool ValueIM
                => intMatrix.IsCoveredBy();
        }

        /// <summary>Creates a predicate to determine whether a geometry crosses another geometry.
        /// <para/>
        /// The <c>Crosses</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The geometries have some but not all interior points in common.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// one of the following patterns:
        /// <list type="bullet">
        /// <item><description><c>[T*T******]</c> (for P/L, P/A, and L/A cases)</description></item>
        /// <item><description><c>[T*****T**]</c> (for L/P, A/P, and A/L cases)</description></item>
        /// <item><description><c>[0********]</c> (for L/L cases)</description></item>
        /// </list></description></item>
        /// </list>
        /// For the A/A and P/P cases this predicate returns <c>false</c>.
        /// <para/>
        /// The SFS defined this predicate only for P/L, P/A, L/L, and L/A cases.
        /// To make the relation symmetric
        /// NTS extends the definition to apply to L/P, A/P and A/L cases as well.
        /// </summary>
        /// <returns>The crosses predicate</returns>
        public static TopologyPredicate Crosses()
        {
            return new CrossesPredicate();
        }

        private class CrossesPredicate : IMPredicate
        {
            public CrossesPredicate() : base("crosses") { }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                bool isBothPointsOrAreas = (dimA == Dimension.P && dimB == Dimension.P)
                    || (dimA == Dimension.A && dimB == Dimension.A);
                Require(!isBothPointsOrAreas);
            }


            public override bool IsDetermined
            {
                get
                {
                    if (dimA == Dimension.L && dimB == Dimension.L)
                    {
                        //-- L/L interaction can only be dim = P
                        if (GetDimension(Location.Interior, Location.Interior) > Dimension.P)
                            return true;
                    }
                    else if (dimA < dimB)
                    {
                        if (IsIntersects(Location.Interior, Location.Interior)
                            && IsIntersects(Location.Interior, Location.Exterior))
                        {
                            return true;
                        }
                    }
                    else if (dimA > dimB)
                    {
                        if (IsIntersects(Location.Interior, Location.Interior)
                            && IsIntersects(Location.Exterior, Location.Interior))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public override bool ValueIM
                => intMatrix.IsCrosses(dimA, dimB);
        }

        /// <summary>
        /// Creates a predicate to determine whether two geometries are topologically equal.
        /// <para/>
        /// The <c>Equals</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description> The two geometries have at least one point in common,
        /// and no point of either geometry lies in the exterior of the other geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// the pattern <c>T*F**FFF*</c></description></item></list>
        /// </summary>
        /// <returns>The predicate instance</returns>
        public static TopologyPredicate EqualsTopologically()
        {
            return new EqualsTopoPredicate();
        }

        private class EqualsTopoPredicate : IMPredicate
        {
            public EqualsTopoPredicate() : base("equals") { }


            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                Require(dimA == dimB);
            }


            public override void Init(Envelope envA, Envelope envB)
            {
                Require(envA.Equals(envB));
            }


            public override bool IsDetermined
            {
                get
                {
                    bool isEitherExteriorIntersects =
                        IsIntersects(Location.Interior, Location.Exterior)
                     || IsIntersects(Location.Boundary, Location.Exterior)
                     || IsIntersects(Location.Exterior, Location.Interior)
                     || IsIntersects(Location.Exterior, Location.Boundary);

                    return isEitherExteriorIntersects;
                }
            }


            public override bool ValueIM
                => intMatrix.IsEquals(dimA, dimB);
        }

        /// <summary>
        /// Creates a predicate to determine whether a geometry overlaps another geometry.
        /// <para/>
        /// The <c>Overlaps</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The geometries have at least one point each not shared by the other
        /// (or equivalently neither covers the other),
        /// they have the same dimension,
        /// and the intersection of the interiors of the two geometries has
        /// the same dimension as the geometries themselves.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// <c>[T*T***T**]</c> (for P/P and A/A cases)
        /// or <c>[1*T***T**]</c> (for L/L cases)</description></item>
        /// </list>
        /// If the geometries are of different dimension this predicate returns <c>false</c>.
        /// This predicate is symmetric.
        /// </summary>
        /// <returns>The predicate instance</returns>
        public static TopologyPredicate Overlaps()
        {
            return new OverlapsPredicate();
        }

        private class OverlapsPredicate : IMPredicate
        {
            public OverlapsPredicate() : base("overlaps") { }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                Require(dimA == dimB);
            }

            public override bool IsDetermined
            {
                get
                {
                    if (dimA == Dimension.A || dimA == Dimension.P)
                    {
                        if (IsIntersects(Location.Interior, Location.Interior)
                            && IsIntersects(Location.Interior, Location.Exterior)
                            && IsIntersects(Location.Exterior, Location.Interior))
                            return true;
                    }
                    if (dimA == Dimension.L)
                    {
                        if (IsDimension(Location.Interior, Location.Interior, Dimension.L)
                            && IsIntersects(Location.Interior, Location.Exterior)
                            && IsIntersects(Location.Exterior, Location.Interior))
                            return true;
                    }
                    return false;
                }
            }

            public override bool ValueIM
                => intMatrix.IsOverlaps(dimA, dimB);
        }

        /// <summary>
        /// Creates a predicate to determine whether a geometry touches another geometry.
        /// <para/>
        /// The <c>Touches</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The geometries have at least one point in common,
        /// but their interiors do not intersect.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// at least one of the following patterns
        /// <list type="bullet">
        /// <item><description><c>[FT*******]</c></description></item>
        /// <item><description><c>[F**T*****]</c></description></item>
        /// <item><description><c>[F***T****]</c></description></item>
        /// </list></description></item>
        /// </list>
        /// If both geometries have dimension 0, the predicate returns <c>false</c>,
        /// since points have only interiors.
        /// This predicate is symmetric.
        /// </summary>
        /// <returns>The predicate instance</returns>
        public static TopologyPredicate Touches()
        {
            return new TouchesPredicate();
        }

        private class TouchesPredicate : IMPredicate
        {
            public TouchesPredicate() : base("touches") { }

            public override void Init(Dimension dimA, Dimension dimB)
            {
                base.Init(dimA, dimB);
                //-- Points have only interiors, so cannot touch
                bool isBothPoints = dimA == 0 && dimB == 0;
                Require(!isBothPoints);
            }

            public override bool IsDetermined
                //-- for touches interiors cannot intersect
                => IsIntersects(Location.Interior, Location.Interior);

            public override bool ValueIM
                => intMatrix.IsTouches(dimA, dimB);
        }

        /// <summary>
        /// Creates a predicate that matches a DE-9IM matrix pattern.
        /// </summary>
        /// <param name="imPattern">The pattern to match</param>
        /// <returns>A predicate that matches the pattern</returns>
        /// <seealso cref="IntersectionMatrixPattern"/>
        public static TopologyPredicate Matches(string imPattern)
        {
            return new IMPatternMatcher(imPattern);
        }
    }
}
