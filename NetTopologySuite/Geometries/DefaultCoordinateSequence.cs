using System;
using System.Text;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// The CoordinateSequence implementation that Geometries use by default. In
    /// this implementation, Coordinates returned by ToArray and GetCoordinates are live --
    /// parties that change them are actually changing the
    /// DefaultCoordinateSequence's underlying data.
    /// </summary>
//#if !SILVERLIGHT
    [Serializable]
//#endif
    [Obsolete("No longer used.")]
    public class DefaultCoordinateSequence : ICoordinateSequence
    {
        private Coordinate[] coordinates = null;

        /// <summary>
        /// Constructs a DefaultCoordinateSequence based on the given array (the
        /// array is not copied).
        /// </summary>
        /// <param name="coordinates">Coordinate array that will be assimilated.</param>
        public DefaultCoordinateSequence(Coordinate[] coordinates)
        {
            if (Geometry.HasNullElements(coordinates))
                throw new ArgumentException("Null coordinate");            
            this.coordinates = coordinates;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordSeq"></param>
        public DefaultCoordinateSequence(ICoordinateSequence coordSeq)
        {
            coordinates = new Coordinate[coordSeq.Count];
            for (int i = 0; i < coordinates.Length; i++)
                coordinates[i] = coordSeq.GetCoordinateCopy(i);
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public DefaultCoordinateSequence(int size)
        {
            coordinates = new Coordinate[size];
            for (int i = 0; i < size; i++)
                coordinates[i] = new Coordinate();
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
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public Coordinate GetCoordinate(int i)
        {
            return coordinates[i];
        }
        /// <summary>
        /// Returns a copy of the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>The copy of the coordinate specified.</return>
        public Coordinate GetCoordinateCopy(int i)
        {
            return new Coordinate(coordinates[i]);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = coordinates[index].X;
            coord.Y = coordinates[index].Y;
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
            return coordinates[index].X;
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
            return coordinates[index].Y;
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
                    return coordinates[index].X;
                case Ordinate.Y: 
                    return coordinates[index].Y;
                case Ordinate.Z: 
                    return coordinates[index].Z;
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
            switch (ordinate)
            {
                case Ordinate.X: 
                    coordinates[index].X = value;
                    break;
                case Ordinate.Y: coordinates[index].Y = value;
                    break;
                case Ordinate.Z: 
                    coordinates[index].Z = value;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public object this[int i]
        {
            get
            {
                return coordinates[i];
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
            for (int i = 0; i < coordinates.Length; i++)
                env.ExpandToInclude(coordinates[i]);
            return env;
        }

        /// <summary>
        /// Returns a deep copy of the object passed.
        /// </summary>
        /// <returns>The copied object.</returns>
        public object Clone()
        {
            Coordinate[] cloneCoordinates = new Coordinate[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
                cloneCoordinates[i] = (Coordinate) coordinates[i].Clone();            
            return new DefaultCoordinateSequence(cloneCoordinates);
        }

        /// <summary>
        /// Returns the elements number of the coordinate sequence.
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                return coordinates.Length;
            }
        }

        /// <summary>
        /// Returns the elements number of the coordinate sequence.
        /// </summary>
        /// <value>The length.</value>
        public int Length
        {
            get
            {
                return coordinates.Length;
            }
        }

        /// <summary>
        /// This method exposes the internal Array of Coordinate Objects.
        /// </summary>
        /// <returns>Coordinate[] array.</returns>
        public Coordinate[] ToCoordinateArray()
        {
            return coordinates;
        }

        /// <summary>
        /// Returns the string Representation of the coordinate array
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            if (coordinates.Length > 0)
            {
                StringBuilder sb = new StringBuilder(17 * coordinates.Length);
                sb.Append('(');
                sb.Append(coordinates[0]);
                for (int i = 1; i < coordinates.Length; i++)
                {
                    sb.Append(", ");
                    sb.Append(coordinates[i].ToString());
                }
                sb.Append(')');
                return sb.ToString();
            }
            else return "()";
        }
    }
}
