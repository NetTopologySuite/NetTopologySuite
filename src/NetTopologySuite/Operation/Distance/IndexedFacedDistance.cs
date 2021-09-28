using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Computes the distance between the facets (segments and vertices)
    /// of two <see cref="Geometry"/>s
    /// using a Branch-and-Bound algorithm.
    /// The Branch-and-Bound algorithm operates over a
    /// traversal of R-trees built
    /// on the target and the query geometries.
    /// <para>
    /// This approach provides the following benefits:
    /// <list type="bullet">
    /// <item><description>
    /// Performance is dramatically improved due to the use of the
    /// R-tree index
    /// and the pruning due to the Branch-and-Bound approach
    /// </description></item><item><description>
    /// The spatial index on the target geometry is cached
    /// which allow reuse in an repeated query situation.</description></item>
    /// </list>
    /// Using this technique is usually much more performant
    /// than using the brute-force <see cref="Geometry.Distance(Geometry)"/>
    /// when one or both input geometries are large,
    /// or when evaluating many distance computations against
    /// a single geometry.
    /// </para>
    /// </summary>
    /// <remarks>This class is thread-safe.</remarks>
    /// <author>
    /// Martin Davis
    /// </author>
    public class IndexedFacetDistance
    {
        private static readonly FacetSequenceDistance FacetSeqDist = new FacetSequenceDistance();

        /// <summary>
        /// Computes the distance between facets of two geometries.
        /// </summary>
        /// <remarks>
        /// For geometries with many segments or points,
        /// this can be faster than using a simple distance
        /// algorithm.
        /// </remarks>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The distance between the two geometries</returns>
        public static double Distance(Geometry g1, Geometry g2)
        {
            var dist = new IndexedFacetDistance(g1);
            return dist.Distance(g2);
        }

        /// <summary>
        /// Tests whether the facets of two geometries lie within a given distance.
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <param name="distance">The distance limit</param>
        /// <returns><c>true</c> if two facets lie with the given distance</returns>
        public static bool IsWithinDistance(Geometry g1, Geometry g2, double distance)
        {
            var dist = new IndexedFacetDistance(g1);
            return dist.IsWithinDistance(g2, distance);
        }

        /// <summary>
        /// Computes the nearest points of the facets of two geometries.
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The nearest points on the facets of the geometries</returns>
        public static Coordinate[] NearestPoints(Geometry g1, Geometry g2)
        {
            var dist = new IndexedFacetDistance(g1);
            return dist.NearestPoints(g2);
        }

        private readonly STRtree<FacetSequence> _cachedTree;
        private readonly Geometry _baseGeometry;

        /// <summary>
        /// Creates a new distance-finding instance for a given target <see cref="Geometry"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Distances will be computed to all facets of the input geometry.
        /// The facets of the geometry are the discrete segments and points
        /// contained in its components.  </para>
        /// <para>
        /// In the case of <see cref="ILineal"/> and <see cref="IPuntal"/> inputs,
        /// this is equivalent to computing the conventional distance.
        /// </para><para>
        /// In the case of <see cref="IPolygonal"/> inputs, this is equivalent
        /// to computing the distance to the polygon boundaries.
        /// </para>
        /// </remarks>
        /// <param name="g1">A Geometry, which may be of any type.</param>
        public IndexedFacetDistance(Geometry g1)
        {
            _baseGeometry = g1;
            _cachedTree = FacetSequenceTreeBuilder.BuildSTRtree(g1);
        }

        /// <summary>
        /// Computes the distance from the base geometry to the given geometry.
        /// </summary>
        /// <param name="g">The geometry to compute the distance to.</param>
        /// <returns>The computed distance</returns>
        public double Distance(Geometry g)
        {
            var tree2 = FacetSequenceTreeBuilder.BuildSTRtree(g);
            var obj = _cachedTree.NearestNeighbour(tree2, FacetSeqDist);
            var fs1 = obj[0];
            var fs2 = obj[1];
            return fs1.Distance(fs2);
        }

        /// <summary>
        /// Computes the nearest locations on the base geometry
        /// and the given geometry.
        /// </summary>
        /// <param name="g">Ihe geometry to compute the nearest location to.</param>
        /// <returns>The nearest locations.</returns>
        public GeometryLocation[] NearestLocations(Geometry g)
        {
            var tree2 = FacetSequenceTreeBuilder.BuildSTRtree(g);
            var obj = _cachedTree.NearestNeighbour(tree2, FacetSeqDist);
            var fs1 = obj[0];
            var fs2 = obj[1];
            return fs1.NearestLocations(fs2);
        }

        /// <summary>
        /// Computes the nearest locations on the target geometry
        /// and the given geometry.
        /// </summary>
        /// <param name="g">Ihe geometry to compute the nearest point to.</param>
        /// <returns>The nearest points.</returns>
        public Coordinate[] NearestPoints(Geometry g)
        {
            var minDistanceLocation = NearestLocations(g);
            var nearestPts = ToPoints(minDistanceLocation);
            return nearestPts;
        }

        private static Coordinate[] ToPoints(GeometryLocation[] locations)
        {
            if (locations == null)
                return null;
            var nearestPts = new [] {locations[0].Coordinate, locations[1].Coordinate};
            return nearestPts;
        }

        /// <summary>
        /// Tests whether the base geometry lies within
        /// a specified distance of the given geometry.
        /// </summary>
        /// <param name="g">The geometry to test</param>
        /// <param name="maxDistance">The maximum distance to test</param>
        /// <returns><c>true</c> if the geometry lies with the specified distance</returns>
        public bool IsWithinDistance(Geometry g, double maxDistance)
        {
            // short-ciruit check
            double envDist = _baseGeometry.EnvelopeInternal.Distance(g.EnvelopeInternal);
            if (envDist > maxDistance)
                return false;

            var tree2 = FacetSequenceTreeBuilder.BuildSTRtree(g);
            return _cachedTree.IsWithinDistance(tree2, FacetSeqDist, maxDistance);
        }

        private class FacetSequenceDistance : IItemDistance<Envelope, FacetSequence>
        {
            public double Distance(IBoundable<Envelope, FacetSequence> item1, IBoundable<Envelope, FacetSequence> item2)
            {
                return item1.Item.Distance(item2.Item);
            }
        }
    }
}
