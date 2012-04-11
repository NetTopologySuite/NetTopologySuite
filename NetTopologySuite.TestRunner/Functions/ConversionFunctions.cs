using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class ConversionFunctions
    {
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