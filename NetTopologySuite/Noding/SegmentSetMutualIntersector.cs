using System.Collections.Generic;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// An intersector for the red-blue intersection problem.
    /// In this class of line arrangement problem,
    /// two disjoint sets of linestrings are provided.
    /// It is assumed that within
    /// each set, no two linestrings intersect except possibly at their endpoints.
    /// Implementations can take advantage of this fact to optimize processing.
    ///</summary>
    public abstract class SegmentSetMutualIntersector
    {
        private ISegmentIntersector _segInt;

        ///<summary>
        /// Gets/Sets the <see cref="ISegmentIntersector"/> to use with this intersector.
        ///</summary>
        /// <remarks>
        /// The SegmentIntersector will either rocord or add intersection nodes
        /// for the input segment strings.
        /// </remarks>
        public ISegmentIntersector SegmentIntersector
        { 
            get { return _segInt; }
            protected internal set { _segInt = value; }
        }

        ///<summary>
        ///</summary>
        /// <param name="segStrings">A collection of {@link SegmentString}s to node</param>
        public abstract void SetBaseSegments(IList<ISegmentString> segStrings);

        ///<summary>
        /// Computes the intersections for two collections of <see cref="ISegmentString"/>s.
        ///</summary>
        public abstract void Process(IList<ISegmentString> segStrings);
    }
}
