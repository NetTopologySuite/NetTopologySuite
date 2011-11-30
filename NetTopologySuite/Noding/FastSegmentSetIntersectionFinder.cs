using System.Collections.Generic;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Finds if two sets of <see cref="ISegmentString"/>s intersect.
    /// </summary>
    /// <remarks>
    /// Uses indexing for fast performance and to optimize repeated tests
    /// against a target set of lines.
    /// Short-circuited to return as soon an intersection is found.
    /// </remarks>
    public class FastSegmentSetIntersectionFinder
    {
        private SegmentSetMutualIntersector _segSetMutInt; 

        //for testing purposes
        //private SimpleSegmentSetMutualIntersector mci;  

        public FastSegmentSetIntersectionFinder(IList<ISegmentString> baseSegStrings)
        {
            Init(baseSegStrings);
        }

        private void Init(IList<ISegmentString> baseSegStrings)
        {
            _segSetMutInt = new MCIndexSegmentSetMutualIntersector();
            //segSetMutInt = new MCIndexIntersectionSegmentSetMutualIntersector();
            //		mci = new SimpleSegmentSetMutualIntersector();
            _segSetMutInt.SetBaseSegments(baseSegStrings);
        }

        ///<summary>Gets the segment set intersector used by this class.</summary>
        /// <remarks>This allows other uses of the same underlying indexed structure.</remarks>
        public SegmentSetMutualIntersector SegmentSetIntersector
        {
            get { return _segSetMutInt; }
        }

        private readonly LineIntersector li = new RobustLineIntersector();

        public bool Intersects(IList<ISegmentString> segStrings)
        {            
            SegmentIntersectionDetector intFinder = new SegmentIntersectionDetector(li);
            _segSetMutInt.SegmentIntersector = intFinder;
            _segSetMutInt.Process(segStrings);
            return intFinder.HasIntersection;
        }

        public bool Intersects(IList<ISegmentString> segStrings, SegmentIntersectionDetector intDetector)
        {
            _segSetMutInt.SegmentIntersector = intDetector;

            _segSetMutInt.Process(segStrings);
            return intDetector.HasIntersection;
        }
    }
}