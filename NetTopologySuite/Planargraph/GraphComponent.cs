using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// The base class for all graph component classes.
    /// </summary>
    /// <remarks>
    /// Maintains flags of use in generic graph algorithms.
    /// Provides two flags:
    /// <list type="table">
    /// <item>
    /// <term>Marked</term>
    /// <description>
    /// Typically this is used to indicate a state that persists
    /// for the course of the graph's lifetime.  For instance, it can be
    /// used to indicate that a component has been logically deleted from the graph.
    /// </description>
    /// <term>Visited</term>
    /// <description>
    /// This is used to indicate that a component has been processed
    /// or visited by an single graph algorithm.  For instance, a breadth-first traversal of the
    /// graph might use this to indicate that a node has already been traversed.
    /// The visited flag may be set and cleared many times during the lifetime of a graph.
    /// </description>
    /// </list>
    /// </remarks>
    public abstract class GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        #region Static

        /// <summary>
        /// Sets the <see cref="GraphComponent{TCoordinate}.Visited" /> state 
        /// for all <see cref="GraphComponent{TCoordinate}" />s in an <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="components">An <see cref="IEnumerable{T}" /> to scan.</param>
        /// <param name="visited">
        /// The state to set the <see cref="GraphComponent{TCoordinate}.Visited" /> flag to.
        /// </param>
        public static void SetVisited(IEnumerable<GraphComponent<TCoordinate>> components, Boolean visited)
        {
            foreach (GraphComponent<TCoordinate> component in components)
            {
                component.Visited = visited;
            }
        }

        /// <summary>
        /// Sets the <see cref="GraphComponent{TCoordinate}.Marked" /> state 
        /// for all <see cref="GraphComponent{TCoordinate}" />s in an <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="components">An <see cref="IEnumerable{T}" /> to scan.</param>
        /// <param name="marked">The state to set the <see cref="GraphComponent{TCoordinate}.Marked" /> flag to.</param>
        public static void SetMarked(IEnumerable<GraphComponent<TCoordinate>> components, Boolean marked)
        {
            foreach (GraphComponent<TCoordinate> component in components)
            {
                component.Marked = marked;
            }
        }

        /// <summary>
        /// Finds the first <see cref="GraphComponent{TCoordinate}" /> 
        /// in an <see cref="IEnumerable{T}" />
        /// which has the specified <see cref="GraphComponent{TCoordinate}.Visited" /> state.
        /// </summary>
        /// <param name="components">An <see cref="IEnumerable{T}" /> to scan.</param>
        /// <param name="visitedState">The <see cref="GraphComponent{TCoordinate}.Visited" /> state to test.</param>
        /// <returns>
        /// The first <see cref="GraphComponent{TCoordinate}" /> found, or <see langword="null" /> if none found.
        /// </returns>
        public static GraphComponent<TCoordinate> GetComponentWithVisitedState(
            IEnumerable<GraphComponent<TCoordinate>> components, Boolean visitedState)
        {
            foreach (GraphComponent<TCoordinate> component in components)
            {
                if (visitedState == component.Visited)
                {
                    return component;
                }
            }

            return null;
        }

        #endregion

        protected Boolean _isMarked;
        protected Boolean _isVisited;

        /// <summary>
        /// Gets a value indicating if a component has been 
        /// visited during the course of a graph algorithm.
        /// </summary>              
        public Boolean IsVisited
        {
            get { return Visited; }
        }

        /// <summary> 
        /// Gets or sets the visited flag for this component.
        /// </summary>
        public Boolean Visited
        {
            get { return _isVisited; }
            set { _isVisited = value; }
        }

        /// <summary>
        /// Gets a value indicating if a component has been marked 
        /// at some point during the processing involving this graph.
        /// </summary>
        public Boolean IsMarked
        {
            get { return Marked; }
        }

        /// <summary>
        /// Gets or sets the marked flag for this component.
        /// </summary>
        public Boolean Marked
        {
            get { return _isMarked; }
            set { _isMarked = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this component 
        /// has been removed from its containing graph.
        /// </summary>
        public abstract Boolean IsRemoved { get; }
    }
}