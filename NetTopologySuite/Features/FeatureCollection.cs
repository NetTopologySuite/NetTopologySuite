namespace NetTopologySuite.Features
{
    using System;
    using System.Collections.ObjectModel;

    using NetTopologySuite.CoordinateSystems;

    ///<summary>
    /// Represents a feature collection.
    ///</summary>
    [Serializable]
    public class FeatureCollection
    {
        /// <summary>
        ///     Gets the features.
        /// </summary>
        /// <value>The features.</value>        
        public Collection<Feature> Features { get; private set; }

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
        public Feature this[int index]
        {
            get { return this.Features[index]; }
        }

        /// <summary>
        /// Returns the number of features contained by this <see cref="FeatureCollection" />.
        /// </summary>
        public int Count
        {
            get { return this.Features.Count; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "FeatureCollection" /> class.
        /// </summary>
        /// <param name = "features">The features.</param>
        public FeatureCollection(Collection<Feature> features)
        {
            this.Type = "FeatureCollection";
            this.Features = features ?? new Collection<Feature>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCollection"/> class.
        /// </summary>
        public FeatureCollection() : this(null) { }

        /// <summary>
        /// Adds the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public void Add(Feature feature)
        {
            this.Features.Add(feature);
        }

        /// <summary>
        /// Removes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns 
        /// false if item was not found in the collection.</returns>
        public bool Remove(Feature feature)
        {
            return this.Features.Remove(feature);
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            this.Features.RemoveAt(index);
        }
    }
}
