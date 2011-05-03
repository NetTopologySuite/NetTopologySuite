using System.Collections;
using GisSharpBlog.NetTopologySuite.Geometries;
using Wintellect.PowerCollections;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Dissolves a noded collection of <see cref="ISegmentString" />s to produce
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
        private readonly IDictionary _ocaMap = new OrderedDictionary<OrientedCoordinateArray, object>();
        
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
        /// Dissolve all <see cref="ISegmentString" />s in the input <see cref="ICollection"/>.
        /// </summary>
        /// <param name="segStrings"></param>
        public void Dissolve(ICollection segStrings)
        {
            foreach(object obj in segStrings)
                Dissolve((ISegmentString)obj);
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
            OrientedCoordinateArray oca = new OrientedCoordinateArray(segString.Coordinates);
            ISegmentString existing = FindMatching(oca /*, segString*/);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oca"></param>
        /*/// <param name="segString"></param>*/
        /// <returns></returns>
        private ISegmentString FindMatching(OrientedCoordinateArray oca /*, ISegmentString segString*/)
        {
            return (ISegmentString)_ocaMap[oca];            
        }        

        /// <summary>
        /// Gets the collection of dissolved (i.e. unique) <see cref="ISegmentString" />s
        /// </summary>
        public ICollection Dissolved
        {
            get
            {
                return _ocaMap.Values;
            }
        }

    }
}
