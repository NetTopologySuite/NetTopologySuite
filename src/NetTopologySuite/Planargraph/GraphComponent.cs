using System.Collections;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// The base class for all graph component classes.
    /// Maintains flags of use in generic graph algorithms.
    /// Provides two flags:
    /// marked - typically this is used to indicate a state that persists
    /// for the course of the graph's lifetime.  For instance, it can be
    /// used to indicate that a component has been logically deleted from the graph.
    /// visited - this is used to indicate that a component has been processed
    /// or visited by an single graph algorithm.  For instance, a breadth-first traversal of the
    /// graph might use this to indicate that a node has already been traversed.
    /// The visited flag may be set and cleared many times during the lifetime of a graph.
    /// </summary>
    public abstract class GraphComponent
    {

        #region Static

        /// <summary>
        /// Sets the <see cref="Visited" /> state
        /// for all <see cref="GraphComponent" />s in an <see cref="IEnumerator" />.
        /// </summary>
        /// <param name="i">A <see cref="IEnumerator" /> to scan.</param>
        /// <param name="visited">The state to set the <see cref="Visited" /> flag to.</param>
        public static void SetVisited(IEnumerator i, bool visited)
        {
            while (i.MoveNext())
            {
                var comp = (GraphComponent) i.Current;
                comp.Visited = visited;
            }
        }

        /// <summary>
        /// Sets the <see cref="Marked" /> state
        /// for all <see cref="GraphComponent" />s in an <see cref="IEnumerator" />.
        /// </summary>
        /// <param name="i">A <see cref="IEnumerator" /> to scan.</param>
        /// <param name="marked">The state to set the <see cref="Marked" /> flag to.</param>
        public static void SetMarked(IEnumerator i, bool marked)
        {
            while (i.MoveNext())
            {
                var comp = (GraphComponent) i.Current;
                comp.Marked = marked;
            }
        }

        /// <summary>
        /// Finds the first <see cref="GraphComponent" />
        /// in a <see cref="IEnumerator" /> set
        /// which has the specified <see cref="Visited" /> state.
        /// </summary>
        /// <param name="i">A <see cref="IEnumerator" /> to scan.</param>
        /// <param name="visitedState">The <see cref="Visited" /> state to test.</param>
        /// <returns>The first <see cref="GraphComponent" /> found, or <c>null</c> if none found.</returns>
        public static GraphComponent GetComponentWithVisitedState(IEnumerator i, bool visitedState)
        {
            while (i.MoveNext())
            {
                var comp = (GraphComponent) i.Current;
                if (comp.IsVisited == visitedState)
                    return comp;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Tests if a component has been visited during the course of a graph algorithm.
        /// </summary>
        public bool IsVisited => Visited;

        /// <summary>
        /// Gets/Sets the visited flag for this component.
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// Tests if a component has been marked at some point during the processing
        /// involving this graph.
        /// </summary>
        public bool IsMarked => Marked;

        /// <summary>
        /// Gets/Sets the marked flag for this component.
        /// </summary>
        public bool Marked { get; set; }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public abstract bool IsRemoved { get; }

        /// <summary>
        /// Gets or sets user defined data for this component
        /// </summary>
        public object Data { get; set; }
    }
}
