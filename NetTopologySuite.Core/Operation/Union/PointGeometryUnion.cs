using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Union
{
    public class PointGeometryUnion
    {
        private readonly IGeometryFactory _geomFact;
        private readonly IGeometry _otherGeom;
        private readonly IGeometry _pointGeom;

        public PointGeometryUnion(IPuntal pointGeom, IGeometry otherGeom)
        {
            _pointGeom = (IGeometry)pointGeom;
            _otherGeom = otherGeom;
            _geomFact = otherGeom.Factory;
        }

        ///<summary>
        /// Computes the union of a <see cref="IPoint"/> geometry with 
        /// another arbitrary <see cref="IGeometry"/>.
        /// Does not copy any component geometries.
        ///</summary>
        ///<param name="pointGeom"></param>
        ///<param name="otherGeom"></param>
        ///<returns></returns>
        public static IGeometry Union(IPuntal pointGeom, IGeometry otherGeom)
        {
            var unioner = new PointGeometryUnion(pointGeom, otherGeom);
            return unioner.Union();
        }

        public IGeometry Union()
        {
            PointLocator locater = new PointLocator();
            // use a set to eliminate duplicates, as required for union
            var exteriorCoords = new HashSet<Coordinate>();

            foreach (IPoint point in PointExtracter.GetPoints(_pointGeom))
            {
                Coordinate coord = point.Coordinate;
                Location loc = locater.Locate(coord, _otherGeom);

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
            ICoordinateSequence coords = _geomFact.CoordinateSequenceFactory.Create(exteriorCoordsArray);
            IGeometry ptComp = coords.Count == 1 ? (IGeometry)_geomFact.CreatePoint(coords.GetCoordinate(0)) : _geomFact.CreateMultiPoint(coords);

            // add point component to the other geometry
            return GeometryCombiner.Combine(ptComp, _otherGeom);
        }
    }
}
