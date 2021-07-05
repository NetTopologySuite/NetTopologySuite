using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Assigns hole rings to shell rings
    /// during polygonization.
    /// Uses spatial indexing to improve performance
    /// of shell lookup.
    /// </summary>
    /// <author>mdavis</author>
    public class HoleAssigner
    {
        /// <summary>
        /// Assigns hole rings to shell rings.
        /// </summary>
        /// <param name="holes">An enumeration of hole rings to assign</param>
        /// <param name="shells">An enumeration of shell rings</param>
        public static void AssignHolesToShells(IEnumerable<EdgeRing> holes, IEnumerable<EdgeRing> shells)
        {
            var assigner = new HoleAssigner(shells);
            assigner.AssignHolesToShells(holes);
        }

        //private readonly IList<EdgeRing> _shells;
        private readonly ISpatialIndex<EdgeRing> _shellIndex;

        /// <summary>
        /// Creates a new hole assigner.
        /// </summary>
        /// <param name="shells">An enumeration of shell rings to assign holes to</param>
        public HoleAssigner(IEnumerable<EdgeRing> shells)
        {
            //_shells = shells;
            _shellIndex = BuildIndex(shells);
        }

        private static ISpatialIndex<EdgeRing> BuildIndex(IEnumerable<EdgeRing> shells)
        {
            var shellIndex = new STRtree<EdgeRing>();
            foreach (var shell in shells)
                shellIndex.Insert(shell.Ring.EnvelopeInternal, shell);
            return shellIndex;
        }

        /// <summary>
        /// Assigns holes to the shells.
        /// </summary>
        /// <param name="holes">An enumeration of holes to assign to shells</param>
        public void AssignHolesToShells(IEnumerable<EdgeRing> holes)
        {
            foreach (var holeER in holes)
            {
                AssignHoleToShell(holeER);
            }
        }

        private void AssignHoleToShell(EdgeRing holeER)
        {
            var shell = FindShellContaining(holeER);
            if (shell != null)
            {
                shell.AddHole(holeER);
            }
        }

        private IList<EdgeRing> QueryOverlappingShells(Envelope ringEnv)
        {
            return _shellIndex.Query(ringEnv);
        }

        /// <summary>
        /// Find the innermost enclosing shell EdgeRing containing the argument EdgeRing, if any.
        /// The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        /// The algorithm used depends on the fact that:
        /// <list type="Bullet">
        /// <item><term>ring A contains ring B if envelope(ring A) contains envelope(ring B)</term></item>
        /// </list>
        /// This routine is only safe to use if the chosen point of the hole
        /// is known to be properly contained in a shell
        /// (which is guaranteed to be the case if the hole does not touch its shell)
        /// </summary>
        /// <param name="testER">An edge ring to test</param>
        /// <returns>
        /// The containing shell EdgeRing, if there is one
        /// or <c>null</c> if no containing EdgeRing is found
        /// </returns>
        private EdgeRing FindShellContaining(EdgeRing testER)
        {
            var testEnv = testER.Ring.EnvelopeInternal;
            var candidateShells = QueryOverlappingShells(testEnv);
            return EdgeRing.FindEdgeRingContaining(testER, candidateShells);
        }
    }

}
