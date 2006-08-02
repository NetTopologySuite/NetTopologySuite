using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// The AngularUnit class holds the standard information stored with angular units.
	/// </summary>
	public class AngularUnit : AbstractInformation, IAngularUnit
	{
		double _radiansPerUnit;

		/// <summary>
		/// Initializes a new instance of the AngularUnit class with a value for the RadiansPerUnit property.
		/// </summary>
		/// <param name="radiansPerUnit">The number of radians per angular unit.</param>
		internal AngularUnit(double radiansPerUnit ) : this(radiansPerUnit, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) { }

		/// <summary>
		/// Initializes a new instance of the AngularUnit class with the specified parameters.
		/// </summary>
		/// <param name="radiansPerUnit">The number of radians per AngularUnit.</param>
		/// <param name="remarks">The provider-supplied remarks.</param>
		/// <param name="authority">The authority-specific identification code.</param>
		/// <param name="authorityCode">The authority-specific identification code.</param>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="abbreviation">The abbreviation.</param>
		internal AngularUnit(double radiansPerUnit, string remarks, string authority, string authorityCode, string name,
							    string alias, string abbreviation)	: base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			_radiansPerUnit = radiansPerUnit;
		}

		#region Implementation of IAngularUnit

		/// <summary>
		///	Gets the number of radians per unit.	
		/// </summary>
		public double RadiansPerUnit
		{
			get
			{
				return _radiansPerUnit;
			}
		}

		#endregion

	}
}
