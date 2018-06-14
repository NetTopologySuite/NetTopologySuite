using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    public class FunctionsUtil
    {

        public static readonly Envelope DefaultEnvelope = new Envelope(0, 100, 0, 100);

        public static Envelope GetEnvelopeOrDefault(IGeometry g)
        {
            return g == null ? DefaultEnvelope : g.EnvelopeInternal;
        }

        private static readonly IGeometryFactory Factory = new GeometryFactory();

        public static IGeometryFactory GetFactoryOrDefault(IGeometry g)
        {
            return g == null ? Factory : g.Factory;
        }

        public static IGeometryFactory GetFactoryOrDefault(IEnumerable<IGeometry> gs)
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

        public static IGeometry BuildGeometry(List<IGeometry> geoms, IGeometry parentGeom)
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

        public static IGeometry BuildGeometry(params IGeometry[] geoms)
        {
            var gf = GetFactoryOrDefault(geoms);
            return gf.CreateGeometryCollection(geoms);
        }

        public static IGeometry BuildGeometry(IGeometry a, IGeometry b)
        {
            int size = 0;
            if (a != null) size++;
            if (b != null) size++;
            var geoms = new IGeometry[size];
            size = 0;
            if (a != null) geoms[size++] = a;
            if (b != null) geoms[size] = b;
            return GetFactoryOrDefault(geoms).CreateGeometryCollection(geoms);
        }
    }
}