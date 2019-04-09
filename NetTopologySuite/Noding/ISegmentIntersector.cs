namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Processes possible intersections detected by a <see cref="INoder"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ISegmentIntersector" /> is passed to a <see cref="INoder" />.
    /// </para>
    /// The <see cref="ProcessIntersections(ISegmentString, int, ISegmentString, int)"/>
    /// method is called whenever the <see cref="INoder" />
    ///  detects that two <see cref="ISegmentString" />s might intersect.
    /// <para>
    /// This class may be used either to find all intersections, or
    /// to detect the presence of an intersection.  In the latter case,
    /// Noders may choose to short-circuit their computation by calling the
    /// <see cref="IsDone"/> property.
    /// </para>
    /// <para>
    /// </para>
    /// This class is an example of the <i>Strategy</i> pattern.
    /// <para>
    /// This class may be used either to find all intersections, or
    /// to detect the presence of an intersection.  In the latter case,
    /// Noders may choose to short-circuit their computation by calling the
    /// <see cref="IsDone"/> property.
    /// </para>
    /// </remarks>
    public interface ISegmentIntersector
    {
        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> interface to process
        /// intersections for two segments of the <see cref="ISegmentString" />s being intersected.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1);

        /// <summary>
        /// Reports whether the client of this class needs to continue testing
        /// all intersections in an arrangement.
        /// </summary>
        /// <returns>if there is no need to continue testing segments</returns>
        bool IsDone { get; }

    }
}
