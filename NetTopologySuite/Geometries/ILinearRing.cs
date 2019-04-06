namespace GeoAPI.Geometries
{
    public interface ILinearRing : ILineString
    {
        /// <summary>
        /// Gets a value indicating whether this ring is oriented counter-clockwise.
        /// </summary>
        bool IsCCW { get; }
    }
}
