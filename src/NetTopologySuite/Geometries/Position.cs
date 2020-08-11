namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Indicates the position of a location relative to a
    /// node or edge component of a planar topological structure.
    /// </summary>
    public enum Position
    {
        /// <summary>
        /// Specifies that a location is <c>on</c> a component
        /// </summary>
        /// <value>0</value>
        On = 0,

        /// <summary>
        /// Specifies that a location is to the <c>left</c> of a component
        /// </summary>
        /// <value>1</value>
        Left = 1,

        /// <summary>
        /// Specifies that a location is to the <c>right</c> of a component
        /// </summary>
        /// <value>2</value>
        Right = 2,

        /// <summary>
        /// Specifies that a location is <c>is parallel to x-axis</c> of a component
        /// </summary>
        /// <value>-1</value>
        Parallel = -1,
    }

    /// <summary>
    /// A Position indicates the position of a Location relative to a graph component
    /// (Node, Edge, or Area).
    /// </summary>
    public static class PositionExtensions
    {
        /// <summary>
        /// Returns <see cref="Position.Left"/> if the position is <see cref="Position.Right"/>,
        /// <see cref="Position.Right"/> if the position is <see cref="Position.Left"/>, or the position
        /// otherwise.
        /// </summary>
        /// <param name="position">The position</param>
        public static Position Opposite(Position position)
        {
            if (position == Position.Left)
                return Position.Right;
            if (position == Position.Right)
                return Position.Left;
            return position;
        }
    }
}
