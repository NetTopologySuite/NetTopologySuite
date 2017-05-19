using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Bintree;
using NetTopologySuite.Index.Chain;
//using System.Collections;
//using GeoAPI.DataStructures;
//using NetTopologySuite.Index.Bintree;

//using Interval = GeoAPI.DataStructures.Interval;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    ///     Implements <c>IPointInRing</c>
    ///     using a <c>MonotoneChain</c>s and a <c>BinTree</c> index to increase performance.
    /// </summary>
    /// <see cref="IndexedPointInAreaLocator" />
    public class MCPointInRing : IPointInRing
    {
        private readonly Interval _interval = new Interval();

        private readonly ILinearRing _ring;
        private int _crossings; // number of segment/ray crossings
        private Bintree<MonotoneChain> _tree;
        //private Interval _interval = Interval.Create();

        /// <summary>
        /// </summary>
        /// <param name="ring"></param>
        public MCPointInRing(ILinearRing ring)
        {
            _ring = ring;
            BuildIndex();
        }

        /// <summary>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsInside(Coordinate pt)
        {
            _crossings = 0;

            // test all segments intersected by ray from pt in positive x direction
            var rayEnv = new Envelope(double.NegativeInfinity, double.PositiveInfinity, pt.Y, pt.Y);
            _interval.Min = pt.Y;
            _interval.Max = pt.Y;
            //_interval = Interval.Create(pt.Y);
            var segs = _tree.Query(_interval);

            var mcSelecter = new MCSelecter(this, pt);
            foreach (var mc in segs)
                TestMonotoneChain(rayEnv, mcSelecter, mc);

            /*
             *  p is inside if number of crossings is odd.
             */
            if (_crossings%2 == 1)
                return true;
            return false;
        }

        /// <summary>
        /// </summary>
        private void BuildIndex()
        {
            _tree = new Bintree<MonotoneChain>();

            var pts = CoordinateArrays.RemoveRepeatedPoints(_ring.Coordinates);
            var mcList = MonotoneChainBuilder.GetChains(pts);

            foreach (var mc in mcList)
            {
                var mcEnv = mc.Envelope;
                _interval.Min = mcEnv.MinY;
                _interval.Max = mcEnv.MaxY;
                /*
                _interval = _interval.ExpandedByInterval(
                    Interval.Create(mcEnv.MinY, mcEnv.MaxY));
                 */
                _tree.Insert(_interval, mc);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="rayEnv"></param>
        /// <param name="mcSelecter"></param>
        /// <param name="mc"></param>
        private static void TestMonotoneChain(Envelope rayEnv, MCSelecter mcSelecter, MonotoneChain mc)
        {
            mc.Select(rayEnv, mcSelecter);
        }

        /// <summary>
        /// </summary>
        /// <param name="p"></param>
        /// <param name="seg"></param>
        private void TestLineSegment(Coordinate p, LineSegment seg)
        {
            double xInt; // x intersection of segment with ray
            double x1; // translated coordinates
            double y1;
            double x2;
            double y2;

            /*
            *  Test if segment crosses ray from test point in positive x direction.
            */
            var p1 = seg.P0;
            var p2 = seg.P1;
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
                    _crossings++;
            }
        }

        /// <summary>
        /// </summary>
        private class MCSelecter : MonotoneChainSelectAction
        {
            private readonly MCPointInRing _container;
            private readonly Coordinate _p;

            /// <summary>
            /// </summary>
            /// <param name="container"></param>
            /// <param name="p"></param>
            public MCSelecter(MCPointInRing container, Coordinate p)
            {
                _container = container;
                _p = p;
            }

            /// <summary>
            /// </summary>
            /// <param name="ls"></param>
            public override void Select(LineSegment ls)
            {
                _container.TestLineSegment(_p, ls);
            }
        }
    }
}