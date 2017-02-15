using System;
#if SERIALIZATION_COMPAT_NETTOPOLOGYSUITE_FEATURES_ATTRIBUTESTABLE
using Hashtable = System.Collections.Hashtable;
#else
using Hashtable = System.Collections.Generic.Dictionary<string, object>;
#endif
using System.Collections.Generic;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Stores all attributes associated with a single <c>Geometry</c> feature.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class AttributesTable : IAttributesTable
    {
        //private const string IndexField = "_NTS_ID_";
        //private const int IndexValue = 0;

        /// <summary>
        /// Gets or sets a value indicating if setting <see cref="this[string]"/> with a 
        /// nonexistant index will throw an exception or if the attribute/value pair will 
        /// silently be added.
        /// </summary>
        public static bool AddAttributeWithIndexer { get; set; }

        private readonly Hashtable _attributes;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public AttributesTable()
        {
            _attributes = new Hashtable(); 
        }

        /// <summary>
        /// Creates an instance of this class using the provided enumeration of key/value pairs
        /// </summary>
        /// <exception cref="ArgumentException">If the enumeration contains objects</exception>
        public AttributesTable(IEnumerable<object[]> objects) : this()
        {
            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;
                if (obj.Length != 2)
                    throw new ArgumentException("objects");

                if (!(obj[0] is string))
                    throw new ArgumentException("objects");

                AddAttribute((string)obj[0], obj[1]);
            }
        }

        /// <summary>
        /// Gets a value indicating the number of attributes
        /// </summary>
        public int Count
        {
            get { return _attributes.Count; }
        }

        /// <summary>
        /// Returns a <c>string</c> array containing 
        /// all names of present attributes.
        /// </summary>
        /// <returns></returns>
        public string[] GetNames()
        {
            int index = 0;
            string[] names = new string[_attributes.Count];
            foreach (string name in _attributes.Keys)
                names[index++] = name;
            return names;
        }

        /// <summary>
        /// Returns a <c>object</c> array containing 
        /// all values of present attributes.
        /// </summary>
        /// <returns></returns>
        public object[] GetValues()
        {
            var index = 0;
            var values = new object[_attributes.Count];
            foreach (var val in _attributes.Values)
                values[index++] = val;
            return values;
        }

        /// <summary>
        /// Verifies if attribute specified already exists.
        /// </summary>
        /// <param name="attributeName">The name of the attribute</param>
        /// <returns><value>true</value> if the attribute exists, otherwise false.</returns>
        public bool Exists(string attributeName)
        {
            return _attributes.ContainsKey(attributeName);
        }

        /// <summary>
        /// Build a field with the given value and add it to attributes table.        
        /// </summary>
        /// <param name="attributeName">Name of the new attribute.</param>        
        /// <param name="attributeValue">Value for attribute (can be null).</param>
        /// <exception cref="ArgumentException">If attribute already exists.</exception>
        public void AddAttribute(string attributeName, object attributeValue)
        {
            if (Exists(attributeName))
                throw new ArgumentException("Attribute " + attributeName + " already exists!");
            _attributes.Add(attributeName, attributeValue);
        }        

        /// <summary>
        /// Delete the specified attribute from the table.
        /// </summary>
        /// <param name="attributeName"></param>       
        public virtual void DeleteAttribute(string attributeName)
        {
            if (!Exists(attributeName))
                throw new ArgumentException("Attribute " + attributeName + " not exists!");
            _attributes.Remove(attributeName);
        }

        /// <summary>
        /// Return the <c>System.Type</c> of the specified attribute, 
        /// useful for casting values retrieved with GetValue methods.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public Type GetType(string attributeName)
        {
            if (!Exists(attributeName))
                throw new ArgumentException("Attribute " + attributeName + " not exists!");

            // if we have null, we can't determine the objects type, thus return typeof(object)
            if (_attributes[attributeName] == null)
                return typeof(object);

            return _attributes[attributeName].GetType();
        }

        /// <summary>
        /// Get the value of the specified attribute.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        protected object GetValue(string attributeName)
        {
            if (!Exists(attributeName))
                throw new ArgumentException("Attribute " + attributeName + " not exists!");
            return _attributes[attributeName];
        }

        /// <summary>
        /// Set the value of the specified attribute.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        protected void SetValue(string attributeName, object attributeValue)
        {
            if (!Exists(attributeName))
            {
                if (!AddAttributeWithIndexer)
                    throw new ArgumentException("Attribute " + attributeName + " not exists!");
                AddAttribute(attributeName, attributeValue);
                return;
            }
            _attributes[attributeName] = attributeValue;
        }

        /// <summary>
        /// Get / Set the value of the specified attribute.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public object this[string attributeName]
        {
            get { return GetValue(attributeName); }
            set { SetValue(attributeName, value); }
        }

        /// <summary>
        /// Method to merge this attribute table with another attribute table
        /// </summary>
        /// <param name="other">The other attribute table</param>
        /// <param name="preferThis">A value indicating if values in this attribute table are preferable 
        /// over those in <paramref name="other"/>.  The default is <value>true</value>.
        /// </param>
        public void MergeWith(IAttributesTable other, bool preferThis = true)
        {
            foreach (var name in other.GetNames())
            {
                if (!Exists(name))
                    AddAttribute(name, other[name]);
                else
                {
                    if (!preferThis)
                        this[name] = other[name];
                }
            }
        }

    }
}
