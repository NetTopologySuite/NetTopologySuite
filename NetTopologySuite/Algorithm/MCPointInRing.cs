using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
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
                            IComputable<Double, TCoordinate>, IConvertible
    {
        //private class MCSelector : MonotoneChainSelectAction<TCoordinate>
        //{
        //    private readonly MCPointInRing<TCoordinate> _container = null;
        //    private readonly TCoordinate _p = default(TCoordinate);

        //    public MCSelector(MCPointInRing<TCoordinate> container, TCoordinate p)
        //    {
        //        _container = container;
        //        _p = p;
        //    }

        //    public override void Select(LineSegment<TCoordinate> ls)
        //    {
        //        _container.testLineSegment(_p, ls);
        //    }
        //}

        private readonly ILinearRing<TCoordinate> _ring;
        private readonly BinTree<MonotoneChain<TCoordinate>> _tree = new BinTree<MonotoneChain<TCoordinate>>();
        private Int32 _crossings = 0; // number of segment/ray crossings

        private Interval _interval = new Interval();

        public MCPointInRing(ILinearRing<TCoordinate> ring)
        {
            _ring = ring;
            buildIndex();
        }

        private void buildIndex()
        {
            ICoordinateSequence<TCoordinate> coordinates = _ring.Coordinates.WithoutRepeatedPoints();
            IEnumerable<MonotoneChain<TCoordinate>> chains = MonotoneChainBuilder.GetChains(coordinates);

            foreach (MonotoneChain<TCoordinate> chain in chains)
            {
                IExtents<TCoordinate> extents = chain.Extents;
                Double min = extents.GetMin(Ordinates.Y);
                Double max = extents.GetMax(Ordinates.Y);
                _interval = new Interval(min, max);
                _tree.Insert(_interval, chain);
            }
        }

        public Boolean IsInside(TCoordinate pt)
        {
            _crossings = 0;

            Double y = pt[Ordinates.Y];

            // test all segments intersected by ray from pt in positive x direction
            IExtents<TCoordinate> rayExtents = new Extents<TCoordinate>(
                Double.NegativeInfinity, Double.PositiveInfinity, y, y);

            _interval = new Interval(y, y);

            IEnumerable<MonotoneChain<TCoordinate>> chains = _tree.Query(_interval);

            foreach (MonotoneChain<TCoordinate> chain in chains)
            {
                testMonotoneChain(rayExtents, pt, chain);
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

        private void testMonotoneChain(IExtents<TCoordinate> rayExtents, TCoordinate point, MonotoneChain<TCoordinate> chain)
        {
            foreach (LineSegment<TCoordinate> segment in chain.Select(rayExtents))
            {
                testLineSegment(point, segment);
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