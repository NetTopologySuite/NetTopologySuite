using System;

namespace GisSharpBlog.NetTopologySuite.Positioning
{
	/// <summary>
	/// A box defined by two positions
	/// </summary>
	/// <remarks>
	/// The two positions must have the same dimension.
	/// Each of the ordinate values in the minimum point must be less than or equal
	/// to the corresponding ordinate value in the maximum point.  Please note that
	/// these two points may be outside the valid domain of their coordinate system.
	/// (Of course the points and envelope do not explicitly reference a coordinate
	/// system, but their implicit coordinate system is defined by their context.)
	/// </remarks>
	public struct Envelope
	{
		/// <summary>
		/// Point containing minimum ordinate values. 
		/// </summary>
		public CoordinatePoint MaxCP;

		/// <summary>
		/// Point containing maximum ordinate values.
		/// </summary>
		public CoordinatePoint MinCP;
	}
}
