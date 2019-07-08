namespace NetTopologySuite.Reprojection
{
    /// <summary>
    /// A class that can create reprojection instances
    /// </summary>
    /// <remarks>
    /// This factory only produces Noop reprojection instances.
    /// </remarks>
    public class ReprojectionFactory
    {
        /// <summary>
        /// Method to create a reprojection instance from <paramref name="source"/> to <paramref name="target"/> <see cref="SpatialReference"/>.
        /// </summary>
        /// <param name="source">The source spatial reference.</param>
        /// <param name="target">The target spatial reference.</param>
        /// <returns>A reprojection instance</returns>
        public virtual Reprojection Create(SpatialReference source, SpatialReference target)
        {
            return new Reprojection(source, target);
        }
    }
}
