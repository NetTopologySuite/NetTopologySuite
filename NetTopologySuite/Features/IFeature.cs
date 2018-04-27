using GeoAPI.Geometries;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Interface for feature classes
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Gets or sets the attributes for the feature
        /// </summary>
        IAttributesTable Attributes { get; set; }

        /// <summary>
        /// Gets or sets the feature's geometry
        /// </summary>
        IGeometry Geometry { get; set; }

        /// <summary>
        /// Gets or sets the feature's geometry
        /// </summary>
        Envelope BoundingBox { get; set; }
    }
}
