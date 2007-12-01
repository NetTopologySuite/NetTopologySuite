using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// An index of <see cref="LineSegment{TCoordinate}"/>s.
    /// </summary>
    public class LineSegmentIndex<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly Quadtree<TCoordinate, LineSegment<TCoordinate>> _index 
            = new Quadtree<TCoordinate, LineSegment<TCoordinate>>();

        public void Add(TaggedLineString<TCoordinate> line)
        {
            foreach (TaggedLineSegment<TCoordinate> segment in line.Segments)
            {
                Add(segment);
            }
        }

        public void Add(LineSegment<TCoordinate> seg)
        {
            _index.Insert(new Extents<TCoordinate>(seg.P0, seg.P1), seg);
        }

        public void Remove(LineSegment<TCoordinate> seg)
        {
            _index.Remove(new Extents<TCoordinate>(seg.P0, seg.P1), seg);
        }

        public IEnumerable<LineSegment<TCoordinate>> Query(LineSegment<TCoordinate> querySeg)
        {
            Extents<TCoordinate> env = new Extents<TCoordinate>(querySeg.P0, querySeg.P1);

            Predicate<LineSegment<TCoordinate>> predicate =
                delegate(LineSegment<TCoordinate> seg)
                {
                    return Extents<TCoordinate>.Intersects(seg.P0, seg.P1, querySeg.P0, querySeg.P1);
                };

            return _index.Query(env, predicate);
        }
    }

    ///// <summary>
    ///// ItemVisitor subclass to reduce volume of query results.
    ///// </summary>
    //public class LineSegmentVisitor<TCoordinate> : IItemVisitor
    //    where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
    //        IComputable<TCoordinate>, IConvertible
    //{
    //    // MD - only seems to make about a 10% difference in overall time.
    //    private LineSegment<TCoordinate> querySeg;
    //    private ArrayList items = new ArrayList();

    //    public LineSegmentVisitor(LineSegment<TCoordinate> querySeg)
    //    {
    //        this.querySeg = querySeg;
    //    }

    //    public void VisitItem(Object item)
    //    {
    //        LineSegment seg = (LineSegment) item;
    //        if ()
    //        {
    //            items.Add(item);
    //        }
    //    }

    //    public ArrayList Items
    //    {
    //        get { return items; }
    //    }
    //}
}