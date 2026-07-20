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
        /// <returns>A linearized approximation of this geometry.</returns>
        T Linearize();

        /// <summary>
        /// Approximates this geometry through linearization of non-linear components.
        /// </summary>
        /// <param name="arcSegmentLength">The maximum length of </param>
        /// <returns>A linearized approximation of this geometry.</returns>
        T Linearize(double arcSegmentLength);

    }
}
