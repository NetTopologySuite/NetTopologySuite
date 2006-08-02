using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Definition of linear units.
	/// </summary>
	public class LinearUnit : AbstractInformation, ILinearUnit
	{
		private double _metersPerUnit = 0.0;
	
		/// <summary>
		/// Returns the meters linear unit.
		/// </summary>
		public static ILinearUnit Meters
		{
			get
			{
				return new LinearUnit(1.0,"Also known as International metre.","EPSG","9001","metre",String.Empty,String.Empty);
			}
		}

		/// <summary>
		///  Initializes a new instance of the LinearUnit class with a value for the PetersPerUnit property.
		/// </summary>
		/// <param name="metersPerUnit"></param>
		public LinearUnit(double metersPerUnit) : base(String.Empty,String.Empty,String.Empty,String.Empty,String.Empty,String.Empty)
		{
			_metersPerUnit = metersPerUnit;
		}
		/// <summary>
		/// Initializes a new instance of the AngularUnit class with the specified parameters.
		/// </summary>
		/// <param name="metersPerUnit">The number of meters per linear unit.</param>
		/// <param name="remarks">The provider-supplied remarks.</param>
		/// <param name="authority">The authority-specific identification code.</param>
		/// <param name="authorityCode">The authority-specific identification code.</param>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="abbreviation">The abbreviation.</param>
		internal LinearUnit(double metersPerUnit, string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
			: base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			_metersPerUnit = metersPerUnit;
		}
		#region Implementation of ILinearUnit


		/// <summary>
		/// Gets the number of meters per LinearUnit.
		/// </summary>
		public double MetersPerUnit
		{
			get
			{
				return _metersPerUnit;
			}
		}

		#endregion
	}
}
