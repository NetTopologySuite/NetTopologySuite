namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Angle orientation
    /// </summary>
    public enum OrientationIndex
    {
        /// <summary>A value that indicates an orientation of collinear, or no turn (straight)</summary>
        Collinear = 0,

        /// <summary>A value that indicates an orientation of collinear, or no turn (straight)</summary>
        None = Collinear,

        /// <summary>A value that indicates an orientation of collinear, or no turn (straight)</summary>
        Straight = Collinear,

        /// <summary>A value that indicates an orientation of counterclockwise, or a left turn.</summary>
        CounterClockwise = 1,

        /// <summary>A value that indicates an orientation of counterclockwise, or a left turn.</summary>
        Left = CounterClockwise,

        /// <summary>A value that indicates an orientation of clockwise or a right turn.</summary>
        Clockwise = -1,

        /// <summary>A value that indicates an orientation of clockwise or a right turn.</summary>
        Right = Clockwise
    }

}
