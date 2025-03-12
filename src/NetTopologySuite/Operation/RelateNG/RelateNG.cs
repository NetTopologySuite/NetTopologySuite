using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Computes the value of topological predicates between two geometries based on the
    /// <a href = "https://en.wikipedia.org/wiki/DE-9IM" > Dimensionally - Extended 9-Intersection Model</a>(DE-9IM).
    /// Standard and custom topological predicates are provided by <see cref="RelatePredicate"/>.
    /// <para/>
    /// The RelateNG algorithm has the following capabilities:
    /// <list type="number">
    /// <item><description>Efficient short-circuited evaluation of topological predicates
    /// (including matching custom DE-9IM matrix patterns)</description></item>
    /// <item><description>Optimized repeated evaluation of predicates against a single geometry
    /// via cached spatial indexes (AKA "prepared mode")</description></item>
    /// <item><description>Robust computation (only point-local topology is required,
    /// so invalid geometry topology does not cause failures)</description></item>
    /// <item><description><see cref="GeometryCollection"/> inputs containing mixed types and overlapping polygons
    /// are supported, using <i>union semantics</i>.</description></item>
    /// <item><description>Zero - length LineStrings are treated as being topologically identical to Points.</description></item>
    /// <item><description>Support for <see cref="IBoundaryNodeRule"/>s.</description></item>
    /// </list>
    ///
    /// See <see cref="IntersectionMatrixPattern"/>
    /// for a description of DE - 9IM patterns.
    ///
    /// If not specified, the standard <see cref="BoundaryNodeRules.Mod2BoundaryNodeRule"/> is used.
    /// RelateNG operates in 2D only; it ignores any Z ordinates.
    ///
    /// This implementation replaces <see cref="Relate.RelateOp"/>
    /// and <see cref="Geometries.Prepared.IPreparedGeometry"/>.
    ///
    /// <h3>FUTURE WORK</h3>
    /// <list type="bullet">
    /// <item><description>Support for a distance tolerance to provide "approximate" predicate evaluation</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="Relate.RelateOp"/>
    /// <seealso cref="Geometries.Prepared.IPreparedGeometry"/>
    public class RelateNG
    {
        /// <summary>
        /// Tests whether the topological relationship between two geometries
        /// satisfies a topological predicate.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <param name="pred">The topological predicate</param>
        /// <returns><c>true</c> if the topological relationship is satisfied</returns>
        public static bool Relate(Geometry a, Geometry b, TopologyPredicate pred)
        {
            var rng = new RelateNG(a, false);
            return rng.Evaluate(b, pred);
        }

        /// <summary>
        /// Tests whether the topological relationship between two geometries
        /// satisfies a topological predicate,
        /// using a given <see cref="IBoundaryNodeRule"/>.</summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <param name="pred">The topological predicate</param>
        /// <param name="bnRule">The Boundary Node Rule to use</param>
        /// <returns><c>true</c> if the topological relationship is satisfied</returns>
        public static bool Relate(Geometry a, Geometry b, TopologyPredicate pred, IBoundaryNodeRule bnRule)
        {
            var rng = new RelateNG(a, false, bnRule);
            return rng.Evaluate(b, pred);
        }

        /// <summary>
        /// Tests whether the topological relationship to a geometry
        /// matches a DE-9IM matrix pattern.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <param name="imPattern">The DE-9IM pattern to match</param>
        /// <returns><c>true</c> if the geometries relationship matches the DE-9IM pattern</returns>
        /// <seealso cref="IntersectionMatrixPattern"/>
        public static bool Relate(Geometry a, Geometry b, string imPattern)
        {
            var rng = new RelateNG(a, false);
            return rng.Evaluate(b, imPattern);
        }

        /// <summary>
        /// Computes the DE-9IM matrix
        /// for the topological relationship between two geometries.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <returns>The DE-9IM matrix for the topological relationship</returns>
        public static IntersectionMatrix Relate(Geometry a, Geometry b)
        {
            var rng = new RelateNG(a, false);
            return rng.Evaluate(b);
        }

        /// <summary>
        /// Computes the DE-9IM matrix
        /// for the topological relationship between two geometries.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="b">The B input geometry</param>
        /// <param name="bnRule">The Boundary Node Rule to use</param>
        /// <returns>The DE-9IM matrix for the topological relationship</returns>
        public static IntersectionMatrix Relate(Geometry a, Geometry b, IBoundaryNodeRule bnRule)
        {
            var rng = new RelateNG(a, false, bnRule);
            return rng.Evaluate(b);
        }

        /// <summary>
        /// Creates a prepared RelateNG instance to optimize the
        /// evaluation of relationships against a single geometry.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <returns>A prepared instance</returns>
        public static RelateNG Prepare(Geometry a)
        {
            return new RelateNG(a, true);
        }

        /// <summary>
        /// Creates a prepared RelateNG instance to optimize the
        /// computation of predicates against a single geometry,
        /// using a given <see cref="IBoundaryNodeRule"/>.
        /// </summary>
        /// <param name="a">The A input geometry</param>
        /// <param name="bnRule">The Boundary Node Rule to use</param>
        /// <returns>A prepared instance</returns>
        public static RelateNG Prepare(Geometry a, IBoundaryNodeRule bnRule)
        {
            return new RelateNG(a, true, bnRule);
        }

        private readonly IBoundaryNodeRule _boundaryNodeRule;
        private readonly RelateGeometry _geomA;
        private MCIndexSegmentSetMutualIntersector _edgeMutualInt;

        private RelateNG(Geometry inputA, bool isPrepared)
            : this(inputA, isPrepared, BoundaryNodeRules.OgcSfsBoundaryRule)
        {
            
        }

        private RelateNG(Geometry inputA, bool isPrepared, IBoundaryNodeRule bnRule)
        {
            _boundaryNodeRule = bnRule;
            _geomA = new RelateGeometry(inputA, isPrepared, _boundaryNodeRule);
        }

        /// <summary>
        /// Computes the DE-9IM matrix for the topological relationship to a geometry.
        /// </summary>
        /// <param name="b">The B geometry</param>
        /// <returns>the DE-9IM matrix</returns>
        public IntersectionMatrix Evaluate(Geometry b)
        {
            var rel = new RelateMatrixPredicate();
            Evaluate(b, rel);
            return rel.IM;
        }

        /// <summary>
        /// Tests whether the topological relationship to a geometry
        /// matches a DE-9IM matrix pattern.</summary>
        /// <param name="b">The B geometry</param>
        /// <param name="imPattern"></param>
        /// <returns><c>true</c> if the geometry's topological relationship matches the DE-9IM pattern</returns>
        /// <seealso cref="IntersectionMatrixPattern"/>
        public bool Evaluate(Geometry b, string imPattern)
        {
            return Evaluate(b, RelatePredicate.Matches(imPattern));
        }

        /// <summary>
        /// Tests whether the topological relationship to a geometry
        /// satisfies a topology predicate.
        /// </summary>
        /// <param name="b">The B geometry</param>
        /// <param name="predicate">The topological predicate</param>
        /// <returns><c>true</c> if the predicate is satisfied</returns>
        public bool Evaluate(Geometry b, TopologyPredicate predicate)
        {
            //-- fast envelope checks
            if (!HasRequiredEnvelopeInteraction(b, predicate))
            {
                return false;
            }

            var geomB = new RelateGeometry(b, _boundaryNodeRule);

            if (_geomA.IsEmpty && geomB.IsEmpty)
            {
                //TODO: what if predicate is disjoint?  Perhaps use result on disjoint envs?
                return FinishValue(predicate);
            }
            var dimA = _geomA.DimensionReal;
            var dimB = geomB.DimensionReal;

            //-- check if predicate is determined by dimension or envelope
            predicate.Init(dimA, dimB);
            if (predicate.IsKnown)
                return FinishValue(predicate);

            predicate.Init(_geomA.Envelope, geomB.Envelope);
            if (predicate.IsKnown)
                return FinishValue(predicate);

            var topoComputer = new TopologyComputer(predicate, _geomA, geomB);

            //-- optimized P/P evaluation
            if (dimA == Dimension.P && dimB == Dimension.P)
            {
                ComputePP(geomB, topoComputer);
                topoComputer.Finish();
                return topoComputer.Result;
            }

            //-- test points against (potentially) indexed geometry first
            ComputeAtPoints(geomB, RelateGeometry.GEOM_B, _geomA, topoComputer);
            if (topoComputer.IsResultKnown)
            {
                return topoComputer.Result;
            }
            ComputeAtPoints(_geomA, RelateGeometry.GEOM_A, geomB, topoComputer);
            if (topoComputer.IsResultKnown)
            {
                return topoComputer.Result;
            }

            if (_geomA.HasEdges && geomB.HasEdges)
            {
                ComputeAtEdges(geomB, topoComputer);
            }

            //-- after all processing, set remaining unknown values in IM
            topoComputer.Finish();
            return topoComputer.Result;
        }

        private bool HasRequiredEnvelopeInteraction(Geometry b, TopologyPredicate predicate)
        {
            var envB = b.EnvelopeInternal;
            bool isInteracts = false;
            if (predicate.RequireCovers(RelateGeometry.GEOM_A))
            {
                if (!_geomA.Envelope.Covers(envB))
                {
                    return false;
                }
                isInteracts = true;
            }
            else if (predicate.RequireCovers(RelateGeometry.GEOM_B))
            {
                if (!envB.Covers(_geomA.Envelope))
                {
                    return false;
                }
                isInteracts = true;
            }
            if (!isInteracts
                && predicate.RequireInteraction()
                && !_geomA.Envelope.Intersects(envB))
            {
                return false;
            }
            return true;
        }


        private bool FinishValue(TopologyPredicate predicate)
        {
            predicate.Finish();
            return predicate.Value;
        }

        /// <summary>
        /// An optimized algorithm for evaluating P/P cases.
        /// It tests one point set against the other.
        /// </summary>
        private void ComputePP(RelateGeometry geomB, TopologyComputer topoComputer)
        {
            var ptsA = _geomA.UniquePoints;
            //TODO: only query points in interaction extent? 
            var ptsB = geomB.UniquePoints;

            int numBinA = 0;
            foreach (var ptB in ptsB)
            {
                if (ptsA.Contains(ptB))
                {
                    numBinA++;
                    topoComputer.AddPointOnPointInterior(ptB);
                }
                else
                {
                    topoComputer.AddPointOnPointExterior(RelateGeometry.GEOM_B, ptB);
                }
                if (topoComputer.IsResultKnown)
                {
                    return;
                }
            }
            /*
             * If number of matched B points is less than size of A, 
             * there must be at least one A point in the exterior of B
             */
            if (numBinA < ptsA.Count)
            {
                //TODO: determine actual exterior point?
                topoComputer.AddPointOnPointExterior(RelateGeometry.GEOM_A, null);
            }
        }

        private void ComputeAtPoints(RelateGeometry geom, bool isA,
            RelateGeometry geomTarget, TopologyComputer topoComputer)
        {
            bool isResultKnown = ComputePoints(geom, isA, geomTarget, topoComputer);
            if (isResultKnown)
                return;

            /*
             * Performance optimization: only check points against target
             * if it has areas OR if the predicate requires checking for 
             * exterior interaction.
             * In particular, this avoids testing line ends against lines 
             * for the intersects predicate (since these are checked
             * during segment/segment intersection checking anyway). 
             * Checking points against areas is necessary, since the input
             * linework is disjoint if one input lies wholly inside an area,
             * so segment intersection checking is not sufficient.
             */
            bool checkDisjointPoints = geomTarget.HasDimension(Dimension.A)
                || topoComputer.IsExteriorCheckRequired(isA);
            if (!checkDisjointPoints)
                return;

            isResultKnown = ComputeLineEnds(geom, isA, geomTarget, topoComputer);
            if (isResultKnown)
                return;

            ComputeAreaVertex(geom, isA, geomTarget, topoComputer);
        }

        private bool ComputePoints(RelateGeometry geom, bool isA, RelateGeometry geomTarget,
            TopologyComputer topoComputer)
        {
            if (!geom.HasDimension(Dimension.P))
            {
                return false;
            }

            var points = geom.GetEffectivePoints();
            foreach (var point in points)
            {
                //TODO: exit when all possible target locations (E,I,B) have been found?
                if (point.IsEmpty)
                    continue;

                var pt = point.Coordinate;
                ComputePoint(isA, pt, geomTarget, topoComputer);
                if (topoComputer.IsResultKnown)
                {
                    return true;
                }
            }
            return false;
        }

        private void ComputePoint(bool isA, Coordinate pt, RelateGeometry geomTarget, TopologyComputer topoComputer)
        {
            int locDimTarget = geomTarget.LocateWithDim(pt);
            var locTarget = DimensionLocation.Location(locDimTarget);
            var dimTarget = DimensionLocation.Dimension(locDimTarget, topoComputer.GetDimension(!isA));
            topoComputer.AddPointOnGeometry(isA, locTarget, dimTarget, pt);
        }

        private bool ComputeLineEnds(RelateGeometry geom, bool isA, RelateGeometry geomTarget,
            TopologyComputer topoComputer)
        {
            if (!geom.HasDimension(Dimension.L))
            {
                return false;
            }

            bool hasExteriorIntersection = false;
            foreach(var elem in new GeometryCollectionEnumerator(geom.Geometry))
            {
                if (elem.IsEmpty)
                    continue;

                if (elem is LineString line) {
                    //-- once an intersection with target exterior is recorded, skip further known-exterior points
                    if (hasExteriorIntersection
                        && elem.EnvelopeInternal.Disjoint(geomTarget.Envelope))
                        continue;

                    var e0 = line.CoordinateSequence.First;
                    hasExteriorIntersection |= ComputeLineEnd(geom, isA, e0, geomTarget, topoComputer);
                    if (topoComputer.IsResultKnown)
                    {
                        return true;
                    }

                    if (!line.IsClosed)
                    {
                        var e1 = line.CoordinateSequence.Last;
                        hasExteriorIntersection |= ComputeLineEnd(geom, isA, e1, geomTarget, topoComputer);
                        if (topoComputer.IsResultKnown)
                        {
                            return true;
                        }
                    }
                    //TODO: break when all possible locations have been found?
                }
            }
            return false;
        }

        /// <summary>
        /// Compute the topology of a line endpoint.
        /// Also reports if the line end is in the exterior of the target geometry,
        /// to optimize testing multiple exterior endpoints.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="isA"></param>
        /// <param name="pt"></param>
        /// <param name="geomTarget"></param>
        /// <param name="topoComputer"></param>
        /// <returns><c>true</c> if the line endpoint is in the exterior of the target</returns>
        private bool ComputeLineEnd(RelateGeometry geom, bool isA, Coordinate pt,
            RelateGeometry geomTarget, TopologyComputer topoComputer)
        {
            int locDimLineEnd = geom.LocateLineEndWithDim(pt);
            var dimLineEnd = DimensionLocation.Dimension(locDimLineEnd, topoComputer.GetDimension(isA));
            //-- skip line ends which are in a GC area
            if (dimLineEnd != Dimension.L)
                return false;
            var locLineEnd = DimensionLocation.Location(locDimLineEnd);

            int locDimTarget = geomTarget.LocateWithDim(pt);
            var locTarget = DimensionLocation.Location(locDimTarget);
            var dimTarget = DimensionLocation.Dimension(locDimTarget, topoComputer.GetDimension(!isA));
            topoComputer.AddLineEndOnGeometry(isA, locLineEnd, locTarget, dimTarget, pt);
            return locTarget == Location.Exterior;
        }

        private bool ComputeAreaVertex(RelateGeometry geom, bool isA, RelateGeometry geomTarget, TopologyComputer topoComputer)
        {
            if (!geom.HasDimension(Dimension.A))
            {
                return false;
            }
            //-- evaluate for line and area targets only, since points are handled in the reverse direction
            if (geomTarget.Dimension < Dimension.L)
                return false;

            bool hasExteriorIntersection = false;
            foreach (var elem in new GeometryCollectionEnumerator(geom.Geometry))
            {
                if (elem.IsEmpty)
                    continue;

                if (elem is Polygon poly) {
                    //-- once an intersection with target exterior is recorded, skip further known-exterior points
                    if (hasExteriorIntersection
                        && elem.EnvelopeInternal.Disjoint(geomTarget.Envelope))
                        continue;

                    hasExteriorIntersection |= ComputeAreaVertex(geom, isA, (LinearRing)poly.ExteriorRing, geomTarget, topoComputer);
                    if (topoComputer.IsResultKnown)
                    {
                        return true;
                    }
                    for (int j = 0; j < poly.NumInteriorRings; j++)
                    {
                        hasExteriorIntersection |= ComputeAreaVertex(geom, isA, (LinearRing)poly.GetInteriorRingN(j), geomTarget, topoComputer);
                        if (topoComputer.IsResultKnown)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool ComputeAreaVertex(RelateGeometry geom, bool isA, LinearRing ring, RelateGeometry geomTarget, TopologyComputer topoComputer)
        {
            //TODO: use extremal (highest) point to ensure one is on boundary of polygon cluster
            var pt = ring.Coordinate;

            var locArea = geom.LocateAreaVertex(pt);
            int locDimTarget = geomTarget.LocateWithDim(pt);
            var locTarget = DimensionLocation.Location(locDimTarget);
            var dimTarget = DimensionLocation.Dimension(locDimTarget, topoComputer.GetDimension(!isA));
            topoComputer.AddAreaVertex(isA, locArea, locTarget, dimTarget, pt);
            return locTarget == Location.Exterior;
        }

        private void ComputeAtEdges(RelateGeometry geomB, TopologyComputer topoComputer)
        {
            var envInt = _geomA.Envelope.Intersection(geomB.Envelope);
            if (envInt.IsNull)
                return;

            var edgesB = geomB.ExtractSegmentStrings(RelateGeometry.GEOM_B, envInt);
            var intersector = new EdgeSegmentIntersector(topoComputer);

            if (topoComputer.IsSelfNodingRequired)
            {
                ComputeEdgesAll(edgesB, envInt, intersector);
            }
            else
            {
                ComputeEdgesMutual(edgesB, envInt, intersector);
            }
            if (topoComputer.IsResultKnown)
            {
                return;
            }

            topoComputer.EvaluateNodes();
        }

        private void ComputeEdgesAll(IList<RelateSegmentString> edgesB, Envelope envInt, EdgeSegmentIntersector intersector)
        {
            //TODO: find a way to reuse prepared index?
            var edgesA = _geomA.ExtractSegmentStrings(RelateGeometry.GEOM_A, envInt);

            var edgeInt = new EdgeSetIntersector(edgesA, edgesB, envInt);
            edgeInt.Process(intersector);
        }

        private void ComputeEdgesMutual(IList<RelateSegmentString> edgesB, Envelope envInt, EdgeSegmentIntersector intersector)
        {
            //-- in prepared mode the A edge index is reused
            if (_edgeMutualInt == null)
            {
                var envExtract = _geomA.IsPrepared ? null : envInt;
                var edgesA = _geomA.ExtractSegmentStrings(RelateGeometry.GEOM_A, envExtract);
                _edgeMutualInt = new MCIndexSegmentSetMutualIntersector(edgesA.Cast<ISegmentString>(), envExtract);
            }

            _edgeMutualInt.Process(edgesB.Cast<ISegmentString>(), intersector);
        }

    }
}
