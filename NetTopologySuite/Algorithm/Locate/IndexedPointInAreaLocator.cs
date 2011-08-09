using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index;
using NetTopologySuite.Index.IntervalRTree;

namespace NetTopologySuite.Algorithm.Locate
{
    ///<summary>
    /// Determines the location of <see cref="ICoordinate"/>s relative to
    /// a <see cref="IPolygonal"/> geometry, using indexing for efficiency.
    /// This algorithm is suitable for use in cases where
    /// many points will be tested against a given area.
    ///</summary>
    /// <author>Martin Davis</author>
    public class IndexedPointInAreaLocator : IPointOnGeometryLocator
    {
        private IGeometry _areaGeom;
        private IntervalIndexedGeometry _index;

        ///<summary>
        /// Creates a new locator for a given <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="g">the Geometry to locate in</param>
        public IndexedPointInAreaLocator(IGeometry g)
        {
            _areaGeom = g;
            if (!(g is IPolygonal))
                throw new ArgumentException("Argument must be Polygonal");
            BuildIndex(g);
        }

        private void BuildIndex(IGeometry g)
        {
            _index = new IntervalIndexedGeometry(g);
        }

        ///<summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="p">The point to test</param>
        /// <returns>The location of the point in the geometry
        /// </returns>
        public Location Locate(ICoordinate p)
        {
            RayCrossingCounter rcc = new RayCrossingCounter(p);


            SegmentVisitor visitor = new SegmentVisitor(rcc);
            _index.Query(p.Y, p.Y, visitor);

            /*
             // MD - slightly slower alternative
            List segs = index.query(p.y, p.y);
            countSegs(rcc, segs);
            */

            return rcc.Location;
        }

        /*
        private void countSegs(RayCrossingCounter rcc, List segs)
        {
          for (Iterator i = segs.iterator(); i.hasNext(); ) {
            LineSegment seg = (LineSegment) i.next();
            rcc.countSegment(seg.getCoordinate(0), seg.getCoordinate(1));
      
            // short-circuit if possible
            if (rcc.isOnSegment()) return;
          }
        }
        */

        private class SegmentVisitor : IItemVisitor<LineSegment>
        {
            private readonly RayCrossingCounter _counter;

            public SegmentVisitor(RayCrossingCounter counter)
            {
                _counter = counter;
            }

            public void VisitItem(LineSegment seg)
            {
                _counter.CountSegment(seg.GetCoordinate(0), seg.GetCoordinate(1));
            }
        }

        private class IntervalIndexedGeometry
        {
            private readonly SortedPackedIntervalRTree<LineSegment> _index = new SortedPackedIntervalRTree<LineSegment>();

            public IntervalIndexedGeometry(IGeometry geom)
            {
                Init(geom);
            }

            private void Init(IGeometry geom)
            {
                var lines = LinearComponentExtracter.GetLines(geom);
                foreach (ILineString line in lines)
                {
                    ICoordinate[] pts = line.Coordinates;
                    AddLine(pts);
                }
            }

            private void AddLine(ICoordinate[] pts)
            {
                for (int i = 1; i < pts.Length; i++)
                {
                    LineSegment seg = new LineSegment(pts[i - 1], pts[i]);
                    double min = Math.Min(seg.P0.Y, seg.P1.Y);
                    double max = Math.Max(seg.P0.Y, seg.P1.Y);
                    _index.Insert(min, max, seg);
                }
            }

            /*
            public IList Query(double min, double max)
            {
                ArrayListVisitor visitor = new ArrayListVisitor();
                index.Query(min, max, visitor);
                return visitor.Items;
            }
             */

            public void Query(double min, double max, IItemVisitor<LineSegment> visitor)
            {
                _index.Query(min, max, visitor);
            }
        }

    }
}