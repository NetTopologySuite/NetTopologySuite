using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;

namespace Open.Topology.TestRunner.Functions
{
    /// <summary>
    /// Implementations for various geometry functions.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class GeometryFunctions
    {
        public static double Length(Geometry g) { return g.Length; }
        public static double Area(Geometry g) { return g.Area; }

        public static bool IsCCW(Geometry g)
        {
            CoordinateSequence pts = null;
            if (g is Polygon)
                pts = ((Polygon)g).ExteriorRing.CoordinateSequence;
            else if (g is LineString && ((LineString)g).IsClosed)
                pts = ((LineString)g).CoordinateSequence;
            if (pts == null)
                return false;
            return Orientation.IsCCW(pts);
        }

        public static bool IsSimple(Geometry g) { return g.IsSimple; }
        public static bool IsValid(Geometry g) { return g.IsValid; }
        public static bool IsRectangle(Geometry g) { return g.IsRectangle; }
        public static bool IsClosed(Geometry g)
        {
            if (g is LineString)
                return ((LineString)g).IsClosed;
            if (g is MultiLineString)
                return ((MultiLineString)g).IsClosed;
            // other geometry types are defined to be closed
            return true;
        }

        public static Geometry Envelope(Geometry g) { return g.Envelope; }
        public static Geometry Reverse(Geometry g) { return g.Reverse(); }
        public static Geometry Normalize(Geometry g)
        {
            var gNorm = g.Copy();
            gNorm.Normalize();
            return gNorm;
        }

        public static Geometry Snap(Geometry g, Geometry g2, double distance)
        {
            var snapped = GeometrySnapper.Snap(g, g2, distance);
            return snapped[0];
        }

        public static Geometry GetGeometryN(Geometry g, int i)
        {
            return g.GetGeometryN(i);
        }

        public static Geometry GetPolygonShell(Geometry g)
        {
            if (g is Polygon)
            {
                var shell = (LinearRing)((Polygon)g).ExteriorRing;
                return g.Factory.CreatePolygon(shell, null);
            }
            if (g is MultiPolygon)
            {
                var poly = new Polygon[g.NumGeometries];
                for (int i = 0; i < g.NumGeometries; i++)
                {
                    var shell = (LinearRing)((Polygon)g.GetGeometryN(i)).ExteriorRing;
                    poly[i] = g.Factory.CreatePolygon(shell, null);
                }
                return g.Factory.CreateMultiPolygon(poly);
            }
            return null;
        }

        private class InternalGeometryFilterImpl : IGeometryFilter
        {
            private readonly IList<Geometry> _holePolys;

            public InternalGeometryFilterImpl(IList<Geometry> holePolys)
            {
                _holePolys = holePolys;
            }

            public void Filter(Geometry geom)
            {
                if (!(geom is Polygon))
                    return;
                var poly = (Polygon)geom;
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    var hole = geom.Factory.CreatePolygon((LinearRing)poly.GetInteriorRingN(i), null);
                    _holePolys.Add(hole);
                }
            }
        }
        public static Geometry GetPolygonHoles(Geometry geom)
        {
            IList<Geometry> holePolys = new List<Geometry>();
            geom.Apply(new InternalGeometryFilterImpl(holePolys));
            return geom.Factory.BuildGeometry(holePolys);
        }

        public static Geometry GetPolygonHoleN(Geometry g, int i)
        {
            if (g is Polygon)
            {
                var ring = (LinearRing)((Polygon)g).GetInteriorRingN(i);
                return ring;
            }
            return null;
        }

        public static Geometry GetCoordinates(Geometry g)
        {
            var pts = g.Coordinates;
            return g.Factory.CreateMultiPointFromCoords(pts);
        }
    }
}
