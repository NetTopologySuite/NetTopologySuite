using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
{	
	/// <summary> 
    /// Creates ExtendedCoordinateSequenceFactory internally represented
	/// as an array of <c>ExtendedCoordinate</c>s.
	/// </summary>
	public class ExtendedCoordinateSequenceFactory : ICoordinateSequenceFactory
	{	
		private static ExtendedCoordinateSequenceFactory instance;
		
		private ExtendedCoordinateSequenceFactory() { }
		
		/// <summary> Returns the singleton instance of ExtendedCoordinateSequenceFactory
		/// </summary>
		public static ExtendedCoordinateSequenceFactory Instance()
		{
			return instance;
		}
		
		/// <summary> Returns an ExtendedCoordinateSequence based on the given array -- the array is used
		/// directly if it is an instance of ExtendedCoordinate[]; otherwise it is
		/// copied.
		/// </summary>
		public virtual ICoordinateSequence Create(ICoordinate[] coordinates)
		{
			return coordinates is ExtendedCoordinate[] ?
                new ExtendedCoordinateSequence((ExtendedCoordinate[]) coordinates) :
                new ExtendedCoordinateSequence(coordinates);
		}

	    public ICoordinateSequence Create(ICoordinateSequence coordSeq)
	    {
	        throw new NotImplementedException();
	    }

	    public ICoordinateSequence Create(int size, int dimension)
	    {
	        throw new NotImplementedException();
	    }

	    static ExtendedCoordinateSequenceFactory()
		{
			instance = new ExtendedCoordinateSequenceFactory();
		}
	}
}