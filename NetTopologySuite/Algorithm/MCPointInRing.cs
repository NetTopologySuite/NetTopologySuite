using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Bintree;
using GisSharpBlog.NetTopologySuite.Index.Chain;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Implements <c>IPointInRing</c>
    /// using a <c>MonotoneChain</c>s and a <c>BinTree</c> index to increase performance.
    /// </summary>
    public class MCPointInRing : IPointInRing
    {
        private class MCSelecter : MonotoneChainSelectAction
        {
            private MCPointInRing container = null;
            private ICoordinate p = null;

            public MCSelecter(MCPointInRing container, ICoordinate p)
            {
                this.container = container;
                this.p = p;
            }

            public override void Select(LineSegment ls)
            {
                container.TestLineSegment(p, ls);
            }
        }

        private ILinearRing ring;
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
            IList segs = tree.Query(interval);

            MCSelecter mcSelecter = new MCSelecter(this, pt);
           
            for (IEnumerator i = segs.GetEnumerator(); i.MoveNext();)
            {
                MonotoneChain mc = (MonotoneChain) i.Current;
                TestMonotoneChain(rayEnv, mcSelecter, mc);
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

        private void TestMonotoneChain(IExtents rayEnv, MCSelecter mcSelecter, MonotoneChain mc)
        {
            mc.Select(rayEnv, mcSelecter);
        }

        private void TestLineSegment(ICoordinate p, LineSegment seg)
        {
            Double xInt; // x intersection of segment with ray
            Double x1; // translated coordinates
            Double y1;
            Double x2;
            Double y2;

            /*
            *  Test if segment crosses ray from test point in positive x direction.
            */
            ICoordinate p1 = seg.P0;
            ICoordinate p2 = seg.P1;
            x1 = p1.X - p.X;
            y1 = p1.Y - p.Y;
            x2 = p2.X - p.X;
            y2 = p2.Y - p.Y;

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