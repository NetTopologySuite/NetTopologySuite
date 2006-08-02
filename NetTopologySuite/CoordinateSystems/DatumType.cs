using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Type of the datum expressed as an enumerated value.
	/// </summary>
	/// <remarks>
	/// The enumeration is split into ranges which indicate the datum's type.
	/// The value should be one of the predefined values, or within the range
	/// for local types.  This will allow OGC to coordinate the
	/// addition of new interoperable codes.
	/// </remarks>
	public enum DatumType
	{
		/// <summary>
		///  These datums, such as ED50, NAD27 and NAD83, have been designed to
		///  support horizontal positions on the ellipsoid as opposed to positions
		///  in 3-D space.  These datums were designed mainly to support a
		///  horizontal component of a position in a domain of limited extent, such
		///  as a country, a region or a continent.
		/// </summary>
		IHD_Classic = 1001,

		/// <summary>
		/// A geocentric datum is a "satellite age" modern geodetic datum mainly of
		/// global extent, such as WGS84 (used in GPS), PZ90 (used in GLONASS) and
		/// ITRF.  These datums were designed to support both a horizontal
		/// component of position and a vertical component of position (through
		/// ellipsoidal heights).  The regional realizations of ITRF, such as
		/// ETRF, are also included in this category.
		/// </summary>
		IHD_Geocentric = 1002,

		/// <summary>
		/// Highest possible value for horizontal datum types.
		/// </summary>
		IHD_Max = 1999,

		/// <summary>
		/// Lowest possible value for horizontal datum types.
		/// </summary>
		IHD_Min = 1000,

		/// <summary>
		/// Unspecified horizontal datum type.
		/// Horizontal datums with this type should never supply
		/// a conversion to WGS84 using Bursa Wolf parameters.
		/// </summary>
		IHD_Other = 1000,

		/// <summary>
		/// Highest possible value for local datum types.
		/// </summary>
		ILD_Max = 32767,

		/// <summary>
		/// Lowest possible value for local datum types.
		/// </summary>
		ILD_Min = 10000,

		/// <summary>
		/// The vertical datum of altitudes or heights in the atmosphere.  These
		/// are approximations of orthometric heights obtained with the help of
		/// a barometer or a barometric altimeter.  These values are usually
		/// expressed in one of the following units: meters, feet, millibars
		/// (used to measure pressure levels),  or theta value (units used to
		/// measure geopotential height).
		/// </summary>
		IVD_AltitudeBarometric = 2003,

		/// <summary>
		///  This attribute is used to support the set of datums generated
		///  for hydrographic engineering projects where depth measurements below
		///  sea level are needed.  It is often called a hydrographic or a marine
		///  datum.  Depths are measured in the direction perpendicular
		///  (approximately) to the actual equipotential surfaces of the earth's
		///  gravity field, using such procedures as echo-sounding.
		/// </summary>
		IVD_Depth = 2006,

		/// <summary>
		/// A vertical datum for ellipsoidal heights that are measured along the
		/// normal to the ellipsoid used in the definition of horizontal datum.
		/// </summary>
		IVD_Ellipsoidal = 2002,

		/// <summary>
		///  A vertical datum of geoid model derived heights, also called
		///  GPS-derived heights. These heights are approximations of
		///  orthometric heights (H), constructed from the ellipsoidal heights
		/// (h) by the use of the given geoid undulation model (N) through the
		/// equation: H=h-N.
		/// </summary>
		IVD_GeoidModelDerived = 2005,

		/// <summary>
		/// Highest possible value for vertical datum types.
		/// </summary>
		IVD_Max = 2999,

		/// <summary>
		/// Lowest possible value for vertical datum types.
		/// </summary>
		IVD_Min = 2000,

		/// <summary>
		/// A normal height system.
		/// </summary>
		IVD_Normal = 2004,

		/// <summary>
		///  A vertical datum for orthometric heights that are measured along the
		/// plumb line.
		/// </summary>
		IVD_Orthometric = 2001,

		/// <summary>
		/// Unspecified vertical datum type.
		/// </summary>
		IVD_Other = 2000, 
	}
}
