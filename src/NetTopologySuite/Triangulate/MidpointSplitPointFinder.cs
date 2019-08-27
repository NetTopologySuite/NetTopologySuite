using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// A simple split point finder which returns the midpoint of the split segment. This is a default
    /// strategy only. Usually a more sophisticated strategy is required to prevent repeated splitting.
    /// Other points which could be used are:
    /// <ul>
    /// <li>The projection of the encroaching point on the segment</li>
    /// <li>A point on the segment which will produce two segments which will not be further encroached</li>
    /// <li>The point on the segment which is the same distance from an endpoint as the encroaching</li>
    /// point
    /// </ul>
    /// </summary>
    /// <author>Martin Davis</author>
    public class MidpointSplitPointFinder : IConstraintSplitPointFinder
    {
        /// <summary>
        /// Gets the midpoint of the split segment
        /// </summary>
        public Coordinate FindSplitPoint(Segment seg, Coordinate encroachPt)
        {
            var p0 = seg.Start;
            var p1 = seg.End;
            return new Coordinate((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
        }
    }
}