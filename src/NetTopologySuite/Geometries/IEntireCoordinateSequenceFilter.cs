namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A variant of <see cref="ICoordinateSequenceFilter"/>, except it receives each
    /// <see cref="CoordinateSequence"/> just once, instead of once for each of its coordinates.
    /// </summary>
    public interface IEntireCoordinateSequenceFilter
    {
        /// <summary>
        /// Reports whether the application of this filter can be terminated.
        /// </summary>
        /// <remarks>
        /// Once this method returns <see langword="true"/> it must continue to return
        /// <see langword="true"/> on every subsequent call.
        /// </remarks>
        bool Done { get; }

        /// <summary>
        /// Reports whether the execution of this filter has modified the coordinates of the geometry.
        /// If so, <see cref="Geometry.GeometryChanged()"/> will be executed
        /// after this filter has finished being executed.
        /// </summary>
        /// <remarks>
        /// Most filters can simply return a constant value reflecting whether they are able to
        /// change the coordinates.
        /// </remarks>
        bool GeometryChanged { get; }

        ///<summary>
        /// Performs an operation on a <see cref="CoordinateSequence"/>.
        ///</summary>
        /// <param name="seq">
        /// The <see cref="CoordinateSequence"/>.
        /// </param>
        void Filter(CoordinateSequence seq);
    }
}
