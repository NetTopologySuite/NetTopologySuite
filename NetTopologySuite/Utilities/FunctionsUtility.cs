using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    public class FunctionsUtil
    {

        public static readonly Envelope DefaultEnvelope = new Envelope(0, 100, 0, 100);

        public static Envelope GetEnvelopeOrDefault(Geometry g)
        {
            return g == null ? DefaultEnvelope : g.EnvelopeInternal;
        }

        private static readonly GeometryFactory Factory = new GeometryFactory();

        public static GeometryFactory GetFactoryOrDefault(Geometry g)
        {
            return g == null ? Factory : g.Factory;
        }

        public static GeometryFactory GetFactoryOrDefault(IEnumerable<Geometry> gs)
        {
            if (gs == null)
                return Factory;
            foreach (var g in gs)
            {
                if (g != null)
                    return g.Factory ?? Factory;
            }
            return Factory;
        }

        public static Geometry BuildGeometry(List<Geometry> geoms, Geometry parentGeom)
        {
            if (geoms.Count <= 0)
                return null;
            if (geoms.Count == 1)
                return geoms[0];
            // if parent was a GC, ensure returning a GC
            if (parentGeom != null && parentGeom.OgcGeometryType == OgcGeometryType.GeometryCollection)
                return parentGeom.Factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));
            // otherwise return MultiGeom
            return GetFactoryOrDefault(geoms).BuildGeometry(geoms);
        }

        public static Geometry BuildGeometry(params Geometry[] geoms)
        {
            var gf = GetFactoryOrDefault(geoms);
            return gf.CreateGeometryCollection(geoms);
        }

        public static Geometry BuildGeometry(Geometry a, Geometry b)
        {
            int size = 0;
            if (a != null) size++;
            if (b != null) size++;
            var geoms = new Geometry[size];
            size = 0;
            if (a != null) geoms[size++] = a;
            if (b != null) geoms[size] = b;
            return GetFactoryOrDefault(geoms).CreateGeometryCollection(geoms);
        }
    }
}