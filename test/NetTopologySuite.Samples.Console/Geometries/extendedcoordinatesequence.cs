using System;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Samples.Geometries
{
    /// <summary>
    /// Demonstrates how to implement a CoordinateSequence for a new kind of
    /// coordinate (an <c>ExtendedCoordinate</c>} in this example). In this
    /// implementation, Coordinates returned by ToArray and #get are live -- parties
    /// that change them are actually changing the ExtendedCoordinateSequence's
    /// underlying data.
    /// </summary>
    public class ExtendedCoordinateSequence : CoordinateSequence
    {
        private static Coordinate[] CopyInternal(Coordinate[] coordinates)
        {
            var copy = new Coordinate[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
                copy[i] = new ExtendedCoordinate(coordinates[i]);
            return copy;

        }

        private readonly Coordinate[] _coordinates;

        /// <summary> Copy constructor -- simply aliases the input array, for better performance.
        /// </summary>
        public ExtendedCoordinateSequence(ExtendedCoordinate[] coordinates)
            : base(coordinates?.Length ?? 0, 4, 1)
        {
            _coordinates = new Coordinate[coordinates.Length];
            Array.Copy(coordinates, _coordinates, coordinates.Length);
        }

        /// <summary> Constructor that makes a copy of an existing array of Coordinates.
        /// Always makes a copy of the input array, since the actual class
        /// of the Coordinates in the input array may be different from ExtendedCoordinate.
        /// </summary>
        public ExtendedCoordinateSequence(Coordinate[] copyCoords)
            : base(copyCoords?.Length ?? 0, 4, 1)
        {
            _coordinates = CopyInternal(copyCoords);
        }

        /// <inheritdoc />
        public override Coordinate CreateCoordinate() => new ExtendedCoordinate();

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
        public override Coordinate GetCoordinate(int i)
        {
            return _coordinates[i];
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override CoordinateSequence Copy()
        {
            var cloneCoordinates = new ExtendedCoordinate[Count];
            for (int i = 0; i < _coordinates.Length; i++)
            {
                cloneCoordinates[i] = (ExtendedCoordinate) _coordinates[i].Copy();
            }

            return new ExtendedCoordinateSequence(cloneCoordinates);
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
        public override Coordinate[] ToCoordinateArray()
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
            var strBuf = new StringBuilder();
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
        public override Coordinate GetCoordinateCopy(int index)
        {
            return _coordinates[index].Copy();
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public override void GetCoordinate(int index, Coordinate coord)
        {
            var exc = (ExtendedCoordinate) _coordinates[index];
            coord.X = exc.X;
            coord.Y = exc.Y;
            coord.Z = exc.Z;
            coord.M = exc.M;
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
            return _coordinates[index].X;
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
            return _coordinates[index].Y;
        }

        /// <inheritdoc />
        public override double GetZ(int index)
        {
            return _coordinates[index].Z;
        }

        /// <inheritdoc />
        public override double GetM(int index)
        {
            return _coordinates[index].M;
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
            var exc = (ExtendedCoordinate) _coordinates[index];
            switch (ordinateIndex)
            {
                case 0:
                    return exc.X;
                case 1:
                    return exc.Y;
                case 2:
                    return exc.Z;
                case 3:
                    return exc.M;
                default:
                    return double.NaN;
            }
        }

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            var exc = (ExtendedCoordinate)_coordinates[index];
            switch (ordinateIndex)
            {
                case 0:
                    exc.X = value;
                    break;
                case 1:
                    exc.Y = value;
                    break;
                case 2:
                    exc.Z = value;
                    break;
                case 3:
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
        public override Envelope ExpandEnvelope(Envelope env)
        {
            for (int i = 0; i < _coordinates.Length; i++)
                env.ExpandToInclude(_coordinates[i]);
            return env;
        }

        /// <summary>
        /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>A reversed version of this sequence</returns>
        public override CoordinateSequence Reversed()
        {
            var coordinates = new ExtendedCoordinate[Count];
            for (int i = 0; i < Count; i++)
            {
                coordinates[Count - i - 1] = new ExtendedCoordinate(coordinates[i]);
            }
            return new ExtendedCoordinateSequence(coordinates);
        }
    }
}
