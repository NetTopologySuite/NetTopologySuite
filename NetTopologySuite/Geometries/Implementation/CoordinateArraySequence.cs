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
    [Serializable]
    public class CoordinateArraySequence : ICoordinateSequence
    {
        protected Coordinate[] Coordinates;

        /**
         * The actual dimension of the coordinates in the sequence.
         * Allowable values are 2, 3 or 4.
         */
        private readonly int _dimension = 2;

        /**
         * The number of measures of the coordinates in the sequence.
         * Allowable values are 0 or 1.
         */
        private readonly int _measures = 0;

        /// <summary>
        /// Constructs a sequence based on the given array of <see cref="Coordinate"/>s.
        /// The coordinate dimension defaults to 2
        /// </summary>
        /// <remarks>
        /// The array is not copied.
        /// </remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        public CoordinateArraySequence(Coordinate[] coordinates)
            : this(coordinates, CoordinateArrays.Dimension(coordinates), CoordinateArrays.Measures(coordinates)) { }

        /// <summary>
        /// Constructs a sequence based on the given array
        /// of <see cref="Coordinate"/>s.
        /// </summary>
        /// <remarks>The Array is not copied</remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        /// <param name="dimension">The dimension of the coordinates</param>
        public CoordinateArraySequence(Coordinate[] coordinates, int dimension)
            : this(coordinates, dimension, CoordinateArrays.Measures(coordinates))
        {
        }

        /// <summary>
        /// Constructs a sequence based on the given array
        /// of <see cref="Coordinate"/>s.
        /// </summary>
        /// <remarks>The Array is not copied</remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        /// <param name="dimension">The dimension of the coordinates</param>
        public CoordinateArraySequence(Coordinate[] coordinates, int dimension, int measures)
        {
            _dimension = dimension;
            _measures = measures;
            if (coordinates == null)
            {
                Coordinates = new Coordinate[0];
            }
            else
            {
                Coordinates = EnforceArrayConsistency(coordinates);
            }
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public CoordinateArraySequence(int size)
            : this(size, 2) { }

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
                Coordinates[i] = Geometries.Coordinates.Create(dimension);
        }

        /// <summary>
        /// Constructs a sequence of a given <paramref name="size"/>, populated
        /// with new <see cref="Coordinate"/>s of the given <paramref name="dimension"/>
        /// with the given number of <paramref name="measures"/>
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">the dimension of the coordinates</param>
        public CoordinateArraySequence(int size, int dimension, int measures)
        {
            Coordinates = new Coordinate[size];
            _dimension = dimension;
            _measures = measures;
            for (int i = 0; i < size; i++)
                Coordinates[i] = CreateCoordinate();
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
            _measures = coordSeq.Measures;
            Coordinates = new Coordinate[coordSeq.Count];

            for (int i = 0; i < Coordinates.Length; i++)
                Coordinates[i] = coordSeq.GetCoordinateCopy(i);
        }

        /// <summary>
        /// Ensure array contents of the same type, making use of <see cref="CreateCoordinate"/> as needed.
        /// <para>
        /// A new array will be created if needed to return a consistent result.
        /// </para>
        /// </summary>
        /// <param name="array">array containing consistent coordinate instances</param>
        protected Coordinate[] EnforceArrayConsistency(Coordinate[] array)
        {
            var sample = CreateCoordinate();
            var type = sample.GetType();
            bool isConsistent = true;
            for (int i = 0; i < array.Length; i++)
            {
                var coordinate = array[i];
                if (coordinate != null && coordinate.GetType() != type)
                {
                    isConsistent = false;
                    break;
                }
            }

            if (isConsistent)
            {
                return array;
            }

            var copy = (Coordinate[])Array.CreateInstance(type, array.Length);
            for (int i = 0; i < copy.Length; i++)
            {
                var coordinate = array[i];
                if (coordinate != null && coordinate.GetType() != type)
                {
                    var duplicate = CreateCoordinate();
                    duplicate.CoordinateValue = coordinate;
                    copy[i] = duplicate;
                }
                else
                {
                    copy[i] = coordinate;
                }
            }

            return copy;
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension => _dimension;

        /// <inheritdoc />
        public int Measures => _measures;

        /// <inheritdoc />
        public bool HasZ => Dimension - Measures > 2;

        /// <inheritdoc />
        public bool HasM => Dimension > 2 && Measures > 0;

        public Ordinates Ordinates => PackedCoordinateSequence.DimensionToOrdinate(_dimension, _measures);

        /// <inheritdoc />
        public Coordinate CreateCoordinate() => Geometries.Coordinates.Create(Dimension, Measures);

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
            var copy = CreateCoordinate();
            copy.CoordinateValue = Coordinates[i];
            return copy;
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.CoordinateValue = Coordinates[index];
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
        /// Returns ordinate Z of the specified coordinate if available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Z ordinate in the index'th coordinate, or Double.NaN if not defined.
        /// </returns>
        public double GetZ(int index)
        {
            if (HasZ)
            {
                return Coordinates[index].Z;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Returns ordinate M of the specified coordinate if available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the M ordinate in the index'th coordinate, or Double.NaN if not defined.
        /// </returns>
        public double GetM(int index)
        {
            if (HasM)
            {
                return Coordinates[index].M;
            }
            else
            {
                return double.NaN;
            }
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
                default:
                    return Coordinates[index][ordinate];
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
            return new CoordinateArraySequence(cloneCoordinates, Dimension, Measures);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected Coordinate[] GetClonedCoordinates()
        {
            var cloneCoordinates = new Coordinate[Count];
            for (int i = 0; i < Coordinates.Length; i++)
            {
                var duplicate = CreateCoordinate();
                duplicate.CoordinateValue = Coordinates[i];
                cloneCoordinates[i] = duplicate;
            }
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
                default:
                    Coordinates[index][ordinate] = value;
                    break;
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
                coordinates[Count - i - 1] = Coordinates[i].Copy();
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
