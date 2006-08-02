using System;

using GisSharpBlog.NetTopologySuite.Positioning;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Abstract class that other more specialized coordinate system inherit from.
	/// </summary>
	public abstract class CoordinateSystem : AbstractInformation, ICoordinateSystem
	{		
		IUnit _unit=null;

		/// <summary>
		/// Initializes a new instance of the CoordinateSystem class.
		/// </summary>
		/// <param name="remarks">The provider-supplied remarks.</param>
		/// <param name="authority">The authority-specific identification code.</param>
		/// <param name="authorityCode">The authority-specific identification code.</param>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="abbreviation">The abbreviation.</param>
		internal CoordinateSystem(string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
			: base(remarks, authority, authorityCode, name, alias, abbreviation) { }

		/// <summary>
		/// Gets axis information for the specified dimension.
		/// </summary>
		/// <param name="Dimension">The dimension to get the axis information for.</param>
		/// <returns>IAxisInfo containing the axis information.</returns>
		public virtual IAxisInfo GetAxis(int Dimension)
		{
			throw new InvalidOperationException("CoordinateSystem does not have axis information.");
		}

		/// <summary>
		/// Gets the unit information for the specified dimension.
		/// </summary>
		/// <param name="dimension">The dimentsion to get the units information for.</param>
		/// <returns>IUnit containing infomation about the units.</returns>
		public virtual IUnit GetUnits(int dimension)
		{
			if (dimension >= 0 && dimension < this.Dimension)
				return _unit;
			throw new ArgumentOutOfRangeException(String.Format("Dimension must be between 0 and {0}", this.Dimension));
		}
		
		/// <summary>
		/// Dimension of the coordinate system.
		/// </summary>
		public virtual int Dimension
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Gets default envelope of coordinate system.
		/// </summary>
		/// <remarks>
		/// Coordinate systems which are bounded should
		/// return the minimum bounding box of their domain. Unbounded coordinate systems should return
		/// a box which is as large as is likely to be used. For example, a (lon,lat) geographic coordinate
		/// system in degrees should return a box from (-180,-90) to (180,90), and a geocentric coordinate
		/// system could return a box from (-r,-r,-r) to (+r,+r,+r) where r is the approximate radius of the
		/// Earth.
		/// </remarks>
		public virtual Envelope DefaultEnvelope
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
