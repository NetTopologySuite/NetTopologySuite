namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An enumeration of ring orientation values
    /// </summary>
    public enum LinearRingOrientation
    {

        /// <summary>
        /// The orientation of the ring's coordinates is counter-clockwise.
        /// </summary>
        CounterClockwise = -1,

        /// <summary>
        /// The orientation of the ring's coordinates follows the left-hand-rule,
        /// saying that if you follow the ring in the direction of its coordinates
        /// your left hand will be inside the ring.
        /// </summary>
        /// <seealso cref="CounterClockwise"/>
        LeftHandRule = CounterClockwise,

        /// <summary>
        /// The orientation of the ring's coordinates is counter-clockwise.
        /// </summary>
        CCW = CounterClockwise,

        /// <summary>
        /// The orientation of the rings coordinates does not matter.
        /// </summary>
        DontCare = 0,

        /// <summary>
        /// The default orientation of the rings coordinates.
        /// Set to <see cref="CounterClockwise"/>
        /// </summary>
        Default = CCW,

        /// <summary>
        /// The orientation of the ring's coordinates is clockwise.
        /// </summary>
        Clockwise = 1,

        /// <summary>
        /// The orientation of the ring's coordinates follows the right-hand-rule,
        /// saying that if you follow the ring in the direction of its coordinates
        /// your right hand will be inside the ring.
        /// </summary>
        /// <seealso cref="Clockwise"/>
        RightHandRule = Clockwise,

        /// <summary>
        /// The orientation of the ring's coordinates is clockwise.
        /// </summary>
        CW = Clockwise
    }
}
