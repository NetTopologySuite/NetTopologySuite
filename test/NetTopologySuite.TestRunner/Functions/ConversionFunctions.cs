using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class ConversionFunctions
    {
        public static Geometry PointsToLine(Geometry g)
        {
            var pts = g.Coordinates;
            var line = g.Factory.CreateLineString(pts);
            return line;
        }

        public static Geometry LineToPolygon(Geometry g)
        {
            if (g is IPolygonal) return g;
            // TODO: ensure ring is valid
            var ringList = new CoordinateList();
            var pts = g.Coordinates;
            foreach (var pt in pts)
            {
                ringList.Add(pt, true);
            }
            ringList.CloseRing();
            var ring = g.Factory.CreateLinearRing(ringList.ToCoordinateArray());
            return g.Factory.CreatePolygon(ring, null);
        }

        public static Geometry ToPoints(Geometry g1, Geometry g2)
        {
            var geoms = FunctionsUtil.BuildGeometry(g1, g2);
            return FunctionsUtil.GetFactoryOrDefault(new[] { g1, g2 })
                .CreateMultiPointFromCoords(geoms.Coordinates);
        }

        public static Geometry ToLines(Geometry g1, Geometry g2)
        {
            var geoms = FunctionsUtil.BuildGeometry(g1, g2);
            return FunctionsUtil.GetFactoryOrDefault(new[] { g1, g2 })
                .BuildGeometry(LinearComponentExtracter.GetLines(geoms));
        }

        public static Geometry ToGeometryCollection(Geometry g, Geometry g2)
        {

            var atomicGeoms = new List<Geometry>();
            if (g != null) AddComponents(g, atomicGeoms);
            if (g2 != null) AddComponents(g2, atomicGeoms);
            return g.Factory.CreateGeometryCollection(
                GeometryFactory.ToGeometryArray(atomicGeoms));
        }

        private static void AddComponents(Geometry g, List<Geometry> atomicGeoms)
        {
            if (!(g is GeometryCollection))
            {
                atomicGeoms.Add(g);
                return;
            }

            foreach (var gi in (GeometryCollection)g)
            {
                if (!(gi is GeometryCollection))
                    atomicGeoms.Add(gi);
            }
        }
    }
}
