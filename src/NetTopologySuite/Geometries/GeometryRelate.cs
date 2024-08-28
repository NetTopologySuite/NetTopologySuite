using System;
using RelateOpV1 = NetTopologySuite.Operation.Relate.RelateOp;
using RelateOpV2 = NetTopologySuite.Operation.RelateNG.RelateNG;
using RelatePredicate = NetTopologySuite.Operation.RelateNG.RelatePredicate;

namespace NetTopologySuite.Geometries
{
    /**
     * Internal class which encapsulates the runtime switch to use RelateNG.
     * <p>
     * This class allows the {@link Geometry} predicate methods to be 
     * switched between the original {@link RelateOp} algorithm 
     * and the modern {@link RelateNG} codebase
     * via a system property <code>jts.relate</code>.
     * <ul>
     * <li><code>jts.relate=old</code> - (default) use original RelateOp algorithm
     * <li><code>jts.relate=ng</code> - use RelateNG
     * </ul>
     * 
     * @author mdavis
     *
     */
    /// <summary>
    /// </summary>
    /// <author>Martin Davis</author>
    public abstract class GeometryRelate
    {
        /// <summary>
        /// Gets a value indicating a geometry relation predicate computation class that uses old NTS relate function set.
        /// </summary>
        public static GeometryRelate Legacy => RelateV1.Instance;

        /// <summary>
        /// Gets a value indicating a geometry relation predicate computation class that uses next-generation NTS relate function set.
        /// </summary>
        public static GeometryRelate NG => RelateV2.Instance;

