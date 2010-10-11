using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NPack.Interfaces;
//using GeoAPI.

namespace NetTopologySuite.Geometries.Utilities
{
    public class GeometryCombiner<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private const bool _skipEmpty = false;
        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private readonly IEnumerable<IGeometry<TCoordinate>> _inputGeoms;

        public GeometryCombiner(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            _geomFactory = ExtractFactory(geoms);
            _inputGeoms = geoms;
        }

        public static IGeometry<TCoordinate> Combine(IEnumerable<IGeometry<TCoordinate>> geoms)
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

        public static IGeometryFactory<TCoordinate> ExtractFactory(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            IGeometry<TCoordinate> geom = Slice.GetFirst(geoms);
            return geom.Factory;
        }

        public IGeometry<TCoordinate> Combine()
        {
            List<IGeometry<TCoordinate>> elems = new List<IGeometry<TCoordinate>>();
            foreach (IGeometry<TCoordinate> geom in _inputGeoms)
                elems.AddRange(ExtractElements(geom));

            if (!Slice.CountGreaterThan(_inputGeoms, 0))
                return _geomFactory != null ? _geomFactory.CreateGeometryCollection(null) : null;
            return _geomFactory.BuildGeometry(elems);
        }

        private static IEnumerable<IGeometry<TCoordinate>> ExtractElements(IGeometry<TCoordinate> geom)
        {
            if (geom == null)
                yield break;

            IGeometryCollection<TCoordinate> geomcoll = geom as IGeometryCollection<TCoordinate>;

            if (geomcoll == null)
                yield return geom;
            else
                foreach (IGeometry<TCoordinate> elemGeom in geomcoll)
                {
                    if (_skipEmpty && elemGeom.IsEmpty)
                        continue;
                    yield return elemGeom;
                }
        }
    }
}