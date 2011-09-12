#if !useFullGeoAPI
using System;

namespace NetTopologySuite.Geometries
{
    ///<summary>
    /// Interface for classes which provide operations that
    /// can be applied to the coordinates in a <see cref="ICoordinateSequence"/>. 
    /// A CoordinateSequence filter can either record information about each coordinate or
    /// change the coordinate in some way. CoordinateSequence filters can be
    /// used to implement such things as coordinate transformations, centroid and
    /// envelope computation, and many other functions.
    /// For maximum efficiency, the execution of filters can be short-circuited.
    /// <see cref="Geometry"/> classes support the concept of applying a
    /// <c>CoordinateSequenceFilter</c> to each 
    /// <see cref="ICoordinateSequence"/>s they contain. 
    ///</summary>
    ///<see cref="Geometry.Apply(ICoordinateSequenceFilter)"/>
    ///<remarks>
    /// <c>CoordinateSequenceFilter</c> is an example of the Gang-of-Four Visitor pattern.
    ///</remarks>
    /// <author>Martin Davis</author>
    public interface ICoordinateSequenceFilter
    {
        ///<summary>
        /// Performs an operation on a coordinate in a <see cref="ICoordinateSequence"/>.
        ///</summary>
        /// <param name="seq">the <c>CoordinateSequence</c> to which the filter is applied</param>
        /// <param name="i">i the index of the coordinate to apply the filter to</param>
        void Filter(ICoordinateSequence seq, int i);

        ///<summary>
        /// Reports whether the application of this filter can be terminated.
        ///</summary>
        ///<remarks>
        /// Once this method returns <c>false</c>, it should 
        /// continue to return <c>false</c> on every subsequent call.
        ///</remarks>
        Boolean Done { get; }

        ///<summary>
        /// Reports whether the execution of this filter has modified the coordinates of the geometry.
        /// If so, <see cref="Geometry.GeometryChanged()"/> will be executed
        /// after this filter has finished being executed.
        /// </summary>
        /// <remarks>Most filters can simply return a constant value reflecting whether they are able to change the coordinates.</remarks>
        Boolean GeometryChanged { get; }
    }

}
#endif