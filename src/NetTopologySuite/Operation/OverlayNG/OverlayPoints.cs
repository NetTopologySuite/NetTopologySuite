using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Performs an overlay operation on inputs which are both point geometries.
    /// <para/>
    /// Semantics are:
    /// <list type="bullet">
    /// <item><description>Points are rounded to the precision model if provided</description></item>
    /// <item><description>Points with identical XY values are merged to a single point</description></item>
    /// <item><description>Extended ordinate values are preserved in the output, apart from merging</description></item>
    /// <item><description>An empty result is returned as <c>POINT EMPTY</c></description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class OverlayPoints
    {
        /// <summary>
        /// Performs an overlay operation on inputs which are both point geometries.
        /// </summary>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The result of the overlay operation</returns>
        public static Geometry Overlay(SpatialFunction opCode, Geometry geom0, Geometry geom1, PrecisionModel pm)
        {
            var overlay = new OverlayPoints(opCode, geom0, geom1, pm);
            return overlay.GetResult();
        }

        private readonly SpatialFunction _opCode;
        private readonly Geometry _geom0;
        private readonly Geometry _geom1;
        private readonly PrecisionModel _pm;
        private readonly GeometryFactory _geometryFactory;
        private List<Point> _resultList;

        /// <summary>
        /// Creates an instance of an overlay operation on inputs which are both point geometries.
        /// </summary>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="pm">The precision model to use</param>
        public OverlayPoints(SpatialFunction opCode, Geometry geom0, Geometry geom1, PrecisionModel pm)
        {
            _opCode = opCode;
            _geom0 = geom0;
            _geom1 = geom1;
            _pm = pm;
            _geometryFactory = geom0.Factory;
        }

        /// <summary>
        /// Gets the result of the overlay.
        /// </summary>
        public Geometry GetResult()
        {
            var map0 = BuildPointMap(_geom0);
            var map1 = BuildPointMap(_geom1);

            _resultList = new List<Point>();
            switch (_opCode)
            {
                case OverlayNG.INTERSECTION:
                    ComputeIntersection(map0, map1, _resultList);
                    break;
                case OverlayNG.UNION:
                    ComputeUnion(map0, map1, _resultList);
                    break;
                case OverlayNG.DIFFERENCE:
                    ComputeDifference(map0, map1, _resultList);
                    break;
                case OverlayNG.SYMDIFFERENCE:
                    ComputeDifference(map0, map1, _resultList);
                    ComputeDifference(map1, map0, _resultList);
                    break;
            }

            if (_resultList.Count == 0)
                return OverlayUtility.CreateEmptyResult(0, _geometryFactory);

            return _geometryFactory.BuildGeometry(_resultList);
        }

        private void ComputeIntersection(Dictionary<Coordinate, Point> map0, Dictionary<Coordinate, Point> map1,
            List<Point> resultList)
        {
            foreach (var entry in map0)
            {
                if (map1.ContainsKey(entry.Key))
                {
                    resultList.Add(CopyPoint(entry.Value));
                }
            }
        }

        private void ComputeDifference(Dictionary<Coordinate, Point> map0, Dictionary<Coordinate, Point> map1,
            List<Point> resultList)
        {
            foreach (var entry in map0)
            {
                if (!map1.ContainsKey(entry.Key))
                {
                    resultList.Add(CopyPoint(entry.Value));
                }
            }
        }

        private void ComputeUnion(Dictionary<Coordinate, Point> map0, Dictionary<Coordinate, Point> map1,
            List<Point> resultList)
        {
            // copy all points
            foreach (var p in map0.Values)
            {
                resultList.Add(CopyPoint(p));
            }

            foreach (var entry in map1)
            {
                if (!map0.ContainsKey(entry.Key))
                {
                    resultList.Add(CopyPoint(entry.Value));
                }
            }
        }

        private Point CopyPoint(Point pt)
        {
            // if pm is floating, the point coordinate is not changed
            if (OverlayUtility.IsFloating(_pm))
                return (Point) pt.Copy();

            // pm is fixed.  Round off X&Y ordinates, copy other ordinates unchanged
            var seq = pt.CoordinateSequence;
            var seq2 = seq.Copy();
            seq2.SetOrdinate(0, Ordinate.X, _pm.MakePrecise(seq.GetX(0)));
            seq2.SetOrdinate(0, Ordinate.Y, _pm.MakePrecise(seq.GetY(0)));
            return _geometryFactory.CreatePoint(seq2);
        }

        private Dictionary<Coordinate, Point> BuildPointMap(Geometry geom)
        {
            var map = new Dictionary<Coordinate, Point>();
            geom.Apply(new GeometryComponentFilter(g =>
            {
                if (!(g is Point pt))
                    return;
                if (g.IsEmpty)
                    return;

                var p = RoundCoord(pt, _pm);
                /*
                 * Only add first occurrence of a point.
                 * This provides the merging semantics of overlay
                 */
                if (!map.ContainsKey(p))
                    map.Add(p, pt);
            }));

            return map;
        }

        /// <summary>
        /// Round the key point if precision model is fixed.
        /// Note: return value is only copied if rounding is performed.
        /// </summary>
        static Coordinate RoundCoord(Point pt, PrecisionModel pm)
        {
            var p = pt.Coordinate;
            if (pm.IsFloating)
                return p;
            var p2 = p.Copy();
            pm.MakePrecise(p2);
            return p2;
        }

    }
}
