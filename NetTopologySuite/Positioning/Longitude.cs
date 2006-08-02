using System;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;

namespace GisSharpBlog.NetTopologySuite.Positioning
{
	/// <summary>
	/// Summary description for Longitude.
	/// </summary>
	internal class Longitude : AngularUnit
	{
		/// <summary>
		/// Initializes a new instance of the Longitude class.
		/// </summary>
		public Longitude(double radiansPerUnit) : base(radiansPerUnit) { }
		
		/// <summary>
		/// Minimum legal value for latitude (-90°).
		/// </summary>
		public static double MinimumValue
		{
			get
			{
				return -180.0;
			}
		}

		/// <summary>
		/// Maximum legal value for latitude (+90°).
		/// </summary>
		public static double MaximumValue
		{
			get
			{
				return +180;
			}
		}
	}
}
