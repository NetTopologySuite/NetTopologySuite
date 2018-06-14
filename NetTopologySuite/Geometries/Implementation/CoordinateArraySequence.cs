using System;
using System.Text;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A <see cref="ICoordinateSequence"/> backed by an array of <see cref="Coordinate"/>s.
    /// This is the implementation that <see cref="IGeometry"/>s use by default.
    /// <para/>
    /// Coordinates returned by <see cref="ToCoordinateArray"/>, <see cref="GetCoordinate(int)"/> and <see cref="GetCoordinate(int, Coordinate)"/> are live --
    /// modifications to them are actually changing the
    /// CoordinateSequence's underlying data.
    /// A dimension may be specified for the coordinates in the sequence,
    /// which may be 2 or 3.
    /// The actual coordinates will always have 3 ordinates,
    /// but the dimension is useful as metadata in some situations.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class CoordinateArraySequence : ICoordinateSequence
    {
        protected Coordinate[] Coordinates;

        /**
         * The actual dimension of the coordinates in the sequence.
         * Allowable values are 2 or 3.
         */
        private readonly int _dimension = 3;

        /// <summary>
        /// Constructs a sequence based on the given array of <see cref="Coordinate"/>s.
        /// The coordinate dimension is 3
        /// </summary>
        /// <remarks>
        /// The array is not copied.
        /// </remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        public CoordinateArraySequence(Coordinate[] coordinates)
            : this(coordinates, 3) { }

        /// <summary>
        /// Constructs a sequence based on the given array
        /// of <see cref="Coordinate"/>s.
        /// </summary>
        /// <remarks>The Array is not copied</remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        /// <param name="dimension">The dimension of the coordinates</param>
        public CoordinateArraySequence(Coordinate[] coordinates, int dimension)
        {
            Coordinates = coordinates;
            _dimension = dimension;
            if (coordinates == null)
                Coordinates = new Coordinate[0];
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public CoordinateArraySequence(int size)
            : this(size, 3) { }

        /// <summary>
        /// Constructs a sequence of a given <paramref name="size"/>, populated
        /// with new <see cref="Coordinate"/>s of the given <paramref name="dimension"/>.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">the dimension of the coordinates</param>
        public CoordinateArraySequence(int size, int dimension)
        {
            Coordinates = new Coordinate[size];
            _dimension = dimension;
            for (int i = 0; i < size; i++)
                Coordinates[i] = new Coordinate();
        }

        /// <summary>
        /// Creates a new sequence based on a deep copy of the given <see cref="ICoordinateSequence"/>.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence that will be copied</param>
        public CoordinateArraySequence(ICoordinateSequence coordSeq)
        {
            if (coordSeq == null)
            {
                Coordinates = new Coordinate[0];
                return;
            }

            _dimension = coordSeq.Dimension;
            Coordinates = new Coordinate[coordSeq.Count];

            for (int i = 0; i < Coordinates.Length; i++)
                Coordinates[i] = coordSeq.GetCoordinateCopy(i);
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension => _dimension;

        public Ordinates Ordinates => _dimension == 3
            ? Ordinates.XYZ
            : Ordinates.XY;

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
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = Coordinates[index].X;
            coord.Y = Coordinates[index].Y;
            coord.Z = Coordinates[index].Z;
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
                    return double.NaN;
            }
        }

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        /// <returns>The deep copy.</returns>
        [Obsolete]
        public virtual object Clone()
        {
            return Copy();

        }
        /// <summary>
        /// Creates a deep copy of the <c>CoordinateArraySequence</c>.
        /// </summary>
        /// <returns>The deep copy.</returns>
        public virtual ICoordinateSequence Copy()
        {
            var cloneCoordinates = GetClonedCoordinates();
            return new CoordinateArraySequence(cloneCoordinates, Dimension);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected Coordinate[] GetClonedCoordinates()
        {
            var cloneCoordinates = new Coordinate[Count];
            for (int i = 0; i < Coordinates.Length; i++)
                cloneCoordinates[i] = Coordinates[i].Copy();
            return cloneCoordinates;
        }

        /// <summary>
        /// Returns the length of the coordinate sequence.
        /// </summary>
        public int Count => Coordinates.Length;

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

        public ICoordinateSequence Reversed()
        {
            var coordinates = new Coordinate[Count];
            for (int i = 0; i < Count; i++ )
            {
                coordinates[Count - i - 1] = new Coordinate(Coordinates[i]);
            }
            return new CoordinateArraySequence(coordinates, Dimension);
        }

        /// <summary>
        /// Returns the string representation of the coordinate array.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Coordinates.Length > 0)
            {
                var strBuf = new StringBuilder(17 * Coordinates.Length);
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
