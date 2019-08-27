using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{

    /// <summary>
    /// A vertex in a Constrained Delaunay Triangulation.
    /// The vertex may or may not lie on a constraint.
    /// If it does it may carry extra information about the original constraint.
    /// </summary>
    /// <author>Martin Davis</author>
    public class ConstraintVertex : Vertex
    {
        private bool isOnConstraint;
        private object constraint;

        /// <summary>
        /// Creates a new constraint vertex
        /// </summary>
        /// <param name="p">the location of the vertex</param>
        public ConstraintVertex(Coordinate p)
            : base(p)
        {
        }

        /// <summary>
        /// Gets or sets whether this vertex lies on a constraint.
        /// </summary>
        /// <remarks>true if the vertex lies on a constraint</remarks>
        public bool IsOnConstraint
        {
            get => isOnConstraint;
            set => this.isOnConstraint = value;
        }

        /// <summary>
        /// Gets or sets the external constraint object
        /// </summary>
        /// <remarks>object which carries information about the constraint this vertex lies on</remarks>
        public object Constraint
        {
            get => constraint;
            set
            {
                isOnConstraint = true;
                this.constraint = value;
            }
        }

        /// <summary>
        /// Merges the constraint data in the vertex <tt>other</tt> into this vertex.
        /// This method is called when an inserted vertex is
        /// very close to an existing vertex in the triangulation.
        /// </summary>
        /// <param name="other">the constraint vertex to merge</param>
        protected internal void Merge(ConstraintVertex other)
        {
            if (other.isOnConstraint) {
                isOnConstraint = true;
                constraint = other.constraint;
            }
        }
    }
}