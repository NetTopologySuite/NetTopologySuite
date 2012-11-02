using System;
using System.Text;
using GeoAPI.Geometries;

namespace NetTopologySuite.Samples.Geometries
{	
	/// <summary> 
    /// Demonstrates how to implement a CoordinateSequence for a new kind of
    /// coordinate (an <c>ExtendedCoordinate</c>} in this example). In this
	/// implementation, Coordinates returned by ToArray and #get are live -- parties
	/// that change them are actually changing the ExtendedCoordinateSequence's 
	/// underlying data.
	/// </summary>	
	public class ExtendedCoordinateSequence : ICoordinateSequence
	{
		[Obsolete]
        public static ExtendedCoordinate[] Copy(Coordinate[] coordinates)
		{
			var copy = new ExtendedCoordinate[coordinates.Length];
            Array.Copy(CopyInternal(coordinates), copy, coordinates.Length);
		    return copy;
		}
		
        private static Coordinate[] CopyInternal(Coordinate[] coordinates)
        {
            var copy = new Coordinate[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
                copy[i] = new ExtendedCoordinate(coordinates[i]);
            return copy;
            
        }

		private readonly Coordinate[] _coordinates;
		
		/// <summary> Copy constructor -- simply aliases the input array, for better performance.
		/// </summary>
		public ExtendedCoordinateSequence(ExtendedCoordinate[] coordinates)
		{
			_coordinates = new Coordinate[coordinates.Length];
            Array.Copy(coordinates, _coordinates, coordinates.Length);
		}
		
		/// <summary> Constructor that makes a copy of an existing array of Coordinates.
		/// Always makes a copy of the input array, since the actual class
		/// of the Coordinates in the input array may be different from ExtendedCoordinate.
		/// </summary>
		public ExtendedCoordinateSequence(Coordinate[] copyCoords)
		{
			_coordinates = CopyInternal(copyCoords);
		}

        /// <summary>
        /// Returns (possibly a copy of) the ith Coordinate in this collection.
        /// Whether or not the Coordinate returned is the actual underlying
        /// Coordinate or merely a copy depends on the implementation.
        /// Note that in the future the semantics of this method may change
        /// to guarantee that the Coordinate returned is always a copy. Callers are
        /// advised not to assume that they can modify a CoordinateSequence by
        /// modifying the Coordinate returned by this method.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
		public Coordinate GetCoordinate(int i)
		{
			return _coordinates[i];
		}

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
		public object Clone()
		{
			var cloneCoordinates = new ExtendedCoordinate[Count];
			for (var i = 0; i < _coordinates.Length; i++)
			{
				cloneCoordinates[i] = (ExtendedCoordinate) _coordinates[i].Clone();
			}
			
			return new ExtendedCoordinateSequence(cloneCoordinates);
		}

	    public Ordinates Ordinates
	    {
	        get { return Ordinates.XYZM; }
	    }

	    /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
		public virtual int Count
		{
            get
            {
                return _coordinates.Length;
            }
		}

        /// <summary>
        /// Returns (possibly copies of) the Coordinates in this collection.
        /// Whether or not the Coordinates returned are the actual underlying
        /// Coordinates or merely copies depends on the implementation. Note that
        /// if this implementation does not store its data as an array of Coordinates,
        /// this method will incur a performance penalty because the array needs to
        /// be built from scratch.
        /// </summary>
        /// <returns></returns>
		public virtual Coordinate[] ToCoordinateArray()
		{
			
            return _coordinates;
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override string ToString()
		{
			StringBuilder strBuf = new StringBuilder();
			strBuf.Append("ExtendedCoordinateSequence [");
			for (int i = 0; i < _coordinates.Length; i++)
			{
				if (i > 0)
					strBuf.Append(", ");
				strBuf.Append(_coordinates[i]);
			}
			strBuf.Append("]");
			return strBuf.ToString();
		}

        /// <summary>
        /// Gets the coordinate copy.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Coordinate GetCoordinateCopy(int index)
        {
            return new Coordinate(_coordinates[index]);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(int index, Coordinate coord)
        {
            var exc = (ExtendedCoordinate) _coordinates[index];
            coord.X = exc.X;
            coord.Y = exc.Y;
            coord.Z = exc.Z;
            
            var exCoord = coord as ExtendedCoordinate;
            if (exCoord != null) 
                exCoord.M = exc.M;
        }


        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public double GetX(int index)
        {
            return _coordinates[index].X;
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public double GetY(int index)
        {
            return _coordinates[index].Y;
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public double GetOrdinate(int index, Ordinate ordinate)
        {
            var exc = (ExtendedCoordinate) _coordinates[index];
            switch (ordinate)
            {
                case Ordinate.X: 
                    return exc.X;
                case Ordinate.Y:
                    return exc.Y;
                case Ordinate.Z:
                    return exc.Z;
                case Ordinate.M:
                    return exc.M;
                default:
                    return Double.NaN;
            }            
        }

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>
        public void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            var exc = (ExtendedCoordinate)_coordinates[index];
            switch (ordinate)
            {
                case Ordinate.X:
                    exc.X = value;
                    break;
                case Ordinate.Y:
                    exc.Y = value;
                    break;
                case Ordinate.Z:
                    exc.Z = value;
                    break;
                case Ordinate.M:
                    exc.M = value;
                    break;
            }
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public Envelope ExpandEnvelope(Envelope env)
        {
            for (int i = 0; i < _coordinates.Length; i++)
                env.ExpandToInclude(_coordinates[i]);
            return env;
        }

	    /// <summary>
	    /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
	    /// </summary>
	    /// <returns>A reversed version of this sequence</returns>
	    public ICoordinateSequence Reversed()
	    {
            var coordinates = new ExtendedCoordinate[Count];
            for (var i = 0; i < Count; i++)
            {
                coordinates[Count - i - 1] = new ExtendedCoordinate(coordinates[i]);
            }
            return new ExtendedCoordinateSequence(coordinates);
        }

	    /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension
        {
            get
            {
                return 4;
            }
        }
	}
}