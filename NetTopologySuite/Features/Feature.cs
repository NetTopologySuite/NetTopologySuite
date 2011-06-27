using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Features
{
//#if !SILVERLIGHT
    [Serializable]
//#endif
     public class Feature
    {
        private IGeometry _geometry;

        /// <summary>
        /// Geometry representation of the feature.
        /// </summary>
        public IGeometry Geometry
        {
            get { return _geometry; }
            set { _geometry = value; }
        }

        private IAttributesTable _attributes;

        /// <summary>
        /// Attributes table of the feature.
        /// </summary>
        public IAttributesTable Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="attributes"></param>
        public Feature(IGeometry geometry, IAttributesTable attributes) : this()
        {
            _geometry = geometry;
            _attributes = attributes;
        }

        /// <summary>
        /// 
        /// </summary>
        public Feature() { }
    }
}
