using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    public class GeometryCombiner
    {
        public static IGeometry Combine(ICollection<IGeometry> geoms)
        {
            var combiner = new GeometryCombiner(geoms);
            return combiner.Combine();
        }

        public static IGeometry Combine(IGeometry g0, IGeometry g1)
        {
            var combiner = new GeometryCombiner(CreateList(g0, g1));
            return combiner.Combine();
        }

        public static IGeometry Combine(IGeometry g0, IGeometry g1, IGeometry g2)
        {
            var combiner = new GeometryCombiner(CreateList(g0, g1, g2));
            return combiner.Combine();
        }

        private static List<IGeometry> CreateList(IGeometry obj0, IGeometry obj1)
        {
            return new List<IGeometry> {obj0, obj1};
        }

        private static List<IGeometry> CreateList(IGeometry obj0, IGeometry obj1, IGeometry obj2)
        {
            return new List<IGeometry> {obj0, obj1, obj2};
        }

        private readonly IGeometryFactory geomFactory;
        private const bool skipEmpty = false;
        private readonly ICollection<IGeometry> inputGeoms;

        public GeometryCombiner(ICollection<IGeometry> geoms)
        {
            geomFactory = ExtractFactory(geoms);
            inputGeoms = geoms;
        }

       public static IGeometryFactory ExtractFactory(ICollection<IGeometry> geoms)
        {
            if (geoms.Count == 0)
                return null;

            var geomenumerator = geoms.GetEnumerator();
            geomenumerator.MoveNext();

            return geomenumerator.Current.Factory;
        }

        public IGeometry Combine()
        {
            var elems = new List<IGeometry>();
            foreach (var geom in inputGeoms)
                ExtractElements(geom, elems);

            if (elems.Count == 0)
                return geomFactory != null ? geomFactory.CreateGeometryCollection(null) : null;
            return geomFactory.BuildGeometry(elems);
        }

        private static void ExtractElements(IGeometry geom, ICollection<IGeometry> elems)
        {
            if (geom == null)
                return;

            for (var i = 0; i < geom.NumGeometries; i++)
            {
                var elemGeom = geom.GetGeometryN(i);
                if (skipEmpty && elemGeom.IsEmpty)
                    continue;
                elems.Add(elemGeom);
            }
        }

    }
}