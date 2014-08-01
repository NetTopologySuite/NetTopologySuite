using System;
using System.Collections.ObjectModel;
using NetTopologySuite.CoordinateSystems;

namespace NetTopologySuite.Features
{
    ///<summary>
    /// Represents a feature collection.
    ///</summary>
#if !PCL
    [Serializable]
#endif
    public class FeatureCollection
    {
        /// <summary>
        ///     Gets the features.
        /// </summary>
        /// <value>The features.</value>        
        public Collection<IFeature> Features { get; private set; }

        /// <summary>
        ///     Gets the (mandatory) type of the <see href = "http://geojson.org/geojson-spec.html#geojson-objects">GeoJSON Object</see>.
        /// </summary>
        /// <value>
        ///     The type of the object.
        /// </value>        
        public string Type { get; private set; }

        /// <summary>
        /// Gets or sets the (optional) <see href="http://geojson.org/geojson-spec.html#coordinate-reference-system-objects">Coordinate Reference System Object</see>.
        /// </summary>
        /// <value>
        /// The Coordinate Reference System Objects.
        /// </value>        
        public ICRSObject CRS { get; set; }

        /// <summary>
        /// Returns the indexTh element in the collection.
        /// </summary>
        /// <returns></returns>
        //[JsonIgnore]
        public IFeature this[int index]
        {
            get { return Features[index]; }
        }

        /// <summary>
        /// Returns the number of features contained by this <see cref="FeatureCollection" />.
        /// </summary>
        public int Count
        {
            get { return Features.Count; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "FeatureCollection" /> class.
        /// </summary>
        /// <param name = "features">The features.</param>
        public FeatureCollection(Collection<IFeature> features)
        {
            Type = "FeatureCollection";
            Features = features ?? new Collection<IFeature>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection"/> class.
        /// </summary>
        public FeatureCollection() : this(null) { }

        /// <summary>
        /// Adds the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public void Add(IFeature feature)
        {
            Features.Add(feature);
        }

        /// <summary>
        /// Removes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns 
        /// false if item was not found in the collection.</returns>
        public bool Remove(IFeature feature)
        {
            return Features.Remove(feature);
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            Features.RemoveAt(index);
        }
    }
}
