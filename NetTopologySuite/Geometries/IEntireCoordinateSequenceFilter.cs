namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A filter that visits each <see cref="CoordinateSequence"/> only once, as opposed to
    /// <see cref="ICoordinateSequenceFilter"/>, which visits each coordinate in the sequence.
    /// </summary>
    public interface IEntireCoordinateSequenceFilter
    {
        ///<summary>
        /// Performs an operation on a <see cref="CoordinateSequence"/>.
        ///</summary>
        /// <param name="seq">the <c>CoordinateSequence</c> to which the filter is applied</param>
        void Filter(CoordinateSequence seq);

        ///<summary>
        /// Reports whether the application of this filter can be terminated.
        ///</summary>
        ///<remarks>
        /// Once this method returns <c>true</c>, it must
        /// continue to return <c>true</c> on every subsequent call.
        ///</remarks>
        bool Done { get; }

        ///<summary>
        /// Reports whether the execution of this filter has modified the coordinates of the geometry.
        /// If so, <see cref="Geometry.GeometryChanged()"/> will be executed
        /// after this filter has finished being executed.
        /// </summary>
        /// <remarks>Most filters can simply return a constant value reflecting whether they are able to change the coordinates.</remarks>
        bool GeometryChanged { get; }
    }
}
