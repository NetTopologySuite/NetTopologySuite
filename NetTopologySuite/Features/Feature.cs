using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Features
{
    ///<summary>
    /// Feature class
    ///</summary>
#if !PCL    
    [Serializable]
#endif
    public class Feature : IFeature
    {        
        private IGeometry _geometry;

        /// <summary>
        /// Geometry representation of the feature.
        /// </summary>
        public virtual IGeometry Geometry
        {
            get { return _geometry; }
            set { _geometry = value; }
        }

        private IAttributesTable _attributes;

        /// <summary>
        /// Attributes table of the feature.
        /// </summary>
        public virtual IAttributesTable Attributes
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
