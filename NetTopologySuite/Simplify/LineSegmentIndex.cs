using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NPack.Interfaces;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// An index of <see cref="LineSegment{TCoordinate}"/>s.
    /// </summary>
    public class LineSegmentIndex<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly Quadtree<TCoordinate, TaggedLineSegment<TCoordinate>> _index;

        public LineSegmentIndex(IGeometryFactory<TCoordinate> geometryFactory)
        {
            _index = new Quadtree<TCoordinate, TaggedLineSegment<TCoordinate>>(geometryFactory);
        }
        
        public void Add(TaggedLineString<TCoordinate> line)
        {
            foreach (TaggedLineSegment<TCoordinate> segment in line.Segments)
            {
                Add(segment);
            }
        }

        public void Add(TaggedLineSegment<TCoordinate> seg)
        {
        
            _index.Insert(seg);
        }

        public void Remove(TaggedLineSegment<TCoordinate> seg)
        {
            _index.Remove(seg);
        }

        public IEnumerable<TaggedLineSegment<TCoordinate>> Query(TaggedLineSegment<TCoordinate> querySeg)
        {
            IExtents<TCoordinate> extents = querySeg.Bounds;

            Predicate<TaggedLineSegment<TCoordinate>> predicate =
                delegate(TaggedLineSegment<TCoordinate> seg)
                {
                    return Extents<TCoordinate>.Intersects(
                        seg.LineSegment.P0, seg.LineSegment.P1,
                        querySeg.LineSegment.P0, querySeg.LineSegment.P1);
                };

            return _index.Query(extents, predicate);
        }

        //public IGeometryFactory<TCoordinate> Factory
        //{
        //    get { return _index.Factory; }
        //}

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