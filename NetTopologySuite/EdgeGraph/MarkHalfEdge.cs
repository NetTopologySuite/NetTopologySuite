using NetTopologySuite.Geometries;

namespace NetTopologySuite.EdgeGraph
{
    /// <summary>
    /// A <see cref="HalfEdge"/> which supports
    /// marking edges with a boolean flag.
    /// Useful for algorithms which perform graph traversals.
    /// </summary>
    public class MarkHalfEdge : HalfEdge
    {
        public static bool IsMarked(HalfEdge e)
        {
            return ((MarkHalfEdge)e).Marked;
        }

        public static void Mark(HalfEdge e)
        {
            ((MarkHalfEdge)e).Mark();
        }

        public static void SetMark(HalfEdge e, bool isMarked)
        {
            ((MarkHalfEdge)e).Marked = isMarked;
        }

        /// <summary>
        /// Sets the mark for the given edge pair to a boolean value.
        /// </summary>
        /// <param name="e">an edge of the pair to update</param>
        /// <param name="isMarked">the mark value to set</param>
        public static void SetMarkBoth(HalfEdge e, bool isMarked)
        {
            ((MarkHalfEdge)e).Marked = isMarked;
            ((MarkHalfEdge)e.Sym).Marked = isMarked;
        }

        /// <summary>
        /// Marks the edges in a pair.
        /// </summary>
        /// <param name="e">an edge of the pair to mark</param>
        public static void MarkBoth(HalfEdge e)
        {
            ((MarkHalfEdge)e).Mark();
            ((MarkHalfEdge)e.Sym).Mark();
        }

        private bool _marked;

        /// <summary>
        /// Creates a new marked edge.
        /// </summary>
        /// <param name="orig">the coordinate of the edge origin</param>
        public MarkHalfEdge(Coordinate orig)
            : base(orig) { }

        /// <summary>
        /// Marks this edge.
        /// </summary>
        public void Mark()
        {
            Marked = true;
        }

        public bool Marked
        {
            get => _marked;
            set => _marked = value;
        }
    }
}
