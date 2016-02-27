using System;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Extension methods for <see cref="IFeature"/>s.
    /// </summary>
    public static class FeatureExtensions
    {
        static FeatureExtensions()
        {
            // According to GeoJSON name of the feature's identifier
            IdAttributeName = "id";
        }

        /// <summary>
        /// Gets or sets a name that is used to retrieve the ID of a feature from the attribute table
        /// </summary>
        public static string IdAttributeName { get; set; }

        /// <summary>
        /// Function to get a feature's ID
        /// </summary>
        /// <param name="feature">The feature</param>
        /// <returns>The feature's ID if one has been assigned, otherwise <value>null</value></returns>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="feature"/> is <valu>null</valu></exception>
        public static object ID(IFeature feature)
        {
            return HasID(feature)
                ? feature.Attributes[IdAttributeName]
                : null;
        }

        /// <summary>
        /// Function to evaluate if a feature has an ID
        /// </summary>
        /// <param name="feature">The feature</param>
        /// <returns><value>true</value> if <paramref name="feature"/> has an identifier assigned, otherwise <value>false</value></returns>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="feature"/> is <valu>null</valu></exception>
        public static bool HasID(IFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException("feature");

            return feature.Attributes.Exists(IdAttributeName);
        }
    }
}