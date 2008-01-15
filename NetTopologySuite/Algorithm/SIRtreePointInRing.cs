using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Implements <see cref="IPointInRing{TCoordinate}"/> using 
    /// an <see cref="SirTree{TItem}"/> index to increase performance.
    /// </summary>
    public class SirTreePointInRing<TCoordinate> : IPointInRing<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ILinearRing<TCoordinate> _ring;
        private readonly SirTree<LineSegment<TCoordinate>> _sirTree
            = new SirTree<LineSegment<TCoordinate>>();
        private Int32 _crossings = 0; // number of segment/ray crossings

        public SirTreePointInRing(ILinearRing<TCoordinate> ring)
        {
            _ring = ring;
            buildIndex();
        }

        public Boolean IsInside(TCoordinate coordinate)
        {
            _crossings = 0;

            // test all segments intersected by vertical ray at pt
            IEnumerable<LineSegment<TCoordinate>> segs = _sirTree.Query(coordinate[Ordinates.Y]);

            foreach (LineSegment<TCoordinate> seg in segs)
            {
                testLineSegment(coordinate, seg);
            }

            /*
            *  p is inside if number of crossings is odd.
            */
            if ((_crossings % 2) == 1)
            {
                return true;
            }

            return false;
        }

        private void buildIndex()
        {
            IEnumerable<TCoordinate> pts = _ring.Coordinates;

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(pts))
            {
                if (pair.First.Equals(pair.Second))
                {
                    continue;
                }

                LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(pair.First, pair.Second);
                _sirTree.Insert(seg.P0[Ordinates.Y], seg.P1[Ordinates.Y], seg);
            }
        }

        private void testLineSegment(TCoordinate p, LineSegment<TCoordinate> seg)
        {
            Double x1; // translated coordinates
            Double y1;
            Double x2;
            Double y2;

            /*
            *  Test if segment crosses ray from test point in positive x direction.
            */
            TCoordinate p1 = seg.P0;
            TCoordinate p2 = seg.P1;

            x1 = p1[Ordinates.X] - p[Ordinates.X];
            y1 = p1[Ordinates.Y] - p[Ordinates.Y];
            x2 = p2[Ordinates.X] - p[Ordinates.X];
            y2 = p2[Ordinates.Y] - p[Ordinates.Y];

            if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
            {
                Double xInt; // x intersection of segment with ray

                /*
                *  segment straddles x axis, so compute intersection.
                */
                xInt = RobustDeterminant.SignOfDet2x2(x1, y1, x2, y2) / (y2 - y1);

                /*
                *  crosses ray if strictly positive intersection.
                */
                if (0.0 < xInt)
                {
                    _crossings++;
                }
            }
        }
    }
}