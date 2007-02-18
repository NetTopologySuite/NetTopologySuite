using System;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// Standard ordinate index values.
    /// </summary>
    public enum Ordinates
    {        
        /// <summary>
        /// X Ordinate = 0.
        /// </summary>
        X = 0,

        /// <summary>
        /// Y Ordinate = 1.
        /// </summary>
        Y = 1,

        /// <summary>
        /// Z Ordinate = 2.
        /// </summary>
        Z = 2,

        /// <summary>
        /// M Ordinate = 3.
        /// </summary>
        M = 3,
    }

    /// <summary>
    /// The internal representation of a list of coordinates inside a Geometry.
    /// <para>
    /// There are some cases in which you might want Geometries to store their
    /// points using something other than the NTS Coordinate class. For example, you
    /// may want to experiment with another implementation, such as an array of x’s
    /// and an array of y’s. or you might want to use your own coordinate class, one
    /// that supports extra attributes like M-values.
    /// </para>
    /// <para>
    /// You can do this by implementing the ICoordinateSequence and
    /// ICoordinateSequenceFactory interfaces. You would then create a
    /// GeometryFactory parameterized by your ICoordinateSequenceFactory, and use
    /// this GeometryFactory to create new Geometries. All of these new Geometries
    /// will use your ICoordinateSequence implementation.
    /// A note on performance. If your ICoordinateSequence is not based on an array
    /// of Coordinates, it may incur a performance penalty when its ToArray() method
    /// is called because the array needs to be built from scratch. 
    /// </para>
    /// </summary>
    /// <seealso cref="DefaultCoordinateSequenceFactory"/>
    public interface ICoordinateSequence : ICloneable
    {
        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        int Dimension { get; }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>        
        int Count { get;} 

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
        ICoordinate GetCoordinate(int i);

        /// <summary>
        /// Returns a copy of the i'th coordinate in this sequence.
        /// This method optimizes the situation where the caller is
        /// going to make a copy anyway - if the implementation
        /// has already created a new Coordinate object, no further copy is needed.
        /// </summary>
        /// <param name="i">The index of the coordinate to retrieve.</param>
        /// <returns>A copy of the i'th coordinate in the sequence</returns>
        ICoordinate GetCoordinateCopy(int i);             

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.  
        /// Only the first two dimensions are copied.        
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        void GetCoordinate(int index, ICoordinate coord);

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The value of the X ordinate in the index'th coordinate.</returns>
        double GetX(int index);

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The value of the Y ordinate in the index'th coordinate.</returns>
        double GetY(int index);

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinates indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).         
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>       
        double GetOrdinate(int index, Ordinates ordinate);

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.       
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>       
        void SetOrdinate(int index, Ordinates ordinate, double value);

        /// <summary>
        /// Returns (possibly copies of) the Coordinates in this collection.
        /// Whether or not the Coordinates returned are the actual underlying
        /// Coordinates or merely copies depends on the implementation. Note that
        /// if this implementation does not store its data as an array of Coordinates,
        /// this method will incur a performance penalty because the array needs to
        /// be built from scratch.
        /// </summary>
        /// <returns></returns>
        ICoordinate[] ToCoordinateArray();

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.      
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>       
        IEnvelope ExpandEnvelope(IEnvelope env);
    }
}
