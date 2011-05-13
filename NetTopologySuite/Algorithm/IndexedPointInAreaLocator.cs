using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.IntervalRTree;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    public class IndexedPointInAreaLocator : IPointInAreaLocator
    {
        private IGeometry _areaGeom;
        private IntervalIndexedGeometry _index;

        /**
         * Creates a new locator for a given {@link Geometry}
         * @param g the Geometry to locate in
         */
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

        /**
         * Determines the {@link Location} of a point in an areal {@link Geometry}.
         * 
         * @param p the point to test
         * @return the location of the point in the geometry  
         */
        public Locations Locate(ICoordinate p)
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

        private class SegmentVisitor : IItemVisitor
        {
            private readonly RayCrossingCounter _counter;

            public SegmentVisitor(RayCrossingCounter counter)
            {
                _counter = counter;
            }

            public void VisitItem(Object item)
            {
                LineSegment seg = (LineSegment)item;
                _counter.CountSegment(seg.GetCoordinate(0), seg.GetCoordinate(1));
            }
        }

        private class IntervalIndexedGeometry
        {
            private readonly SortedPackedIntervalRTree _index = new SortedPackedIntervalRTree();

            public IntervalIndexedGeometry(IGeometry geom)
            {
                Init(geom);
            }

            private void Init(IGeometry geom)
            {
                IList lines = LinearComponentExtracter.GetLines(geom);
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

            public IList Query(double min, double max)
            {
                ArrayListVisitor visitor = new ArrayListVisitor();
                _index.Query(min, max, visitor);
                return visitor.Items;
            }

            public void Query(double min, double max, IItemVisitor visitor)
            {
                _index.Query(min, max, visitor);
            }
        }

    }
}
