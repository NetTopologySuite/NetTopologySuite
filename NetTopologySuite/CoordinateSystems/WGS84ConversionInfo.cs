using GisSharpBlog.NetTopologySuite.Positioning;
namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{ 



	/// <summary>
	/// Parameters for a geographic transformation into WGS84.
	/// </summary>
	/// <remarks>
	/// The Bursa Wolf parameters should be applied to geocentric coordinates,
	/// where the X axis points towards the Greenwich Prime Meridian, the Y axis
	/// points East, and the Z axis points North.
	/// </remarks>
	public struct WGS84ConversionInfo
	{
		/// <summary>
		/// Human readable text describing intended region of transformation.
		/// </summary>
		public string AreaOfUse;
		/// <summary>
		/// Bursa Wolf shift in meters.
		/// </summary>
		public double Dx;
		/// <summary>
		/// Bursa Wolf shift in meters.
		/// </summary>
		public double Dy;
		/// <summary>
		/// Bursa Wolf shift in meters.
		/// </summary>
		public double Dz;
		/// <summary>
		/// Bursa Wolf rotation in arc seconds.
		/// </summary>
		public double Ex;
		/// <summary>
		/// Bursa Wolf rotation in arc seconds.
		/// </summary>
		public double Ey;
		/// <summary>
		/// Bursa Wolf rotation in arc seconds.
		/// </summary>
		public double Ez;
		/// <summary>
		/// Bursa Wolf scaling in parts per million.
		/// </summary>
		public double Ppm;
	}
}

