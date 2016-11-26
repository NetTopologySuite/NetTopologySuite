using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation
{
    ///<summary>
    /// Computes the boundary of a <see cref="IGeometry"/>.
    /// Allows specifying the <see cref="IBoundaryNodeRule"/> to be used.
    /// This operation will always return a <see cref="IGeometry"/> of the appropriate
    /// dimension for the boundary (even if the input geometry is empty).
    /// The boundary of zero-dimensional geometries (Points) is
    /// always the empty <see cref="IGeometryCollection"/>.
    ///</summary>
    /// <author>Martin Davis</author>
    public class BoundaryOp
    {

        public static IGeometry GetBoundary(IGeometry g)
        {
            var bop = new BoundaryOp(g);    
            return bop.GetBoundary();
        }

        public static IGeometry GetBoundary(IGeometry g, IBoundaryNodeRule bnRule)
        {
            var bop = new BoundaryOp(g, bnRule);
            return bop.GetBoundary();
        }

        private readonly IGeometry _geom;
        private readonly IGeometryFactory _geomFact;
        private readonly IBoundaryNodeRule _bnRule;

        public BoundaryOp(IGeometry geom)
            : this(geom, BoundaryNodeRules.Mod2BoundaryRule)
        {
        }

        public BoundaryOp(IGeometry geom, IBoundaryNodeRule bnRule)
        {
            _geom = geom;
            _geomFact = geom.Factory;
            _bnRule = bnRule;
        }

        public IGeometry GetBoundary()
        {
            if (_geom is ILineString) return BoundaryLineString((ILineString)_geom);
            if (_geom is IMultiLineString) return BoundaryMultiLineString((IMultiLineString)_geom);
            return _geom.Boundary;
        }

        private IMultiPoint GetEmptyMultiPoint()
        {
            return _geomFact.CreateMultiPoint((ICoordinateSequence)null);
        }

        private IGeometry BoundaryMultiLineString(IMultiLineString mLine)
        {
            if (_geom.IsEmpty)
            {
                return GetEmptyMultiPoint();
            }

            Coordinate[] bdyPts = ComputeBoundaryCoordinates(mLine);

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

        /// <summary>
        /// A map which maintains the edges in sorted order around the node.
        /// </summary>
        private IDictionary<Coordinate, Counter> _endpointMap;
        //private Map endpointMap;

        private Coordinate[] ComputeBoundaryCoordinates(IMultiLineString mLine)
        {
            IList<Coordinate> bdyPts = new List<Coordinate>();
            _endpointMap = new SortedDictionary<Coordinate, Counter>();
            for (int i = 0; i < mLine.NumGeometries; i++)
            {
                ILineString line = (ILineString)mLine.GetGeometryN(i);
                if (line.NumPoints == 0)
                    continue;
                AddEndpoint(line.GetCoordinateN(0));
                AddEndpoint(line.GetCoordinateN(line.NumPoints - 1));
            }

            foreach (KeyValuePair<Coordinate, Counter> entry in _endpointMap)
            {
                Counter counter = entry.Value;
                int valence = counter.Count;
                if (_bnRule.IsInBoundary(valence))
                {
                    bdyPts.Add(entry.Key);
                }
            }

            return CoordinateArrays.ToCoordinateArray(bdyPts);
        }

        private void AddEndpoint(Coordinate pt)
        {
            Counter counter;
            if (!_endpointMap.TryGetValue(pt, out counter))
            {
                counter = new Counter();
                _endpointMap.Add(pt, counter);
            }
            counter.Count++;
        }

        private IGeometry BoundaryLineString(ILineString line)
        {
            if (_geom.IsEmpty)
            {
                return GetEmptyMultiPoint();
            }

            if (line.IsClosed)
            {
                // check whether endpoints of valence 2 are on the boundary or not
                bool closedEndpointOnBoundary = _bnRule.IsInBoundary(2);
                if (closedEndpointOnBoundary)
                {
                    return line.StartPoint;
                }
                return _geomFact.CreateMultiPoint((Coordinate[])null);
            }
            return _geomFact.CreateMultiPoint(new[]
                        {
                             line.StartPoint,
                             line.EndPoint
                        });
        }
    }

    ///<summary>
    /// Stores an integer count, for use as a Map entry.
    ///</summary>
    /// <author>Martin Davis</author>
    class Counter
    {
        //<see cref="The value of the count"/>
        public int Count;
    }
}