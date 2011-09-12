#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
#endif
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
        private bool _isOnConstraint;
        private object _constraint;

        /// <summary>
        /// Creates a new constraint vertex
        /// </summary>
        /// <param name="p">the location of the vertex</param>
        public ConstraintVertex(ICoordinate p)
            : base(p)
        {
        }

        /// <summary>
        /// Gets or sets whether this vertex lies on a constraint.
        /// </summary>
        /// <remarks>true if the vertex lies on a constraint</remarks>
        public bool IsOnConstraint
        {
            get
            {
                return _isOnConstraint;
            }
            set
            {
                this._isOnConstraint = value;
            }
        }

        /// <summary>
        /// Gets or sets the external constraint object
        /// </summary>
        /// <remarks>object which carries information about the constraint this vertex lies on</remarks>
        public object Constraint
        {
            get
            {
                return _constraint;
            }
            set
            {
                _isOnConstraint = true;
                this._constraint = value;
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
            if (other._isOnConstraint) {
                _isOnConstraint = true;
                _constraint = other._constraint;
            }
        }
    }
}