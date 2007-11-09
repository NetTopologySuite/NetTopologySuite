namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A Position indicates the position of a Location relative to a graph component
    /// (Node, Edge, or Area).
    /// </summary>
    public static class Position
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
            {
                return Positions.Right;
            }

            if (position == Positions.Right)
            {
                return Positions.Left;
            }

            return position;
        }
    }
}