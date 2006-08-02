using System;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;

namespace GisSharpBlog.NetTopologySuite.Positioning
{
	/// <summary>
	/// 
	/// </summary>
	internal class Latitude : AngularUnit
	{
		/// <summary>
		/// Initializes a new instance of the Longitude class.
		/// </summary>
        /// <param name="radiansPerUnit"></param>
		public Latitude(double radiansPerUnit) : base(radiansPerUnit) { }
	
		
		/// <summary>
		/// Minimum legal value for latitude (-90°).
		/// </summary>
		public static double MinimumValue
		{
			get
			{
				return -90.0;
			}
		}

		/// <summary>
		/// Maximum legal value for latitude (+90°).
		/// </summary>
		public static double MaximumValue
		{
			get
			{
				return +90;
			}
		}
	}
}
