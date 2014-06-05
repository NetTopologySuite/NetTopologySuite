using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Operation.Overlay.Snap;

namespace Open.Topology.TestRunner.Functions
{
    /// <summary>
    /// Implementations for various geometry functions.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class GeometryFunctions
    {
        public static double Length(IGeometry g) { return g.Length; }
        public static double Area(IGeometry g) { return g.Area; }

        public static bool IsCCW(IGeometry g)
        {
            Coordinate[] pts = null;
            if (g is IPolygon)
                pts = ((IPolygon)g).ExteriorRing.Coordinates;
            else if (g is ILineString && ((ILineString)g).IsClosed)
                pts = g.Coordinates;
            if (pts == null)
                return false;
            return CGAlgorithms.IsCCW(pts);
        }

        public static bool IsSimple(IGeometry g) { return g.IsSimple; }
        public static bool IsValid(IGeometry g) { return g.IsValid; }
        public static bool IsRectangle(IGeometry g) { return g.IsRectangle; }
        public static bool IsClosed(IGeometry g)
        {
            if (g is ILineString)
                return ((ILineString)g).IsClosed;
            if (g is IMultiLineString)
                return ((IMultiLineString)g).IsClosed;
            // other geometry types are defined to be closed
            return true;
        }

        public static IGeometry Envelope(IGeometry g) { return g.Envelope; }
        public static IGeometry Reverse(IGeometry g) { return g.Reverse(); }
        public static IGeometry Normalize(IGeometry g)
        {
            IGeometry gNorm = (IGeometry)g.Clone();
            gNorm.Normalize();
            return gNorm;
        }

        public static IGeometry Snap(IGeometry g, IGeometry g2, double distance)
        {
            IGeometry[] snapped = GeometrySnapper.Snap(g, g2, distance);
            return snapped[0];
        }

        public static IGeometry GetGeometryN(IGeometry g, int i)
        {
            return g.GetGeometryN(i);
        }

        public static IGeometry GetPolygonShell(IGeometry g)
        {
            if (g is IPolygon)
            {
                ILinearRing shell = (ILinearRing)((IPolygon)g).ExteriorRing;
                return g.Factory.CreatePolygon(shell, null);
            }
            if (g is IMultiPolygon)
            {
                IPolygon[] poly = new IPolygon[g.NumGeometries];
                for (int i = 0; i < g.NumGeometries; i++)
                {
                    ILinearRing shell = (ILinearRing)((IPolygon)g.GetGeometryN(i)).ExteriorRing;
                    poly[i] = g.Factory.CreatePolygon(shell, null);
                }
                return g.Factory.CreateMultiPolygon(poly);
            }
            return null;
        }

        private class InternalGeometryFilterImpl : IGeometryFilter
        {
            private readonly IList<IGeometry> _holePolys;

            public InternalGeometryFilterImpl(IList<IGeometry> holePolys)
            {
                _holePolys = holePolys;
            }

            public void Filter(IGeometry geom)
            {
                if (!(geom is IPolygon))
                    return;
                IPolygon poly = (IPolygon)geom;
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    IPolygon hole = geom.Factory.CreatePolygon((ILinearRing)poly.GetInteriorRingN(i), null);
                    _holePolys.Add(hole);
                }
            }
        }
        public static IGeometry GetPolygonHoles(IGeometry geom)
        {
            IList<IGeometry> holePolys = new List<IGeometry>();
            geom.Apply(new InternalGeometryFilterImpl(holePolys));
            return geom.Factory.BuildGeometry(holePolys);
        }

        public static IGeometry GetPolygonHoleN(IGeometry g, int i)
        {
            if (g is IPolygon)
            {
                ILinearRing ring = (ILinearRing)((IPolygon)g).GetInteriorRingN(i);
                return ring;
            }
            return null;
        }

        public static IGeometry ConvertToPolygon(IGeometry g)
        {
            if (g is IPolygon)
                return g;
            // TODO: ensure ring is valid
            ILinearRing ring = g.Factory.CreateLinearRing(g.Coordinates);
            return g.Factory.CreatePolygon(ring, null);
        }

        public static IGeometry GetCoordinates(IGeometry g)
        {
            Coordinate[] pts = g.Coordinates;
            return g.Factory.CreateMultiPoint(pts);
        }
    }
}