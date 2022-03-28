using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    internal class AddZDimension
    {

        public static Geometry Do(Geometry geometry)
        {
            if (geometry.Coordinate is CoordinateZ)
                return geometry;

            var addZ = new AddZFilter(geometry.Factory);
            geometry.Apply(addZ);

            return addZ.ZGeometry;
        }

        private class AddZFilter : IGeometryFilter
        {
            private readonly GeometryFactory _factory;
            private Geometry _zgeometry;

            public AddZFilter(GeometryFactory factory)
            {
                _factory = factory;
            }

            public void Filter(Geometry geom)
            {
                _zgeometry = AddZ(geom);
            }

            public Geometry ZGeometry => _zgeometry;

            private Geometry AddZ(Geometry geom)
            {
                if (geom is Point pt)
                    return _factory.CreatePoint(AddZ(pt.CoordinateSequence));

                if (geom is LinearRing lr)
                    return _factory.CreateLinearRing(AddZ(lr.CoordinateSequence));

                if (geom is LineString ls)
                    return _factory.CreateLineString(AddZ(ls.CoordinateSequence));

                if (geom is Polygon pg)
                {
                    var shell = (LinearRing)AddZ(pg.ExteriorRing);
                    if (pg.NumInteriorRings == 0) return _factory.CreatePolygon(shell);

                    var holes = new LinearRing[pg.NumInteriorRings];
                    for (int i = 0; i < pg.NumInteriorRings; i++)
                        holes[i] = (LinearRing)AddZ(pg.GetInteriorRingN(i));
                    return _factory.CreatePolygon(shell, holes);
                }

                var zgeoms = new Geometry[geom.NumGeometries];
                for (int i = 0; i < geom.NumGeometries; i++)
                    zgeoms[i] = AddZ(geom.GetGeometryN(i));

                return _factory.BuildGeometry(zgeoms);
            }

            private CoordinateSequence AddZ(CoordinateSequence seq)
            {
                if (seq.HasZ) return seq.Copy();

                var res = _factory.CoordinateSequenceFactory.Create(seq.Count, seq.Dimension + 1, seq.Measures);
                for (int i = 0; i < seq.Count; i++)
                {
                    for (int j = 0; j < seq.Spatial; j++)
                        res.SetOrdinate(i, j, seq.GetOrdinate(i, j));
                    for (int j = seq.Spatial; j < seq.Dimension; j++)
                        res.SetOrdinate(i, j + 1, seq.GetOrdinate(i, j));
                }
                return res;
            }
        }

    }
}
