using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Features
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Feature
    {
        private IGeometry geometry = null;

        /// <summary>
        /// Geometry representation of the feature.
        /// </summary>
        public IGeometry Geometry
        {
            get { return geometry; }
            set { geometry = value; }
        }

        private IAttributesTable attributes = null;

        /// <summary>
        /// Attributes table of the feature.
        /// </summary>
        public IAttributesTable Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="attributes"></param>
        public Feature(IGeometry geometry, IAttributesTable attributes) : this()
        {
            this.geometry = geometry;
            this.attributes = attributes;
        }

        /// <summary>
        /// 
        /// </summary>
        public Feature() { }
    }
}
