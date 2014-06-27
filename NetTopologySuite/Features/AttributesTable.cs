using System;
#if PCL
using Hashtable = System.Collections.Generic.Dictionary<object, object>;
#else
using System.Collections;
#endif

namespace NetTopologySuite.Features
{
    /// <summary>
    /// Stores all attributes associated with a single <c>Geometry</c> feature.
    /// </summary>
#if !PCL
    [Serializable]
#endif
    public class AttributesTable : IAttributesTable
    {        
        private const string IndexField = "_NTS_ID_";
        private const int IndexValue = 0;
        
        private readonly Hashtable _attributes = new Hashtable();

        /// <summary>
        /// 
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
            int index = 0;
            object[] values = new object[_attributes.Count];
            foreach (object val in _attributes.Values)
                values[index++] = val;
            return values;
        }

        /// <summary>
        /// Verifies if attribute specified already exists.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
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
                throw new ArgumentException("Attribute " + attributeName + " not exists!");
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
    }
}
