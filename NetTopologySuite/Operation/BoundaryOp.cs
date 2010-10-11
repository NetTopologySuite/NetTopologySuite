using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation
{
    public class BoundaryOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private IGeometry<TCoordinate> _geom;
        private IGeometryFactory<TCoordinate> _geomFact;
        private IBoundaryNodeRule _bnRule;

        public BoundaryOp(IGeometry<TCoordinate> geom)
            : this(geom, new Mod2BoundaryNodeRule())
        {
        }

        public BoundaryOp(IGeometry<TCoordinate> geom, IBoundaryNodeRule bnRule)
        {
            _geom = geom;
            _geomFact = geom.Factory;
            _bnRule = bnRule;
        }

        public IGeometry<TCoordinate> GetBoundary()
        {
            if (_geom is ILineString<TCoordinate>)
                return BoundaryLineString((ILineString<TCoordinate>)_geom);
        
            if (_geom is IMultiLineString<TCoordinate>)
                return BoundaryMultiLineString((IMultiLineString<TCoordinate>) _geom);

            return _geom.Boundary;
        }

        private IMultiPoint<TCoordinate> GetEmptyMultiPoint()
        {
            return _geomFact.CreateMultiPoint();
        }

        private IGeometry<TCoordinate> BoundaryMultiLineString(IMultiLineString<TCoordinate> mLine)
        {
            if (_geom.IsEmpty)
                return GetEmptyMultiPoint();

            TCoordinate[] bdyPts = ComputeBoundaryCoordinates(mLine);

            // return Point or MultiPoint
            if (bdyPts.Length == 1)
            {
                return _geomFact.CreatePoint(bdyPts[0]);
            }
            // this handles 0 points case as well
            return _geomFact.CreateMultiPoint(bdyPts);
        }

        /*
        // MD - superseded
          private Coordinate[] computeBoundaryFromGeometryGraph(MultiLineString mLine)
          {
            GeometryGraph g = new GeometryGraph(0, mLine, bnRule);
            Coordinate[] bdyPts = g.getBoundaryPoints();
            return bdyPts;
          }
        */

        private SortedDictionary<TCoordinate, Counter> _endpointMap;

        private TCoordinate[] ComputeBoundaryCoordinates(IMultiLineString<TCoordinate> mLine)
        {
            List<TCoordinate> bdyPts = new List<TCoordinate>();
            _endpointMap = new SortedDictionary<TCoordinate, Counter>();
            foreach (ILineString<TCoordinate> line in ((IEnumerable<ILineString<TCoordinate>>)mLine))
            {
                if ( line.IsEmpty ) continue;
                AddEndpoint(line.StartPoint.Coordinate);
                AddEndpoint(line.EndPoint.Coordinate);
            }

            foreach (KeyValuePair<TCoordinate, Counter> map in _endpointMap)
            {
                if ( _bnRule.IsInBoundary(map.Value.Count))
                    bdyPts.Add(map.Key);
            }

            return bdyPts.ToArray();
        }

        private void AddEndpoint(TCoordinate pt)
        {
            Counter counter = null;
            if (!_endpointMap.TryGetValue(pt, out counter))
            {
                counter = new Counter();
                _endpointMap.Add(pt, counter);
            }
            counter.Count++;
        }

        private IGeometry<TCoordinate> BoundaryLineString(ILineString<TCoordinate> line)
        {
            if (_geom.IsEmpty)
                return GetEmptyMultiPoint();

            if (line.IsClosed)
            {
                // check whether endpoints of valence 2 are on the boundary or not
                Boolean closedEndpointOnBoundary = _bnRule.IsInBoundary(2);
                if (closedEndpointOnBoundary)
                    return line.StartPoint;

                return _geomFact.CreateMultiPoint();
            }
            return _geomFact.CreateMultiPoint(line.StartPoint, line.EndPoint);

        }

        /**
 * Stores an integer count, for use as a Map entry.
 *
 * @author Martin Davis
 * @version 1.7
 */
        private class Counter
        {
            /**
             * The value of the count
             */
            public int Count = 0;
        }

    }
}
