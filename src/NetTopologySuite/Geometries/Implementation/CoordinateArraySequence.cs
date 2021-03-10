using System;
using System.Text;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A <see cref="CoordinateSequence"/> backed by an array of <see cref="Coordinate"/>s.
    /// This is the implementation that <see cref="Geometry"/>s use by default.
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
    public class CoordinateArraySequence : CoordinateSequence
    {
        /// <summary>
        /// Array of coordinates in sequence
        /// </summary>
        protected Coordinate[] Coordinates;

        /// <summary>
        /// Constructs a sequence based on the given array of <see cref="Coordinate"/>s.
        /// The coordinate dimension defaults to 2
        /// </summary>
        /// <remarks>
        /// The array is not copied.
        /// </remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        public CoordinateArraySequence(Coordinate[] coordinates)
            : this(coordinates, GetDimensionAndMeasures(coordinates, out int measures), measures) { }

        /// <summary>
        /// Constructs a sequence based on the given array
        /// of <see cref="Coordinate"/>s.
        /// </summary>
        /// <remarks>The Array is not copied</remarks>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        /// <param name="dimension">The dimension of the coordinates</param>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public CoordinateArraySequence(Coordinate[] coordinates, int dimension)
            : this(coordinates, GetDimensionAndMeasures(coordinates, out int measures), measures)
        {
        }

        /// <summary>
        /// Constructs a sequence based on the given array
        /// of <see cref="Coordinate"/>s.
        /// <para/>
        /// The Array is <b>not</b> copied
        /// <para/>
        /// It is your responsibility to ensure the array contains Coordinates of the
        /// indicated dimension and measures (See <see cref="CoordinateArrays.EnforceConsistency(Coordinate[])"/>).
        /// </summary>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        /// <param name="dimension">The dimension of the coordinates</param>
        /// <param name="measures">The number of measure ordinate values.</param>
        public CoordinateArraySequence(Coordinate[] coordinates, int dimension, int measures)
            : base(coordinates?.Length ?? 0, dimension, measures)
        {
            if (coordinates == null)
            {
                Coordinates = new Coordinate[0];
            }
            else
            {
                Coordinates = coordinates;
            }
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public CoordinateArraySequence(int size)
            : this(size, 2, 0) { }

        /// <summary>
        /// Constructs a sequence of a given <paramref name="size"/>, populated
        /// with new <see cref="Coordinate"/>s of the given <paramref name="dimension"/>.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">the dimension of the coordinates</param>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public CoordinateArraySequence(int size, int dimension)
            : base(size, dimension, 0)
        {
            Coordinates = new Coordinate[size];
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
        /// <param name="measures">the number of measures of the coordinates</param>
        public CoordinateArraySequence(int size, int dimension, int measures)
            : base(size, dimension, measures)
        {
            Coordinates = new Coordinate[size];
            for (int i = 0; i < size; i++)
                Coordinates[i] = Geometries.Coordinates.Create(dimension, measures);
        }

        /// <summary>
        /// Creates a new sequence based on a deep copy of the given <see cref="CoordinateSequence"/>.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence that will be copied</param>
        public CoordinateArraySequence(CoordinateSequence coordSeq)
            : base(coordSeq?.Count ?? 0, coordSeq?.Dimension ?? 2, coordSeq?.Measures ?? 0)
        {
            if (coordSeq == null)
            {
                Coordinates = new Coordinate[0];
                return;
            }

            Coordinates = new Coordinate[coordSeq.Count];

            for (int i = 0; i < Coordinates.Length; i++)
                Coordinates[i] = coordSeq.GetCoordinateCopy(i);
        }

        /// <summary>
        /// Ensure array contents of the same type, making use of <see cref="CoordinateSequence.CreateCoordinate"/> as needed.
        /// <para>
        /// A new array will be created if needed to return a consistent result.
        /// </para>
        /// </summary>
        /// <param name="array">array containing consistent coordinate instances</param>
        [Obsolete("It is the clients responsibility to provide consistent arrays")]
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
        /// Get the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>The requested Coordinate instance.</returns>
        public override Coordinate GetCoordinate(int i)
        {
            return Coordinates[i];
        }

        /// <summary>
        /// Get a copy of the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>A copy of the requested Coordinate.</returns>
        public override Coordinate GetCoordinateCopy(int i)
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
        public override void GetCoordinate(int index, Coordinate coord)
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
        public override double GetX(int index)
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
        public override double GetY(int index)
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
        public override double GetZ(int index)
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
        public override double GetM(int index)
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
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            switch (ordinateIndex)
            {
                case 0:
                    return Coordinates[index].X;
                case 1:
                    return Coordinates[index].Y;
                default:
                    return Coordinates[index][ordinateIndex];
            }
        }

        /// <summary>
        /// Creates a deep copy of the <c>CoordinateArraySequence</c>.
        /// </summary>
        /// <returns>The deep copy.</returns>
        public override CoordinateSequence Copy()
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
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            switch (ordinateIndex)
            {
                case 0:
                    Coordinates[index].X = value;
                    break;
                case 1:
                    Coordinates[index].Y = value;
                    break;
                default:
                    Coordinates[index][ordinateIndex] = value;
                    break;
            }
        }

        /// <summary>
        ///This method exposes the internal Array of Coordinate Objects.
        /// </summary>
        /// <returns></returns>
        public override Coordinate[] ToCoordinateArray()
        {
            return Coordinates;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override Envelope ExpandEnvelope(Envelope env)
        {
            for (int i = 0; i < Coordinates.Length; i++ )
                env.ExpandToInclude(Coordinates[i]);
            return env;
        }

        /// <inheritdoc cref="CoordinateSequence.Reversed()"/>
        public override CoordinateSequence Reversed()
        {
            var coordinates = new Coordinate[Count];
            for (int i = 0; i < Count; i++ )
            {
                coordinates[Count - i - 1] = Coordinates[i].Copy();
            }
            return new CoordinateArraySequence(coordinates, Dimension, Measures);
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

        private static int GetDimensionAndMeasures(Coordinate[] coords, out int measures)
        {
            int dimension;
            (_, dimension, measures) = CoordinateSequenceFactory.GetCommonSequenceParameters(coords);
            return dimension;
        }
    }
}
