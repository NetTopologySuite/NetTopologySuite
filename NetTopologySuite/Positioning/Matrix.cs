using System;

namespace GisSharpBlog.NetTopologySuite.Positioning
{
	/// <summary>
	/// A two dimensional array of numbers. 
	/// </summary>
	public struct Matrix
	{
		/// <summary>
		/// Elements of the matrix. 
		/// </summary>
		/// <remarks>
		/// The elements should be stored in a rectangular two dimensional array
		/// So in Java/ C#, all double[] elements of the outer array must have the
		/// same size.  In COM, this is represented as a 2D SAFEARRAY.
		/// </remarks>
		public double[] Elt;
	}
}
