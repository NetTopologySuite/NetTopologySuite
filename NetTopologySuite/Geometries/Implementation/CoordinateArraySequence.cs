using System;
using System.Text;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// The <c>ICoordinateSequence</c> implementation that <c>Geometry</c>s use by default.
    /// In this implementation, Coordinates returned by ToArray and Coordinate are live --
    /// modifications to them are actually changing the
    /// CoordinateSequence's underlying data.
    /// </summary>
    [Serializable]
    public class CoordinateArraySequence : ICoordinateSequence
    {    
        protected Coordinate[] Coordinates;
       
        /// <summary>
        /// Constructs a sequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        public CoordinateArraySequence(Coordinate[] coordinates) 
        {
            this.Coordinates = coordinates;
            if (coordinates == null)
                this.Coordinates = new Coordinate[0];
        }
        
        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public CoordinateArraySequence(int size) 
        {
            Coordinates = new Coordinate[size];
            for (int i = 0; i < size; i++) 
                Coordinates[i] = new Coordinate();
        }

        /// <summary>
        /// Constructs a sequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordSeq">The coordinate array that will be referenced.</param>      
        public CoordinateArraySequence(ICoordinateSequence coordSeq)
        {
            if (coordSeq != null)
                 Coordinates = new Coordinate[coordSeq.Count];
            else Coordinates = new Coordinate[0];
            for (int i = 0; i < Coordinates.Length; i++) 
                Coordinates[i] = coordSeq.GetCoordinateCopy(i);
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension
        {
            get
            {
                return 3;
            }
        }

        public Ordinates Ordinates
        {
            get { return Ordinates.XYZ; }
        }

        /// <summary>
        /// Get the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>The requested Coordinate instance.</returns>
        public Coordinate GetCoordinate(int i) 
        {
            return Coordinates[i];
        }

        /// <summary>
        /// Get a copy of the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>A copy of the requested Coordinate.</returns>
        public virtual Coordinate GetCoordinateCopy(int i) 
        {
            return new Coordinate(Coordinates[i]);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(int index, Coordinate coord) 
        {
            coord.X = Coordinates[index].X;
            coord.Y = Coordinates[index].Y;
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
            return Coordinates[index].X;
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
            return Coordinates[index].Y;
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
            switch (ordinate) 
            {
                case Ordinate.X:  
                    return Coordinates[index].X;
                case Ordinate.Y:  
                    return Coordinates[index].Y;
                case Ordinate.Z:  
                    return Coordinates[index].Z;
                default:
                    return Double.NaN;
            }            
        }

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        /// <returns>The deep copy.</returns>
        public virtual object Clone()
        {
            Coordinate[] cloneCoordinates = GetClonedCoordinates();
            return new CoordinateArraySequence(cloneCoordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected Coordinate[] GetClonedCoordinates() 
        {
            Coordinate[] cloneCoordinates = new Coordinate[Count];
            for (int i = 0; i < Coordinates.Length; i++) 
                cloneCoordinates[i] = (Coordinate) Coordinates[i].Clone();
            return cloneCoordinates;
        }

        /// <summary>
        /// Returns the length of the coordinate sequence.
        /// </summary>
        public int Count 
        {
            get
            {
                return Coordinates.Length;
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
            switch (ordinate) 
            {
                case Ordinate.X:  
                    Coordinates[index].X = value;
                    break;
                case Ordinate.Y: 
                    Coordinates[index].Y = value;
                    break;
                case Ordinate.Z: 
                    Coordinates[index].Z = value;
                    break;
                //default:
                //    //throw new ArgumentException("invalid ordinate index: " + ordinate);
            }
        }

        /// <summary>
        ///This method exposes the internal Array of Coordinate Objects.       
        /// </summary>
        /// <returns></returns>
        public Coordinate[] ToCoordinateArray() 
        {
            return Coordinates;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public Envelope ExpandEnvelope(Envelope env)
        {
            for (int i = 0; i < Coordinates.Length; i++ ) 
                env.ExpandToInclude(Coordinates[i]);            
            return env;
        }

        /// <summary>
        /// Returns the string representation of the coordinate array.
        /// </summary>
        /// <returns></returns>
        public override string ToString() 
        {
            if (Coordinates.Length > 0) 
            {
                StringBuilder strBuf = new StringBuilder(17 * Coordinates.Length);
                strBuf.Append('(');
                strBuf.Append(Coordinates[0]);
                for (int i = 1; i < Coordinates.Length; i++) 
                {
                    strBuf.Append(", ");
                    strBuf.Append(Coordinates[i]);
                }
                strBuf.Append(')');
                return strBuf.ToString();
            } 
            else return "()";
        }
    }
}
