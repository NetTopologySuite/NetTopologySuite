namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// 
    /// </summary>
    public enum Positions
    {
        /// <summary>
        ///  An indicator that a Location is <c>on</c> a GraphComponent (0)
        /// </summary>
        On = 0,

        /// <summary>
        /// An indicator that a Location is to the <c>left</c> of a GraphComponent (1)
        /// </summary>
        Left = 1,

        /// <summary> 
        /// An indicator that a Location is to the <c>right</c> of a GraphComponent (2)
        /// </summary> 
        Right = 2,

        /// <summary> 
        /// An indicator that a Location is <c>is parallel to x-axis</c> of a GraphComponent (-1)
        /// /// </summary> 
        Parallel = -1,
    }

    /// <summary> 
    /// A Position indicates the position of a Location relative to a graph component
    /// (Node, Edge, or Area).
    /// </summary>
    public class Position 
    {
        /// <summary> 
        /// Returns Positions.Left if the position is Positions.Right, 
        /// Positions.Right if the position is Left, or the position
        /// otherwise.
        /// </summary>
        /// <param name="position"></param>
        public static Positions Opposite(Positions position)
        {
            if (position == Positions.Left)
                return Positions.Right;
            if (position == Positions.Right)
                return Positions.Left;
            return position;
        }
    }
}
