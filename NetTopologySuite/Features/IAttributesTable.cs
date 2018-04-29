using System;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Interface definition for an object capable of storing <see cref="IFeature"/>'s attribute data
    /// </summary>
    public interface IAttributesTable
    {
        /// <summary>
        /// Method to add the attribute &quot;<paramref name="attributeName"/>&quot; from the attribute table.
        /// </summary>
        /// <param name="attributeName">The name (or key) of the attribute</param>
        /// <param name="value"></param>
        void AddAttribute(string attributeName, object value);

        /// <summary>
        /// Method to delete the attribute &quot;<paramref name="attributeName"/>&quot; from the attribute table.
        /// </summary>
        /// <param name="attributeName">The name (or key) of the attribute</param>
        void DeleteAttribute(string attributeName);

        /// <summary>
        /// Function to query the <see cref="System.Type"/> of the Attribute &quot;<paramref name="attributeName"/>&quot;
        /// </summary>
        /// <param name="attributeName">The name (or key) of the attribute</param>
        /// <returns>The <see cref="System.Type"/> of the specified attribute</returns>
        Type GetType(string attributeName);

        /// <summary>
        /// Gets or sets the attribute value for the specified <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="attributeName">The name (or key) of the attribute</param>
        /// <returns>The attribute value</returns>
        object this[string attributeName] { get; set; }

        /// <summary>
        /// Function to verify if attribute data for the specified <paramref name="attributeName"/> does exist.
        /// </summary>
        /// <param name="attributeName">The name of the attribute</param>
        /// <returns><value>true</value> if the attribute data exists, otherwise false.</returns>
        bool Exists(string attributeName);

        /// <summary>
        /// Gets a value indicating the number of attributes
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Function to retrieve the names (or keys) of the feature's attributes
        /// </summary>
        /// <returns>
        /// Returns an array of <see cref="string"/> values</returns>
        string[] GetNames();

        /// <summary>
        /// Function to retrieve the attribute data of the feature
        /// </summary>
        /// <returns>
        /// Returns an array of <see cref="object"/> values</returns>
        object[] GetValues();
    }
}
