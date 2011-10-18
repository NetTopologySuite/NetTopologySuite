using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class ConversionFunctions
    {
        public static IGeometry toGeometryCollection(IGeometry g)
        {
            if (!(g is IGeometryCollection))
            {
                return g.Factory.CreateGeometryCollection(new[] {g});
            }

            var atomicGeoms = new List<IGeometry>();
            var it = new GeometryCollectionEnumerator(g as IGeometryCollection);
            while (it.MoveNext())
            {
                var g2 = it.Current;
                if (!(g2 is IGeometryCollection))
                    atomicGeoms.Add(g2);
            }

            return g.Factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(atomicGeoms));
        }
    }
}