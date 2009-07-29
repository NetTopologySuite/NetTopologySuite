using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Index.IntervalRTree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm.Locate
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
            if (! (g is IPolygon<TCoordinate>))
              throw new ArgumentException("Argument must be IPolygon<TCoordinate>");
            BuildIndex(g);
        }

        private void BuildIndex(IGeometry<TCoordinate> g)
        {
            _index = new IntervalIndexedGeometry(g);
        }

        /**
       * Determines the {@link Location} of a point in an areal <see cref="IGeometry{TCoordinate}"/>.
       * 
       * @param p the point to test
       * @return the location of the point in the geometry  
       */
        public Locations Locate(TCoordinate p)
        {
            RayCrossingCounter<TCoordinate> rcc = new RayCrossingCounter<TCoordinate>(p);

            foreach (var lineSegment in _index.Query(new Interval(p[Ordinates.Y], p[Ordinates.Y])))
                rcc.CountSegment(lineSegment.P0,lineSegment.P1);

            /*
             // MD - slightly slower alternative
            List segs = index.query(p.y, p.y);
            countSegs(rcc, segs);
            */

            return rcc.Location;
        }

        private class IntervalIndexedGeometry
        //private class IntervalIndexedGeometry<TCoordinate>
        //    where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
        //                        IComparable<TCoordinate>, IConvertible,
        //                        IComputable<Double, TCoordinate>
        {

            private SortedPackedIntervalRTree<LineSegment<TCoordinate>> _index= 
                new SortedPackedIntervalRTree<LineSegment<TCoordinate>>();

            public IntervalIndexedGeometry(IGeometry<TCoordinate> geom)
            {
                Init(geom);
            }

            private void Init(IGeometry<TCoordinate> geom)
            {
                foreach (var line in LinearComponentExtracter<TCoordinate>.GetLines(geom))
                    AddLine(line.Coordinates);
            }

            private void AddLine(ICoordinateSequence<TCoordinate> iCoordinateSequence)
            {
                TCoordinate p0 = default(TCoordinate);
                foreach (var p1 in iCoordinateSequence)
                {
                    if ( !p0.Equals(default(TCoordinate)))
                    {
                        LineSegment<TCoordinate> segment = new LineSegment<TCoordinate>(p0, p1);
                        _index.Insert(segment.Bounds, segment);
                    }
                    p0 = p1;
                }
            }

            public IEnumerable<LineSegment<TCoordinate>> Query(Interval interval)
            {
              return _index.Query( interval );
            }

        }

    }
}
