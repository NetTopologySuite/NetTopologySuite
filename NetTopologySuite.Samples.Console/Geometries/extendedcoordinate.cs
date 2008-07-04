using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
{
	public class ExtendedCoordinate : Coordinate
	{				
		// A Coordinate subclass should provide all of these methods
		
		/// <summary> 
        /// Default constructor
		/// </summary>
		public ExtendedCoordinate() : base()
		{
			this.m = 0.0;
		}
		
		public ExtendedCoordinate(double x, double y, double z, double m) : base(x, y, z)
		{
			this.m = m;
		}
		
		public ExtendedCoordinate(ICoordinate coord) : base(coord)
		{
			m = 0.0;
		}
		
		public ExtendedCoordinate(ExtendedCoordinate coord) : base(coord)
		{
			m = coord.m;
		}
		
		/// <summary> 
        /// An example of extended data.
		/// The m variable holds a measure value for linear referencing
		/// </summary>		
		private double m;
        
        virtual public double M
        {
            get { return m; }
            set { this.m = value; }
        }
		
		public override string ToString()
		{
			string stringRep = X + " " + Y + " m=" + M;
			return stringRep;
		}
	}
}