namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Base Interface for CRSBase Object types.
    /// </summary>
    public interface ICRSObject
    {
        /// <summary>
        /// Gets the CRS type.
        /// </summary>
        CRSTypes Type { get; }
    }
}