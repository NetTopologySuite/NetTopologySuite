namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Interface for geometries that can be either approximated to linear geometries themselves or their components
    /// </summary>
    /// <typeparam name="T">The type of the linearized geometry</typeparam>
    public interface ILinearizable<out T> where T:Geometry
    {
        /// <summary>
        /// Approximates this geometry through linearization of non-linear components.
        /// </summary>
        /// <returns>
        /// A linearized approximation of this geometry. Until arc-aware
        /// densification lands, this returns the control-point chord approximation.
        /// </returns>
        T Linearize();

        /// <summary>
        /// Approximates this geometry through linearization of non-linear components,
        /// with a maximum chord length along each arc.
        /// </summary>
        /// <param name="arcSegmentLength">
        /// The maximum length of each linearized arc segment. Until
        /// tolerance-driven densification is implemented, this overload throws
        /// <see cref="System.NotSupportedException"/>; call <see cref="Linearize()"/>
        /// for the explicit control-point chord approximation.
        /// </param>
        /// <returns>A linearized approximation of this geometry.</returns>
        T Linearize(double arcSegmentLength);

    }
}
