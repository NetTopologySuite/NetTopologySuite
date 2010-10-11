using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.RTree;
using NPack.Interfaces;
#if DOTNET35
using sl = System.Linq;
#endif

namespace NetTopologySuite.Algorithm.Locate
{
    public class IndexedPointInAreaLocator<TCoordinate> : IPointOnGeometryLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private IntervalIndexedGeometry _index;

        ///<summary>
        /// Creates a new locator for a given <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        ///<param name="g">the Geometry to locate in</param>
        ///<exception cref="ArgumentException"></exception>
        public IndexedPointInAreaLocator(IGeometry<TCoordinate> g)
        {
            if (! (g is IPolygonal<TCoordinate>))
                throw new ArgumentException("Argument must be IPolygonal<TCoordinate>");
            BuildIndex(g);
        }

        /**
       * Determines the {@link Location} of a point in an areal <see cref="IGeometry{TCoordinate}"/>.
       * 
       * @param p the point to test
       * @return the location of the point in the geometry  
       */

        #region IPointOnGeometryLocator<TCoordinate> Members

        public Locations Locate(TCoordinate p)
        {
            RayCrossingCounter<TCoordinate> rcc = new RayCrossingCounter<TCoordinate>(p);

            foreach (LineSegment<TCoordinate> lineSegment in _index.Query(new Interval(p[Ordinates.Y], p[Ordinates.Y])))
                rcc.CountSegment(lineSegment.P0, lineSegment.P1);

            /*
             // MD - slightly slower alternative
            List segs = index.query(p.y, p.y);
            countSegs(rcc, segs);
            */

            return rcc.Location;
        }

        #endregion

        private void BuildIndex(IGeometry<TCoordinate> g)
        {
            _index = new IntervalIndexedGeometry(g);
        }

        #region Nested type: IntervalIndexedGeometry

        private class IntervalIndexedGeometry
            //private class IntervalIndexedGeometry<TCoordinate>
            //    where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            //                        IComparable<TCoordinate>, IConvertible,
            //                        IComputable<Double, TCoordinate>
        {
            private static readonly IBoundsFactory<Interval> _boundsFactory = new IntervalFactory();

            private readonly SortedPackedRTree<Interval, LineSegment<TCoordinate>> _index =
                new SortedPackedRTree<Interval, LineSegment<TCoordinate>>(_boundsFactory);

            public IntervalIndexedGeometry(IGeometry<TCoordinate> geom)
            {
                Init(geom);
            }

            private void Init(IGeometry<TCoordinate> geom)
            {
                foreach (ILineString<TCoordinate> line in LinearComponentExtracter<TCoordinate>.GetLines(geom))
                    AddLine(line.Coordinates);
            }

            private void AddLine(ICoordinateSequence<TCoordinate> coordinateSequence)
            {
                TCoordinate p0 = coordinateSequence.First;
                foreach (TCoordinate p1 in sl.Enumerable.Skip(coordinateSequence,1))
                {
                    LineSegment<TCoordinate> segment = new LineSegment<TCoordinate>(p0, p1);
                    _index.Insert(segment.Bounds, segment);
                    p0 = p1;
                }
            }

            public IEnumerable<LineSegment<TCoordinate>> Query(Interval interval)
            {
                return _index.Query(interval);
            }
        }

        #endregion
    }
}