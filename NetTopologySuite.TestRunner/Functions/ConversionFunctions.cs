using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class ConversionFunctions
    {
        public static IGeometry toPoints(IGeometry g)
        {
            return g.Factory.CreateMultiPoint(g.Coordinates);
        }

        public static IGeometry toLines(IGeometry g)
        {
            return g.Factory.BuildGeometry(LinearComponentExtracter.GetLines(g));
        }

        public static IGeometry toGeometryCollection(IGeometry g, IGeometry g2)
        {

            var atomicGeoms = new List<IGeometry>();
            if (g != null) addComponents(g, atomicGeoms);
            if (g2 != null) addComponents(g2, atomicGeoms);
            return g.Factory.CreateGeometryCollection(
                GeometryFactory.ToGeometryArray(atomicGeoms));
        }

        private static void addComponents(IGeometry g, List<IGeometry> atomicGeoms)
        {
            if (!(g is IGeometryCollection))
            {
                atomicGeoms.Add(g);
                return;
            }

            foreach (var gi in (IGeometryCollection)g)
            {
                if (!(gi is IGeometryCollection))
                    atomicGeoms.Add(gi);
            }
        }
    }
}