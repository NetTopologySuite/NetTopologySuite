using System;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	/// <summary>
	/// Helper class that makes it easy to get named parameter from a ListDictionary. In particular this 
	/// class makes it easier to get a double as a named value in the ListDictionary.
	/// </summary>
	public class ParameterList : System.Collections.Specialized.ListDictionary
	{

		/// <summary>
		/// Gets an item from the collection and converts it to a double.
		/// </summary>
		/// <param name="key">The key of the item in the collection.</param>
		/// <param name="defaultValue">A default value if the item is not in the collection.</param>
		/// <returns>Double.</returns>
		public double GetDouble(string key, double defaultValue)
		{
			if (this.Contains(key))
			{
				return (double)this[key];
			}
			else
			{
				return defaultValue;
			}
		}
		/// <summary>
		/// Gets an item from the collection and converts it to a double.
		/// </summary>
		/// <param name="key">The key of the item in the collection.</param>
		/// <returns>Double</returns>
		/// <exception cref="ArgumentException">If the key does not exist or the value cannot be cast to a double.</exception>
		public double GetDouble(string key)
		{
			if (this.Contains(key))
			{
				try
				{
					return (double)this[key];
				}
				catch(Exception e)
				{
					throw new ArgumentException(String.Format("key {0} has an invalid entry.",key),e);
				}
			}
			else
			{
				throw new ArgumentException(String.Format("The key with a value of '{0}' is not in the list.",key));
			}
		}

	}
}
