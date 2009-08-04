using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;
//using GeoAPI.

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    public class GeometryCombiner<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private const bool skipEmpty = false;
        private readonly IGeometryFactory<TCoordinate> geomFactory;
        private readonly IList<IGeometry<TCoordinate>> inputGeoms;

        public GeometryCombiner(IList<IGeometry<TCoordinate>> geoms)
        {
            geomFactory = ExtractFactory(geoms);
            inputGeoms = geoms;
        }

        public static IGeometry<TCoordinate> Combine(IList<IGeometry<TCoordinate>> geoms)
        {
            GeometryCombiner<TCoordinate> combiner = new GeometryCombiner<TCoordinate>(geoms);
            return combiner.Combine();
        }

        public static IGeometry<TCoordinate> Combine(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            GeometryCombiner<TCoordinate> combiner = new GeometryCombiner<TCoordinate>(CreateList(g0, g1));
            return combiner.Combine();
        }

        public static IGeometry<TCoordinate> Combine(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1,
                                                     IGeometry<TCoordinate> g2)
        {
            GeometryCombiner<TCoordinate> combiner = new GeometryCombiner<TCoordinate>(CreateList(g0, g1, g2));
            return combiner.Combine();
        }

        private static IList<IGeometry<TCoordinate>> CreateList(IGeometry<TCoordinate> obj0, IGeometry<TCoordinate> obj1)
        {
            return new List<IGeometry<TCoordinate>> {obj0, obj1};
        }

        private static IList<IGeometry<TCoordinate>> CreateList(IGeometry<TCoordinate> obj0, IGeometry<TCoordinate> obj1,
                                                                IGeometry<TCoordinate> obj2)
        {
            return new List<IGeometry<TCoordinate>> {obj0, obj1, obj2};
        }

        public static IGeometryFactory<TCoordinate> ExtractFactory(IList<IGeometry<TCoordinate>> geoms)
        {
            if (geoms.Count == 0)
                return null;

            IEnumerator<IGeometry<TCoordinate>> geomenumerator = geoms.GetEnumerator();
            geomenumerator.MoveNext();

            return geomenumerator.Current.Factory;
        }

        public IGeometry<TCoordinate> Combine()
        {
            List<IGeometry<TCoordinate>> elems = new List<IGeometry<TCoordinate>>();
            foreach (IGeometry<TCoordinate> geom in inputGeoms)
                ExtractElements(geom, elems);

            if (elems.Count == 0)
                return geomFactory != null ? geomFactory.CreateGeometryCollection(null) : null;
            return geomFactory.BuildGeometry(elems);
        }

        private static void ExtractElements(IGeometry<TCoordinate> geom, ICollection<IGeometry<TCoordinate>> elems)
        {
            if (geom == null)
                return;

            IGeometryCollection<TCoordinate> geomcoll = geom as IGeometryCollection<TCoordinate>;

            if (geomcoll == null)
                elems.Add(geom);
            else
                foreach (IGeometry<TCoordinate> elemGeom in geomcoll)
                {
                    if (skipEmpty && elemGeom.IsEmpty)
                        continue;
                    elems.Add(elemGeom);
                }
            ;
        }
    }
}