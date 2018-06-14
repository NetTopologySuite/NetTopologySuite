using System;
using GeoAPI.Geometries;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Computes the distance between the facets (segments and vertices)
    /// of two <see cref="IGeometry"/>s
    /// using a Branch-and-Bound algorithm.
    /// The Branch-and-Bound algorithm operates over a
    /// traversal of R-trees built
    /// on the target and possibly also the query geometries.
    /// <para>
    /// This approach provides the following benefits:
    /// <list type="Bullet">
    /// <item>
    /// Performance is improved due to the effects of the R-tree index
    /// and the pruning due to the Branch-and-Bound approach
    /// </item><item>
    /// The spatial index on the target geometry can be cached
    /// to allow reuse in an incremental query situation.</item>
    /// </list>
    /// Using this technique can be much more performant
    /// than using <see cref="IGeometry.Distance(IGeometry)"/>
    /// when one or both input geometries are large,
    /// or when evaluating many distance computations against
    /// a single geometry.
    /// </para>
    /// </summary>
    /// <remarks>This class is not thread-safe.</remarks>
    /// <author>
    /// Martin Davis
    /// </author>
    public class IndexedFacetDistance
    {
        /// <summary>
        /// Computes the distance between two geometries using the indexed approach.
        /// </summary>
        /// <remarks>
        /// For geometries with many segments or points,
        /// this can be faster than using a simple distance
        /// algorithm.
        /// </remarks>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The distance between the two geometries</returns>
        public static double Distance(IGeometry g1, IGeometry g2)
        {
            var dist = new IndexedFacetDistance(g1);
            return dist.GetDistance(g2);
        }

        private readonly STRtree<FacetSequence> _cachedTree;

        /// <summary>
        /// Creates a new distance-finding instance for a given target <see cref="IGeometry"/>.
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
        /// to computing the distance to the polygons boundaries.
        /// </para>
        /// </remarks>
        /// <param name="g1">A Geometry, which may be of any type.</param>
        public IndexedFacetDistance(IGeometry g1)
        {
            _cachedTree = FacetSequenceTreeBuilder.BuildSTRtree(g1);
        }

        /// <summary>
        /// Computes the distance from the base geometry to the given geometry.
        /// </summary>
        /// <param name="g">The geometry to compute the distance to.</param>
        /// <returns>The computed distance</returns>
        public double GetDistance(IGeometry g)
        {
            var tree2 = FacetSequenceTreeBuilder.BuildSTRtree(g);
            var obj = _cachedTree.NearestNeighbour(tree2, new FacetSequenceDistance());
            return FacetDistance(obj);
        }

        private static double FacetDistance(FacetSequence[] obj)
        {
            return obj[0].Distance(obj[1]);
        }

        /**
         * Computes the distance from the base geometry to
         * the given geometry, up to and including a given
         * maximum distance.
         *
         * @param g the geometry to compute the distance to
         * @param maximumDistance the maximum distance to compute.
         *
         * @return the computed distance,
         *    or <tt>maximumDistance</tt> if the true distance is determined to be greater
         */
        // TODO: implement this
        /*
        public double getDistanceWithin(Geometry g, double maximumDistance)
        {
          STRtree tree2 = FacetSequenceTreeBuilder.build(g);
          Object[] obj = cachedTree.nearestNeighbours(tree2,
              new FacetSequenceDistance());
          return facetDistance(obj);
        }
        */

        /**
         * Tests whether the base geometry lies within
         * a specified distance of the given geometry.
         *
         //* @param g the geometry to test
         //* @param maximumDistance the maximum distance to test
         //* @return true if the geometry lies with the specified distance
         */
        // TODO: implement this
        /*
        public boolean isWithinDistance(Geometry g, double maximumDistance)
        {
          STRtree tree2 = FacetSequenceTreeBuilder.build(g);
          double dist = findMinDistance(cachedTree.getRoot(), tree2.getRoot(), maximumDistance);
          if (dist <= maximumDistance)
            return false;
          return true;
        }
        */

        private class FacetSequenceDistance : IItemDistance<Envelope, FacetSequence>
        {
            public double Distance(IBoundable<Envelope, FacetSequence> item1, IBoundable<Envelope, FacetSequence> item2)
            {
                return item1.Item.Distance(item2.Item);
            }
        }
    }
}