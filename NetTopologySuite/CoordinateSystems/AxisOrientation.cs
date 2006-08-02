using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Describes the orienation of the axis.
	/// </summary>
	public enum AxisOrientation
	{
		/// <summary>
		/// Down
		/// </summary>
		Down = 6,

		/// <summary>
		/// East.
		/// </summary>
		East = 3,
		
        /// <summary>
		/// North.
		/// </summary>
		North = 1,
		
        /// <summary>
		/// Other
		/// </summary>
		Other = 0,
		
        /// <summary>
		/// South.
		/// </summary>
		South = 2,
		
        /// <summary>
		/// Up.
		/// </summary>
		Up = 5,
		
        /// <summary>
		/// West
		/// </summary>
		West = 4, 
	}
}
