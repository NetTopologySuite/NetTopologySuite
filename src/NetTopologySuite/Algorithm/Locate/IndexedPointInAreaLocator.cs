using System;
using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index;
using NetTopologySuite.Index.IntervalRTree;

namespace NetTopologySuite.Algorithm.Locate
{
    /// <summary>
    /// Determines the <see cref="Location"/> of <see cref="Coordinate"/>s relative to
    /// an areal geometry, using indexing for efficiency.
    /// This algorithm is suitable for use in cases where
    /// many points will be tested against a given area.
    /// <para/>
    /// The <c>Location</c> is computed precisely, th that points
    /// located on the geometry boundary or segments will
    /// return <see cref="Location.Boundary"/>.
    /// <para/>
    /// <see cref="IPolygonal"/> and <see cref="LinearRing"/> geometries are supported.
    /// <para/>
    /// The index is lazy-loaded, which allows
    /// creating instances even if they are not used.
    /// <para/>
    /// Thread-safe and immutable.
    /// </summary>
    /// <author>Martin Davis</author>
    public class IndexedPointInAreaLocator : IPointOnGeometryLocator
    {
        private Geometry _geom;
        private IntervalIndexedGeometry _index;

        /// <summary>
        /// Creates a new locator for a given <see cref="Geometry"/>.
        /// Geometries containing <see cref="IPolygonal"/>s and <see cref="LinearRing"/> geometries are supported
        /// </summary>
        /// <param name="g">The Geometry to locate in</param>
        public IndexedPointInAreaLocator(Geometry g)
        {
            _geom = g;
        }

        /// <summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="Geometry"/>.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <returns>The location of the point in the geometry
        /// </returns>
        public Location Locate(Coordinate p)
        {
            // avoid calling synchronized method improves performance
            if (_index == null) CreateIndex();

            var rcc = new RayCrossingCounter(p);

            var visitor = new SegmentVisitor(rcc);
            _index.Query(p.Y, p.Y, visitor);

            /*
             // MD - slightly slower alternative
             List segs = index.query(p.y, p.y);
             countSegs(rcc, segs);
             */

            return rcc.Location;
        }

        /// <summary>
        /// Creates the indexed geometry, creating it if necessary.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CreateIndex()
        {
            if (_index == null)
            {
                _index = new IntervalIndexedGeometry(_geom);
                // no need to hold onto geom
                _geom = null;
            }
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
            private readonly bool _isEmpty;
            private readonly SortedPackedIntervalRTree<LineSegment> _index = new SortedPackedIntervalRTree<LineSegment>();

            public IntervalIndexedGeometry(Geometry geom)
            {
                if (geom.IsEmpty)
                    _isEmpty = true;
                else
                    Init(geom);
            }

            private void Init(Geometry geom)
            {
                var lines = LinearComponentExtracter.GetLines(geom);
                foreach (LineString line in lines)
                {
                    //-- only include rings of Polygons or LinearRings
                    if (!line.IsClosed)
                        continue;
                    var pts = line.Coordinates;
                    AddLine(pts);
                }
            }

            private void AddLine(Coordinate[] pts)
            {
                for (int i = 1; i < pts.Length; i++)
                {
                    var seg = new LineSegment(pts[i - 1], pts[i]);
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
                if (_isEmpty)
                    return;

                _index.Query(min, max, visitor);
            }
        }

    }
}
