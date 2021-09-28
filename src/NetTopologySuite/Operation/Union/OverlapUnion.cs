using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Unions MultiPolygons efficiently by
    /// using full topological union only for polygons which may overlap,
    /// and combining with the remaining polygons.
    /// Polygons which may overlap are those which intersect the common extent of the inputs.
    /// Polygons wholly outside this extent must be disjoint to the computed union.
    /// They can thus be simply combined with the union result,
    /// which is much more performant.
    /// (There is one caveat to this, which is discussed below).
    /// <para/>
    /// This situation is likely to occur during cascaded polygon union,
    /// since the partitioning of polygons is done heuristically
    /// and thus may group disjoint polygons which can lie far apart.
    /// It may also occur in real world data which contains many disjoint polygons
    /// (e.g. polygons representing parcels on different street blocks).
    /// </summary>
    /// <remarks>
    /// <h2>Algorithm</h2>
    /// The overlap region is determined as the common envelope of intersection.
    /// The input polygons are partitioned into two sets:
    /// <list type="bullet">
    /// <item><term>Overlapping</term><description>Polygons which intersect the overlap region, and thus potentially overlap each other</description></item>
    /// <item><term>Disjoint</term><description>Polygons which are disjoint from (lie wholly outside) the overlap region</description></item>
    /// </list>
    /// The Overlapping set is fully unioned, and then combined with the Disjoint set.
    /// Performing a simple combine works because
    /// the disjoint polygons do not interact with each
    /// other(since the inputs are valid MultiPolygons).
    /// They also do not interact with the Overlapping polygons,
    /// since they are outside their envelope.
    /// <h2>Discussion</h2>
    /// In general the Overlapping set of polygons will
    /// extend beyond the overlap envelope.  This means that the union result
    /// will extend beyond the overlap region.
    /// There is a small chance that the topological
    /// union of the overlap region will shift the result linework enough
    /// that the result geometry intersects one of the Disjoint geometries.
    /// This situation is detected and if it occurs
    /// is remedied by falling back to performing a full union of the original inputs.
    /// Detection is done by a fairly efficient comparison of edge segments which
    /// extend beyond the overlap region.  If any segments have changed
    /// then there is a risk of introduced intersections, and full union is performed.
    /// <para/>
    /// This situation has not been observed in JTS using floating precision,
    /// but it could happen due to snapping.  It has been observed
    /// in other APIs(e.g.GEOS) due to more aggressive snapping.
    /// It is more likely to happen if a Snap - Rounding overlay is used.
    /// <para/>
    /// <b>NOTE: Test has shown that using this heuristic impairs performance.</b>
    /// </remarks>
    /// <author>Martin Davis</author>
    [Obsolete("Due to impairing performance")]
    public class OverlapUnion
    {
        /// <summary>
        /// Union a pair of geometries,
        /// using the more performant overlap union algorithm if possible.
        /// </summary>
        /// <param name="g0">A geometry to union</param>
        /// <param name="g1">A geometry to union</param>
        /// <returns>The union of the inputs</returns>
        public static Geometry Union(Geometry g0, Geometry g1)
        {
            var union = new OverlapUnion(g0, g1);
            return union.Union();
        }

        /// <summary>
        /// Union a pair of geometries,
        /// using the more performant overlap union algorithm if possible.
        /// </summary>
        /// <param name="g0">A geometry to union</param>
        /// <param name="g1">A geometry to union</param>
        /// <param name="unionFun">Function to union two geometries</param>
        /// <returns>The union of the inputs</returns>
        public static Geometry Union(Geometry g0, Geometry g1, UnionStrategy unionFun)
        {
            var union = new OverlapUnion(g0, g1, unionFun);
            return union.Union();
        }

        private readonly GeometryFactory _geomFactory;

        private readonly Geometry _g0;
        private readonly Geometry _g1;


        private readonly UnionStrategy _unionFun;

        /// <summary>
        /// Creates a new instance for unioning the given geometries.
        /// </summary>
        /// <param name="g0">A geometry to union</param>
        /// <param name="g1">A geometry to union</param>
        public OverlapUnion(Geometry g0, Geometry g1) : this(g0, g1, CascadedPolygonUnion.ClassicUnion)
        { }

        /// <summary>
        /// Creates a new instance for unioning the given geometries.
        /// </summary>
        /// <param name="g0">A geometry to union</param>
        /// <param name="g1">A geometry to union</param>
        /// <param name="unionFun">Function to union two geometries</param>
        public OverlapUnion(Geometry g0, Geometry g1, UnionStrategy unionFun)
        {
            if (g0 == null)
            {
                throw new ArgumentNullException(nameof(g0));
            }

            _g0 = g0;
            _g1 = g1;
            _geomFactory = g0.Factory;
            _unionFun = unionFun;
        }

        /// <summary>
        /// Union a pair of geometries,
        /// using the more performant overlap union algorithm if possible.
        /// </summary>
        /// <returns>The union of the inputs</returns>
        public Geometry Union()
        {
            var overlapEnv = OverlapEnvelope(_g0, _g1);

            /*
             * If no overlap, can just combine the geometries
             */
            if (overlapEnv.IsNull)
            {
                var g0Copy = _g0.Copy();
                var g1Copy = _g1.Copy();
                return GeometryCombiner.Combine(g0Copy, g1Copy);
            }

            var disjointPolys = new List<Geometry>();
            var g0Overlap = ExtractByEnvelope(overlapEnv, _g0, disjointPolys);
            var g1Overlap = ExtractByEnvelope(overlapEnv, _g1, disjointPolys);

            //Console.WriteLine($"# geoms in common: {intersectingPolys.Count}");
            var unionGeom = UnionFull(g0Overlap, g1Overlap);

            Geometry result;
            IsUnionOptimized = IsBorderSegmentsSame(unionGeom, overlapEnv);
            if (!IsUnionOptimized)
            {
                // overlap union changed border segments... need to do full union
                //System.out.println("OverlapUnion: Falling back to full union");
                result = UnionFull(_g0, _g1);
            }
            else
            {
                //System.out.println("OverlapUnion: fast path");
                result = Combine(unionGeom, disjointPolys);
            }

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the optimized
        /// or full union was performed.
        /// </summary>
        /// <remarks>Used for unit testing.</remarks>>
        /// <returns><c>true</c> if the optimized union was performed</returns>
        internal bool IsUnionOptimized { get; private set; }

        private static Envelope OverlapEnvelope(Geometry g0, Geometry g1)
        {
            var g0Env = g0.EnvelopeInternal;
            var g1Env = g1.EnvelopeInternal;
            var overlapEnv = g0Env.Intersection(g1Env);
            return overlapEnv;
        }

        private static Geometry Combine(Geometry unionGeom, List<Geometry> disjointPolys)
        {
            if (disjointPolys.Count <= 0)
                return unionGeom;

            disjointPolys.Add(unionGeom);
            var result = GeometryCombiner.Combine(disjointPolys);
            return result;
        }

        private Geometry ExtractByEnvelope(Envelope env, Geometry geom,
            IList<Geometry> disjointGeoms)
        {
            var intersectingGeoms = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var elem = geom.GetGeometryN(i);
                if (elem.EnvelopeInternal.Intersects(env))
                {
                    intersectingGeoms.Add(elem);
                }
                else
                {
                    var copy = elem.Copy();
                    disjointGeoms.Add(copy);
                }
            }

            return _geomFactory.BuildGeometry(intersectingGeoms);
        }

        private Geometry UnionFull(Geometry geom0, Geometry geom1)
        {
            // if both are empty collections, just return a copy of one of them
            if (geom0.NumGeometries == 0
                && geom1.NumGeometries == 0)
                return geom0.Copy();

            var union = _unionFun.Union(geom0, geom1);
            //var union = geom0.Union(geom1);
            return union;
        }

        /// <summary>
        /// Implements union using the buffer-by-zero trick.
        /// This seems to be more robust than overlay union,
        /// for reasons somewhat unknown.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <returns>The union of the geometries </returns>
        private static Geometry UnionBuffer(Geometry g0, Geometry g1)
        {
            var factory = g0.Factory;
            var gColl = factory.CreateGeometryCollection(new Geometry[] { g0, g1 });
            var union = gColl.Buffer(0.0);
            return union;
        }
        private bool IsBorderSegmentsSame(Geometry result, Envelope env)
        {
            var segsBefore = ExtractBorderSegments(_g0, _g1, env);

            var segsAfter = new List<LineSegment>();
            ExtractBorderSegments(result, env, segsAfter);

            //System.out.println("# seg before: " + segsBefore.size() + " - # seg after: " + segsAfter.size());
            return IsEqual(segsBefore, segsAfter);
        }

        private static bool IsEqual(ICollection<LineSegment> segs0, ICollection<LineSegment> segs1)
        {
            if (segs0.Count != segs1.Count)
                return false;

            var segIndex = new HashSet<LineSegment>(segs0);

            foreach (var seg in segs1)
            {
                if (!segIndex.Contains(seg))
                {
                    //System.out.println("Found changed border seg: " + seg);
                    return false;
                }
            }

            return true;
        }

        private IList<LineSegment> ExtractBorderSegments(Geometry geom0, Geometry geom1, Envelope env)
        {
            var segs = new List<LineSegment>();
            ExtractBorderSegments(geom0, env, segs);
            if (geom1 != null)
                ExtractBorderSegments(geom1, env, segs);
            return segs;
        }

        private static void ExtractBorderSegments(Geometry geom, Envelope env, ICollection<LineSegment> segs)
        {
            geom.Apply(new BorderSegmentCoordinateFilter(env, segs));
        }

        private class BorderSegmentCoordinateFilter : ICoordinateSequenceFilter
        {
            private readonly ICollection<LineSegment> _segments;
            private readonly Envelope _envelope;
            public BorderSegmentCoordinateFilter(Envelope env, ICollection<LineSegment> segments)
            {
                _envelope = env;
                _segments = segments;
            }

            public void Filter(CoordinateSequence seq, int i)
            {
                if (i <= 0) return;

                // extract LineSegment
                var p0 = seq.GetCoordinate(i - 1);
                var p1 = seq.GetCoordinate(i);
                bool isBorder = Intersects(_envelope, p0, p1) && !ContainsProperly(_envelope, p0, p1);
                if (isBorder)
                {
                    var seg = new LineSegment(p0, p1);
                    _segments.Add(seg);
                }
            }

            public bool Done { get; } = false;
            public bool GeometryChanged { get; } = false;
            
            private static bool Intersects(Envelope env, Coordinate p0, Coordinate p1)
            {
                return env.Intersects(p0) || env.Intersects(p1);
            }

            private static bool ContainsProperly(Envelope env, Coordinate p0, Coordinate p1)
            {
                return ContainsProperly(env, p0) && ContainsProperly(env, p1);
            }

            private static bool ContainsProperly(Envelope env, Coordinate p)
            {
                if (env.IsNull) return false;
                return p.X > env.MinX &&
                       p.X < env.MaxX &&
                       p.Y > env.MinY &&
                       p.Y < env.MaxY;
            }
        }
    }
}
