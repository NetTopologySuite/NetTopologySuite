using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Evaluates the full relate <see cref="IntersectionMatrix"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class RelateMatrixPredicate : IMPredicate
    {
        public RelateMatrixPredicate() : base("relateMatrix")
        {
        }

        public override bool RequireInteraction()
        {
            // -- ensure entire matrix is computed
            return false;
        }

        public override bool IsDetermined 
            //-- ensure entire matrix is computed
            => false;
        

        public override bool ValueIM
            //-- indicates full matrix is being evaluated
            => false;


        /// <summary>
        /// Gets the current state of the IM matrix (which may only be partially complete).
        /// </summary>
        public IntersectionMatrix IM => intMatrix;

    }
}
