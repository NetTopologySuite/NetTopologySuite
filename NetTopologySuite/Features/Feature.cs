using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Features
{
    ///<summary>
    /// Feature class
    ///</summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class Feature : IFeature
    {
        /// <summary>
        /// Gets or sets a value indicating how bounding box on <see cref="Feature"/> should be handled
        /// </summary>
        /// <remarks>Default is <value>false</value></remarks>
        public static bool ComputeBoundingBoxWhenItIsMissing { get; set; }

        private IGeometry _geometry;
        private IAttributesTable _attributes;
        private Envelope _boundingBox;

        /// <summary>
        /// Creates an instace of this class
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="attributes">The attributes</param>
        public Feature(IGeometry geometry, IAttributesTable attributes)
            : this()
        {
            _geometry = geometry;
            _attributes = attributes;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public Feature() { }
        
        /// <summary>
        /// Geometry representation of the feature.
        /// </summary>
        public virtual IGeometry Geometry
        {
            get => _geometry;
            set => _geometry = value;
        }

        /// <summary>
        /// Attributes table of the feature.
        /// </summary>
        public virtual IAttributesTable Attributes
        {
            get => _attributes;
            set => _attributes = value;
        }


        /// <summary>
        /// Gets or sets the (optional) <see href="http://geojson.org/geojson-spec.html#geojson-objects"> Bounding box (<c>bbox</c>) Object</see>.
        /// </summary>
        /// <value>
        /// A <see cref="Envelope"/> describing the bounding box or <value>null</value>.
        /// </value>        
        public Envelope BoundingBox
        {
            get
            {
                if (_boundingBox != null)
                    return new Envelope(_boundingBox);

                if (_geometry != null && ComputeBoundingBoxWhenItIsMissing)
                    return _geometry.EnvelopeInternal;

                return null;
            }
            set => _boundingBox = value;
        }
    }
}
