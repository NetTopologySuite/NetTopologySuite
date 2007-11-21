using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Bintree;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Implements <see cref="IPointInRing{TCoordinate}"/>
    /// using a <see cref="MonotoneChain{TCoordinate}"/> and a <see cref="BinTree{TCoordinate}"/> 
    /// index to increase performance.
    /// </summary>
    public class MCPointInRing<TCoordinate> : IPointInRing<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private class MCSelector : MonotoneChainSelectAction<TCoordinate>
        {
            private readonly MCPointInRing<TCoordinate> _container = null;
            private TCoordinate _p = default(TCoordinate);

            public MCSelector(MCPointInRing<TCoordinate> container, TCoordinate p)
            {
                _container = container;
                _p = p;
            }

            public override void Select(LineSegment<TCoordinate> ls)
            {
                _container.TestLineSegment(_p, ls);
            }
        }

        private ILinearRing<TCoordinate> ring;
        private Bintree tree;
        private Int32 crossings = 0; // number of segment/ray crossings

        private Interval interval = new Interval();

        public MCPointInRing(ILinearRing ring)
        {
            this.ring = ring;
            BuildIndex();
        }

        private void BuildIndex()
        {
            tree = new Bintree();

            ICoordinate[] pts = CoordinateArrays.RemoveRepeatedPoints(ring.Coordinates);
            IList mcList = MonotoneChainBuilder.GetChains(pts);

            for (Int32 i = 0; i < mcList.Count; i++)
            {
                MonotoneChain mc = (MonotoneChain) mcList[i];
                IExtents mcEnv = mc.Envelope;
                interval.Min = mcEnv.MinY;
                interval.Max = mcEnv.MaxY;
                tree.Insert(interval, mc);
            }
        }

        public Boolean IsInside(ICoordinate pt)
        {
            crossings = 0;

            // test all segments intersected by ray from pt in positive x direction
            IExtents rayEnv = new Extents(Double.NegativeInfinity, Double.PositiveInfinity, pt.Y, pt.Y);
            interval.Min = pt.Y;
            interval.Max = pt.Y;
            IEnumerable<MonotoneChain<TCoordinate>> segs = tree.Query(interval);

            MCSelector mcSelecter = new MCSelector(this, pt);
           
            for (IEnumerator i = segs.GetEnumerator(); i.MoveNext();)
            {
                MonotoneChain<TCoordinate> mc = (MonotoneChain) i.Current;
                testMonotoneChain(rayEnv, mcSelecter, mc);
            }

            /*
            *  p is inside if number of crossings is odd.
            */
            if ((crossings%2) == 1)
            {
                return true;
            }

            return false;
        }

        private static void testMonotoneChain(IExtents<TCoordinate> rayExtents, MCSelector mcSelecter, MonotoneChain<TCoordinate> mc)
        {
            mc.Select(rayExtents, mcSelecter);
        }

        private void TestLineSegment(ICoordinate p, LineSegment<TCoordinate> seg)
        {
            Double xInt; // x intersection of segment with ray
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
                /*
                *  segment straddles x axis, so compute intersection.
                */
                xInt = RobustDeterminant.SignOfDet2x2(x1, y1, x2, y2)/(y2 - y1);

                /*
                *  crosses ray if strictly positive intersection.
                */
                if (0.0 < xInt)
                {
                    crossings++;
                }
            }
        }
    }
}