using System.Collections;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Dissolves a noded collection of <see cref="SegmentString" />s to produce
    /// a set of merged linework with unique segments.
    /// A custom merging strategy can be applied when two identical (up to orientation)
    /// strings are dissolved together.
    /// The default merging strategy is simply to discard the merged string.
    ///<para>
    /// A common use for this class is to merge noded edges
    /// while preserving topological labelling.
    /// </para>
    /// </summary>
    public class SegmentStringDissolver
    {
        /// <summary>
        /// 
        /// </summary>
        public interface ISegmentStringMerger
        {
            /// <summary>
            /// Updates the context data of a <see cref="SegmentString" />
            /// when an identical (up to orientation) one is found during dissolving.
            /// </summary>
            /// <param name="mergeTarget">The segment string to update.</param>
            /// <param name="ssToMerge">The segment string being dissolved.</param>
            /// <param name="isSameOrientation">
            /// <c>true</c> if the strings are in the same direction,
            /// <c>false</c> if they are opposite.
            /// </param>
            void Merge(SegmentString mergeTarget, SegmentString ssToMerge, bool isSameOrientation);
        }

        private ISegmentStringMerger merger;
        private IDictionary ocaMap = new SortedList();
        
        /// <summary>
        /// Creates a dissolver with a user-defined merge strategy.
        /// </summary>
        /// <param name="merger"></param>
        public SegmentStringDissolver(ISegmentStringMerger merger)
        {
            this.merger = merger;
        }

        /// <summary>
        /// Creates a dissolver with the default merging strategy.
        /// </summary>
        public SegmentStringDissolver()
            : this(null) { }

        /// <summary>
        /// Dissolve all <see cref="SegmentString" />s in the input <see cref="ICollection"/>.
        /// </summary>
        /// <param name="segStrings"></param>
        public void Dissolve(ICollection segStrings)
        {
            foreach(object obj in segStrings)
                Dissolve((SegmentString)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oca"></param>
        /// <param name="segString"></param>
        private void Add(OrientedCoordinateArray oca, SegmentString segString)
        {
            ocaMap.Add(oca, segString);
        }

        /// <summary>
        /// Dissolve the given <see cref="SegmentString" />.
        /// </summary>
        /// <param name="segString"></param>
        public void Dissolve(SegmentString segString)
        {
            OrientedCoordinateArray oca = new OrientedCoordinateArray(segString.Coordinates);
            SegmentString existing = FindMatching(oca, segString);
            if (existing == null)
                Add(oca, segString);            
            else
            {
                if (merger != null)
                {
                    bool isSameOrientation = CoordinateArrays.Equals(existing.Coordinates, segString.Coordinates);
                    merger.Merge(existing, segString, isSameOrientation);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oca"></param>
        /// <param name="segString"></param>
        /// <returns></returns>
        private SegmentString FindMatching(OrientedCoordinateArray oca, SegmentString segString)
        {
            return (SegmentString)ocaMap[oca];            
        }        

        /// <summary>
        /// Gets the collection of dissolved (i.e. unique) <see cref="SegmentString" />s
        /// </summary>
        public ICollection Dissolved
        {
            get
            {
                return ocaMap.Values;
            }
        }

    }
}
