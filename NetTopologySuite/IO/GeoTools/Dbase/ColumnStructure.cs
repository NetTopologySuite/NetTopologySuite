using System;
using System.ComponentModel;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// This class is used in conjunction with RowStructure. 
	/// </summary>
	/// <remarks>
	/// For an explaination of PropertyDescriptor see http://www.devx.com/dotnet/Article/7874
	/// and the remarks for RowStructure. This class inherits from PropertyDescriptor. 
	/// The PropertyDescriptor describes a property - in this case a dynamically generated property.
	/// </remarks>
	internal class ColumnStructure : PropertyDescriptor
	{
		private DbaseFieldDescriptor _dbaseField;
		private int _index;

		/// <summary>
		/// Initializes a new instance of the ColumnStructure class.
		/// </summary>
        /// <param name="dbaseField"></param>
        /// <param name="index"></param>
		public ColumnStructure(DbaseFieldDescriptor dbaseField, int index) : base(dbaseField.Name, null)
		{
			_dbaseField = dbaseField;
			_index=index;	
		}

        /// <summary>
        /// 
        /// </summary>
		public override Type ComponentType
		{
			get
			{
				return typeof(RowStructure);
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public override Type PropertyType
		{
			get
			{
				// return the type of the dbase field.
				return _dbaseField.Type;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
		public override bool CanResetValue(object component)
		{
			return false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
		public override void ResetValue(object component) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
		public override void SetValue(object component, object value) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
		public override object GetValue(object component)
		{
			// gets the 'parent' and gets a value of out of the ColumnValues property.
			return ((RowStructure)component).ColumnValues[_index];
		}


		// awc: Added this propety, because when creating a DataSet from the DataReader, we need
		// to know how long the field length is in the dbase file so we can create a column
		// of the appropriate length in the database.
        /// <summary>
        /// 
        /// </summary>
		public int Length
		{
			get
			{
				return _dbaseField.Length;
			}
		}
	}
}
