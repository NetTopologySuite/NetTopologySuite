using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Some commonly used axis information. 
	/// </summary>
	public class AxisInfo : IAxisInfo
	{
		private string _name=String.Empty;

		private AxisOrientation _orientation;

		/// <summary>
		/// Default axis info for <var>x</var> values. Increasing ordinates values go East. This 
		/// is usually used with projected coordinate systems.
		/// </summary>
		public static IAxisInfo X
		{
			get
			{
				return new AxisInfo("x",AxisOrientation.East);
			}
		}

		/// <summary> 
		/// Default axis info for <var>y</var> values. Increasing ordinates values go North. This
		/// is usually used with projected coordinate systems.
		/// </summary> 
		public static  IAxisInfo Y 
		{
			get
			{
				return new AxisInfo("y", AxisOrientation.North);
			}
		}

		/// <summary>
		/// Default axis info for longitudes. Increasing ordinates values go East.
		/// This is usually used with geographic coordinate systems.
		/// </summary>
		public static  IAxisInfo Longitude
		{
			get
			{
				return new AxisInfo("Longitude", AxisOrientation.East);
			}
		}

		/// <summary>
		///  Default axis info for latitudes.Increasing ordinates values go North.
		///  This is usually used with geographic coordinate systems.
		/// </summary>
		public static IAxisInfo Latitude
		{
			get
			{
				return new AxisInfo("Latitude", AxisOrientation.North);
			}
		}

		/// <summary>
		/// The default axis for altitude values. Increasing ordinates values go up.
		/// </summary>
		public static  IAxisInfo Altitude
		{
			get
			{
				return new AxisInfo("Altitude",AxisOrientation.Up);
			}
		}

		/// <summary>
		/// Initializes a new instance of the AxisInfo class with a value for the RadiansPerUnit property.
		/// </summary>
		/// <param name="name">The name of the new axis.</param>
		/// <param name="orientation">The orietation of the axis.</param>
		public AxisInfo(string name, AxisOrientation orientation)
		{
			_name = name;
			_orientation = orientation;
		}

		/// <summary>
		/// Gets the name of the axis.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}  

		/// <summary>
		/// Returns the orientation of the axis.
		/// </summary>
		public AxisOrientation Orientation
		{
			get
			{
				return _orientation;
			}
		}
	}
}
