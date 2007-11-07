using System;
using System.Text;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// The CoordinateSequence implementation that Geometries use by default. In
    /// this implementation, Coordinates returned by ToArray and GetCoordinates are live --
    /// parties that change them are actually changing the
    /// DefaultCoordinateSequence's underlying data.
    /// </summary>
    [Serializable]
    [Obsolete("No longer used.")]
    public class DefaultCoordinateSequence : ICoordinateSequence
    {
        private ICoordinate[] coordinates = null;

        /// <summary>
        /// Constructs a DefaultCoordinateSequence based on the given array (the
        /// array is not copied).
        /// </summary>
        /// <param name="coordinates">Coordinate array that will be assimilated.</param>
        public DefaultCoordinateSequence(ICoordinate[] coordinates)
        {
            if (Geometry2D.HasNullElements(coordinates))
            {
                throw new ArgumentException("Null coordinate");
            }
            this.coordinates = coordinates;
        }

        public DefaultCoordinateSequence(ICoordinateSequence coordSeq)
        {
            coordinates = new ICoordinate[coordSeq.Count];
            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = coordSeq.GetCoordinateCopy(i);
            }
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public DefaultCoordinateSequence(Int32 size)
        {
            coordinates = new ICoordinate[size];
            for (Int32 i = 0; i < size; i++)
            {
                coordinates[i] = new Coordinate();
            }
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        public Int32 Dimension
        {
            get { return 3; }
        }

        /// <summary>
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public ICoordinate GetCoordinate(Int32 i)
        {
            return coordinates[i];
        }

        /// <summary>
        /// Returns a copy of the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>The copy of the coordinate specified.</return>
        public ICoordinate GetCoordinateCopy(Int32 i)
        {
            return new Coordinate(coordinates[i]);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(Int32 index, ICoordinate coord)
        {
            coord.X = coordinates[index].X;
            coord.Y = coordinates[index].Y;
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public Double GetX(Int32 index)
        {
            return coordinates[index].X;
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public Double GetY(Int32 index)
        {
            return coordinates[index].Y;
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinates indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        public Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    return coordinates[index].X;
                case Ordinates.Y:
                    return coordinates[index].Y;
                case Ordinates.Z:
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
        public void SetOrdinate(Int32 index, Ordinates ordinate, Double value)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    coordinates[index].X = value;
                    break;
                case Ordinates.Y:
                    coordinates[index].Y = value;
                    break;
                case Ordinates.Z:
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
        public object this[Int32 i]
        {
            get { return coordinates[i]; }
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public IExtents ExpandEnvelope(IExtents env)
        {
            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                env.ExpandToInclude(coordinates[i]);
            }

            return env;
        }

        /// <summary>
        /// Returns a deep copy of the object passed.
        /// </summary>
        /// <returns>The copied object.</returns>
        public object Clone()
        {
            ICoordinate[] cloneCoordinates = new ICoordinate[coordinates.Length];

            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                cloneCoordinates[i] = (Coordinate) coordinates[i].Clone();
            }

            return new DefaultCoordinateSequence(cloneCoordinates);
        }

        /// <summary>
        /// Returns the elements number of the coordinate sequence.
        /// </summary>
        public Int32 Count
        {
            get { return coordinates.Length; }
        }

        /// <summary>
        /// Returns the elements number of the coordinate sequence.
        /// </summary>
        /// <value>The length.</value>
        public Int32 Length
        {
            get { return coordinates.Length; }
        }

        /// <summary>
        /// This method exposes the internal Array of Coordinate Objects.
        /// </summary>
        /// <returns>Coordinate[] array.</returns>
        public ICoordinate[] ToCoordinateArray()
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
                StringBuilder sb = new StringBuilder(17*coordinates.Length);
                sb.Append('(');
                sb.Append(coordinates[0]);

                for (Int32 i = 1; i < coordinates.Length; i++)
                {
                    sb.Append(", ");
                    sb.Append(coordinates[i].ToString());
                }

                sb.Append(')');
                return sb.ToString();
            }
            else
            {
                return "()";
            }
        }
    }
}