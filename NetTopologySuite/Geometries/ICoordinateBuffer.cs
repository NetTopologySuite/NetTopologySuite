using GeoAPI.Geometries;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// Interface for a coordinate buffer
    /// </summary>
    public interface ICoordinateBuffer
    {
        /// <summary>
        /// Gets the (current) capacity of the buffer
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets the (current) number of coordinates in the buffer
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a coordinate made up of the ordinates (x, y, z, m) to the buffer.
        /// </summary>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="z">The (optional) z-Ordinate</param>
        /// <param name="m">The (optional) m-Ordinate</param>
        /// <param name="allowRepeated">Allows repated coordinates to be added</param>
        /// <returns><value>true</value> if the coordinate was successfully added.</returns>
        bool AddCoordinate(double x, double y, double? z = null, double? m = null, bool allowRepeated = true);

        /// <summary>
        /// Inserts a coordinate made up of the ordinates (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>, <paramref name="m"/>) at index <paramref name="index"/> to the buffer.
        ///  </summary>
        /// <param name="index">The index at which to insert the ordinate.</param>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="z">The (optional) z-Ordinate</param>
        /// <param name="m">The (optional) m-Ordinate</param>
        /// <param name="allowRepeated">Allows repated coordinates to be added</param>
        /// <returns><value>true</value> if the coordinate was successfully inserted.</returns>
        bool InsertCoordinate(int index, double x, double y, double? z = null, double? m = null, bool allowRepeated = true);

        /// <summary>
        /// Sets a m-value at the provided <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="m">The value</param>
        void SetM(int index, double m);

        /// <summary>
        /// Sets a z-value at the provided <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="z">The value</param>
        void SetZ(int index, double z);

        /// <summary>
        /// Converts the contents of this <see cref="ICoordinateBuffer"/> to a <see cref="ICoordinateSequence"/>.
        /// <br/>Optionally you may assign a factory to create the sequence
        /// </summary>
        /// <param name="factory">The factory to use in order to create the sequence.</param>
        /// <returns>A coordinate sequence</returns>
        ICoordinateSequence ToSequence(ICoordinateSequenceFactory factory = null);
    }
}