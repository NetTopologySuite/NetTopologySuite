namespace NetTopologySuite.Elevation
{
    /// <summary>
    /// An interface for classes that may have access to an <see cref="ElevationModel"/>.
    /// </summary>
    internal interface IHasElevationModel
    {
        /// <summary>
        /// Gets the elevation model 
        /// </summary>
        ElevationModel ElevationModel { get; }

        /// <summary>
        /// Method to attempt and set the <see cref="ElevationModel"/>.
        /// </summary>
        /// <param name="model">The elevation model to set</param>
        /// <returns><c>true</c> if setting the elevation model was successful.</returns>
        bool TrySetElevationModel(ElevationModel model);
    }
}
