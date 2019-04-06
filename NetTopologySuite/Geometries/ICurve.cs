namespace GeoAPI.Geometries
{
    /// <summary>
    /// Interface for a curve
    /// </summary>
    public interface ICurve : IGeometry
    {
        /// <summary>
        /// Gets a value indicating the sequence of coordinates that make up curve
        /// </summary>
        ICoordinateSequence CoordinateSequence { get; }

        /// <summary>
        /// Gets a value indicating the start point of the curve
        /// </summary>
        IPoint StartPoint { get; }

        /// <summary>
        /// Gets a value indicating the end point of the curve
        /// </summary>
        IPoint EndPoint { get; }

        /// <summary>
        /// Gets a value indicating that the curve is closed. 
        /// In this case <see cref="StartPoint"/> an <see cref="EndPoint"/> are equal.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Gets a value indicating that the curve is a ring. 
        /// </summary>
        bool IsRing { get; }        
    }
}
