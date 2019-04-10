using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Union
{
    public class PointGeometryUnion
    {
        private readonly GeometryFactory _geomFact;
        private readonly Geometry _otherGeom;
        private readonly Geometry _pointGeom;

        public PointGeometryUnion(IPuntal pointGeom, Geometry otherGeom)
        {
            _pointGeom = (Geometry)pointGeom;
            _otherGeom = otherGeom;
            _geomFact = otherGeom.Factory;
        }

        /// <summary>
        /// Computes the union of a <see cref="Point"/> geometry with
        /// another arbitrary <see cref="Geometry"/>.
        /// Does not copy any component geometries.
        /// </summary>
        /// <param name="pointGeom"></param>
        /// <param name="otherGeom"></param>
        /// <returns></returns>
        public static Geometry Union(IPuntal pointGeom, Geometry otherGeom)
        {
            var unioner = new PointGeometryUnion(pointGeom, otherGeom);
            return unioner.Union();
        }

        public Geometry Union()
        {
            var locater = new PointLocator();
            // use a set to eliminate duplicates, as required for union
            var exteriorCoords = new HashSet<Coordinate>();

            foreach (Point point in PointExtracter.GetPoints(_pointGeom))
            {
                var coord = point.Coordinate;
                var loc = locater.Locate(coord, _otherGeom);

                if (loc == Location.Exterior)
                {
                    exteriorCoords.Add(coord);
                }
            }

            // if no points are in exterior, return the other geom
            if (exteriorCoords.Count == 0)
            {
                return _otherGeom;
            }

            // make a puntal geometry of appropriate size
            var exteriorCoordsArray = new Coordinate[exteriorCoords.Count];
            exteriorCoords.CopyTo(exteriorCoordsArray, 0);
            Array.Sort(exteriorCoordsArray);
            var coords = _geomFact.CoordinateSequenceFactory.Create(exteriorCoordsArray);
            var ptComp = coords.Count == 1
                ? (Geometry)_geomFact.CreatePoint(coords)
                : _geomFact.CreateMultiPoint(coords);

            // add point component to the other geometry
            return GeometryCombiner.Combine(ptComp, _otherGeom);
        }
    }
}
