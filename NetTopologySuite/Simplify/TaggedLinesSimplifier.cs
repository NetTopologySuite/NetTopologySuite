using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a collection of TaggedLineStrings, preserving topology
    /// (in the sense that no new intersections are introduced).
    /// </summary>
    public class TaggedLinesSimplifier
    {
        private LineSegmentIndex inputIndex = new LineSegmentIndex();
        private LineSegmentIndex outputIndex = new LineSegmentIndex();
        private Double distanceTolerance = 0.0;

        public TaggedLinesSimplifier() {}

        /// <summary>
        /// Gets or sets the distance tolerance for the simplification.
        /// Points closer than this tolerance to a simplified segment may
        /// be removed.
        /// </summary>        
        public Double DistanceTolerance
        {
            get { return distanceTolerance; }
            set { distanceTolerance = value; }
        }

        /// <summary>
        /// Simplify a collection of <c>TaggedLineString</c>s.
        /// </summary>
        /// <param name="taggedLines">The collection of lines to simplify.</param>
        public void Simplify(IList taggedLines)
        {
            for (IEnumerator i = taggedLines.GetEnumerator(); i.MoveNext();)
            {
                inputIndex.Add((TaggedLineString) i.Current);
            }
            for (IEnumerator i = taggedLines.GetEnumerator(); i.MoveNext();)
            {
                TaggedLineStringSimplifier tlss
                    = new TaggedLineStringSimplifier(inputIndex, outputIndex);
                tlss.DistanceTolerance = distanceTolerance;
                tlss.Simplify((TaggedLineString) i.Current);
            }
        }
    }
}