        /// <summary>
        /// Tests if the input geometries <paramref name="a"/> and <paramref name="b"/> intersect.
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if geometries <paramref name="a"/> and <paramref name="b"/> intersect.</returns>
        /// <seealso cref="Disjoint"/>
        public abstract bool Intersects(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> contains the input geometry <paramref name="b"/>.
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if the geometry <paramref name="a"/> contains <paramref name="b"/>.</returns>
        /// <seealso cref="Within"/>
        public abstract bool Contains(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> covers the input geometry <paramref name="b"/>
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if the geometry <paramref name="a"/> covers <paramref name="b"/>.</returns>
        /// <seealso cref="CoveredBy"/>
        public abstract bool Covers(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> is covered by the input geometry <paramref name="b"/>
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c>if the geometry <paramref name="a"/> is covered by <paramref name="b"/></returns>
        /// <seealso cref="Covers"/>
        public abstract bool CoveredBy(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> crosses the input geometry <paramref name="b"/>
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if the geometry <paramref name="a"/> crosses <paramref name="b"/></returns>
        public abstract bool Crosses(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> and <paramref name="b"/> are disjoint.
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c>> if geometries <paramref name="a"/> and <paramref name="b"/> are disjoint.</returns>
        /// <seealso cref="Intersects"/>
        public abstract bool Disjoint(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> and <paramref name="b"/> are topologically equal.
        /// <para/>
        /// The <c>Equals</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description> The two geometries have at least one point in common,
        /// and no point of either geometry lies in the exterior of the other geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// the pattern <c>T*F**FFF*</c></description></item></list>
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if the geometries <paramref name="a"/> and <paramref name="b"/> are topologically equal</returns>
        public abstract bool EqualsTopologically(Geometry a, Geometry b);

        /// <summary>
        /// Test if the input geometry <paramref name="a"/> overlaps the input geometry <paramref name="b"/>.
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if the geometry <paramref name="a"/> overlaps <paramref name="b"/></returns>
        public abstract bool Overlaps(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="a"/> touches the <paramref name="b"/>.
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if geometry <paramref name="a"/> touches <paramref name="b"/></returns>
        public abstract bool Touches(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the the input geometry <paramref name="a"/> is within the input geometry <paramref name="b"/>.
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
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns><c>true</c> if the geometry <paramref name="a"/> is within <paramref name="b"/>.</returns>
        /// <seealso cref="Contains"/>
        /// <seealso cref="CoveredBy"/>
        public abstract bool Within(Geometry a, Geometry b);

        /// <summary>
        /// Computes the DE-9IM matrix
        /// for the topological relationship between two geometries.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns>The DE-9IM matrix for the topological relationship</returns>
        public abstract IntersectionMatrix Relate(Geometry a, Geometry b);

        /// <summary>
        /// Tests if the input geometry <paramref name="b"/> relates to
        /// <paramref name="a"/> in the way defined by <paramref name="intersectionPattern"/>
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <param name="intersectionPattern">The encoded DE-9IM pattern describing the topological relation to test</param>
        /// <returns><c>true</c> if the geometry <paramref name="b"/> relates to <paramref name="a"/> in the way defined by <paramref name="intersectionPattern"/>.</returns>
        public abstract bool Relate(Geometry a, Geometry b, string intersectionPattern);

        #region Standard implementation

        private sealed class RelateV1 : GeometryRelate
        {
            public static RelateV1 Instance => new RelateV1();

            public override bool Intersects(Geometry a, Geometry b)
            {
                if (a.OgcGeometryType == OgcGeometryType.GeometryCollection ||
                    b.OgcGeometryType == OgcGeometryType.GeometryCollection)
                {
                    for (int i = 0; i < a.NumGeometries; i++)
                    {
                        for (int j = 0; j < b.NumGeometries; j++)
                        {
                            if (a.GetGeometryN(i).Intersects(b.GetGeometryN(j)))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                return RelateOpV1.Relate(a, b).IsIntersects();
            }

            public override bool Contains(Geometry a, Geometry b)
            {
                // optimization - lower dimension cannot contain areas
                if (b.Dimension == Dimension.A && a.Dimension < Dimension.A)
                {
                    return false;
                }
                // optimization - P cannot contain a non-zero-length L
                // Note that a point can contain a zero-length lineal geometry,
                // since the line has no boundary due to Mod-2 Boundary Rule
                if (b.Dimension == Dimension.L && a.Dimension < Dimension.L && b.Length > 0.0)
                {
                    return false;
                }
                // optimization - envelope test
                if (!a.EnvelopeInternal.Contains(b.EnvelopeInternal))
                    return false;

                return RelateOpV1.Relate(a, b).IsContains();
            }

            public override bool Covers(Geometry a, Geometry b)
            {
                // optimization - lower dimension cannot cover areas
                if (b.Dimension == Dimension.A && a.Dimension < Dimension.A)
                {
                    return false;
                }
                // optimization - P cannot cover a non-zero-length L
                // Note that a point can cover a zero-length lineal geometry
                if (b.Dimension == Dimension.L && a.Dimension < Dimension.L && b.Length > 0.0)
                {
                    return false;
                }
                // optimization - envelope test
                if (!a.EnvelopeInternal.Covers(b.EnvelopeInternal))
                    return false;
                // optimization for rectangle arguments
                if (a.IsRectangle)
                {
                    // since we have already tested that the test envelope is covered
                    return true;
                }
                return RelateOpV1.Relate(a, b).IsCovers();
            }

            public override bool CoveredBy(Geometry a, Geometry b)
            {
                return Covers(b, a);
            }

            public override bool Crosses(Geometry a, Geometry b)
            {
                // short-circuit test
                if (!a.EnvelopeInternal.Intersects(b.EnvelopeInternal))
                    return false;
                return RelateOpV1.Relate(a, b).IsCrosses(a.Dimension, b.Dimension);
            }

            public override bool Disjoint(Geometry a, Geometry b)
            {
                return !Intersects(a, b);
            }

            public override bool EqualsTopologically(Geometry a, Geometry b)
            {
                if (!a.EnvelopeInternal.Equals(b.EnvelopeInternal))
                    return false;
                return RelateOpV1.Relate(a, b).IsEquals(a.Dimension, b.Dimension);
            }

            public override bool Overlaps(Geometry a, Geometry b)
            {
                if (!a.EnvelopeInternal.Intersects(b.EnvelopeInternal))
                    return false;
                return RelateOpV1.Relate(a, b).IsOverlaps(a.Dimension, b.Dimension);
            }

            public override bool Touches(Geometry a, Geometry b)
            {
                if (!a.EnvelopeInternal.Intersects(b.EnvelopeInternal))
                    return false;
                return RelateOpV1.Relate(a, b).IsTouches(a.Dimension, b.Dimension);
            }

            public override bool Within(Geometry a, Geometry b)
            {
                return Contains(b, a);
            }

            public override IntersectionMatrix Relate(Geometry a, Geometry b)
            {
                CheckNotGeometryCollection(a, b);
                return RelateOpV1.Relate(a, b);
            }

            public override bool Relate(Geometry a, Geometry b, string intersectionPattern)
            {
                return Relate(a, b).Matches(intersectionPattern);
            }

            public override string ToString()
            {
                return "Legacy";
            }

            private static void CheckNotGeometryCollection(Geometry a, Geometry b)
            {
                if (a.OgcGeometryType == OgcGeometryType.GeometryCollection)
                    throw new ArgumentException("Operation does not support GeometryCollection arguments", nameof(a));
                if (b.OgcGeometryType == OgcGeometryType.GeometryCollection)
                    throw new ArgumentException("Operation does not support GeometryCollection arguments", nameof(b));
            }

        }

        private sealed class RelateV2 : GeometryRelate
        {
            public static RelateV2 Instance => new RelateV2();

            public override bool Intersects(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Intersects());
            }

            public override bool Contains(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Contains());
            }

            public override bool Covers(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Covers());
            }

            public override bool CoveredBy(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.CoveredBy());
            }

            public override bool Crosses(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Crosses());
            }

            public override bool Disjoint(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Disjoint());
            }

            public override bool EqualsTopologically(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.EqualsTopologically());
            }

            public override bool Overlaps(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Overlaps());
            }

            public override bool Touches(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Touches());
            }

            public override bool Within(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b, RelatePredicate.Within());
            }

            public override IntersectionMatrix Relate(Geometry a, Geometry b)
            {
                return RelateOpV2.Relate(a, b);
            }

            public override bool Relate(Geometry a, Geometry b, string intersectionPattern)
            {
                return RelateOpV2.Relate(a, b, intersectionPattern);
            }

            public override string ToString()
            {
                return "NG";
            }

        }

        #endregion
    }
}
