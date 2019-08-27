using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Quadtree;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// An index of LineSegments.
    /// </summary>
    public class LineSegmentIndex
    {
        private readonly ISpatialIndex<LineSegment>_index = new Quadtree<LineSegment>();

        /*
        /// <summary>
        ///
        /// </summary>
        public LineSegmentIndex() { }
        */
        /// <summary>
        ///
        /// </summary>
        /// <param name="line"></param>
        public void Add(TaggedLineString line)
        {
            var segs = line.Segments;
            for (int i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                Add(seg);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seg"></param>
        public void Add(LineSegment seg)
        {
            _index.Insert(new Envelope(seg.P0, seg.P1), seg);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seg"></param>
        public void Remove(LineSegment seg)
        {
            _index.Remove(new Envelope(seg.P0, seg.P1), seg);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="querySeg"></param>
        /// <returns></returns>
        public IList<LineSegment> Query(LineSegment querySeg)
        {
            var env = new Envelope(querySeg.P0, querySeg.P1);

            var visitor = new LineSegmentVisitor(querySeg);
            _index.Query(env, visitor);
            var itemsFound = visitor.Items;

            return itemsFound;
        }
    }

    /// <summary>
    /// ItemVisitor subclass to reduce volume of query results.
    /// </summary>
    public class LineSegmentVisitor : IItemVisitor<LineSegment>
    {
        // MD - only seems to make about a 10% difference in overall time.
        private readonly LineSegment _querySeg;
        private readonly IList<LineSegment> _items = new List<LineSegment>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="querySeg"></param>
        public LineSegmentVisitor(LineSegment querySeg)
        {
            _querySeg = querySeg;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        public void VisitItem(LineSegment item)
        {
            var seg = item;
            if (Envelope.Intersects(seg.P0, seg.P1, _querySeg.P0, _querySeg.P1))
                _items.Add(seg);
        }

        /// <summary>
        ///
        /// </summary>
        public IList<LineSegment> Items => _items;
    }
}
