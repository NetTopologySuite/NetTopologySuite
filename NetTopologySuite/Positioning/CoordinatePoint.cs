using System;

namespace GisSharpBlog.NetTopologySuite.Positioning
{
	/// <summary>
	/// A position defined by a list of numbers.
	/// </summary>
	/// <remarks>
	/// The ordinate values are indexed from 0 to (NumDim-1), where NumDim is the
	/// dimension of the coordinate system the coordinate point belongs in.
	/// </remarks>
	public struct CoordinatePoint
	{
		/// <summary>
		/// The ordinates of the coordinate point.
		/// </summary>
		public double[] Ord;
	}
}
