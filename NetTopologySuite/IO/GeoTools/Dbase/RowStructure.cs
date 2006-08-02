using System;
using System.Collections;
using System.ComponentModel;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Implements ICustomTypeDescriptor so we can simulate a row object having a property for every field.
	/// </summary>
	/// <remarks>
	/// For an explaination of ICustomTypeDescriptor see http://www.devx.com/dotnet/Article/7874
	/// By implementing this interface, we are able to simulate that an object has lots of properties.
	/// These properties are determined dynamically at run-time. When enumerating throught the 
	/// ShapefileDataReader, RowStructure is the object that gets returned. 
	/// <code>
	/// foreach(object obj in shpDataReader)
	/// {
	///		if (obj.GetType().Name!="RowStructure")
	///		{
	///			// this proves the type returned by shpDataReader
	///		} 
	/// }
	/// </code>
	/// </remarks>
	internal struct RowStructure : ICustomTypeDescriptor 
	{
		private DbaseFieldDescriptor[] _dbaseFields;
		private ArrayList _columnValues;
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbaseFields"></param>
        /// <param name="columnValues"></param>
		public RowStructure(DbaseFieldDescriptor[] dbaseFields, ArrayList columnValues) 
		{
			_dbaseFields = dbaseFields;
			_columnValues  = columnValues;
		}

        /// <summary>
        /// 
        /// </summary>
		public  ArrayList ColumnValues
		{
			get
			{
				return _columnValues;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public AttributeCollection GetAttributes() 
		{
			return AttributeCollection.Empty;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public string GetClassName() 
		{
			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public string GetComponentName() 
		{
			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public TypeConverter GetConverter() 
		{
			return null;
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
		public object GetEditor(Type t) 
		{
			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public EventDescriptor GetDefaultEvent() 
		{
			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
		public EventDescriptorCollection GetEvents(Attribute[] a) 
		{
			return GetEvents();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public EventDescriptorCollection GetEvents() 
		{
			return EventDescriptorCollection.Empty;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
		public object GetPropertyOwner(PropertyDescriptor pd) 
		{
			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public PropertyDescriptor GetDefaultProperty() 
		{
			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
		public PropertyDescriptorCollection GetProperties(Attribute[] a) 
		{
			return GetProperties();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public PropertyDescriptorCollection GetProperties() 
		{
			// add an extra field at the beginning - this will hold the WKT for the Geometry object.
			PropertyDescriptor[] pd = new PropertyDescriptor[_dbaseFields.Length];

			// the regular fields
			for (int i = 0; i < _dbaseFields.Length; i++)
				pd[i] = new ColumnStructure( _dbaseFields[i], i );		
			return new PropertyDescriptorCollection(pd);
		}
	}
}
