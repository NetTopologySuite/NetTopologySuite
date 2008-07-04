using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// An index of LineSegments.
    /// </summary>
    public class LineSegmentIndex
    {
        private Quadtree index = new Quadtree();

        /// <summary>
        /// 
        /// </summary>
        public LineSegmentIndex() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void Add(TaggedLineString line) 
        {
            TaggedLineSegment[] segs = line.Segments;
            for (int i = 0; i < segs.Length - 1; i++) 
            {
                TaggedLineSegment seg = segs[i];
                Add(seg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        public void Add(LineSegment seg)
        {
            index.Insert(new Envelope(seg.P0, seg.P1), seg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        public void Remove(LineSegment seg)
        {
            index.Remove(new Envelope(seg.P0, seg.P1), seg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="querySeg"></param>
        /// <returns></returns>
        public IList Query(LineSegment querySeg)
        {
            Envelope env = new Envelope(querySeg.P0, querySeg.P1);

            LineSegmentVisitor visitor = new LineSegmentVisitor(querySeg);
            index.Query(env, visitor);
            IList itemsFound = visitor.Items;        

            return itemsFound;
        }
    }

    /// <summary>
    /// ItemVisitor subclass to reduce volume of query results.
    /// </summary>
    public class LineSegmentVisitor : IItemVisitor
    {
        // MD - only seems to make about a 10% difference in overall time.
        private LineSegment querySeg;
        private ArrayList items = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="querySeg"></param>
        public LineSegmentVisitor(LineSegment querySeg) 
        {
            this.querySeg = querySeg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void VisitItem(Object item)
        {
            LineSegment seg = (LineSegment) item;
            if (Envelope.Intersects(seg.P0, seg.P1, querySeg.P0, querySeg.P1))
                items.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public ArrayList Items 
        {
            get
            {
                return items;
            }
        }
    }
}
