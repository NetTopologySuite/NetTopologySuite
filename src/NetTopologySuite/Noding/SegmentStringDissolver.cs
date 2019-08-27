using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{

    /// <summary>
    /// Dissolves a noded collection of <see cref="ISegmentString" />s to produce
    /// a set of merged linework with unique segments.
    /// </summary>
    /// <remarks>
    /// A custom <see cref="ISegmentStringMerger"/> merging strategy
    /// can be supplied.
    /// This strategy will be called when two identical (up to orientation)
    /// strings are dissolved together.
    /// The default merging strategy is simply to discard one of the merged strings.
    /// <para>
    /// A common use for this class is to merge noded edges
    /// while preserving topological labelling.
    /// This requires a custom merging strategy to be supplied
    /// to merge the topology labels appropriately.
    /// </para>
    /// </remarks>
    public class SegmentStringDissolver
    {
        /// <summary>
        /// A merging strategy which can be used to update the context data of <see cref="ISegmentString"/>s
        /// which are merged during the dissolve process.
        /// </summary>
        /// <author>mbdavis</author>
        public interface ISegmentStringMerger
        {
            /// <summary>
            /// Updates the context data of a <see cref="ISegmentString" />
            /// when an identical (up to orientation) one is found during dissolving.
            /// </summary>
            /// <param name="mergeTarget">The segment string to update.</param>
            /// <param name="ssToMerge">The segment string being dissolved.</param>
            /// <param name="isSameOrientation">
            /// <c>true</c> if the strings are in the same direction,
            /// <c>false</c> if they are opposite.
            /// </param>
            void Merge(ISegmentString mergeTarget, ISegmentString ssToMerge, bool isSameOrientation);
        }

        private readonly ISegmentStringMerger _merger;
        private readonly IDictionary<OrientedCoordinateArray, ISegmentString> _ocaMap =
            new SortedDictionary<OrientedCoordinateArray, ISegmentString>();

        /// <summary>
        /// Creates a dissolver with a user-defined merge strategy.
        /// </summary>
        /// <param name="merger"></param>
        public SegmentStringDissolver(ISegmentStringMerger merger)
        {
            _merger = merger;
        }

        /// <summary>
        /// Creates a dissolver with the default merging strategy.
        /// </summary>
        public SegmentStringDissolver()
            : this(null) { }

        /// <summary>
        /// Dissolve all <see cref="ISegmentString" />s in the input <see cref="IEnumerable{ISegmentString}"/>.
        /// </summary>
        /// <param name="segStrings"></param>
        public void Dissolve(IEnumerable<ISegmentString> segStrings)
        {
            foreach(var obj in segStrings)
                Dissolve(obj);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="oca"></param>
        /// <param name="segString"></param>
        private void Add(OrientedCoordinateArray oca, ISegmentString segString)
        {
            _ocaMap.Add(oca, segString);
        }

        /// <summary>
        /// Dissolve the given <see cref="ISegmentString" />.
        /// </summary>
        /// <param name="segString"></param>
        public void Dissolve(ISegmentString segString)
        {
            var oca = new OrientedCoordinateArray(segString.Coordinates);
            var existing = FindMatching(oca /*, segString*/);
            if (existing == null)
                Add(oca, segString);
            else
            {
                if (_merger != null)
                {
                    bool isSameOrientation = CoordinateArrays.Equals(existing.Coordinates, segString.Coordinates);
                    _merger.Merge(existing, segString, isSameOrientation);
                }
            }
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="oca"></param>
        /*/// <param name="segString"></param>*/
        /// <returns></returns>
        private ISegmentString FindMatching(OrientedCoordinateArray oca /*, ISegmentString segString*/)
        {
            ISegmentString ret;
            if (_ocaMap.TryGetValue(oca, out ret))
                return ret;
            return null;
        }

        /// <summary>
        /// Gets the collection of dissolved (i.e. unique) <see cref="ISegmentString" />s
        /// </summary>
        public ICollection<ISegmentString> Dissolved => _ocaMap.Values;
    }
}
